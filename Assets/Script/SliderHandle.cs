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
            Vector3 handPosition = handTransform.position - grabOffset * movementSensitivity;
            Vector3 closestPointOnTrack = GetClosestPointOnTrack(handPosition);
            transform.position = new Vector3(closestPointOnTrack.x, transform.position.y, transform.position.z);

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

    private Vector3 GetClosestPointOnTrack(Vector3 handPosition)
    {
        // Project the hand's position onto the track defined by start and end points
        Vector3 trackDirection = (trackEnd.position - trackStart.position).normalized;
        float maxDistance = Vector3.Distance(trackStart.position, trackEnd.position);
        Vector3 startToHand = handPosition - trackStart.position;
        float projectionDistance = Mathf.Clamp(Vector3.Dot(startToHand, trackDirection), 0, maxDistance);
        Vector3 closestPoint = trackStart.position + trackDirection * projectionDistance;
        return closestPoint;
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
