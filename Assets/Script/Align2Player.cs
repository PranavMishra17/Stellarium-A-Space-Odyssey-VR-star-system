using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Align2Player : MonoBehaviour
{
    public Transform playerTransform; // Assign the player's Transform component in the Inspector

    private Vector3 initialPositionOffset; // Initial offset from the player to the panel
    private Quaternion initialRotationOffset; // Initial rotation offset from the player to the panel

    void Start()
    {
        // Calculate and store the initial offset and rotation at the start
        initialPositionOffset = transform.position - playerTransform.position;
        initialRotationOffset = Quaternion.Inverse(playerTransform.rotation) * transform.rotation;
    }

    void Update()
    {
        // Update the panel's position to maintain the initial offset
        transform.position = playerTransform.position + playerTransform.rotation * initialPositionOffset;

        // Update the panel's rotation to maintain the initial relative rotation
        transform.rotation = playerTransform.rotation * initialRotationOffset;
    }
}
