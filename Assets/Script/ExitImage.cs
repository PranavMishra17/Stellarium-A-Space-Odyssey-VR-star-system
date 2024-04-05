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

    public Renderer[] renderers; 

    public bool exit = false;
    void Start()
    {
        //rectTransform = image.GetComponent<RectTransform>();

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

    public float startDelay = 2f; // Delay before starting the fade
    public float fadeDuration = 2f; // Duration of the fade


    public void ExitChild()
    {
        StartCoroutine(FadeChildren());
    }

    IEnumerator FadeChildren()
    {
        yield return new WaitForSeconds(startDelay);

        // Get all renderers in children
        //Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Store the initial alpha value of each material
        float[] initialAlphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            initialAlphas[i] = materials[0].color.a; // Assuming all materials in the renderer have the same alpha value
        }

        // Gradually change the alpha value of each material to 0
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = renderers[i].materials;
                foreach (Material material in materials)
                {
                    Color color = material.color;
                    color.a = Mathf.Lerp(initialAlphas[i], 0f, t);
                    material.color = color;
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the alpha value is exactly 0 when fading is complete
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].materials;
            foreach (Material material in materials)
            {
                Color color = material.color;
                color.a = 0f;
                material.color = color;
            }
        }
    }

}
