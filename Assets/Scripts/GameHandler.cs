using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour {
    private LevelGrid levelGrid;

    private void Start() {
        Debug.Log("GameHandler.Start");

        levelGrid = new LevelGrid(20, 20);

        Snake snake = FindAnyObjectByType<Snake>();
        if (snake != null) {
            snake.SetLevelGrid(levelGrid);
        }
    }

    void Update() {
        
    } 
}
