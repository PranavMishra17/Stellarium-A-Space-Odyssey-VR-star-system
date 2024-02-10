using System.Collections.Generic;
using UnityEngine;

public class StarField : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private float starSizeMin = 0f;
    [Range(0, 100)]
    [SerializeField] private float starSizeMax = 5f;

    private List<StarDataLoader.Star> stars;
    private List<GameObject> starObjects;

    private readonly int starFieldScale = 800;

    void Start()
    {
        // Read in the star data using the new StarDataLoader.
        StarDataLoader starDataLoader = new StarDataLoader();
        stars = starDataLoader.LoadData();
        starObjects = new List<GameObject>();

        foreach (StarDataLoader.Star star in stars)
        {
            // Create star game objects as quads.
            GameObject starObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            starObject.transform.parent = transform;
            starObject.name = $"HR {star.hipparcosNumber}";
            starObject.transform.localPosition = star.position * starFieldScale;
            starObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.size);
            starObject.transform.LookAt(transform.position);
            starObject.transform.Rotate(0, 180, 0);

            // Assign the Unlit/StarShader to the material.
            Material material = starObject.GetComponent<MeshRenderer>().material;
            material.shader = Shader.Find("Unlit/StarShader");

            // Set material properties.
            material.SetFloat("_Size", Mathf.Lerp(starSizeMin, starSizeMax, star.size));
            material.color = star.colour;

            starObjects.Add(starObject);
        }
    }

    // Could also do in Update with Time.deltatime scaling.
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Camera.main.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.right, Input.GetAxis("Mouse Y"));
            Camera.main.transform.RotateAround(Camera.main.transform.position, Vector3.up, -Input.GetAxis("Mouse X"));
        }
    }

    private void OnValidate()
    {
        if (starObjects != null)
        {
            for (int i = 0; i < starObjects.Count; i++)
            {
                // Update the size set in the shader.
                Material material = starObjects[i].GetComponent<MeshRenderer>().material;
                material.SetFloat("_Size", Mathf.Lerp(starSizeMin, starSizeMax, stars[i].size));
            }
        }
    }
}