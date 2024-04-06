using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    public GameObject panel; // Reference to the panel whose state controls the fading

    private Renderer[] renderers; // Array to hold all renderers of children gameobjects
    private float fadeDuration = 5f; // Duration of the fade effect in seconds

    void Start()
    {
        // Get all renderers of children gameobjects
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        // Check if the panel is disabled
        if (panel != null && !panel.activeSelf)
        {
            // If the panel is disabled, set alpha to zero for all renderers
            SetAlpha(0f);
        }
        else if (panel != null && panel.activeSelf)
        {
            // If the panel is enabled, start the fade-in coroutine
            StartCoroutine(FadeInAlpha());
        }
    }

    IEnumerator FadeInAlpha()
    {
        float elapsedTime = 0f;
        float alpha = 0f;

        // Gradually increase alpha from 0 to 1 over the specified duration
        while (elapsedTime < fadeDuration)
        {
            alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            SetAlpha(alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure alpha is set to 1 at the end of the coroutine
        SetAlpha(1f);
    }

    void SetAlpha(float alpha)
    {
        // Set alpha for all renderers
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                Color color = material.color;
                color.a = alpha;
                material.color = color;
            }
        }
    }
}
