/*
Author: Kyle Peniston
Date: 11/18/2024
Description: The SnakeController script manages the snake's movement, growth, collision detection, and game state.
Movement: Moves the snake in the grid and allows directional changes.
Collision: Detects collisions with the grid boundaries, itself, or apples.
Growth: Adds body segments when the snake eats an apple.
Game State: Handles game-over logic and displays UI elements like score and restart button.
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SnakeController : MonoBehaviour
{
    //Game Objects
    public AppleSpawner appleSpawner;
    public GameObject bodyPrefab;
    public Material headMaterial;
    public Material bodyMaterial;

    //Movement settings
    public float moveDelay = 0.2f;
    private Vector3 direction = Vector3.forward; //Start moving along the z-axis
    private float timer = 0f;
    private bool canTurn = true;

    //Snake segments
    public List<Transform> snake = new List<Transform>();

    //UI Elements
    public Text countText;
    public Text gameOverText;
    public Button restartButton;
    private int count = 0;
    private bool isGameOver = false;

    void Start()
    {
        //Initialize UI
        countText.text = "Count: " + count.ToString();
        gameOverText.text = "";
        restartButton.gameObject.SetActive(false);

        //Add the current GameObject (SnakeHead) to the snake list
        snake.Add(transform);

        //Assign the head material to the SnakeHead object
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && headMaterial != null)
        {
            renderer.material = headMaterial;
        }

        //Add initial body segments
        Vector3 startingPosition = transform.position;
        for (int i = 1; i < 5; i++) //Start from 1 as the head is already added
        {
            Vector3 segmentPosition = startingPosition + new Vector3(0, 0, -i); //Z decreases for tail
            GameObject segment = Instantiate(bodyPrefab, segmentPosition, Quaternion.identity);

            //Assign the body material to each segment
            Renderer segmentRenderer = segment.GetComponent<Renderer>();
            if (segmentRenderer != null && bodyMaterial != null)
            {
                segmentRenderer.material = bodyMaterial;
            }

            //Add to the snake list
            snake.Add(segment.transform);
        }
    }

    void Update()
    {
        if (isGameOver) return;

        //Update movement timer
        timer += Time.deltaTime;
        if (timer >= moveDelay)
        {
            Move();
            timer = 0f;
            canTurn = true;
        }

        //Handle user input
        HandleInput();
    }

    void Move()
    {
        if (isGameOver) return;

        //Calculate the next position for the snake's head
        Vector3 nextPosition = snake[0].position + direction;

        //Handle collisions
        if (IsCollision(nextPosition))
        {
            GameOver();
            return;
        }

        //Handle apple collision
        if (appleSpawner != null && appleSpawner.CurrentApple != null)
        {
            Vector3 applePosition = appleSpawner.CurrentApple.transform.position;

            //Check if the head reaches the apple
            if (Vector3.Distance(nextPosition, applePosition) < 0.1f)
            {
                EatApple();
            }
        }

        //Move the body segments
        for (int i = snake.Count - 1; i > 0; i--)
        {
            snake[i].position = snake[i - 1].position;
        }

        //Move the snake head
        snake[0].position = nextPosition;
    }

    void HandleInput()
    {
        //Prevent multiple turns in a single step
        if (!canTurn) return;

        //Change direction based on arrow key input
        if (Input.GetKeyDown(KeyCode.UpArrow) && direction != Vector3.back)
        {
            //Move along z-axis (positive)
            direction = Vector3.forward;
            canTurn = false;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Vector3.forward)
        {
            //Move along z-axis (negative)
            direction = Vector3.back;
            canTurn = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Vector3.right)
        {
            //Move along x-axis (negative)
            direction = Vector3.left; 
            canTurn = false;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Vector3.left)
        {
            //Move along x-axis (positive)
            direction = Vector3.right;
            canTurn = false;
        }
    }

    void EatApple()
    {
        //Add 3 segments to the snake
        AddBodyParts(3);

        //Destroy the consumed apple
        if (appleSpawner.CurrentApple != null)
        {
            Destroy(appleSpawner.CurrentApple);
        }

        //Spawn a new apple
        appleSpawner.SpawnApple();

        //Update apple count
        count++;
        countText.text = "Count: " + count.ToString();

        //Bite Apple Audio
        AudioSource[] sounds = GetComponents<AudioSource>();
        sounds[1].Play();
    }

    public void AddBodyParts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            //Add new segments at the tail's last position
            Transform tail = snake[snake.Count - 1];
            Vector3 newSegmentPosition = tail.position;
            GameObject segment = Instantiate(bodyPrefab, newSegmentPosition, Quaternion.identity);
            snake.Add(segment.transform);
        }
    }

    bool IsCollision(Vector3 nextPosition)
    {
        //Grid boundaries
        int gridMinX = 0; //Minimum x-coordinate
        int gridMaxX = 19; //Maximum x-coordinate (grid size - 1)
        int gridMinZ = 0; //Minimum z-coordinate
        int gridMaxZ = 19; //Maximum z-coordinate (grid size - 1)

        //Check if the next position is outside the grid boundaries
        if (nextPosition.x < gridMinX || nextPosition.x > gridMaxX ||
            nextPosition.z < gridMinZ || nextPosition.z > gridMaxZ)
        {
            return true;
        }

        //Snake remains on the correct y-plane (y = 1)
        if (nextPosition.y != 1)
        {
            return true;
        }

        //Check if the next position overlaps the snake's body
        foreach (Transform segment in snake)
        {
            if (segment.position == nextPosition) return true;
        }

        return false;
    }

    void GameOver()
    {
        //Zap Audio
        AudioSource[] sounds = GetComponents<AudioSource>();
        sounds[0].Play();

        isGameOver = true;
        gameOverText.text = "GAME OVER!";
        restartButton.gameObject.SetActive(true);
    }

    public void OnRestartButtonPress()
    {
        //Restart scene
        SceneManager.LoadScene("SampleScene");
    }
}