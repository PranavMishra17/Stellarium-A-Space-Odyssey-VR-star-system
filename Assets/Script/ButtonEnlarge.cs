using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;
using TMPro;

public class ButtonEnlarge : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public GameObject text;
    public float scaleFactor = 2f;
    public float transitionDuration = 0.5f;
    public Vector3 textShiftAmount;
    public bool shouldShiftSprite;

    private Vector3 originalButtonScale;
    private Vector3 originalTextPosition;
    private Coroutine currentButtonCoroutine;
    private Coroutine currentTextCoroutine;

    private TextMeshProUGUI buttonText;

    public Sprite[] sprites;

    private void Start()
    {
        button = GetComponent<Button>();
        //buttonText = text.GetComponent<TextMeshProUGUI>();
        originalButtonScale = button.transform.localScale;
        //originalTextPosition = buttonText.transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentButtonCoroutine != null)
        {
            StopCoroutine(currentButtonCoroutine);
        }
        currentButtonCoroutine = StartCoroutine(ScaleButton(button, originalButtonScale * scaleFactor, transitionDuration));
        text.SetActive(true);

        if (shouldShiftSprite)
        {
            /*
            if (currentTextCoroutine != null)
            {
                StopCoroutine(currentTextCoroutine);
            }
            currentTextCoroutine = StartCoroutine(ShiftText(text, originalTextPosition + textShiftAmount, transitionDuration));
            */
            button.image.sprite = sprites[0];
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentButtonCoroutine != null)
        {
            StopCoroutine(currentButtonCoroutine);
        }
        currentButtonCoroutine = StartCoroutine(ScaleButton(button, originalButtonScale, transitionDuration));
        text.SetActive(false);

        if (shouldShiftSprite)
        {
            /*
            if (currentTextCoroutine != null)
            {
                StopCoroutine(currentTextCoroutine);
            }
            currentTextCoroutine = StartCoroutine(ShiftText(text, originalTextPosition, transitionDuration));
            */
            button.image.sprite = sprites[1];
        }
    }

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

    private IEnumerator ShiftText(GameObject text, Vector3 targetPosition, float duration)
    {
        float time = 0f;
        Vector3 startPosition = text.transform.localPosition;

        while (time < duration)
        {
            float t = time / duration;
            text.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            time += Time.deltaTime;
            yield return null;
        }

        text.transform.localPosition = targetPosition;
    }

    // Function to initiate the fade out and destroy process
    public void FadeOutAndDestroy()
    {
        if (currentButtonCoroutine != null)
        {
            StopCoroutine(currentButtonCoroutine);
        }
        currentButtonCoroutine = StartCoroutine(FadeOutAndDestroyCoroutine(button, transitionDuration, text));
        button.interactable = false;
    }

    // Coroutine to fade out the button and then destroy it
    private IEnumerator FadeOutAndDestroyCoroutine(Button button, float duration, GameObject text)
    {
        // Get the components that need to fade out
        Image buttonImage = button.GetComponent<Image>();
        TextMeshProUGUI buttonText = text.GetComponent<TextMeshProUGUI>();

        float time = 0f;
        Color startColorImage = buttonImage.color;
        Color startColorText = buttonText.color;

        while (time < duration)
        {
            float t = time / duration;
            // Lerp the alpha value to zero
            buttonImage.color = new Color(startColorImage.r, startColorImage.g, startColorImage.b, Mathf.Lerp(startColorImage.a, 0, t));
            //buttonText.color = new Color(startColorText.r, startColorText.g, startColorText.b, Mathf.Lerp(startColorText.a, 0, t));
            time += Time.deltaTime;
            yield return null;
        }

        // Set final alpha to 0 to ensure it's fully transparent
        buttonImage.color = new Color(startColorImage.r, startColorImage.g, startColorImage.b, 0);
        buttonText.color = new Color(startColorText.r, startColorText.g, startColorText.b, 0);

        // Optionally, disable the button here if you want to prevent further interaction
       

        // Destroy the button's game object
        Destroy(button.gameObject);
    }
}
