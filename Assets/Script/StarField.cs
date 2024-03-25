using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;

public class StarField : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private float starSizeMin = 0f;
    [Range(0, 100)]
    [SerializeField] private float starSizeMax = 5f;
    [SerializeField] private float positionScale = 0.01f; // Adjust this value as needed to scale star positions

    private List<StarDataLoader.Star> stars;
    private List<GameObject> starObjects;

    // New field to hold loaded star data from JSON
    private StarGameObjectInfoList loadedData;

    private readonly int starFieldScale = 800; // Adjust this value as needed for your scene

    private Dictionary<int, GameObject> starMap; // Maps Hipparcos number to GameObject
    private List<Constellation> constellationstxt = new List<Constellation>();

    private Dictionary<int, GameObject> constellationLines = new Dictionary<int, GameObject>();

    MaterialPropertyBlock propBlock;

    public float starlineWidth = 1f;

    public int numberOfStarsToLoad = -1; // Default to -1, indicating "load all"
    public int speedScale = 100000;

    public Material sharedMaterial;

    public GameObject starQuad;

    public class StarInfoWrapper
    {
        public List<StarGameObjectInfo> stars;
    }

    [System.Serializable]
    public class StarGameObjectInfoList
    {
        public List<StarGameObjectInfo> stars = new List<StarGameObjectInfo>();
    }


    [System.Serializable]
    public class StarGameObjectInfo
    {
        public float[] position = new float[3];
        public float[] rotation = new float[4]; // Quaternion rotation
        public float hipparcosNumber; // Link to the Star data
        public Color starcolor;
        public Vector3 starVelocity;

        public float[] originalPosition = new float[3];
        public float[] lastPosition = new float[3];

        public float absoluteMagnitude;
        public float relativeMagnitude;

        //public StarGameObjectInfo() { }

        public StarGameObjectInfo(GameObject go, float hip, Color colour, Vector3 velocity, float absMag, float relMag)
        {
            position[0] = go.transform.position.x;
            position[1] = go.transform.position.y;
            position[2] = go.transform.position.z;

            Quaternion rot = go.transform.rotation;
            rotation[0] = rot.x;
            rotation[1] = rot.y;
            rotation[2] = rot.z;
            rotation[3] = rot.w;

            hipparcosNumber = hip;
            starcolor = colour;
            starVelocity = velocity;
            originalPosition = position;
            lastPosition = position;

            absoluteMagnitude = absMag;
            relativeMagnitude = relMag;
        }
    }


    void Start()
    {
        starMap = new Dictionary<int, GameObject>();

        //LoadStarsGO(); // Laod the stars from the excel file on runtime

        //SaveStarsDataAndGOInfo();  // load stars from excel file and saves their attributes in a json file

        LoadStarsFromJSON();  // load stars from json file in resources

        LoadConstellations();
    }

    private void LoadStarsGO()
    {
        // Load star data
        StarDataLoader starDataLoader = new StarDataLoader();
        stars = starDataLoader.LoadData(numberOfStarsToLoad);
        starObjects = new List<GameObject>();

        if (stars.Count == 0)
        {
            Debug.LogError("No stars were loaded. Ensure the data is correct and the file is in the Resources folder.");
            return;
        }

        // Prepare a shared material property block for instancing
        propBlock = new MaterialPropertyBlock();
        bool alphaCentauriFound = false; // Flag to check if Alpha Centauri is found

        foreach (StarDataLoader.Star star in stars)
        {
            GameObject starObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

            starObject.GetComponent<MeshCollider>().convex = true; // Remove the MeshCollider
            //starObject.AddComponent<Rigidbody>(); // Add a BoxCollider

            starObject.transform.parent = transform;
            starObject.name = $"HR {star.hipparcosNumber}";
            Vector3 scaledPosition = star.position * positionScale;
            starObject.transform.localPosition = scaledPosition * starFieldScale;
            starObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude);
            starObject.transform.LookAt(transform.position);
            starObject.transform.Rotate(0, 180, 0);

            // Checking if the current star is Alpha Centauri
            if (star.hipparcosNumber == 71683)
            {
                Debug.Log($"Alpha Centauri (Rigil Kentaurus) found! Position: {scaledPosition}, Color: {star.colour}, Size: {Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude)}");
                alphaCentauriFound = true;
            }

            // Set common material (with GPU instancing enabled) to all stars
            MeshRenderer renderer = starObject.GetComponent<MeshRenderer>();
            renderer.material = sharedMaterial; // Ensure this material has GPU Instancing enabled and uses the optimized shader

            // Customize each star's appearance using a material property block
            propBlock.Clear();
            propBlock.SetColor("_Color", star.colour);
            renderer.SetPropertyBlock(propBlock);

            starObject.tag = "Star";
            starObject.layer = 9;

            starObjects.Add(starObject);

            // Map star objects for easy access
            starMap.Add((int)star.hipparcosNumber, starObject);
        }

        if (!alphaCentauriFound)
        {
            Debug.LogWarning("Alpha Centauri (Rigil Kentaurus) not found in the dataset.");
        }

        // Debug statements for total stars
        Debug.Log($"Total number of stars loaded: {stars.Count}");
    }

    private void SaveStarsDataAndGOInfo()
    {
        // Load star data
        StarDataLoader starDataLoader = new StarDataLoader();
        stars = starDataLoader.LoadData(numberOfStarsToLoad);
        starObjects = new List<GameObject>();
        List<StarGameObjectInfo> starGOInfoList = new List<StarGameObjectInfo>(); // List to hold star GameObject info

        if (stars.Count == 0)
        {
            Debug.LogError("No stars were loaded. Ensure the data is correct and the file is in the Resources folder.");
            return;
        }

        propBlock = new MaterialPropertyBlock();

        foreach (StarDataLoader.Star star in stars)
        {
            GameObject starObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

            starObject.GetComponent<MeshCollider>().convex = true; // Make MeshCollider convex or remove it
            Destroy(starObject.GetComponent<MeshCollider>()); // Optional: Remove the MeshCollider

            starObject.transform.parent = transform;
            starObject.name = $"HR {star.hipparcosNumber}";
            Vector3 scaledPosition = star.position * positionScale;
            starObject.transform.localPosition = scaledPosition * starFieldScale;
            starObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude);
            starObject.transform.LookAt(transform.position);
            starObject.transform.Rotate(0, 180, 0);

            if (star.hipparcosNumber == 71683)
            {
                Debug.Log($"Alpha Centauri (Rigil Kentaurus) found! Position: {scaledPosition}, Color: {star.colour}, Size: {Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude)}");
                //alphaCentauriFound = true;
            }

            MeshRenderer renderer = starObject.GetComponent<MeshRenderer>();
            renderer.material = sharedMaterial;
            propBlock.Clear();
            propBlock.SetColor("_Color", star.colour);
            renderer.SetPropertyBlock(propBlock);

            starObject.tag = "Star";
            starObject.layer = 9;

            starObjects.Add(starObject);

            // Add star GameObject info to the list
            StarGameObjectInfo goInfo = new StarGameObjectInfo(starObject, star.hipparcosNumber, star.colour, star.velocity, star.absoluteMagnitude, star.relativeMagnitude);
            starGOInfoList.Add(goInfo);
        }

        // Save the star GameObject info to a JSON file
        SaveStarsGameObjectInfo(starGOInfoList);

        Debug.Log($"Total number of stars loaded: {stars.Count}");
    }

    public void SaveStarsGameObjectInfo(List<StarGameObjectInfo> starGOInfoList, string filename = "starsGOInfo.json")
    {
        StarGameObjectInfoList container = new StarGameObjectInfoList();
        container.stars = starGOInfoList;

        string path = Path.Combine(Application.persistentDataPath, filename);
        string json = JsonUtility.ToJson(container, true);
        File.WriteAllText(path, json);
        Debug.Log($"Star GameObject info saved to {path}");
    }


    private void LoadStarsFromJSON(string filename = "starsGOInfo")
    {
        starObjects = new List<GameObject>();

        // Note: No need for file extension; assume "starsGOInfo" as filename/resourcePath
        TextAsset textAsset = Resources.Load<TextAsset>(filename);
        if (textAsset == null)
        {
            Debug.LogError($"Star GameObject info file not found in Resources at {filename}");
            return;
        }

        // Deserialize JSON content to your container class
        loadedData = JsonUtility.FromJson<StarGameObjectInfoList>(textAsset.text);
        if (loadedData == null || loadedData.stars == null || loadedData.stars.Count == 0)
        {
            Debug.LogError("Failed to load star data from JSON.");
            return;
        }

        foreach (StarGameObjectInfo info in loadedData.stars)
        {
            GameObject starObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(starObject.GetComponent<MeshCollider>()); // Assuming you don't need the MeshCollider

            // Applying saved position, rotation, and scale
            starObject.transform.position = new Vector3(info.position[0], info.position[1], info.position[2]);
            starObject.transform.rotation = new Quaternion(info.rotation[0], info.rotation[1], info.rotation[2], info.rotation[3]);
            starObject.transform.localScale = Vector3.one; // Adjust scale if necessary

            // Apply the shader material and color
            MeshRenderer renderer = starObject.GetComponent<MeshRenderer>();
            renderer.material = sharedMaterial;
            renderer.material.color = info.starcolor; // Apply the saved color

            starObject.name = $"HR {info.hipparcosNumber}";
            starObject.tag = "Star";
            starObject.layer = 9; // Assuming layer 9 is assigned to stars

            starObjects.Add(starObject);

            // Map star objects for easy access
            starMap.Add((int)info.hipparcosNumber, starObject);
        }

        Debug.Log($"Loaded {loadedData.stars.Count} stars from JSON.");
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
    ///



    private int currentConstellationIndex = -1; // Starts with no constellation displayed
    private GameObject currentConstellationHolder = null;

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
    }

    void DrawConstellation(int index)
    {
        if (index < 0 || index >= constellationstxt.Count) return;

        // Destroy the previous constellation GameObject
        if (currentConstellationHolder != null)
        {
            Destroy(currentConstellationHolder);
        }

        Constellation constellation = constellationstxt[index];
        Debug.Log($"Drawing Constellation: {constellation.Name}");

        currentConstellationHolder = new GameObject($"Constellation_{constellation.Name}");

        for (int i = 0; i < constellation.StarPairs.Length; i += 2)
        {
            int starIndex1 = constellation.StarPairs[i];
            int starIndex2 = constellation.StarPairs[i + 1];

            if (starMap.TryGetValue(starIndex1, out GameObject star1) && starMap.TryGetValue(starIndex2, out GameObject star2))
            {
                DrawLineBetweenStars(star1, star2, currentConstellationHolder);
            }
            else
            {
                Debug.LogWarning($"Stars {starIndex1} or {starIndex2} not found in starMap.");
            }
        }

        Debug.Log($"Constellation '{constellation.Name}' star pairs: {string.Join(", ", constellation.StarPairs)}");
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
        if (loadedData != null)
        {
            StartCoroutine(MoveStarsInBatches());
        }
        else
        {
            Debug.LogError("Stars data not loaded.");
        }
    }

    public void ResetStars()
    {
        if (loadedData != null)
        {
            StartCoroutine(ResetStarsInBatches());
        }
        else
        {
            Debug.LogError("Stars data not loaded.");
        }
    }


    public IEnumerator MoveStarsInBatches()
    {
        int batchSize = 1000; // Adjust based on performance
        float timeAdjustment = speedSlider.value * speedScale * timeSpeed;

        for (int i = 0; i < loadedData.stars.Count; i += batchSize)
        {
            for (int j = i; j < Mathf.Min(i + batchSize, loadedData.stars.Count); j++)
            {
                StarGameObjectInfo starInfo = loadedData.stars[j];
                if (starMap.TryGetValue((int)starInfo.hipparcosNumber, out GameObject starObject))
                {
                    Vector3 velocity = starInfo.starVelocity * timeAdjustment;
                    Vector3 newPosition = new Vector3(starInfo.lastPosition[0], starInfo.lastPosition[1], starInfo.lastPosition[2]) + velocity;
                    starObject.transform.position = newPosition;
                    starInfo.lastPosition = new float[] { newPosition.x, newPosition.y, newPosition.z };
                }
            }
            yield return null; // Wait for the next frame
        }
        DrawConstellation(currentConstellationIndex);
    }



    public IEnumerator ResetStarsInBatches()
    {
        int batchSize = 1000; // Adjust based on performance

        for (int i = 0; i < loadedData.stars.Count; i += batchSize)
        {
            for (int j = i; j < Mathf.Min(i + batchSize, loadedData.stars.Count); j++)
            {
                StarGameObjectInfo starInfo = loadedData.stars[j];
                if (starMap.TryGetValue((int)starInfo.hipparcosNumber, out GameObject starObject))
                {
                    Vector3 originalPosition = new Vector3(starInfo.originalPosition[0], starInfo.originalPosition[1], starInfo.originalPosition[2]);
                    starObject.transform.position = originalPosition;
                    // Resetting lastPosition to originalPosition as well
                    starInfo.lastPosition = starInfo.originalPosition;
                }
            }
            yield return null; // Wait for the next frame
        }

        playtext.text = "MOVE";
        DrawConstellation(currentConstellationIndex);
    }



    public void AdjustTimeSpeed()
    {
        timeSpeed = speedSlider.value;
    }


}