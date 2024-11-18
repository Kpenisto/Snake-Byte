/*
Author: Kyle Peniston
Date: 11/18/2024
Description: The AppleSpawner script is responsible for spawning apples at valid positions on the grid.
Apples do not overlap with the snake or spawn too close to the snake's head.
A new apple is spawned whenever the previous one is consumed.
*/

using UnityEngine;

public class AppleSpawner : MonoBehaviour
{
    //Game Objects
    public GameObject applePrefab;
    private GameObject currentApple;
    public GameObject CurrentApple => currentApple;
    public SnakeController snake;

    //Grid
    public Vector3 gridSize = new Vector3(20, 0, 20);

    void Start()
    {
        if (snake == null)
        {
            Debug.Log("SnakeController reference is not assigned in AppleSpawner.");
            return;
        }

        //Spawn the first apple at the start of the game
        SpawnApple();
    }

    public void SpawnApple()
    {
        //Destroy current apple if one exists
        if (currentApple != null)
        {
            Destroy(currentApple);
        }

        Vector3 spawnPosition;
        do
        {
            //Generate a random position within the grid
            int x = Random.Range(0, (int)gridSize.x); //Random x-coordinate within the grid
            int z = Random.Range(0, (int)gridSize.z); //Random y-coordinate within the grid
            spawnPosition = new Vector3(x, 1, z); //Y is fixed at 1 for grid alignment
        } while (!IsValidPosition(spawnPosition));

        //Instantiate a new apple at the chosen random position
        currentApple = Instantiate(applePrefab, spawnPosition, Quaternion.identity);
    }

    private bool IsValidPosition(Vector3 position)
    {
        //Ensure the apple doesn't spawn on any part of the snake
        foreach (Transform segment in snake.snake)
        {
            if (segment.position == position) return false;
        }

        //Ensure the apple spawns at least 5 units away from the snake's head
        if (snake.snake.Count > 0)
        {
            Vector3 snakeHeadPosition = snake.snake[0].position;
            if (Vector3.Distance(position, snakeHeadPosition) < 5f) return false;
        }

        //Position is now valid, return true
        return true;
    }
}