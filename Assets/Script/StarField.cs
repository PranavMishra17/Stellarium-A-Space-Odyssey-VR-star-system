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

    private bool starParsec = true;
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
        public float[] position = new float[3]; // Position in parsecs
        public float[] positionInFeet = new float[3]; // Position in feet
        public float[] rotation = new float[4]; // Quaternion rotation
        public float hipparcosNumber; // Link to the Star data
        public Color starcolor;
        public Vector3 starVelocity;

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
        }
    }



    void Start()
    {
        starMap = new Dictionary<int, GameObject>();

        //LoadStarsGO(); // Laod the stars from the excel file on runtime

        //SaveStarsDataAndGOInfo();  // load stars from excel file and saves their attributes in a json file

        LoadStarsFromJSON();  // load stars from json file in resources

        LoadAndDrawConstellations();
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
            starObject.transform.localPosition = star.position * positionScale;
            //starObject.transform.localPosition = scaledPosition * starFieldScale;


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

            // Make the starObject look in the direction of the central y-axis cylinder
            //starObject.transform.LookAt(transform.position + directionToYAxis);

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
    private Coroutine cameraMoveCoroutine = null;

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
    }

    private GameObject previouslyFocusedConstellation = null;

    void ToggleConstellation(int direction)
    {
        if (constellationstxt.Count == 0) return;

        // Reset material of the previously focused constellation
        if (previouslyFocusedConstellation != null)
        {
            var lineRenderers = previouslyFocusedConstellation.GetComponentsInChildren<LineRenderer>();
            foreach (var lr in lineRenderers)
            {
                lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive")); // Reset to default material
            }
        }

        // Update current index based on direction
        currentConstellationIndex += direction;
        if (currentConstellationIndex >= constellationstxt.Count) currentConstellationIndex = 0;
        else if (currentConstellationIndex < 0) currentConstellationIndex = constellationstxt.Count - 1;

        Constellation focusedConstellation = constellationstxt[currentConstellationIndex];
        GameObject constellationGO = GameObject.Find($"Constellation_{focusedConstellation.Name}");

        if (constellationGO != null)
        {
            var lineRenderers = constellationGO.GetComponentsInChildren<LineRenderer>();
            foreach (var lr in lineRenderers)
            {
                lr.material = focusedConstellationMaterial; // Set focused material
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

        while (elapsed < duration)
        {
            cameraCtrl.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }


    private void SwapConstellationMaterials()
    {
        if (constellationstxt.Count == 0) return;

        Constellation focusedConstellation = constellationstxt[currentConstellationIndex];
        GameObject constellationGO = GameObject.Find($"Constellation_{focusedConstellation.Name}");

        Debug.Log($"Focused on Constellation GO: {constellationGO.name}");
        if (constellationGO != null)
        {
            var lineRenderers = constellationGO.GetComponentsInChildren<LineRenderer>();
            foreach (var lr in lineRenderers)
            {
                lr.material = focusedConstellationMaterial; // Set focused material
            }
            //previouslyFocusedConstellation = constellationGO; // Update previously focused constellation
        }

        // Turn camera towards the constellation
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
        }
        if (focusedConstellation.CenterPosition == Vector3.zero)
        {
            //ToggleConstellation(1);
        }
        else
        {
            cameraMoveCoroutine = StartCoroutine(TurnCameraToConstellation(focusedConstellation.CenterPosition));
            Debug.Log($"Focused on Constellation: {focusedConstellation.Name}");
        }
    }


    public void EraseAndRedrawConstellations()
    {
        EraseAllConstellationLines();
        DrawAllConstellations();
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
        for (int i = 0; i < constellation.StarPairs.Length; i += 2)
        {
            if (i + 1 >= constellation.StarPairs.Length) break; // Safety check

            int hip1 = constellation.StarPairs[i];
            int hip2 = constellation.StarPairs[i + 1];

            if (starMap.TryGetValue(hip1, out GameObject star1) && starMap.TryGetValue(hip2, out GameObject star2))
            {
                GameObject lineObject = DrawLineBetweenStars(star1, star2, constellation.Name);
                lineObject.transform.parent = constellationParent.transform; // Set parent
            }
            else
            {
                Debug.LogWarning($"Constellation {constellation.Name} missing stars: {hip1} or {hip2}");
            }
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
    public Text playtext;

    public Text timeElapsedText; // Assign in Unity Inspector
    private float totalSimulatedYears = 0f; // Total simulated time in year

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

    // This function is called whenever stars are moved.
    public void UpdateTimeElapsed(float timeAdjustment)
    {
        // Assuming each move operation in your simulation represents an advancement of time
        // proportional to the time adjustment factor.
        float yearsPassed = timeAdjustment; // As timeAdjustment is already in years
        totalSimulatedYears += yearsPassed;

        // Update the UI text component to display the total simulated time elapsed
        timeElapsedText.text = $"{totalSimulatedYears:F2}";
    }

    public IEnumerator MoveStarsInBatches()
    {
        int batchSize = 1000; // Adjust based on performance
        float timeAdjustment = 0f;

        if (starParsec)
        {
            timeAdjustment = speedSlider.value * speedScale * timeSpeed; 
        }
        else
        {
            timeAdjustment = speedSlider.value * speedScale * timeSpeed * 3.28084f;
        }

        // Update the time elapsed before moving stars
        UpdateTimeElapsed(timeAdjustment);

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
        EraseAndRedrawConstellations();
    }

    public void ResetStars()
    {
        if (loadedData != null)
        {
            StartCoroutine(ResetStarsInBatches());

            totalSimulatedYears = 0f;
            UpdateTimeElapsed(0f);
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

        playtext.text = "MOVE";
        EraseAndRedrawConstellations();
        //SwapConstellationMaterials();
    }

    public void AdjustTimeSpeed()
    {
        timeSpeed = speedSlider.value;
    }


    public void SwitchUnit()
    {
        if (starParsec)
        {
            ShiftStarsToFeet(); starParsec = false;
        }
        else
        {
            //ShiftStarsToParsecs();
            ResetStars();
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
        playtext.text = "Switch to Parsec";
    }

    public void ShiftStarsToParsecs()
    {
        if (loadedData == null)
        {
            Debug.LogError("Stars data not loaded.");
            return;
        }

        StartCoroutine(ShiftStarsPosition(loadedData, false)); // False for shifting to parsecs
        playtext.text = "Switch to Feet";
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
                        newPosition = new Vector3(starInfo.position[0], starInfo.position[1], starInfo.position[2]) * 3.28084f;
                    }
                    else
                    {
                        // Convert feet position back to parsecs
                        newPosition = new Vector3(starInfo.position[0], starInfo.position[1], starInfo.position[2]) / 3.28084f;
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

}