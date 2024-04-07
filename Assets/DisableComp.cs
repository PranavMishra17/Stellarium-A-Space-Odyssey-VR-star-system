using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableComp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetActiveFalse();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActiveFalse()
    {
        StartCoroutine(FadeOutStars());
    }

    IEnumerator FadeOutStars()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
}
