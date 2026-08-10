using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    private LevelGrid levelGrid;

    private Vector2Int gridMoveDirection;
    private Vector2Int gridPosition;

    private float gridMoveTimer;
    private float gridMoveTimerMax;

    private int snakeBodySize;
    private List<Vector2Int> snakeMovePositionList;
    private List<Transform> snakeBodyTransformList;

    private void Awake() {
        gridPosition = new Vector2Int(10, 10);

        gridMoveTimerMax = 0.1f;
        gridMoveTimer = gridMoveTimerMax;

        gridMoveDirection = new Vector2Int(1, 0);

        snakeBodySize = 0;

        snakeMovePositionList = new List<Vector2Int>();
        snakeBodyTransformList = new List<Transform>();

        UpdateSnakeRotation();
    }

    private Transform CreateSnakeBodyTransform() {
        GameObject snakeBodyGameObject = new GameObject("Snake Body", typeof(SpriteRenderer));
        snakeBodyGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.i.snakeBodySprite;
        return snakeBodyGameObject.transform;
    }

    private void Update() {
        HandleInput();
        HandleGridMovement();
    }

    public void SetLevelGrid(LevelGrid levelGrid) {
        this.levelGrid = levelGrid;
    }

    private void UpdateSnakeRotation() {
        float angle = Mathf.Atan2(gridMoveDirection.y, gridMoveDirection.x) * Mathf.Rad2Deg + 90f;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private void HandleInput() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (gridMoveDirection.y != -1) {
                gridMoveDirection.x = 0;
                gridMoveDirection.y = 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (gridMoveDirection.y != 1) {
                gridMoveDirection.x = 0;
                gridMoveDirection.y = -1;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (gridMoveDirection.x != 1) {
                gridMoveDirection.x = -1;
                gridMoveDirection.y = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (gridMoveDirection.x != -1) {
                gridMoveDirection.x = 1;
                gridMoveDirection.y = 0;
            }
        }
    }

    private void HandleGridMovement() {
        gridMoveTimer += Time.deltaTime;

        if (gridMoveTimer >= gridMoveTimerMax) {
            gridMoveTimer -= gridMoveTimerMax;

            // Store the snake head's current position before moving
            snakeMovePositionList.Insert(0, gridPosition);

            // Move the snake head
            gridPosition += gridMoveDirection;

            bool snakeAteFood = levelGrid.TrySnakeEatFood(gridPosition);

            if (snakeAteFood) {
                // Grow: keep the extra history entry we just inserted instead of
                // trimming it away, and use it as the new tail segment's starting
                // position -- so the new body part appears immediately at the
                // correct trailing spot, with no extra move needed to reveal it.
                snakeBodySize++;

                Transform newBodyTransform = CreateSnakeBodyTransform();
                Vector2Int newBodyPosition = snakeMovePositionList[snakeMovePositionList.Count - 1];
                newBodyTransform.position = new Vector3(newBodyPosition.x, newBodyPosition.y, 0);
                snakeBodyTransformList.Add(newBodyTransform);
            } else {
                // Not growing this tick: trim the oldest history entry so the
                // list stays exactly as long as the current body size.
                if (snakeMovePositionList.Count > snakeBodySize) {
                    snakeMovePositionList.RemoveAt(snakeMovePositionList.Count - 1);
                }
            }

            // Update each body part to follow along the recorded path
            for (int i = 0; i < snakeBodyTransformList.Count; i++) {
                snakeBodyTransformList[i].position = new Vector3(snakeMovePositionList[i].x, snakeMovePositionList[i].y, 0);
            }

            UpdateSnakeRotation();
        }

        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    public List<Vector2Int> GetFullSnakeGridPositionList() {
        List<Vector2Int> fullSnakeGridPositionList = new List<Vector2Int>() { gridPosition };
        fullSnakeGridPositionList.AddRange(snakeMovePositionList.GetRange(0, snakeBodySize));
        return fullSnakeGridPositionList;
    }
}
