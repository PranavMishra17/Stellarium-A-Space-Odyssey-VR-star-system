using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddButton : MonoBehaviour
{
    public GameObject text;
    public float scaleFactor = 2f;
    public float transitionDuration = 0.5f;
    public Material[] materials; // Assign 0: Enlarged material, 1: Normal material

    private Vector3 originalScale;
    private Coroutine currentScaleCoroutine;

    public StarField sf;
    public string thisButtonName;

    public WandRaycaster wrc;

    void Start()
    {
        originalScale = transform.localScale;
    }

    private bool isLineOnQuad = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter");
        if (other.CompareTag("Ray"))
        {
            isLineOnQuad = true;
            Debug.Log("Line entered the quad.");
            wrc.button = gameObject;
            OnPointerEnter();
            // Call any other functions or events you want to trigger when the line enters the quad
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit");
        if (other.CompareTag("Ray"))
        {
            isLineOnQuad = false;
            Debug.Log("Line exited the quad.");
            wrc.button = null;
            OnPointerExit();
            // Call any other functions or events you want to trigger when the line exits the quad
        }
    }

    public void OnPointerEnter()
    {
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
        }
        currentScaleCoroutine = StartCoroutine(ScaleQuad(transform, originalScale * scaleFactor, transitionDuration));
        text.SetActive(true);

        GetComponent<Renderer>().material = materials[0]; // Switch to the "enlarged" material
    }

    public void OnPointerExit()
    {
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
        }
        currentScaleCoroutine = StartCoroutine(ScaleQuad(transform, originalScale, transitionDuration));
        text.SetActive(false);

        GetComponent<Renderer>().material = materials[1]; // Switch back to the normal material
    }

    private IEnumerator ScaleQuad(Transform quadTransform, Vector3 targetScale, float duration)
    {
        float time = 0f;
        Vector3 startScale = quadTransform.localScale;

        while (time < duration)
        {
            float t = time / duration;
            quadTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        quadTransform.localScale = targetScale;
    }

    public void Push()
    {
        OnPointerExit();
        switch (thisButtonName)
        {
            case "play":
                 PlayButton(); return;
            case "reset":
                ResetButton(); return; 
            case "switch2feet":
                SwitchFeetButton(); return; 
            case "switchcolor":
                SwitchColorButton(); return; 
            case "":
                return;
            default:
                return; 
        }
    }

    public void PlayButton()
    {
        sf.MoveStars();
    }

    public void ResetButton()
    {
        sf.ResetStars();
    }

    public void SwitchFeetButton()
    {
        sf.MoveStars();
    }

    public void SwitchColorButton()
    {
        sf.MoveStars();
    }

}
