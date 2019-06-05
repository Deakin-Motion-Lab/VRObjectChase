/*
   This class:
   - controls the game;
   - activates a game timer countdown;
   - spawns 'Chaser' game objects at random intervals (until the game timer runs out)
   - keeps score of all 'Chaser' objects caught / missed
   - allows user to restart game (using 'TouchPad' on HTC Vive controller)
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class GameControl : MonoBehaviour
{
    public GameObject chaserObject;
    public GameObject controllerRight;
    public float minSpawnWaitTime;
    public float maxSpawnWaitTime;

    [Tooltip("Maximum number of objects that can be present in the scene at a given time")]
    public int spawnCount;
    [Tooltip("Game Time (in seconds)")]
    public float gameTime = 60f;
    public Text scoreText;
    //public Text totalText;
    public Text missedText;
    public Text timerText;
    public Text gameOverText;
    public Text restartText;
    private List<GameObject> _ChaserObjs = new List<GameObject>();
    private float _ResetTime;
    private int _Score;
    private int _Total;
    private int _Missed;
    private bool _GameOver;
    private SteamVR_Action_Boolean _TouchPadAction;

    // Initialise
    private void Awake()
    {
        _TouchPadAction = SteamVR_Actions.default_Teleport;
    }

    // Start is called before the first frame update
    void Start()
    {
        _ResetTime = gameTime;
        StartNewGame();
    }

    private void Update()
    {
        if (!_GameOver)
        {
            UpdateTime();
            CheckPathComplete();
            CheckControllerInteraction();
        }
        else
        {
            // Check if user has activated the 'Touch Pad' on the controller
            if (_TouchPadAction.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                RestartGame();
            }
        }
    }

    /// <summary>
    /// Initialises game variables and starts spawning game objects
    /// </summary>
    private void StartNewGame()
    {
        _Score = -1;
        _Total = 0;
        _Missed = -1;
        gameTime++;         // Allow 1 second delay while game scene loads  [ NOTE: TBC when Build is tested ]
        StartCoroutine(SpawnObject());
        _GameOver = false;
        gameOverText.enabled = false;
        restartText.enabled = false;
        UpdateMissed();
        UpdateScore();
        _ChaserObjs.Clear();
    }

    /// <summary>
    /// Updates the timer countdown on the UI
    /// </summary>
    private void UpdateTime()
    {
        const float WARNING_TIME = 11f;
        const float ZERO_TIME = 0f;
        const float SECONDS = 60f;
        int minutes = (int)Mathf.Floor(gameTime / SECONDS);
        int seconds = (int)Mathf.Floor(gameTime % SECONDS);

        // Update Game Time
        gameTime -= Time.deltaTime;

        // Stop game when timer reaches zero
        if (gameTime <= ZERO_TIME)
        {
            EndGame();
        }
        else if (gameTime <= WARNING_TIME)
        {
            // Change timer colour to RED to warn user
            timerText.color = Color.red;
        }

        // Update Time value on UI
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Ends the game, updates UI to adivse 'Game Over' and removes all 'Chaser' objects from the scene.
    /// </summary>
    private void EndGame()
    {
        _GameOver = true;
        DestroyAllSpawnedObjects();
        gameOverText.enabled = true;
        restartText.enabled = true;
    }

    /// <summary>
    /// Checks if game object completed its path and removes it from scene if true
    /// </summary>
    private void CheckPathComplete()
    {
        // Check if each spawned object has reached end of path
        for (int i = 0; i < _ChaserObjs.Count; i++)
        {
            if (_ChaserObjs[i].GetComponent<MoveOnPath>().IsPathComplete())
            {
                DestroySpawnedObject(i);
            }
        }
    }

    /// <summary>
    /// Checks if controller "catches" game object
    /// </summary>
    private void CheckControllerInteraction()
    {
        // TO DO: consider implementing a collider
        // ISSUE: need to add a collider mesh to the steam vr controller (one is automatically included in SteamVR Player prefab, but not in
        //        [CameraRig])

        float interactDistance = 0.3f;

        // Check if controller has interacted with one (or more) of the spawned objects
        for (int i = 0; i < _ChaserObjs.Count; i++)
        {
            float distance = Vector3.Distance(controllerRight.transform.position, _ChaserObjs[i].transform.position);
            if (distance <= interactDistance)
            {
                _ChaserObjs[i].GetComponent<MoveOnPath>().Caught();
                DestroySpawnedObject(i);
                UpdateScore();
                break;
            }
        }
    }

    /// <summary>
    /// Destroys all spawned objects and clears list (on game over)
    /// </summary>
    private void DestroyAllSpawnedObjects()
    {
        // Iterate through list and destroy each currently spawned 'Chaser' game object
        for (int i = 0; i < _ChaserObjs.Count; i++)
        {
            Destroy(_ChaserObjs[i]);
        }

        // Clear the list
        _ChaserObjs.Clear();
    }

    /// <summary>
    /// Destroys game object from scene and updates list
    /// </summary>
    /// <param name="index"></param>
    private void DestroySpawnedObject(int index)
    {
        Destroy(_ChaserObjs[index]);                // Remove from scene
        _ChaserObjs.Remove(_ChaserObjs[index]);     // Remove from list
        Debug.Log("Total List: " + _ChaserObjs.Count);
    }

    /// <summary>
    /// Increments score value and updates score text in game UI
    /// </summary>
    private void UpdateScore()
    {
        _Score++;
        scoreText.text = _Score.ToString();
    }

    /// <summary>
    /// Increments the total spawned objects value and updates total text in game UI
    /// </summary>
    private void UpdateTotal()
    {
        _Total++;
        // Not currently displaying total on scoreboard
        //totalText.text = _Total.ToString();
    }

    /// <summary>
    /// Increments the missed objects value and updates text in game UI
    /// </summary>
    private void UpdateMissed()
    {
        _Missed = _Total - _Score;
        missedText.text = _Missed.ToString();
    }

    private void RestartGame()
    {
        gameTime = _ResetTime;      // Reset Game Time
        StartNewGame();
    }


    /// <summary>
    /// Co-routine to spawn game objects at random intervals (up to maximum set by user)
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnObject()
    {
        // TO DO: Implement a 10? sec 'GET READY' time before starting game

        // Continue to randomly spawn objects until game is over
        while(!_GameOver)
        {
            // Check if maximum spawns in scene is not exceeded
            if (_ChaserObjs.Count < spawnCount)
            {
                // Spawn object and add to list
                _ChaserObjs.Add(Instantiate(chaserObject));
                UpdateTotal();
                UpdateMissed();
            }
            // Apply a random delay before iterating loop
            yield return new WaitForSeconds(Random.Range(minSpawnWaitTime, maxSpawnWaitTime));
        }
    }
}
