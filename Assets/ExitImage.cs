using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitImage : MonoBehaviour
{
    public RawImage image; // Reference to the Image UI element
    public float enlargeSpeed = 0.02f; // Speed of enlargement
    public float translationSpeed = 10f; // Speed of downward translation
    public float duration = 20f; // Duration before disabling the GameObject

    private RectTransform rectTransform; // Reference to the RectTransform component of the image
    private float elapsedTime = 0f; // Elapsed time since the exit started
    private Animator animator;
    public AnimationClip exitAnim;

    public bool exit = false;
    void Start()
    {
        rectTransform = image.GetComponent<RectTransform>();

        animator = GetComponent<Animator>(); // Get the Animation component attached to this GameObject
        if (animator != null)
        {
            animator.enabled = false; // Disable the animator by default
        }
    }

    void Update()
    {
        if(exit)
        {
            // Update the elapsed time
            elapsedTime += Time.deltaTime;

            // Check if the duration has elapsed
            if (elapsedTime >= duration)
            {
                // Disable the GameObject
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

    }

    public void ExitBool()
    {
        exit = true;
        if (animator != null)
        {
            animator.enabled = true; // Enable the animator
            animator.Play(exitAnim.name); // Play the exit animation
        }
    }
}
