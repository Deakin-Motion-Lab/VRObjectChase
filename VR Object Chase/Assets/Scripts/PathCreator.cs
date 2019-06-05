/*
   This class handles creation of waypoint paths for Chaser object to follow, from spawn to destruction
*/

using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    public Color rayColour = Color.white;       // Set default colour
    public float sphereRadius = 0.3f;           // Set default radius
    public List<Transform> pathObjects = new List<Transform>();
    private Transform[] _TransformArray;

    // Visually represent path in Editor "Scene" using Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = rayColour;
        _TransformArray = GetComponentsInChildren<Transform>();
        
        // Clear "previous" path
        pathObjects.Clear();

        // Update "new" path
        foreach (Transform pathObjTransform in _TransformArray)
        {
            if (pathObjTransform != transform)
            {
                pathObjects.Add(pathObjTransform);
            }
        }

        // Traverse list and draw "current" path
        for (int i = 0; i < pathObjects.Count; i++)
        {
            Vector3 currentPosition = pathObjects[i].position;

            if (i > 0)
            {
                Vector3 previousPosition = pathObjects[i - 1].position;
                Gizmos.DrawLine(previousPosition, currentPosition);
                Gizmos.DrawWireSphere(currentPosition, sphereRadius);
            }
        }
    }
}
