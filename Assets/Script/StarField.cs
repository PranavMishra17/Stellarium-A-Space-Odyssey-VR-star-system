using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.IO;
using VolumetricLines;

public class StarField : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private float starSizeMin = 0f;
    [Range(0, 100)]
    [SerializeField] private float starSizeMax = 5f;
    [SerializeField] private float positionScale = 0.01f; // Adjust this value as needed to scale star positions

    private List<StarDataLoader.Star> stars;
    private List<GameObject> starObjects;

    private List<Explanet.Planet> planets;
    private List<GameObject> planetObjects;

    // New field to hold loaded star data from JSON
    private StarGameObjectInfoList loadedData;
    private PlanetGameObjectInfoList loadedPlanetData;
    private List<GameObject> constellationLineObjects = new List<GameObject>();

    private readonly int starFieldScale = 800; // Adjust this value as needed for your scene

    private Dictionary<int, GameObject> starMap; // Maps Hipparcos number to GameObject
    private List<Constellation> constellationstxt = new List<Constellation>();

    private Dictionary<int, GameObject> constellationLines = new Dictionary<int, GameObject>();

    MaterialPropertyBlock propBlock;

    public float starlineWidth = 1f;

    public int numberOfStarsToLoad = -1; // Default to -1, indicating "load all"
    public int speedScale = 100000;

    public Material sharedMaterial;
    public Material focusedConstellationMaterial;

    public GameObject starQuad;

    public GameObject cameraCtrl;
    public GameObject cameraCtrl2;

    private bool starParsec = true;
    public GameObject starParentGO;
    private GameObject planetParentGO;
    public Text loadingProgressText;
    public TextMesh loadingProgressText2;

    public int starBatch = 100;
    public GameObject btn2load;
    public bool loadStars = false;

    [System.Serializable]
    public class StarGameObjectInfoList
    {
        public List<StarGameObjectInfo> stars = new List<StarGameObjectInfo>();
    }


    [System.Serializable]
    public class StarGameObjectInfo
    {
        public float[] position = new float[3]; // Position in parsecs
        public float[] positionInFeet = new float[3]; // Position in feet
        public float[] rotation = new float[4]; // Quaternion rotation
        public float hipparcosNumber; // Link to the Star data
        public Color starcolor;
        public Vector3 starVelocity;

        public Vector3 starfeetVelocity;

        public float[] originalPosition = new float[3];
        public float[] lastPosition = new float[3];

        public float absoluteMagnitude;
        public float relativeMagnitude;


        public StarGameObjectInfo(GameObject go, float hip, Color colour, Vector3 velocity, float absMag, float relMag)
        {
            // Position in parsecs
            position[0] = go.transform.position.x;
            position[1] = go.transform.position.y;
            position[2] = go.transform.position.z;

            // Convert and store position in feet
            Vector3 positionFeet = go.transform.position * 3.28084f; // 1 parsec = 3.28084 feet
            positionInFeet[0] = positionFeet.x;
            positionInFeet[1] = positionFeet.y;
            positionInFeet[2] = positionFeet.z;

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


            Vector3 velocityFeet = velocity * 3.28084f; // 1 parsec = 3.28084 feet
            this.starfeetVelocity = velocityFeet;
        }
    }

    [System.Serializable]
    public class PlanetGameObjectInfoList
    {
        public List<PlanetGameObjectInfo> planets = new List<PlanetGameObjectInfo>();
    }


    [System.Serializable]
    public class PlanetGameObjectInfo
    {
        public float[] position = new float[3]; // Position in parsecs
        public float[] positionInFeet = new float[3]; // Position in feet
        public float[] rotation = new float[4]; // Quaternion rotation
        public Color starcolor;

        public float[] originalPosition = new float[3];
        public float[] lastPosition = new float[3];


        public PlanetGameObjectInfo(GameObject go, Color colour)
        {
            // Position in parsecs
            position[0] = go.transform.position.x;
            position[1] = go.transform.position.y;
            position[2] = go.transform.position.z;

            // Convert and store position in feet
            Vector3 positionFeet = go.transform.position * 3.28084f; // 1 parsec = 3.28084 feet
            positionInFeet[0] = positionFeet.x;
            positionInFeet[1] = positionFeet.y;
            positionInFeet[2] = positionFeet.z;

            Quaternion rot = go.transform.rotation;
            rotation[0] = rot.x;
            rotation[1] = rot.y;
            rotation[2] = rot.z;
            rotation[3] = rot.w;

            starcolor = colour;
            originalPosition = position;
            lastPosition = position;

        }
    }


    public SpacePartitioner spacepart;

    private Dictionary<int, char> exoplanet;

    void Start()
    {
        spacepart = GetComponent<SpacePartitioner>();
        starMap = new Dictionary<int, GameObject>();

        exoplanet = LoadExoplanetData();

        //LoadStarsGO(); // Laod the stars from the excel file on runtime

        //SaveStarsDataAndGOInfo();  // load stars from excel file and saves their attributes in a json file

        if (loadStars)
        {
            StartCoroutine(LoadStarsFromJSONCoroutine());
            //LoadStarsFromJSON();  // load stars from json file in resources
        }

        //SavePlanetsDataAndGOInfo(); // load planets from excel file and save attr in json

        //LoadPlanetsFromJSON(); // load planets from json

    }

    public void LoadStars()
    {
        StartCoroutine(LoadStarsFromJSONCoroutine());
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
        //bool alphaCentauriFound = false; // Flag to check if Alpha Centauri is found

        foreach (StarDataLoader.Star star in stars)
        {
            GameObject starObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

            starObject.GetComponent<MeshCollider>().convex = true; // Remove the MeshCollider
            //starObject.AddComponent<Rigidbody>(); // Add a BoxCollider

            starObject.transform.parent = transform;
            starObject.name = $"HR {star.hipparcosNumber}";
            Vector3 scaledPosition = star.position * positionScale;
            starObject.transform.localPosition = star.position * positionScale;
            //starObject.transform.localPosition = scaledPosition * starFieldScale;


            starObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude);
            starObject.transform.LookAt(transform.position);
            starObject.transform.Rotate(0, 180, 0);

            // Checking if the current star is Alpha Centauri
            if (star.hipparcosNumber == 71683)
            {
                Debug.Log($"Alpha Centauri (Rigil Kentaurus) found! Position: {scaledPosition}, Color: {star.colour}, Size: {Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude)}");
                //alphaCentauriFound = true;
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
            starObject.transform.localPosition = scaledPosition;
            starObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, star.absoluteMagnitude);

            // Calculate the direction vector from the starObject's position to the central y-axis cylinder
            Vector3 directionToYAxis = Vector3.Normalize(transform.up - starObject.transform.position);

            starObject.transform.LookAt(directionToYAxis);

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


    private void LoadStarsFromJSON(string filename = "reduced_starsGOinfo")
    {
        starObjects = new List<GameObject>();
        GameObject star_parent = new GameObject("star_parent");
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
            starObject.transform.parent = star_parent.transform;
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
        starParentGO = star_parent;
    }


    private IEnumerator LoadStarsFromJSONCoroutine(string filename = "reduced_starsGOinfo")
    {
        starObjects = new List<GameObject>();
        GameObject star_parent = new GameObject("star_parent");
        TextAsset textAsset = Resources.Load<TextAsset>(filename);

        if (textAsset == null)
        {
            Debug.LogError($"Star GameObject info file not found in Resources at {filename}");
            yield break; // Exit the coroutine early if file not found
        }

        loadedData = JsonUtility.FromJson<StarGameObjectInfoList>(textAsset.text);
        if (loadedData == null || loadedData.stars == null || loadedData.stars.Count == 0)
        {
            Debug.LogError("Failed to load star data from JSON.");
            yield break; // Exit the coroutine early if data failed to load
        }

        int totalStars = loadedData.stars.Count;
        int batchCount = starBatch;
        int frameCounter = 0;

        for (int i = 0; i < totalStars; i += batchCount)
        {
            for (int j = i; j < Mathf.Min(i + batchCount, totalStars); j++)
            {
                StarGameObjectInfo info = loadedData.stars[j];
                GameObject starObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                starObject.transform.parent = star_parent.transform;
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
                starObject.SetActive(false);

                starObjects.Add(starObject);
                
                // Map star objects for easy access
                starMap.Add((int)info.hipparcosNumber, starObject);
            }

            if (frameCounter % 10 == 0) // Update text every 20 frames
            {
                float progress = (float)i / totalStars;
                loadingProgressText.text = $"Loading stars... {progress * 100f:F2}%";
                loadingProgressText2.text = $"Loading stars... {progress * 100f:F2}%";
            }

            frameCounter++;
            yield return null; // Wait for the next frame
        }

        loadingProgressText.text = "Stars Loaded!";
        loadingProgressText2.text = "Stars Loaded!";
        Destroy(loadingProgressText, 3f);
        Destroy(loadingProgressText2, 3f);

        Debug.Log($"Loaded {totalStars} stars from JSON.");
        starParentGO = star_parent;

        foreach (GameObject starObject in starObjects)
        {
            starObject.SetActive(true);
        }

        LoadAndDrawConstellations();

        spacepart.CategorizeStars();
        spacepart.SetActiveStarsInside(true);
        spacepart.SetActiveStarsOutside(false);

        btn2load.SetActive(true);
    }


    /// <summary>
    /// /////////////////////////////////////////////// EXOPLANET LOAD //////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    private void SavePlanetsDataAndGOInfo()
    {
        // Load planet data
        Explanet explanet = new Explanet();
        planets = explanet.LoadData(-1);
        planetObjects = new List<GameObject>();
        List<PlanetGameObjectInfo> planetGOInfoList = new List<PlanetGameObjectInfo>(); // List to hold star GameObject info

        if (planets.Count == 0)
        {
            Debug.LogError("No planets were loaded. Ensure the data is correct and the file is in the Resources folder.");
            return;
        }

        propBlock = new MaterialPropertyBlock();

        foreach (Explanet.Planet planet in planets)
        {
            GameObject planetObject = GameObject.CreatePrimitive(PrimitiveType.Quad);

            planetObject.GetComponent<MeshCollider>().convex = true; // Make MeshCollider convex or remove it
            Destroy(planetObject.GetComponent<MeshCollider>()); // Optional: Remove the MeshCollider

            planetObject.transform.parent = transform;
            planetObject.name = $"HR {planet.hipparcosNumber}";
            Vector3 scaledPosition = planet.position * positionScale;
            planetObject.transform.localPosition = scaledPosition;
            planetObject.transform.localScale = Vector3.one * Mathf.Lerp(starSizeMin, starSizeMax, planet.size);

            // Calculate the direction vector from the starObject's position to the central y-axis cylinder
            Vector3 directionToYAxis = Vector3.Normalize(transform.up - planetObject.transform.position);

            planetObject.transform.LookAt(directionToYAxis);

            planetObject.transform.Rotate(0, 180, 0);


            MeshRenderer renderer = planetObject.GetComponent<MeshRenderer>();
            renderer.material = sharedMaterial;
            propBlock.Clear();
            propBlock.SetColor("_Color", planet.colour);
            renderer.SetPropertyBlock(propBlock);

            planetObject.tag = "Star";
            planetObject.layer = 9;

            planetObjects.Add(planetObject);

            // Add star GameObject info to the list
            PlanetGameObjectInfo goInfo = new PlanetGameObjectInfo(planetObject, planet.colour);
            planetGOInfoList.Add(goInfo);
        }

        // Save the star GameObject info to a JSON file
        SavePlanetGameObjectInfo(planetGOInfoList);

        Debug.Log($"Total number of planets loaded: {planets.Count}");
    }

    public void SavePlanetGameObjectInfo(List<PlanetGameObjectInfo> planetGOInfoList, string filename = "planetsGOInfo.json")
    {
        PlanetGameObjectInfoList container = new PlanetGameObjectInfoList();
        container.planets = planetGOInfoList;

        string path = Path.Combine(Application.persistentDataPath, filename);
        string json = JsonUtility.ToJson(container, true);
        File.WriteAllText(path, json);
        Debug.Log($"Planet GameObject info saved to {path}");
    }


    private void LoadPlanetsFromJSON(string filename = "planetsGOInfo")
    {
        planetObjects = new List<GameObject>();
        GameObject planet_parent = new GameObject("planet_parent");
        // Note: No need for file extension; assume "starsGOInfo" as filename/resourcePath
        TextAsset textAsset = Resources.Load<TextAsset>(filename);
        if (textAsset == null)
        {
            Debug.LogError($"Star GameObject info file not found in Resources at {filename}");
            return;
        }

        // Deserialize JSON content to your container class
        loadedPlanetData = JsonUtility.FromJson<PlanetGameObjectInfoList>(textAsset.text);
        if (loadedPlanetData == null || loadedPlanetData.planets == null || loadedPlanetData.planets.Count == 0)
        {
            Debug.LogError("Failed to load star data from JSON.");
            return;
        }

        foreach (PlanetGameObjectInfo info in loadedPlanetData.planets)
        {

            GameObject planetObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            planetObject.transform.parent = planet_parent.transform;
            Destroy(planetObject.GetComponent<MeshCollider>()); // Assuming you don't need the MeshCollider

            // Applying saved position, rotation, and scale
            planetObject.transform.position = new Vector3(info.position[0], info.position[1], info.position[2]);
            planetObject.transform.rotation = new Quaternion(info.rotation[0], info.rotation[1], info.rotation[2], info.rotation[3]);
            planetObject.transform.localScale = Vector3.one; // Adjust scale if necessary

            // Apply the shader material and color
            MeshRenderer renderer = planetObject.GetComponent<MeshRenderer>();
            renderer.material = sharedMaterial;
            renderer.material.color = info.starcolor; // Apply the saved color

            planetObject.name = $"PR {info}";
            planetObject.tag = "Star";
            planetObject.layer = 9; // Assuming layer 9 is assigned to stars

            planetObjects.Add(planetObject);

            // Map star objects for easy access
            //starMap.Add((int)info.hipparcosNumber, planetObject);
        }

        Debug.Log($"Loaded {loadedPlanetData.planets.Count} stars from JSON.");

        planetParentGO = planet_parent;
        planetParentGO.SetActive(false);
        isPlanet = false;
    }


    private Dictionary<int, char> LoadExoplanetData(string filename = "final_exx")
    {
        Dictionary<int, char> exoplanetData = new Dictionary<int, char>();

        // Assume the file is placed in the Resources folder
        TextAsset data = Resources.Load<TextAsset>(filename);
        if (data != null)
        {
            string[] lines = data.text.Split('\n');
            for (int i = 1; i < lines.Length; i++) // Start from 1 to skip header
            {
                string[] columns = lines[i].Split(',');
                if (columns.Length >= 3)
                {
                    if (int.TryParse(columns[1], out int hipId) && columns[2].Length > 0)
                    {
                        char spectType = columns[2][0]; // Get the first character for spectral type
                        exoplanetData[hipId] = spectType;
                    }
                }
            }
        }

        return exoplanetData;
    }

    public void UpdateStarColors(Dictionary<int, char> exoplanetData)
    {
        int matchCount = 0; // Counter for matching stars
        foreach (var starInfo in loadedData.stars)
        {
            // Check if the current star's HIP number is present in the exoplanet data
            if (exoplanetData.TryGetValue((int)starInfo.hipparcosNumber, out char spectType))
            {
                // Find the star GameObject using the star map
                GameObject starObject;
                if (starMap.TryGetValue((int)starInfo.hipparcosNumber, out starObject))
                {
                    // Convert spectral type to color
                    Color newColor = SpectralTypeToColor(spectType);

                    // Update the star's color using the calculated new color
                    starObject.GetComponent<MeshRenderer>().material.color = newColor;

                    // Increment the match counter
                    matchCount++;
                }
            }
        }

        // Log the number of stars that were updated
        Debug.Log($"Updated colors for {matchCount} stars based on exoplanet data.");
    }

    public void RevertStarColors()
    {
        int revertCount = 0; // Counter for reverted stars
        foreach (var starInfo in loadedData.stars)
        {
            // Find the star GameObject using the star map
            GameObject starObject;
            if (starMap.TryGetValue((int)starInfo.hipparcosNumber, out starObject))
            {
                // Revert the star's color to its original color stored in starInfo
                starObject.GetComponent<MeshRenderer>().material.color = starInfo.starcolor;

                // Increment the revert counter
                revertCount++;
            }
        }

        // Log the number of stars that had their colors reverted
        Debug.Log($"Reverted colors for {revertCount} stars to their original colors.");
    }

    // Method to convert spectral types to color. Implement based on your color scheme.
    private Color SpectralTypeToColor(char spectType)
    {
        // Example of mapping spectral types to Unity colors. Customize as needed.
        switch (spectType)
        {
            case '0': return Color.white;
            case '1': return Color.blue;
            case '2': return Color.cyan;
            case '3': return Color.magenta;
            case '4': return Color.red;
            case '5': return Color.green; // White yellowish
            case '6': return Color.grey; // pink
            case '7': return new Color(0.6f, 0.4f, 0.2f);

            default: return Color.white; // Default color if spectral type is unknown
        }
    }


    public bool isPlanet = false;
    public void Switch2Planet()
    {
        UpdateStarColors(exoplanet);
        if (!isPlanet)
        {
            UpdateStarColors(exoplanet);

            isPlanet = true;
        }
        else
        {
            RevertStarColors();

            isPlanet = false;
        }
    }


    public void FadeOut()
    {
        StartCoroutine(FadeOutStars());
    }

    IEnumerator FadeOutStars()
    {
        yield return new WaitForSeconds(5f);
        starParentGO.SetActive(false);
    }


    /////////////////////////////////////////////////////- Constellations below -//////////////////////////////////////////////////////////////////
    ///



    private int currentConstellationIndex = -1; // Starts with no constellation displayed
    private GameObject currentConstellationHolder = null;
    private Coroutine cameraMoveCoroutine = null;

    private bool displayAllConstellations = true;
    public List<int> selectedConstellationIndices = new List<int>();

    public GameObject volumetricLinePrefab; // Assign in inspector
    private GameObject volumetricLineInstance;
    private VolumetricLineBehavior volumetricLinesScript;
    public float rayWidth;
    public Color raycolor;

    public Color raycolorFocused;


    class Constellation
    {
        public string Name;
        public int PairCount;
        public int[] StarPairs;
        public Vector3 CenterPosition; // Add this line

        public Constellation(string name, int pairCount, int[] starPairs)
        {
            Name = name;
            PairCount = pairCount;
            StarPairs = starPairs;
            CenterPosition = Vector3.zero; // Initialize with zero
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ToggleConstellation(-1); // Move to the previous constellation
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ToggleConstellation(1); // Move to the next constellation
        }

        UpdateDistanceText();
    }

    private GameObject previouslyFocusedConstellation = null;

    void ToggleConstellation(int direction)
    {
        if (constellationstxt.Count == 0) return;

        // Reset material of the previously focused constellation
        if (previouslyFocusedConstellation != null)
        {
            var lineRenderers = previouslyFocusedConstellation.GetComponentsInChildren<VolumetricLineBehavior>();
            foreach (var lr in lineRenderers)
            {
                lr.LineColor = raycolor; // Reset to default material
            }
        }

        if (displayAllConstellations)
        {
            currentConstellationIndex += direction;
            if (currentConstellationIndex >= constellationstxt.Count) currentConstellationIndex = 0;
            else if (currentConstellationIndex < 0) currentConstellationIndex = constellationstxt.Count - 1;
        }
        else
        {
            // Find the current index in the subset
            int currentIndexInSubset = selectedConstellationIndices.IndexOf(currentConstellationIndex);
            currentIndexInSubset += direction;

            if (currentIndexInSubset >= selectedConstellationIndices.Count) currentIndexInSubset = 0;
            else if (currentIndexInSubset < 0) currentIndexInSubset = selectedConstellationIndices.Count - 1;

            // Update currentConstellationIndex to the new value based on the subset
            currentConstellationIndex = selectedConstellationIndices[currentIndexInSubset];
        }


        Constellation focusedConstellation = constellationstxt[currentConstellationIndex];
        GameObject constellationGO = GameObject.Find($"Constellation_{focusedConstellation.Name}");

        if (constellationGO != null)
        {
            var lineRenderers = constellationGO.GetComponentsInChildren<VolumetricLineBehavior>();
            foreach (var lr in lineRenderers)
            {
                lr.LineColor = raycolorFocused; // Set focused material
            }
            previouslyFocusedConstellation = constellationGO; // Update previously focused constellation
        }

        // Turn camera towards the constellation
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
        }
        if(focusedConstellation.CenterPosition == Vector3.zero)
        {
            ToggleConstellation(direction);
        }
        else
        {
            cameraMoveCoroutine = StartCoroutine(TurnCameraToConstellation(focusedConstellation.CenterPosition));
            Debug.Log($"Focused on Constellation: {focusedConstellation.Name}");
        }
       
    }

    IEnumerator TurnCameraToConstellation(Vector3 targetPosition)
    {
        float duration = 2.0f; // Duration of the rotation
        float elapsed = 0.0f;
        Quaternion startRotation = cameraCtrl.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - cameraCtrl.transform.position);

        Vector3 origin = Vector3.zero;
        //Quaternion targetRotation2 = Quaternion.LookRotation(origin - cameraCtrl2.transform.position);

        while (elapsed < duration)
        {
            cameraCtrl.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        //cameraCtrl2.transform.rotation = new Quaternion (0,0,0,0);
    }

    public void ToggleConstellationToIndex(int index)
    {
        if (constellationstxt.Count == 0) return; // Return if no constellations are loaded

        // Reset material of the previously focused constellation
        if (previouslyFocusedConstellation != null)
        {
            var lineRenderers = previouslyFocusedConstellation.GetComponentsInChildren<VolumetricLineBehavior>();
            foreach (var lr in lineRenderers)
            {
                lr.LineColor = raycolor; // Reset to the default color
            }
        }

        if (displayAllConstellations)
        {
            if (index < 0 || index >= constellationstxt.Count) return; // Validate index
            currentConstellationIndex = index;
        }
        else
        {
            if (!selectedConstellationIndices.Contains(index)) return; // Validate index is in the subset
            currentConstellationIndex = index; // Set the current constellation index to the specified index
        }

        Constellation focusedConstellation = constellationstxt[currentConstellationIndex];
        GameObject constellationGO = GameObject.Find($"Constellation_{focusedConstellation.Name}");

        if (constellationGO != null)
        {
            // Highlight the newly focused constellation
            var lineRenderers = constellationGO.GetComponentsInChildren<VolumetricLineBehavior>();
            foreach (var lr in lineRenderers)
            {
                lr.LineColor = raycolorFocused; // Set focused color
            }
            previouslyFocusedConstellation = constellationGO; // Update the reference to the currently focused constellation
        }

        // Orient the camera to focus on the new constellation
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
        }
        if (focusedConstellation.CenterPosition != Vector3.zero)
        {
            cameraMoveCoroutine = StartCoroutine(TurnCameraToConstellation(focusedConstellation.CenterPosition));
            Debug.Log($"Focused on Constellation: {focusedConstellation.Name}");
        }
        else
        {
            Debug.LogError("Constellation Center Position is not set properly.");
        }
    }


    public void EraseAndRedrawConstellations()
    {
        EraseAllConstellationLines();

        if (displayAllConstellations)
        {
            DrawAllConstellations();
        }
        else
        {
            DrawSelectedConstellations(selectedConstellationIndices);
        }
    }

    private void DrawSelectedConstellations(List<int> indices)
    {
        foreach (int index in indices)
        {
            if (index >= 0 && index < constellationstxt.Count)
            {
                DrawConstellation(constellationstxt[index]);
                Debug.Log("Constellation drawing " + index);
            }
        }
    }

    public void SetupConstellationDisplay(bool displayAll, List<int> indices = null)
    {
        displayAllConstellations = displayAll;
        if (!displayAll && indices != null)
        {
            selectedConstellationIndices = indices;
        }

        EraseAndRedrawConstellations();
    }

    private bool switchconstellation = false;

    public void Switch2Constellations()
    {
        if (switchconstellation)
        {
            SetupConstellationDisplay(true);
            switchconstellation = false;
        }
        else
        {
            SetupConstellationDisplay(false, selectedConstellationIndices);
            switchconstellation = true;
        }
    }


    private void EraseAllConstellationLines()
    {
        // Assuming each constellation's lines are under a parent GameObject named with a specific pattern,
        // for example, "Constellation_{ConstellationName}"
        foreach (var constellation in constellationstxt)
        {
            GameObject constellationParent = GameObject.Find($"Constellation_{constellation.Name}");
            if (constellationParent != null)
            {
                Destroy(constellationParent);
            }
        }
    }


    private void DrawAllConstellations()
    {
        foreach (var constellation in constellationstxt)
        {
            DrawConstellation(constellation);
        }
    }

    private void LoadAndDrawConstellations()
    {
        // Assuming LoadConstellations fills `constellations` with data
        LoadConstellations();

        foreach (var constellation in constellationstxt)
        {
            DrawConstellation(constellation);
        }
    }

    private void DrawConstellation(Constellation constellation)
    {
        GameObject constellationParent = new GameObject($"Constellation_{constellation.Name}");
        constellationParent.transform.parent = starParentGO.transform;

        int expectedLines = constellation.StarPairs.Length / 2; // Assuming pairs of stars define a line
        int drawnLines = 0;

        for (int i = 0; i < constellation.StarPairs.Length; i += 2)
        {
            if (i + 1 >= constellation.StarPairs.Length) break; // Safety check

            int hip1 = constellation.StarPairs[i];
            int hip2 = constellation.StarPairs[i + 1];

            if (starMap.TryGetValue(hip1, out GameObject star1) && starMap.TryGetValue(hip2, out GameObject star2))
            {
                //GameObject lineObject = DrawLineBetweenStars(star1, star2, constellation.Name);

                GameObject lineObject = DrawLineRenBetweenStars(star1, star2, constellation.Name);

                lineObject.transform.parent = constellationParent.transform; // Set parent
                drawnLines++;

                Debug.Log($"Constellation drawn {constellation.Name}]");
            }
            else
            {
                //Debug.LogWarning($"Constellation {constellation.Name} missing stars: {hip1} or {hip2}");
            }
        }

        // After attempting to draw all lines, check if any were missing
        if (drawnLines < expectedLines)
        {
           // Debug.Log($"Constellation {constellation.Name} is missing some lines. {drawnLines}/{expectedLines} were drawn.");
        }
        else
        {
            //Debug.Log($"Constellation {constellation.Name} fully drawn with {drawnLines}/{expectedLines} lines.");
        }
    }

    private GameObject DrawLineBetweenStars(GameObject star1, GameObject star2, string constellationName)
    {
        LineRenderer lineRenderer = new GameObject($"{constellationName}_Line").AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        lineRenderer.startWidth = lineRenderer.endWidth = starlineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { star1.transform.position, star2.transform.position });
        return lineRenderer.gameObject; // Return the GameObject
    }

    private GameObject DrawLineRenBetweenStars(GameObject star1, GameObject star2, string constellationName)
    {
        if (volumetricLinePrefab != null)
        {
            volumetricLineInstance = Instantiate(volumetricLinePrefab, transform.position, Quaternion.identity);
            volumetricLineInstance.name = $"{constellationName}_Line";

            volumetricLinesScript = volumetricLineInstance.GetComponent<VolumetricLineBehavior>();
            //volumetricLineInstance.SetActive(false);
            volumetricLinesScript.LineWidth = rayWidth;
            volumetricLinesScript.LineColor = raycolor;
        }

        Vector3 start = star1.transform.position; // Start at the position of the wand
        Vector3 end = star2.transform.position; // End a few units in the direction the wand is pointing

        // Update positions in the VolumetricLines script
        if (volumetricLinesScript != null)
        {
            volumetricLinesScript.m_startPos = start - volumetricLineInstance.transform.position;
            volumetricLinesScript.m_endPos = end - volumetricLineInstance.transform.position;
        }

        return volumetricLineInstance.gameObject;
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

        // After adding stars to the constellation
        foreach (var constellation in constellationstxt)
        {
            Vector3 totalPosition = Vector3.zero;
            int starsCount = 0;
            for (int i = 0; i < constellation.StarPairs.Length; i++)
            {
                int hipNumber = constellation.StarPairs[i];
                if (starMap.TryGetValue(hipNumber, out GameObject starObj))
                {
                    totalPosition += starObj.transform.position;
                    starsCount++;
                }
            }
            if (starsCount > 0)
            {
                constellation.CenterPosition = totalPosition / starsCount; // Calculate average position
            }
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

    public SliderHandle speedSliderValue;
    public Text playtext;

    public Text timeElapsedText; // Assign in Unity Inspector
    private float totalSimulatedYears = 0f; // Total simulated time in year

    private bool isMoving = false;
    private float timeSpeed = 1.0f;


    public Text slidertext;


    public TextMesh switchFeetTxt;
    public TextMesh timeTxt;
    public TextMesh sliderTxt;
    public TextMesh distunderTxt;
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

    // This function is called whenever stars are moved.
    public void UpdateTimeElapsed(float timeAdjustment)
    {
        // Assuming each move operation in your simulation represents an advancement of time
        // proportional to the time adjustment factor.
        float yearsPassed = timeAdjustment; // As timeAdjustment is already in years
        totalSimulatedYears += yearsPassed;

        // Update the UI text component to display the total simulated time elapsed
        timeTxt.text = $"{totalSimulatedYears:F2}";
    }

    public IEnumerator MoveStarsInBatches()
    {
        int batchSize = 1000; // Adjust based on performance
        float timeAdjustment = 0f;

        Vector3 velocity = Vector3.zero;
        timeAdjustment = speedSliderValue.currentValue * speedScale; 



        for (int i = 0; i < loadedData.stars.Count; i += batchSize)
        {
            for (int j = i; j < Mathf.Min(i + batchSize, loadedData.stars.Count); j++)
            {
                StarGameObjectInfo starInfo = loadedData.stars[j];
                if (starMap.TryGetValue((int)starInfo.hipparcosNumber, out GameObject starObject))
                {
                    if (starParsec)
                    {
                        velocity = starInfo.starVelocity * timeAdjustment;
                    }
                    else
                    {
                        velocity = starInfo.starfeetVelocity * timeAdjustment;
                    }
                    Vector3 newPosition = new Vector3(starInfo.lastPosition[0], starInfo.lastPosition[1], starInfo.lastPosition[2]) + velocity;
                    starObject.transform.position = newPosition;
                    starInfo.lastPosition = new float[] { newPosition.x, newPosition.y, newPosition.z };
                }
            }
            yield return null; // Wait for the next frame
        }
        EraseAndRedrawConstellations();

        // Update the time elapsed before moving stars
        UpdateTimeElapsed(timeAdjustment);
    }

    public void ResetStars()
    {
        if (loadedData != null)
        {
            StartCoroutine(ResetStarsInBatches());

            totalSimulatedYears = 0f;
            UpdateTimeElapsed(0f);

            player.transform.position = gameObject.transform.position;

            player.transform.rotation = gameObject.transform.rotation;

            playtext.text = "Switch to Feet";
            switchFeetTxt.text = "Switch to Feet";
        }
        else
        {
            Debug.LogError("Stars data not loaded.");
        }
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

        EraseAndRedrawConstellations();
    }

    public void AdjustTimeSpeed()
    {
        timeSpeed = speedSlider.value;
        sliderTxt.text = speedSlider.value.ToString();
    }


    public void SwitchUnit()
    {
        if (starParsec)
        {
            ShiftStarsToFeet(); starParsec = false;
        }
        else
        {
            ShiftStarsToParsecs();
            //ResetStars();
            starParsec = true;
        }
    }


    public void ShiftStarsToFeet()
    {
        if (loadedData == null)
        {
            Debug.LogError("Stars data not loaded.");
            return;
        }

        StartCoroutine(ShiftStarsPosition(loadedData, true)); // True for shifting to feet
        switchFeetTxt.text = "Switch to Parsec";
        distunderTxt.text = "feet";
        timeElapsedText.text = "0";
    }

    public void ShiftStarsToParsecs()
    {
        if (loadedData == null)
        {
            Debug.LogError("Stars data not loaded.");
            return;
        }

        StartCoroutine(ShiftStarsPosition(loadedData, false)); // False for shifting to parsecs
        switchFeetTxt.text = "Switch to Feet";
        distunderTxt.text = "parsec";
        timeElapsedText.text = "0";

    }

    private IEnumerator ShiftStarsPosition(StarGameObjectInfoList data, bool toFeet)
    {
        int batchSize = 1000;
        for (int i = 0; i < data.stars.Count; i += batchSize)
        {
            for (int j = i; j < Mathf.Min(i + batchSize, data.stars.Count); j++)
            {
                StarGameObjectInfo starInfo = data.stars[j];
                if (starMap.TryGetValue((int)starInfo.hipparcosNumber, out GameObject starObject))
                {
                    Vector3 newPosition;
                    if (toFeet)
                    {
                        // Convert parsec position to feet and update
                        newPosition = new Vector3(starInfo.positionInFeet[0], starInfo.positionInFeet[1], starInfo.positionInFeet[2]);
                    }
                    else
                    {
                        // Convert feet position back to parsecs
                        newPosition = new Vector3(starInfo.originalPosition[0], starInfo.originalPosition[1], starInfo.originalPosition[2]);
                    }
                    starObject.transform.position = newPosition;
                    // Update the lastPosition to match the new unit
                    starInfo.lastPosition = new float[] { newPosition.x, newPosition.y, newPosition.z };
                }
            }
            yield return null; // Allow frame rendering between batches for performance
        }

        EraseAndRedrawConstellations(); // Re-draw constellations since the stars have moved
    }

    public Transform player; // Reference to the player's transform
    public Text distanceText; // Reference to the Text UI for displaying distance
    public TextMesh distanceTxt;


    // Update the distance text based on the distance between the player and the button
    private void UpdateDistanceText()
    {
        if (player != null && distanceTxt != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            distanceTxt.text =distance.ToString("F2");
        }
    }

}