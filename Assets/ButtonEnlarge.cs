using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;

public class ButtonEnlarge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public float scaleFactor = 2f; // Factor by which to scale the button when hovered
    public float transitionDuration = 0.5f; // Duration of the transition animation in seconds

    private Vector3 originalScale;
    private Coroutine currentCoroutine;

    private void Start()
    {
        button = GetComponent<Button>();
        originalScale = button.transform.localScale;
    }

    // Called when the mouse pointer enters the Button
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ScaleButton(button, originalScale * scaleFactor, transitionDuration));
    }

    // Called when the mouse pointer exits the Button
    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(ScaleButton(button, originalScale, transitionDuration));
    }

    // Coroutine to smoothly scale the button over time
    private IEnumerator ScaleButton(Button button, Vector3 targetScale, float duration)
    {
        float time = 0f;
        Vector3 startScale = button.transform.localScale;

        while (time < duration)
        {
            float t = time / duration;
            button.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        button.transform.localScale = targetScale;
    }
}
