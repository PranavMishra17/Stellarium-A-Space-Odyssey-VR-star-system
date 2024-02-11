using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnStar : MonoBehaviour
{
    public float checkRadius = 50f; // The radius within which stars will be affected

    // Optional: Adjust these to optimize performance based on your game's specific needs
    public float checkInterval = 0.25f; // How often to perform the check (in seconds)
    private float checkTimer = 0f;

    void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f; // Reset timer

            // Use Physics.OverlapSphere to find stars within the specified radius
            Collider[] starColliders = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("StarLayer"));

            foreach (Collider starCollider in starColliders)
            {
                GameObject star = starCollider.gameObject;
                star.transform.LookAt(2 * star.transform.position - transform.position);

            }
        }
    }
}
