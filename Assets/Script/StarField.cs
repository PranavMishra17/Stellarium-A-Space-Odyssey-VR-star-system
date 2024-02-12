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

    private Dictionary<int, GameObject> starMap; // Maps Hipparcos number to GameObject

    // Define constellations using Hipparcos numbers (simplified for brevity)
    private readonly List<(int[], int[])> constellations = new List<(int[], int[])>
    {
        // Dummy constellation
                (new int[] { 1, 2, 3, 4, 5, 6},
         new int[] { 1,3, 5, 6 }),

                        // Dummy constellation
                (new int[] { 11, 20, 32, 45, 54, 61},
         new int[] { 11,32, 54, 61 }),
                        (new int[] { 98036, 97649, 97649, 97278, 97649, 95501, 95501, 97804, 99473, 97804, 95501, 93747, 93747, 93244, 95501, 93805 },
         new int[] { 98036, 97649, 97649, 97278, 97649, 95501, 95501, 97804, 99473, 97804, 95501, 93747, 93747, 93244, 95501, 93805 }),

                // Aql - Aquila
        (new int[] { 98036, 97649, 97649, 97278, 97649, 95501, 95501, 97804, 99473, 97804, 95501, 93747, 93747, 93244, 95501, 93805 },
         new int[] { 98036, 97649, 97649, 97278, 97649, 95501, 95501, 97804, 99473, 97804, 95501, 93747, 93747, 93244, 95501, 93805 }),

        // And - Andromeda
        (new int[] { 677, 3092, 3092, 5447, 9640, 5447, 5447, 4436, 4436, 3881 },
         new int[] { 677, 3092, 3092, 5447, 9640, 5447, 5447, 4436, 4436, 3881 }),

            // Orion Constellation Example
            (new int[] { 1948, 1903, 1852, 2004, 1713, 2061, 1790, 1907, 2124,
                 2199, 2135, 2047, 2159, 1543, 1544, 1570, 1552, 1567 },
             new int[] { 1713, 2004, 1713, 1852, 1852, 1790, 1852, 1903, 1903, 1948,
                 1948, 2061, 1948, 2004, 1790, 1907, 1907, 2061, 2061, 2124,
                 2124, 2199, 2199, 2135, 2199, 2159, 2159, 2047, 1790, 1543,
                 1543, 1544, 1544, 1570, 1543, 1552, 1552, 1567, 2135, 2047 }),

            // Monceros
            (new int[] { 2970, 3188, 2714, 2356, 2227, 2506, 2298, 2385, 2456, 2479 },
            new int[] { 2970, 3188, 3188, 2714, 2714, 2356, 2356, 2227, 2714, 2506,
                 2506, 2298, 2298, 2385, 2385, 2456, 2479, 2506, 2479, 2385 }),

            // Gemini
            (new int[] { 2890, 2891, 2990, 2421, 2777, 2473, 2650, 2216, 2895,
                 2343, 2484, 2286, 2134, 2763, 2697, 2540, 2821, 2905, 2985},
            new int[] { 2890, 2697, 2990, 2905, 2697, 2473, 2905, 2777, 2777, 2650,
                 2650, 2421, 2473, 2286, 2286, 2216, 2473, 2343, 2216, 2134,
                 2763, 2484, 2763, 2777, 2697, 2540, 2697, 2821, 2821, 2905, 2905, 2985 }),

            // Cancer
            (new int[] {3475, 3449, 3461, 3572, 3249},
             new int[] {3475, 3449, 3449, 3461, 3461, 3572, 3461, 3249}),

            // Leo
            (new int[] { 3982, 4534, 4057, 4357, 3873, 4031, 4359, 3975, 4399, 4386, 3905, 3773, 3731 },
            new int[] { 4534, 4357, 4534, 4359, 4357, 4359, 4357, 4057, 4057, 4031,
                 4057, 3975, 3975, 3982, 3975, 4359, 4359, 4399, 4399, 4386,
                 4031, 3905, 3905, 3873, 3873, 3975, 3873, 3773, 3773, 3731, 3731, 3905 }),

            // Leo Minor
            (new int[] { 3800, 3974, 4100, 4247, 4090 },
            new int[] { 3800, 3974, 3974, 4100, 4100, 4247, 4247, 4090, 4090, 3974 }),

            // Lynx
            (new int[] { 3705, 3690, 3612, 3579, 3275, 2818, 2560, 2238 },
            new int[] { 3705, 3690, 3690, 3612, 3612, 3579, 3579, 3275, 3275, 2818,
                 2818, 2560, 2560, 2238 }),

            // Ursa Major
            (new int[] { 3569, 3594, 3775, 3888, 3323, 3757, 4301, 4295, 4554, 4660,
                 4905, 5054, 5191, 4518, 4335, 4069, 4033, 4377, 4375 },
            new int[] { 3569, 3594, 3594, 3775, 3775, 3888, 3888, 3323, 3323, 3757,
                 3757, 3888, 3757, 4301, 4301, 4295, 4295, 3888, 4295, 4554,
                 4554, 4660, 4660, 4301, 4660, 4905, 4905, 5054, 5054, 5191,
                 4554, 4518, 4518, 4335, 4335, 4069, 4069, 4033, 4518, 4377, 4377, 4375 }),

            // Example for Ursa Major
            (new int[] { 54061, 53910, 58001, 59774, 62956, 65378, 67301 },
             new int[] { 54061, 53910, 53910, 58001, 58001, 59774, 59774, 62956, 62956, 65378, 65378, 67301 }),

            // Example for Cassiopeia
            (new int[] { 3179, 746, 4427, 2646, 2550 },
             new int[] { 3179, 746, 746, 4427, 4427, 2646, 2646, 2550 })
        // Add other constellations here
    };
    private Dictionary<int, GameObject> constellationLines = new Dictionary<int, GameObject>();


    public float starlineWidth = 1f;

    void Start()
    {
        starMap = new Dictionary<int, GameObject>();

        // Read in the star data using the StarDataLoader.
        StarDataLoader starDataLoader = new StarDataLoader();
        stars = starDataLoader.LoadData();
        starObjects = new List<GameObject>();

        float minSize = float.MaxValue;
        float maxSize = float.MinValue;

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
            starObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude);
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

            //Debug.Log($"Star Size: {star.size}");
            if (star.absoluteMagnitude < minSize) minSize = star.absoluteMagnitude;
            if (star.absoluteMagnitude > maxSize) maxSize = star.absoluteMagnitude;

            // Uncomment and adjust the following line when the shader is available
            // material.shader = Shader.Find("Unlit/StarShader");
            // Use the color defined in the StarDataLoader

            starObjects.Add(starObject);

            // Assume each star has a hipparcosNumber property
            if (!starMap.ContainsKey(((int)star.hipparcosNumber)))
            {
                // Assuming starObjects are in the same order as stars
                starMap.Add(((int)star.hipparcosNumber), starObjects[stars.IndexOf(star)]);
            }
        }

        if (starObjects.Count == 0)
        {
            Debug.Log("No stars loaded.");
            return;
        }

        Debug.Log($"Minimum Star Size: {minSize}");
        Debug.Log($"Maximum Star Size: {maxSize}");

        Debug.Log($"Minimum Star Size: {minSize}");
        Debug.Log($"Maximum Star Size: {maxSize}");
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


    /////////////////////////////////////////////////////- Constellations below -//////////////////////////////////////////////////////////////////
    private int currentConstellationIndex = -1; // Starts with no constellation displayed

    void Update()
    {
        // Numeric key toggling for direct constellation access
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                ToggleConstellation(i - 1); // Adjust based on how you're indexing constellations
            }
        }

        // Arrow key toggling for sequential constellation access
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int newIndex = currentConstellationIndex - 1;
            if (newIndex < 0) newIndex = constellations.Count - 1; // Loop back to the last constellation
            ToggleConstellation(newIndex, true);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int newIndex = (currentConstellationIndex + 1) % constellations.Count; // Loop to the first constellation
            ToggleConstellation(newIndex, true);
        }
    }

    void ToggleConstellation(int index, bool sequentialToggle = false)
    {
        if (index < 0 || index >= constellations.Count) return;

        // If toggling sequentially, first clear the currently displayed constellation
        if (sequentialToggle && currentConstellationIndex != -1 && constellationLines.ContainsKey(currentConstellationIndex))
        {
            Destroy(constellationLines[currentConstellationIndex]);
            constellationLines.Remove(currentConstellationIndex);
        }

        // If the same constellation is selected (or in sequential toggle mode), just update the current index without redrawing
        if (constellationLines.ContainsKey(index))
        {
            if (!sequentialToggle) // For direct access toggles, remove and reset the current constellation
            {
                Destroy(constellationLines[index]);
                constellationLines.Remove(index);
                currentConstellationIndex = -1; // Reset the index as no constellation is now displayed
            }
        }
        else
        {
            if (currentConstellationIndex != index || sequentialToggle) // Avoid redrawing the same constellation if already displayed
            {
                // Show the new constellation
                var constellation = constellations[index];
                DrawConstellation(index, constellation.Item1, constellation.Item2);
                currentConstellationIndex = index; // Update the currently displayed constellation index
            }
        }
    }

    void DrawConstellation(int index, int[] stars, int[] lines)
    {
        Debug.Log("Constellation_ " + index);
        GameObject constellationHolder = new GameObject($"Constellation_{index}");
        constellationLines[index] = constellationHolder;

        for (int i = 0; i < lines.Length; i += 2)
        {
            int starIndex1 = lines[i];
            int starIndex2 = lines[i + 1];

            if (starMap.ContainsKey(starIndex1) && starMap.ContainsKey(starIndex2))
            {
                DrawLineBetweenStars(starMap[starIndex1], starMap[starIndex2], constellationHolder);
            }
        }
    }

    void DrawLineBetweenStars(GameObject star1, GameObject star2, GameObject parent)
    {
        LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
        lineRenderer.transform.parent = parent.transform;

        // Set line renderer properties
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, star1.transform.position);
        lineRenderer.SetPosition(1, star2.transform.position);
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        lineRenderer.startWidth = lineRenderer.endWidth = starlineWidth; // Adjust line width as needed
    }


}