using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Reference to the player's transform
    public float smoothness = 0.5f; // Adjust this value for the desired smoothness
    public float height = 10.0f; // Fixed height of the camera above the player
    public Vector3 offset = new Vector3(0f, 0f, -5f); // Offset from the player in world space

    private void LateUpdate()
    {
        if (target != null)
        {
            // Calculate the desired position for the camera with a fixed Y position and offset
            Vector3 desiredPosition = target.position + offset;
            desiredPosition.y = height;

            // Use Vector3.Lerp to smoothly interpolate between the current position and the desired position
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothness * Time.deltaTime);
        }
    }
}