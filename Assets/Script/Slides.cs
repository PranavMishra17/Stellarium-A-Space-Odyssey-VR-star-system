using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slides : MonoBehaviour
{

    public Sprite[] images;
    public string[] text;
    public AudioClip[] audioClip;
    public float time;

    public int len;
    // Start is called before the first frame update
    void Start()
    {
        len = images.Length;
    }
}
