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

    [SerializeField] private Sprite snakeBodySprite;

    private void Awake() {
        gridPosition = new Vector2Int(10, 10);

        gridMoveTimerMax = 1f;
        gridMoveTimer = gridMoveTimerMax;

        gridMoveDirection = new Vector2Int(1, 0);

        snakeBodySize = 10;

        snakeMovePositionList = new List<Vector2Int>();
        snakeBodyTransformList = new List<Transform>();

        // Create the persistent body part(s) once, using the White (1x1) sprite asset.
        // Start them stacked on the head so they don't flash at the world origin
        // before they've received real position history.
        for (int i = 0; i < snakeBodySize; i++) {
            Transform bodyTransform = CreateSnakeBodyTransform();
            bodyTransform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
            snakeBodyTransformList.Add(bodyTransform);
        }
        UpdateSnakeRotation();
    }

    private Transform CreateSnakeBodyTransform() {
        GameObject bodyPart = new GameObject("Snake Body");

        SpriteRenderer spriteRenderer = bodyPart.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = snakeBodySprite;

        bodyPart.transform.localScale = Vector3.one * 0.75f;

        return bodyPart.transform;
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

            // Keep only as many stored positions as there are body parts
            if (snakeMovePositionList.Count > snakeBodySize) {
                snakeMovePositionList.RemoveAt(snakeMovePositionList.Count - 1);
            }

            // Move the snake head
            gridPosition += gridMoveDirection;

            // Update each persistent body part to follow behind the head.
            // Only update parts for which we have recorded position history so far
            // (early on, the list has fewer entries than there are body parts).
            for (int i = 0; i < snakeBodyTransformList.Count && i < snakeMovePositionList.Count; i++) {
                Vector2Int snakeMovePosition = snakeMovePositionList[i];

                snakeBodyTransformList[i].position = new Vector3(snakeMovePosition.x, snakeMovePosition.y, 0);
            }

            if (levelGrid != null) {
                levelGrid.SnakeMoved(gridPosition);
            }

            UpdateSnakeRotation();
        }

        transform.position = new Vector3(gridPosition.x,gridPosition.y, 0);
    }
}
