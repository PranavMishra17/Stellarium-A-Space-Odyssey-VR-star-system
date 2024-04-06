using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PresentationController : MonoBehaviour
{
    public Renderer quadRenderer; // Renderer of the Quad GameObject
    public TextMesh textMesh; // TextMesh component for displaying text
    public AudioSource audioSource; // AudioSource component for playing audio

    public Presentation presentations;
    private int currentPresentationIndex = 0;
    private int currentSlideIndex = 0;
    private float slideShowTimer = 0f;
    public float slideDuration = 5f;
    public float typeWriterSpeed = 0.05f; // Speed of the typewriter effect
    public float fadeDuration = 0.2f; // Duration of fade-in and fade-out effects

    private Coroutine typeTextCoroutine; // Reference to the currently running typing coroutine

    public bool isOn = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        SetActiveFalse();
    }

    void Update()
    {
        if (presentations.slides.Count == 0)
            return;



        if (isOn)
        {

            slideShowTimer += Time.deltaTime;
            if (slideShowTimer >= slideDuration)
            {
                NextSlide();
                slideShowTimer = 0;
            }
        }
    }


    public void SetActiveFalse()
    {
        StartCoroutine(FadeOutStars());
    }

    IEnumerator FadeOutStars()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    public void StartPresentation(int index)
    {
        if (index < 0 || index >= presentations.slides.Count)
        {
            Debug.LogError("Invalid presentation index.");
            return;
        }

        currentPresentationIndex = index;
        currentSlideIndex = 0;
        ShowSlide(currentPresentationIndex, currentSlideIndex);
    }

    private void NextSlide()
    {
        var currentSlides = presentations.slides[currentPresentationIndex];
        //currentSlideIndex = (currentSlideIndex + 1) % currentSlides.images.Length;

        currentSlideIndex = (currentSlideIndex + 1);

        if (currentSlideIndex  <= currentSlides.images.Length)
        {
            ShowSlide(currentPresentationIndex, currentSlideIndex);
        }
        else
        {
            Debug.Log("END OF SLIDES");
        }
    }
    private void ShowSlide(int presentationIndex, int slideIndex)
    {
        if (presentationIndex < 0 || presentationIndex >= presentations.slides.Count) return;
        var presentation = presentations.slides[presentationIndex];
        if (slideIndex < 0 || slideIndex >= presentation.images.Length) return;

        Slides slide = presentation;

        StartCoroutine(FadeOutIn(
            () =>
            {
                // Fade out complete
                //quadRenderer.material.mainTexture = slide.images[slideIndex].texture;
                // Fade out complete
                Sprite spriteToDisplay = slide.images[slideIndex];
                quadRenderer.material.mainTexture = spriteToDisplay.texture;

                // Resize the quad to maintain the aspect ratio of the sprite
                ResizeQuadToSprite(spriteToDisplay);

                // Set the slide duration to the length of the audio clip in seconds
                if (slide.audioClip[slideIndex] != null)
                {
                    slideDuration = slide.audioClip[slideIndex].length + 4f;
                    Debug.Log(slideDuration + 4f + " is clip length");
                }
                else
                {
                    // Default duration or a fallback duration if no audio clip is present
                    slideDuration = 5f; // Default to 5 seconds or any appropriate fallback duration
                }
            },
            () =>
            {
                // Fade in complete: Update the Quad texture and start new typing coroutine

                audioSource.clip = slide.audioClip[slideIndex];

                audioSource.Play(1);

                // If a typing coroutine is already running, stop it
                if (typeTextCoroutine != null)
                {
                    StopCoroutine(typeTextCoroutine);
                }

                // Start a new typing coroutine
                typeTextCoroutine = StartCoroutine(TypeText(slide.text[slideIndex]));
            }
        ));
    }


    private IEnumerator TypeText(string text)
    {
        // The maximum number of characters in a line
        int maxCharsPerLine = "Zeus as a constellation, along with h".Length;

        // Split the input text into words
        string[] words = text.Split(' ');

        string currentLine = "";
        string formattedText = "";

        foreach (string word in words)
        {
            // Check if adding the next word would exceed the line length
            if (currentLine.Length + word.Length > maxCharsPerLine)
            {
                // If it does, append the current line to formattedText, then start a new one
                formattedText += currentLine + "\n";
                currentLine = word + " "; // Start the new line with the current word
            }
            else
            {
                // If it doesn't exceed the line length, add the word to the current line
                currentLine += word + " ";
            }
        }

        // Append the last line if there's any
        formattedText += currentLine;

        // Now type out the formatted text character by character
        textMesh.text = ""; // Clear existing text
        foreach (char c in formattedText)
        {
            textMesh.text += c;
            yield return new WaitForSeconds(typeWriterSpeed);
        }
    }



    private IEnumerator FadeOutIn(System.Action onFadeOutComplete, System.Action onFadeInComplete)
    {
        // Fade out
        float timer = 0f;
        while (timer <= fadeDuration)
        {
            SetQuadAlpha(Mathf.Lerp(1f, 0f, timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        onFadeOutComplete?.Invoke();

        yield return new WaitForSeconds(0.1f); // Optional delay between fade out and in

        // Fade in
        timer = 0f;
        while (timer <= fadeDuration)
        {
            SetQuadAlpha(Mathf.Lerp(0f, 1f, timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }

        onFadeInComplete?.Invoke();
    }


    private void ResizeQuadToSprite(Sprite sprite)
    {
        float quadHeight = 2.0f; // Max height allowed
        float quadWidth = 3.5f; // Max width allowed
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;

        // Calculate the scaling factor to fit the sprite within the quad dimensions while maintaining aspect ratio
        float widthScale = quadWidth / spriteWidth;
        float heightScale = quadHeight / spriteHeight;
        float scale = Mathf.Min(widthScale, heightScale);

        // Apply the calculated scale to the quad
        quadRenderer.transform.localScale = new Vector3(spriteWidth * scale, spriteHeight * scale, 1f);
    }

    public void SetPresentation(int index)
    {
        if (index >= 0 && index < presentations.slides.Count)
        {
            currentPresentationIndex = index;
            currentSlideIndex = -1; // Reset to -1 so NextSlide will start from the first slide
            slideShowTimer = presentations.slides[currentPresentationIndex].time; // Immediate switch to the first slide of the new presentation
            slideDuration = slideShowTimer;
        }
    }

    void SetQuadAlpha(float alpha)
    {
        Color color = quadRenderer.material.color;
        color.a = alpha;
        quadRenderer.material.color = color;
    }

    // Call this method from your button click event handlers to switch presentations
    public void SwitchToPresentation(int index)
    {
        isOn = true;
        StartPresentation(index);
    }
}
