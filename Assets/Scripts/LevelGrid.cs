using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;

public class LevelGrid {
    private Vector2Int foodGridPosition;
    private GameObject foodGameObject;
    private int width;
    private int height;
    private Snake snake;

    public LevelGrid(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public void SetSnake(Snake snake) {
        this.snake = snake;
    }

    public void SpawnFood() {
        List<Vector2Int> occupiedGridPositionList = snake != null
            ? snake.GetFullSnakeGridPositionList()
            : new List<Vector2Int>();

        Vector2Int candidateGridPosition;
        do {
            candidateGridPosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        } while (occupiedGridPositionList.Contains(candidateGridPosition)); // Avoid spawning on the snake's head or body

        foodGridPosition = candidateGridPosition;

        foodGameObject = new GameObject("Food", typeof(SpriteRenderer));
        foodGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.i.foodSprite;
        foodGameObject.transform.position = new Vector3(foodGridPosition.x, foodGridPosition.y);
    }

    public bool TrySnakeEatFood(Vector2Int snakeGridPosition) {
        if (snakeGridPosition == foodGridPosition) {
            Debug.Log("Snake ate the food");
            Object.Destroy(foodGameObject);
            SpawnFood();
            return true;
        }
        else {
            return false;
        }
    }
}
