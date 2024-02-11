using System.Collections.Generic;
using UnityEngine;

public class StarField : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private float starSizeMin = 0f;
    [Range(0, 100)]
    [SerializeField] private float starSizeMax = 5f;
    [SerializeField] private float positionScale = 0.01f; // Adjust this value as needed to scale star positions

    private List<StarDataLoader.Star> stars;
    private List<GameObject> starObjects;

    private readonly int starFieldScale = 800; // Adjust this value as needed for your scene

    void Start()
    {
        // Read in the star data using the StarDataLoader.
        StarDataLoader starDataLoader = new StarDataLoader();
        stars = starDataLoader.LoadData();
        starObjects = new List<GameObject>();

        if (stars.Count == 0)
        {
            Debug.LogError("No stars were loaded. Ensure the data is correct and the file is in the Resources folder.");
            return;
        }

        foreach (StarDataLoader.Star star in stars)
        {
            // Create star game objects as quads.
            GameObject starObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            starObject.transform.parent = transform;
            starObject.name = $"HR {star.hipparcosNumber}";

            // Apply the position scaling factor here
            Vector3 scaledPosition = star.position * positionScale;

            starObject.transform.localPosition = scaledPosition * starFieldScale;
            starObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.size);
            starObject.transform.LookAt(transform.position);
            starObject.transform.Rotate(0, 180, 0);

            // Assign the Unlit/StarShader to the material.
            Material material = starObject.GetComponent<MeshRenderer>().material;

            Shader starShader = Shader.Find("Unlit/StarShader");
            if (starShader != null)
            {
                material.shader = starShader;
                material.color = star.colour;
                starObject.AddComponent<SphereCollider>();
                starObject.GetComponent<SphereCollider>().radius = 50f;
                starObject.GetComponent<SphereCollider>().isTrigger = true;
                starObject.tag = "Star";
                starObject.layer = 9;
            }
            else
            {
                Debug.LogError("StarShader not found. Make sure the shader name is correct.");
                material.color = star.colour;
            }
            // Uncomment and adjust the following line when the shader is available
            // material.shader = Shader.Find("Unlit/StarShader");
            // Use the color defined in the StarDataLoader

            starObjects.Add(starObject);
        }
    }

    private void FixedUpdate()
    {
        // Rotate the camera around the scene on right mouse button drag
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Camera.main.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.right, Input.GetAxis("Mouse Y") * Time.deltaTime * 200);
            Camera.main.transform.RotateAround(Camera.main.transform.position, Vector3.up, -Input.GetAxis("Mouse X") * Time.deltaTime * 200);
        }
    }

    private void OnValidate()
    {
        // This function will update the star sizes in the editor when the starSizeMin or starSizeMax values are changed
        if (starObjects != null && stars != null && starObjects.Count == stars.Count)
        {
            for (int i = 0; i < starObjects.Count; i++)
            {
                // Update the size set in the shader (if using one) or just the scale of the object
                starObjects[i].transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, stars[i].size);
                // If using a shader that has a size property, you would set it like this:
                // starObjects[i].GetComponent<MeshRenderer>().material.SetFloat("_Size", Mathf.Lerp(starSizeMin, starSizeMax, stars[i].size));
            }
        }
    }
}