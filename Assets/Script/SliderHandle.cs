using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderHandle : MonoBehaviour
{
    public Transform trackStart, trackEnd; // Assign in the inspector
    public float currentValue = 0f; // Current slider value
    public bool isGrabbed = false;

    public Transform handTransform; // To keep track of the hand's (or wand's) transform
    private Vector3 grabOffset; // Offset between handle and hand at the moment of grabbing

    public TextMesh sliderValue;

    [SerializeField]
    private float movementSensitivity = 1.0f; // Control the sensitivity of slider movement

    public float sliderScale = 100f;

    void Start()
    {
        // Set the initial slider value here
        SetSliderValue(10.01f);
    }

    void Update()
    {
        if (isGrabbed && handTransform != null)
        {
            Vector3 handPosition = handTransform.position + grabOffset;
            Vector3 closestPointOnTrack = GetClosestPointOnLine(trackStart.position, trackEnd.position, handPosition);
            transform.position = closestPointOnTrack;

            // Calculate slider value based on handle position
            UpdateSliderValue();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ray")) // Make sure your hand (or wand) GameObject has the "Ray" tag
        {
            isGrabbed = true;
            handTransform = other.transform; // Keep a reference to the hand's transform
            grabOffset = handTransform.position - transform.position; // Calculate the offset at the moment of grabbing

            handTransform.gameObject.GetComponentInParent<WandRaycaster>().slider = this;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ray"))
        {
            // isGrabbed = false;
            // handTransform = null;
        }
    }

    // This function projects a point onto the line defined by two points (lineStart and lineEnd)
    // and returns the closest point on the line to the given point
    private Vector3 GetClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = (lineEnd - lineStart).normalized;
        float projectionLength = Vector3.Dot(point - lineStart, lineDirection);
        projectionLength = Mathf.Clamp(projectionLength, 0f, Vector3.Distance(lineStart, lineEnd));
        return lineStart + lineDirection * projectionLength;
    }

    private void UpdateSliderValue()
    {
        float totalDistance = Vector3.Distance(trackStart.position, trackEnd.position);
        float currentDistance = Vector3.Distance(trackStart.position, transform.position);
        currentValue = (currentDistance / totalDistance) * sliderScale; // Assuming a 0-100 range for the slider
        sliderValue.text = $"{currentValue:F2}";
    }

    private void SetSliderValue(float value)
    {
        float totalDistance = Vector3.Distance(trackStart.position, trackEnd.position);
        float distanceForValue = (value / sliderScale) * totalDistance; // Calculate the distance along the track for the given value

        Vector3 trackDirection = (trackEnd.position - trackStart.position).normalized;
        Vector3 newPosition = trackStart.position + trackDirection * distanceForValue;
        transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);

        // Update the slider value display
        UpdateSliderValue();
    }
}
