using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour {
    private LevelGrid levelGrid;
    private Vector2Int gridMoveDirection;
    private Vector2Int gridPosition;
    private float gridMoveTimer;
    private float gridMoveTimerMax;

    private void Awake() {
        gridPosition = new Vector2Int(10,10);
        gridMoveTimerMax = 0.2f;
        gridMoveTimer = gridMoveTimerMax;
        gridMoveDirection = new Vector2Int(1,0);

        UpdateSnakeRotation();
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
            gridPosition += gridMoveDirection;
            gridMoveTimer -= gridMoveTimerMax;

            levelGrid?.SnakeMoved(gridPosition);

            UpdateSnakeRotation();
        }
        transform.position = new Vector3(gridPosition.x, gridPosition.y);
    }

    public Vector2Int GetGridPosition() {
        return gridPosition;
    }
}
