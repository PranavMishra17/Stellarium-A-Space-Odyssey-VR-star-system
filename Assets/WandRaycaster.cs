using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandRaycaster : MonoBehaviour
{
    public float rayLength = 5f; // Length of the ray
    private LineRenderer lineRenderer;
    public GameObject cylinder;

    public GameObject button;
    private AddButton ab;
    void Start()
    {
        // Try to get the LineRenderer attached to the wand, add one if none is found
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        SetupLineRenderer();
    }

    void Update()
    {

        if (Input.GetMouseButton(2))
        {
            lineRenderer.enabled = true;
            DrawRay();
            
            cylinder.SetActive(true);
        }

        if (Input.GetMouseButtonUp(2))
        {

            cylinder.SetActive(false);
            lineRenderer.enabled = false;

            if (button != null)
            {
                button.GetComponent<AddButton>().Push();
            }
        }
        else 
        { 
            
        }
    }

    void SetupLineRenderer()
    {
        // Set the color of the line
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;

        // Set the width of the line
        lineRenderer.startWidth = 0.0005f; // Adjust as needed
        lineRenderer.endWidth = 0.005f; // Adjust as needed

        // Use World space coordinates
        lineRenderer.useWorldSpace = true;
    }

    void DrawRay()
    {
        Vector3 start = transform.position; // Start at the position of the wand
        Vector3 end = start + transform.forward * rayLength; // End a few units in the direction the wand is pointing

        // Set positions
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
