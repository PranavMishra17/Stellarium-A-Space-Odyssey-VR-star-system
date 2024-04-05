using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectButton : MonoBehaviour
{

    public GameObject buttonRn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with " + gameObject.tag);
        if (collision.gameObject.CompareTag("Ray"))
        {
            buttonRn = collision.gameObject;
        }
    }

    void OnCollisionExit(Collision collision)
    {

        if (collision.gameObject.CompareTag("Ray"))
        {
            buttonRn = null;
        }
    }


    /*

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Button"))
        {
            buttonRn = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ray"))
        {
            buttonRn = null;
        }
    }*/
}
