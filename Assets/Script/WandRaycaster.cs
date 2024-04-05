using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class WandRaycaster : MonoBehaviour
{
    public float rayLength = 5f; // Length of the ray

    public GameObject cylinder;
    public GameObject button;
    private AddButton ab;
    public SliderHandle slider;

    void Start()
    {

    }

    void Update()
    {

        if (Input.GetMouseButton(2))
        {
            cylinder.SetActive(true);
        }

        if (Input.GetMouseButtonUp(2))
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
