using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStars : MonoBehaviour
{
    public SpacePartitioner spacePart;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider entering the trigger zone has the tag "Player"
        if (other.CompareTag("MainCamera"))
        {
            spacePart.SetActiveStarsInside(true);
            spacePart.SetActiveStarsOutside(false);
            // Call your desired function or execute your code here
            Debug.Log("Player entered the trigger zone!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the collider exiting the trigger zone has the tag "Player"
        if (other.CompareTag("MainCamera"))
        {
            spacePart.SetActiveStarsInside(false);
            spacePart.SetActiveStarsOutside(true);
            // Call your desired function or execute your code here
            Debug.Log("Player exited the trigger zone!");
        }
    }
}
