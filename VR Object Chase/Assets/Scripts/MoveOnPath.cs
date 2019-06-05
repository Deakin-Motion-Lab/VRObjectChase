using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveOnPath : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;

    [Tooltip("Closest distance to waypoint before next waypoint can be selected (used for motion smoothing)")]
    public float reachDistance;             
    private PathCreator _PathToFollow;
    private int _CurrentWayPointID;         // Index for list
    private float _RandomSpeed;             // Random value selected within the minSpeed and maxSpeed range set by user
    //public float rotationSpeed = 5.0f;
    private string _PathName;
    private bool _GoForward;                // Used when 'reverse' path travel functionality is required
    private bool _Stop;
    private bool _PathComplete;

    // Start is called before the first frame update
    void Start()
    {
        StartNewPath();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_Stop)
        {
            FollowPath();
            //FollowPathWithReverse();
        }
    }

    /// <summary>
    /// Moves the game object along the path, from waypoint to waypoint, reversing direction when the end of the path is reached
    /// </summary>
    private void FollowPathWithReverse()
    {
        float distance = Vector3.Distance(_PathToFollow.pathObjects[_CurrentWayPointID].position, transform.position);
        transform.position = Vector3.MoveTowards(transform.position, _PathToFollow.pathObjects[_CurrentWayPointID].position, _RandomSpeed * Time.deltaTime);

        if (distance <= reachDistance)
        {
            if (_GoForward)
            {
                _CurrentWayPointID++;
            }
            else
            {
                _CurrentWayPointID--;
            }
        }

        // If at end of path, reverse direction
        if (_GoForward && _CurrentWayPointID >= _PathToFollow.pathObjects.Count)
        {
            _GoForward = false;
            _CurrentWayPointID--;
        }

        if (!_GoForward && _CurrentWayPointID < 0)
        {
            _GoForward = true;
            _CurrentWayPointID = 0;
        }
    }

    /// <summary>
    /// Moves the game object along the path, from waypoint to waypoint, until the end of the path is reached
    /// </summary>
    private void FollowPath()
    {
        // Calculate distance between desired waypoint and current position
        float distance = Vector3.Distance(_PathToFollow.pathObjects[_CurrentWayPointID].position, transform.position);

        // Move object required distance
        transform.position = Vector3.MoveTowards(transform.position, _PathToFollow.pathObjects[_CurrentWayPointID].position, _RandomSpeed * Time.deltaTime);
        
        if (distance <= reachDistance)
        {
            // Select next waypoint
            _CurrentWayPointID++;
        }

        // If reached end of path, signal complete
        if (_CurrentWayPointID >= _PathToFollow.pathObjects.Count)
        {
            //float timer = Random.Range(1f, 4f);
            _Stop = true;
            _PathComplete = true;

            //Invoke("StartNewPath", timer);
            // Destroy object
        }
    }

    public bool IsPathComplete()
    {
        return _PathComplete;
    }

    /// <summary>
    /// Changes the game object colour to show it has been "caught" by the user and stops the game object's movement
    /// </summary>
    public void Caught()
    {
        GetComponent<Renderer>().material.color = Color.red;
        _Stop = true;
    }

    /// <summary>
    /// Selects a random path and initiliases the game object state
    /// </summary>
    private void StartNewPath()
    {
        // Get random path
        GameObject objectPath = GetComponent<RandomPath>().GetRandomPath();
        _PathToFollow = objectPath.GetComponent<PathCreator>();

        // Initialisation
        _CurrentWayPointID = 0;
        transform.position = _PathToFollow.pathObjects[_CurrentWayPointID].position;
        _GoForward = true;
        _Stop = false;
        _PathComplete = false;
        GetComponent<Renderer>().material.color = Color.yellow;
        _RandomSpeed = Random.Range(minSpeed, maxSpeed);
    }
}
