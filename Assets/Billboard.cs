using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
          // Ensure the quad faces the camera directly each frame
          transform.LookAt(transform.position + mainCamera.transform.forward);
        
    }
}