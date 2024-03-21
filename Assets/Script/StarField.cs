using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private List<Constellation> constellationstxt = new List<Constellation>();

    private Dictionary<int, GameObject> constellationLines = new Dictionary<int, GameObject>();

     

    public float starlineWidth = 1f;

    public int numberOfStarsToLoad = -1; // Default to -1, indicating "load all"
    public int speedScale = 100000;


    void Start()
    {
        starMap = new Dictionary<int, GameObject>();
        LoadConstellations();

        // Read in the star data using the StarDataLoader.
        StarDataLoader starDataLoader = new StarDataLoader();
        stars = starDataLoader.LoadData(numberOfStarsToLoad);
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
            if (star.position.x < minSize) minSize = star.position.x;
            if (star.position.x > maxSize) maxSize = star.position.x;

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

        if (isMoving)
        {
            // Calculate the actual speed scale based on the slider's value (0 to 1 range) scaling up to 5
            float actualSpeedScale = timeSpeed * speedScale; // timeSpeed is the value from the slider

            foreach (var star in stars)
            {
                if (starMap.TryGetValue((int)star.hipparcosNumber, out GameObject starObject))
                {
                    // Scale the velocity by the actual speed scale and deltaTime for frame-independent movement
                    Vector3 movementDelta = star.velocity * actualSpeedScale * Time.deltaTime;
                    starObject.transform.position += movementDelta;

                    // Optionally, debug log the movement of the first few stars
                    if (stars.IndexOf(star) < 5) // Adjust as needed
                    {
                        Debug.Log($"Star {star.hipparcosNumber} moved to {starObject.transform.position} with velocity {star.velocity.magnitude} and slider speed: {timeSpeed} with actual speed {movementDelta.magnitude}");
                    }
                }
            }
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
    ///



    private int currentConstellationIndex = -1; // Starts with no constellation displayed

    void Update()
    {
        ToggleConstellationsWithKeys();
    }

    void ToggleConstellationsWithKeys()
    {
        // Numeric key toggling for direct constellation access
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                ToggleConstellation(i - 1); // Adjust based on your constellation indexing
            }
        }

        // Arrow key toggling for sequential constellation access
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int newIndex = currentConstellationIndex - 1;
            if (newIndex < 0) newIndex = constellationstxt.Count - 1; // Loop back to the last constellation
            ToggleConstellation(newIndex, true);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int newIndex = (currentConstellationIndex + 1) % constellationstxt.Count; // Loop to the first constellation
            ToggleConstellation(newIndex, true);
        }
    }

    void ToggleConstellation(int index, bool sequentialToggle = false)
    {
        if (index < 0 || index >= constellationstxt.Count) return;

        if (sequentialToggle && currentConstellationIndex != -1 && constellationLines.ContainsKey(currentConstellationIndex))
        {
            Destroy(constellationLines[currentConstellationIndex]);
            constellationLines.Remove(currentConstellationIndex);
        }

        // Check if we're toggling within the same constellation and need to exit
        if (sequentialToggle && index == currentConstellationIndex) {
            currentConstellationIndex = -1;
            return;
        }
       

        // Attempt to draw the new constellation
        DrawConstellation(index);
        currentConstellationIndex = index;

        /*
        if (constellationLines.ContainsKey(index))
        {
            if (!sequentialToggle)
            {
                Destroy(constellationLines[index]);
                constellationLines.Remove(index);
                currentConstellationIndex = -1;
            }
        }
        else
        {
            if (currentConstellationIndex != index || sequentialToggle)
            {
                DrawConstellation(index); // Adapted to use the new index
                currentConstellationIndex = index;
            }
        }
        */
    }

    void DrawConstellation(int index)
    {
        if (index < 0 || index >= constellationstxt.Count) return;

        Constellation constellation = constellationstxt[index];
        Debug.Log($"Drawing Constellation: {constellation.Name}");

        bool allLinesDrawn = true;
        GameObject constellationHolder = new GameObject($"Constellation_{constellation.Name}");

        for (int i = 0; i < constellation.StarPairs.Length; i += 2)
        {
            int starIndex1 = constellation.StarPairs[i];
            int starIndex2 = constellation.StarPairs[i + 1];

            if (starMap.ContainsKey(starIndex1) && starMap.ContainsKey(starIndex2))
            {
                DrawLineBetweenStars(starMap[starIndex1], starMap[starIndex2], constellationHolder);
            }
            else
            {
                allLinesDrawn = false;
                Debug.LogWarning($"Stars {starIndex1} or {starIndex2} not found in starMap.");
                break; // Early exit on missing stars
            }
        }

        if (!allLinesDrawn)
        {
            DestroyImmediate(constellationHolder); // Cleanup on incomplete constellation
            int nextIndex = (index + 1) % constellationstxt.Count;
            if (nextIndex != currentConstellationIndex)
            {
                ToggleConstellation(nextIndex, true); // Try next constellation
            }
        }
        else
        {
            constellationLines[index] = constellationHolder; // Successfully drawn, track the GameObject
            currentConstellationIndex = index; // Update current index
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

    class Constellation
    {
        public string Name;
        public int PairCount;
        public int[] StarPairs;

        public Constellation(string name, int pairCount, int[] starPairs)
        {
            Name = name;
            PairCount = pairCount;
            StarPairs = starPairs;
        }
    }

    void LoadConstellations()
    {
        TextAsset constellationData = Resources.Load<TextAsset>("constellationship");
        string[] lines = constellationData.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) continue; // Skip invalid lines

            string constellationName = parts[0];
            if (!int.TryParse(parts[1], out int pairCount)) continue;

            List<int> starPairs = new List<int>();
            for (int i = 2; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out int hipNumber))
                {
                    starPairs.Add(hipNumber);
                }
            }

            constellationstxt.Add(new Constellation(constellationName, pairCount, starPairs.ToArray()));
        }

        // Example: Now constellations list is filled, process as needed
        Debug.Log($"Loaded {constellationstxt.Count} constellations.");
    }

    /// <summary>
    /// //////////////////////////////////////////////   UI Elements - Speed, reset //////////////////////////////////////////////////////////
    /// </summary>

    public Button moveButton;
    public Button resetButton;
    public Slider speedSlider;
    public Text playtext;

    private bool isMoving = false;
    private float timeSpeed = 1.0f;

    public void MoveStars()
    {
        if (isMoving) { isMoving = false; playtext.text = "Play"; }
        else { isMoving = true; playtext.text = "Pause"; }

    }

    public void ResetStars()
    {
        foreach (var star in stars)
        {
            star.position = star.originalPosition; // Reset to original position
            GameObject starObject = starMap[(int)star.hipparcosNumber];
            starObject.transform.position = star.originalPosition * positionScale * starFieldScale;
        }
        isMoving = false; // Stop movement
        playtext.text = "Play";
    }

    public void AdjustTimeSpeed()
    {
        timeSpeed = speedSlider.value;
    }


}