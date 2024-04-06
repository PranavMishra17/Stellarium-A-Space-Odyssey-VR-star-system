using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {


    public GameObject wrc;

	// Use this for initialization
	void Start () {

        // Calculate the rotation needed to make the GameObject look at the head
        Quaternion rotation = Quaternion.LookRotation(transform.position - wrc.gameObject.transform.position);

        // Apply the rotation to the GameObject
        transform.rotation = rotation;
    }
	
	// Update is called once per frame
	void Update () {


    }
}
