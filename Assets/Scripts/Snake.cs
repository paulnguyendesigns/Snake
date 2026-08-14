using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;

public class Snake : MonoBehaviour {
    private enum Direction {
        Up,
        Down,
        Left,
        Right,
    }

    private class SnakeMovePosition {
        public Vector2Int gridPosition;
        public Direction direction;

        public SnakeMovePosition(Vector2Int gridPosition, Direction direction) {
            this.gridPosition = gridPosition;
            this.direction = direction;
        }
    }

    private LevelGrid levelGrid;

    private Vector2Int gridMoveDirection;
    private Vector2Int gridPosition;

    private float gridMoveTimer;
    private float gridMoveTimerMax;

    private int snakeBodySize;
    private List<SnakeMovePosition> snakeMovePositionList;
    private List<Transform> snakeBodyTransformList;
    private bool isGameOver;
    private bool hasStarted;

    private void Awake() {
        gridPosition = new Vector2Int(10, 10);

        gridMoveTimerMax = 0.1f;
        gridMoveTimer = gridMoveTimerMax;

        gridMoveDirection = new Vector2Int(1, 0);

        snakeBodySize = 0;

        snakeMovePositionList = new List<SnakeMovePosition>();
        snakeBodyTransformList = new List<Transform>();

        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0); // Set scene position to match grid position
        UpdateSnakeRotation();
    }

    private Transform CreateSnakeBodyTransform() {
        GameObject snakeBodyGameObject = new GameObject("Snake Body", typeof(SpriteRenderer));
        snakeBodyGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.i.snakeBodySprite;
        return snakeBodyGameObject.transform;
    }

    private void Update() {
        if (isGameOver) {
            return;
        }

        HandleInput();

        if (hasStarted) {
            HandleGridMovement();
        }
    }

    public void SetLevelGrid(LevelGrid levelGrid) {
        this.levelGrid = levelGrid;
    }

    private void UpdateSnakeRotation() {
        float angle = Mathf.Atan2(gridMoveDirection.y, gridMoveDirection.x) * Mathf.Rad2Deg + 90f;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    private Direction GetDirection(Vector2Int vector) {
        if (vector.x > 0) return Direction.Right;
        if (vector.x < 0) return Direction.Left;
        if (vector.y > 0) return Direction.Up;
        return Direction.Down;
    }

    private void HandleInput() {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) {
            hasStarted = true;
        }

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

            Vector2Int nextHeadGridPosition = gridPosition + gridMoveDirection;

            // Check for death BEFORE mutating any state, so a fatal move never
            // touches the position history / body transforms in the first place.
            if (!levelGrid.ValidateGridPosition(nextHeadGridPosition) || IsSnakeBodyGridPosition(nextHeadGridPosition)) {
                HandleDeath();
                return;
            }

            // Store the snake head's current position and the direction it's
            // about to move in, before actually moving
            Direction currentMoveDirection = GetDirection(gridMoveDirection);
            snakeMovePositionList.Insert(0, new SnakeMovePosition(gridPosition, currentMoveDirection));

            // Move the snake head
            gridPosition = nextHeadGridPosition;

            bool snakeAteFood = levelGrid.TrySnakeEatFood(gridPosition);

            if (snakeAteFood) {
                // Grow: keep the extra history entry we just inserted instead of
                // trimming it away, and use it as the new tail segment's starting
                // position -- so the new body part appears immediately at the
                // correct trailing spot, with no extra move needed to reveal it.
                snakeBodySize++;

                Transform newBodyTransform = CreateSnakeBodyTransform();
                Vector2Int newBodyPosition = snakeMovePositionList[snakeMovePositionList.Count - 1].gridPosition;
                newBodyTransform.position = new Vector3(newBodyPosition.x, newBodyPosition.y, 0);
                snakeBodyTransformList.Add(newBodyTransform);
            } else {
                // Not growing this tick: trim the oldest history entry so the
                // list stays exactly as long as the current body size.
                if (snakeMovePositionList.Count > snakeBodySize) {
                    snakeMovePositionList.RemoveAt(snakeMovePositionList.Count - 1);
                }
            }

            // Update each body part's position and rotation
            for (int i = 0; i < snakeBodyTransformList.Count; i++) {
                UpdateSnakeBodyPart(i);
            }

            UpdateSnakeRotation();
        }

        transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    // Positions and rotates body segment 'index'. Straight segments (previous
    // direction == current direction, the 'default' angle) snap to that
    // direction's normal 90-degree-increment angle. Corner segments get a
    // 45-degree diagonal angle plus a small positional nudge toward the
    // inside of the turn, so the two connecting edges read as a seamless bend.
    //
    // 'currentDirection' is this segment's OWN direction (where it's heading,
    // toward the head). 'previousDirection' is the direction of the segment
    // BEHIND it (tail-ward, index+1 in the history list) -- i.e. how this
    // segment was arrived at. Comparing the two tells us whether this exact
    // segment is the corner (not the one before or after it).
    private void UpdateSnakeBodyPart(int index) {
        SnakeMovePosition snakeMovePosition = snakeMovePositionList[index];
        Vector2Int bodyGridPosition = snakeMovePosition.gridPosition;

        Direction currentDirection = snakeMovePosition.direction;
        Direction previousDirection = (index + 1 < snakeMovePositionList.Count)
            ? snakeMovePositionList[index + 1].direction
            : currentDirection; // tail segment: nothing behind it, so treat as straight

        Vector3 bodyPosition = new Vector3(bodyGridPosition.x, bodyGridPosition.y, 0f);
        float angle;

        switch (currentDirection) {
            default:
            case Direction.Up:
                angle = 180f;
                switch (previousDirection) {
                    case Direction.Left:
                        angle = 90f - 45f;
                        bodyPosition += new Vector3(+0.2f, +0.2f, 0f);
                        break;
                    case Direction.Right:
                        angle = 90f + 45f;
                        bodyPosition += new Vector3(-0.2f, +0.2f, 0f);
                        break;
                }
                break;
            case Direction.Down:
                angle = 0f;
                switch (previousDirection) {
                    case Direction.Left:
                        angle = 90f + 45f;
                        bodyPosition += new Vector3(+0.2f, -0.2f, 0f);
                        break;
                    case Direction.Right:
                        angle = 90f - 45f;
                        bodyPosition += new Vector3(-0.2f, -0.2f, 0f);
                        break;
                }
                break;
            case Direction.Left:
                angle = 270f;
                switch (previousDirection) {
                    case Direction.Up:
                        angle = 180f + 45f;
                        bodyPosition += new Vector3(-0.2f, -0.2f, 0f);
                        break;
                    case Direction.Down:
                        angle = 180f - 45f;
                        bodyPosition += new Vector3(-0.2f, +0.2f, 0f);
                        break;
                }
                break;
            case Direction.Right:
                angle = 90f;
                switch (previousDirection) {
                    case Direction.Up:
                        angle = 180f - 45f;
                        bodyPosition += new Vector3(+0.2f, -0.2f, 0f);
                        break;
                    case Direction.Down:
                        angle = 180f + 45f;
                        bodyPosition += new Vector3(+0.2f, +0.2f, 0f);
                        break;
                }
                break;
        }

        snakeBodyTransformList[index].position = bodyPosition;
        snakeBodyTransformList[index].eulerAngles = new Vector3(0, 0, angle);
    }

    // Checks whether a grid position overlaps the snake's own body.
    // The tail segment is excluded because it vacates its cell as part of
    // this same move (unless the snake is growing), so moving into the
    // current tail cell is a normal, non-fatal move.
    private bool IsSnakeBodyGridPosition(Vector2Int testGridPosition) {
        for (int i = 0; i < snakeBodySize - 1; i++) {
            if (snakeMovePositionList[i].gridPosition == testGridPosition) {
                return true;
            }
        }
        return false;
    }

    private void HandleDeath() {
        isGameOver = true;
        CMDebug.TextPopup("DEAD!", transform.position);
    }

    public List<Vector2Int> GetFullSnakeGridPositionList() {
        List<Vector2Int> fullSnakeGridPositionList = new List<Vector2Int>() { gridPosition };
        for (int i = 0; i < snakeBodySize; i++) {
            fullSnakeGridPositionList.Add(snakeMovePositionList[i].gridPosition);
        }
        return fullSnakeGridPositionList;
    }
}
