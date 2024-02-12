using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelAnim : MonoBehaviour
{
    public TextMeshProUGUI fadeInText; // Assign your text to fade in
    public TextMeshProUGUI continueText; // Assign your "Press any button to continue" text
    public CanvasGroup panelCanvasGroup; // Assign your panel's CanvasGroup
    public float fadeInDuration = 2f; // Duration for text to fully appear
    public float continueDelay = 2f; // Delay before showing "Press any button to continue"
    public float fadeOutDuration = 2f; // Duration for panel and text to fade out

    private bool canContinue = false; // Flag to control the continuation

    public FPSController fps;

    public AudioSource asss;
    public AudioClip entrysf;

    void Start()
    {
        StartCoroutine(AnimateText());
        asss = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Wait for any key press to start fade out if canContinue is true
        if (canContinue && Input.anyKeyDown)
        {
            StartCoroutine(FadeOutUI());
            canContinue = false; // Prevent this block from running again
            fps.isFreeToMove = true;
            asss.PlayOneShot(entrysf);
        }
    }

    IEnumerator AnimateText()
    {
        // Set initial alpha to 0 for both texts
        fadeInText.color = new Color(fadeInText.color.r, fadeInText.color.g, fadeInText.color.b, 0);
        continueText.color = new Color(continueText.color.r, continueText.color.g, continueText.color.b, 0);

        // Fade in the text
        while (fadeInText.color.a < 1)
        {
            fadeInText.color += new Color(0, 0, 0, Time.deltaTime / fadeInDuration);
            yield return null;
        }

        // Wait for a few seconds
        yield return new WaitForSeconds(continueDelay);

        // Show "Press any button to continue"
        while (continueText.color.a < 1)
        {
            continueText.color += new Color(0, 0, 0, Time.deltaTime / fadeInDuration);
            yield return null;
        }

        // Enable continuation
        canContinue = true;
    }

    IEnumerator FadeOutUI()
    {
        // Expand text out of screen

        float startTime = Time.time;
        Vector3 startScale = fadeInText.transform.localScale;
        Vector3 endScale = startScale * 2; // Adjust the multiplier to control the expand effect

        while (Time.time < startTime + fadeOutDuration)
        {
            fadeInText.transform.localScale = Vector3.Lerp(startScale, endScale, (Time.time - startTime) / fadeOutDuration);
            yield return null;
        }

        // Fade out the panel
        while (panelCanvasGroup.alpha > 0)
        {
            panelCanvasGroup.alpha -= Time.deltaTime / fadeOutDuration;
            yield return null;
        }

        // Deactivate the panel when done
        panelCanvasGroup.gameObject.SetActive(false);
    }
}
