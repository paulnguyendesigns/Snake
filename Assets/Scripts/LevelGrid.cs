using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;

public class LevelGrid {
    private Vector2Int foodGridPosition;
    private GameObject foodGameObject;
    private int width;
    private int height;
    public LevelGrid(int width, int height) {
        this.width = width;
        this.height = height;

        SpawnFood();
    }
    private void SpawnFood() {
        foodGridPosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        foodGameObject = new GameObject("Food", typeof(SpriteRenderer));
        foodGameObject.GetComponent<SpriteRenderer>().sprite = GameAssets.i.foodSprite;
        foodGameObject.transform.position = new Vector3(foodGridPosition.x, foodGridPosition.y);
    }

    public void SnakeMoved(Vector2Int snakeGridPosition) {
        if (snakeGridPosition == foodGridPosition) {
            Debug.Log("Snake ate the food");
            Object.Destroy(foodGameObject);
            SpawnFood();
        }
    }
}
