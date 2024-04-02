using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacePartitioner : MonoBehaviour
{
    public LayerMask starLayer; // Ensure this matches the layer your stars are on
    public float radius; // The radius of the cylinder
    public float height; // The height of the cylinder

    private List<GameObject> starsInside = new List<GameObject>();
    private List<GameObject> starsOutside = new List<GameObject>();
    private List<GameObject> starsOutsideF = new List<GameObject>();
    private List<GameObject> starsOutsideB = new List<GameObject>();

    private void Start()
    {
        // When the scene loads, categorize stars into inside and outside the cylinder
        //CategorizeStars();
    }

    public void CategorizeStars()
    {
        // Reset lists
        starsInside.Clear();
        starsOutside.Clear();

        // Define the top and bottom points of the cylinder
        Vector3 topPoint = new Vector3(0, height / 2, 0);
        Vector3 bottomPoint = new Vector3(0, -height / 2, 0);

        // Find all stars
        GameObject[] allStars = GameObject.FindGameObjectsWithTag("Star");

        // Check each star and categorize it
        foreach (var star in allStars)
        {
            // Calculate star's position relative to cylinder's center
            Vector3 starPosition = star.transform.position;
            bool isInCylinder = (starPosition.y <= topPoint.y && starPosition.y >= bottomPoint.y &&
                                 Vector3.Distance(new Vector3(starPosition.x, 0, starPosition.z),
                                 new Vector3(0, 0, 0)) <= radius);

            if (isInCylinder)
            {
                starsInside.Add(star);
            }
            else
            {
                if(star.transform.position.z > 0)
                {
                    starsOutsideF.Add(star);
                }
                else
                {
                    starsOutsideB.Add(star);
                }
                starsOutside.Add(star);
            }
        }

        // Debug information
        //Debug.Log("Stars inside cylinder: " + starsInside.Count);
        //Debug.Log("Stars outside cylinder: " + starsOutside.Count);
        //Debug.Log("Stars outside cylinderF: " + starsOutsideF.Count);
        //Debug.Log("Stars outside cylinderB: " + starsOutsideB.Count);

        // Optional: Do something immediately after categorization, like deactivate all stars outside
        SetActiveStarsOutside(false);
    }

    public void SetActiveStarsInside(bool isActive)
    {
        foreach (var star in starsInside)
        {
            star.SetActive(isActive);
        }
    }

    public void SetActiveStarsOutside(bool isActive)
    {
        foreach (var star in starsOutside)
        {
            star.SetActive(isActive);
        }
    }

    public void SetActiveStarsOutsideF(bool isActive)
    {
        foreach (var star in starsOutsideF)
        {
            star.SetActive(isActive);
        }
    }

    public void SetActiveStarsOutsideB(bool isActive)
    {
        foreach (var star in starsOutsideB)
        {
            star.SetActive(isActive);
        }
    }

}
