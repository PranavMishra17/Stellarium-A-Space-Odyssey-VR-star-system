using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddButton : MonoBehaviour
{
    public GameObject text;
    public float scaleFactor = 2f;
    public float transitionDuration = 0.5f;
    public Material[] materials; // Assign 0: Enlarged material, 1: Normal material
    public Color[] colors; // Assign 0: Enlarged material, 1: Normal material

    private Vector3 originalScale;
    private Coroutine currentScaleCoroutine;

    public StarField sf;
    public string thisButtonName;

    public WandRaycaster wrc;
    public bool isEnter = false;

    public ExitImage exi;
    public AudioSource audiosrc;
    public PlaySF playsf;

    public GameObject constButtonPanel;
    public GameObject btnPanel;
    public int constIndex;

    public GameObject infoBtn;

    void Start()
    {
        originalScale = transform.localScale;

        //transform.localRotation = Quaternion.LookRotation.look

        // Calculate the rotation needed to make the GameObject look at the head
        Quaternion rotation = Quaternion.LookRotation(transform.position - wrc.gameObject.transform.position);

        // Apply the rotation to the GameObject
        transform.rotation = rotation;
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

        if(text != null)
        {
            text.SetActive(true);
        }

        if(isEnter)
        {
            GetComponent<Renderer>().material.color = colors[0];
        }
        else
        {
            GetComponent<Renderer>().material = materials[0]; // Switch to the "enlarged" material
        }

    }

    public void OnPointerExit()
    {
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
        }
        currentScaleCoroutine = StartCoroutine(ScaleQuad(transform, originalScale, transitionDuration));
        if (text != null)
        {
            text.SetActive(false);
        }

        if (isEnter)
        {
            GetComponent<Renderer>().material.color = colors[1];
        }
        else
        {
            GetComponent<Renderer>().material = materials[1]; // Switch back to the normal material
        }

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
            case "enter":
                EnterButton(); return;
            case "slider":
                EnterButton(); return;
            case "const":
                ConstellationButton(); return;
            case "open":
                OpenPanel(); return;
            case "focus":
                FocusonConst(); return;
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
        sf.SwitchUnit();
    }

    public void SwitchColorButton()
    {
        sf.Switch2Planet();
    }

    public void EnterButton()
    {
        exi.ExitBool();
        exi.ExitChild();
        playsf.PlaySoundByIndex(0);
        Destroy(gameObject, 1f);
    }

    public void ConstellationButton()
    {
        sf.Switch2Constellations();

        if (constButtonPanel.gameObject.activeSelf)
        {
            constButtonPanel.SetActive(false);
        }
        else
        {
            constButtonPanel.SetActive(true);
        }
    }

    public void OpenPanel()
    {

        if (btnPanel.gameObject.activeSelf)
        {
            btnPanel.SetActive(false);
            text.GetComponent<TextMesh>().text = "Open";
        }
        else
        {
            btnPanel.SetActive(true);
            text.GetComponent<TextMesh>().text = "Close";
        }
        
    }

    public void FocusonConst()
    {
        Debug.Log("Focus called");
        sf.ToggleConstellationToIndex(constIndex);

        if(constIndex == 59)
        {
            infoBtn.SetActive(true);
        }
        else
        {
            infoBtn.SetActive(false);
        }
    }

}
