using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class WandRaycaster : MonoBehaviour
{

    public GameObject cylinder;
    public GameObject button;
    private AddButton ab;
    public SliderHandle slider;

    public float rayLength = 20f; // Length of the ray
    public LayerMask buttonLayer; // Layer mask for customButton objects

    public bool wandHit;
    LineRenderer laser;

    public bool laserButtonPressed;

    public int wandID = 1;
    public CAVE2.Button laserButton = CAVE2.Button.Button3;


    float laserDistance;
    Vector3 laserPosition;

    void Start()
    {
        laser = gameObject.AddComponent<LineRenderer>();
    }

    void Update()
    {
        laserButtonPressed = CAVE2.Input.GetButton(wandID, laserButton);

        if (Input.GetMouseButton(0))
        {
            cylinder.SetActive(true);

        }

        if (Input.GetMouseButtonUp(0))
        {
            cylinder.SetActive(false);

            if (button != null)
            {
                button.GetComponent<AddButton>().Push();
                button = null;
            }

            if (slider != null)
            {
                slider.isGrabbed = false;
                slider.handTransform = null;
                slider = null;
            }

        }

        if(CAVE2.Input.GetButton(wandID, laserButton))
        {
            cylinder.SetActive(true);
        }

        if (CAVE2.Input.GetButtonUp(wandID, laserButton))
        {
            cylinder.SetActive(false);

            if (button != null)
            {
                button.GetComponent<AddButton>().Push();
                button = null;
            }

            if (slider != null)
            {
                slider.isGrabbed = false;
                slider.handTransform = null;
                slider = null;
            }
        }

    }
}
