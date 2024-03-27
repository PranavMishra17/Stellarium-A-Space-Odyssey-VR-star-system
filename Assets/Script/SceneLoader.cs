using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoader : MonoBehaviour
{
    public Slider loadingSlider; // Optional: Assign in the Inspector, if you have a slider for progress
    public Text loadingText; // Optional: Assign in the Inspector, for showing loading percentage
    public Button continueButton; // Assign this button in the Inspector

    public string sceneToLoad = "StarSystem";

    private void Start()
    {
        if (continueButton != null)
            continueButton.gameObject.SetActive(false); // Hide the continue button initially

        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Prevent automatic activation

        // Loop until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            // Use asyncLoad.progress to get the loading progress percentage.
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // Dividing by 0.9f because progress only reaches 0.9

            // Update UI elements with the current loading progress
            if (loadingSlider != null)
                loadingSlider.value = progress;
            if (loadingText != null)
                loadingText.text = (progress * 100f).ToString("F2") + "%";

            // Check if the loading is almost done (reached 90%)
            if (asyncLoad.progress >= 0.9f)
            {
                // Show the continue button only after reaching 90%
                if (continueButton != null && !continueButton.gameObject.activeSelf)
                {
                    continueButton.gameObject.SetActive(true);
                    // Attach a listener to the continue button to allow scene activation when clicked
                    continueButton.onClick.AddListener(() => asyncLoad.allowSceneActivation = true);
                }
            }

            yield return null;
        }
    }

}
