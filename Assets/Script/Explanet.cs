using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Explanet : MonoBehaviour
{
    public class Planet
    {
        public float hipparcosNumber;
        public float distanceFromSol;
        public Vector3 position;
        public Color colour;
        public float size;
        public Vector3 originalPosition;

        public Vector3 originalfeetPosition;


        public Planet(float hipparcosNumber, float distanceFromSol, Vector3 position, Color colour, float size)
        {
            this.hipparcosNumber = hipparcosNumber;
            this.distanceFromSol = distanceFromSol;
            this.position = position;
            this.colour = colour;
            this.size = size;
            this.originalPosition = position;
            this.originalfeetPosition = originalPosition * 3.28084f;
        }

        // Get the starting position shown in the file.
        public Vector3 GetBasePosition()
        {
            // Return as float
            return position;
        }

        public Color SetColour(byte spectral_type, byte spectral_index)
        {
            Color IntColour(int r, int g, int b)
            {
                return new Color(r / 255f, g / 255f, b / 255f);
            }

            Color[] col = new Color[8];
            col[0] = IntColour(0x5c, 0x7c, 0xff); // O1
            col[1] = IntColour(0x5d, 0x7e, 0xff); // B0.5
            col[2] = IntColour(0x79, 0x96, 0xff); // A0
            col[3] = IntColour(0xb8, 0xc5, 0xff); // F0
            col[4] = IntColour(0xff, 0xef, 0xed); // G1
            col[5] = IntColour(0xff, 0xde, 0xc0); // K0
            col[6] = IntColour(0xff, 0xa2, 0x5a); // M0
            col[7] = IntColour(0xff, 0x7d, 0x24); // M9.5

            int col_idx = -1;
            if (spectral_type == 'O')
            {
                col_idx = 0;
            }
            else if (spectral_type == 'B')
            {
                col_idx = 1;
            }
            else if (spectral_type == 'A')
            {
                col_idx = 2;
            }
            else if (spectral_type == 'F')
            {
                col_idx = 3;
            }
            else if (spectral_type == 'G')
            {
                col_idx = 4;
            }
            else if (spectral_type == 'K')
            {
                col_idx = 5;
            }
            else if (spectral_type == 'M')
            {
                col_idx = 6;
            }

            // If unknown, make white.
            if (col_idx == -1)
            {
                return Color.white;
            }

            // Map second part 0 -> 0, 10 -> 100
            float percent = (spectral_index - 0x30) / 10.0f;
            return Color.Lerp(col[col_idx], col[col_idx + 1], percent);
        }

        public float SetSize(short magnitude)
        {
            // Linear isn't factually accurate, but the effect is sufficient.
            return 1 - Mathf.InverseLerp(-146, 796, magnitude);
        }

    }

    public List<Planet> LoadData(int numberOfPlanetsToLoad)
    {
        List<Planet> planets = new List<Planet>();

        const string filename = "ex_planet"; // Updated file name

        TextAsset textAsset = Resources.Load<TextAsset>(filename);

        if (textAsset == null)
        {
            Debug.LogError($"Could not load {filename} from Resources folder.");
            return planets;
        }

        StringReader reader = new StringReader(textAsset.text);
        string headerLine = reader.ReadLine(); // Read and discard the header line

        int validPlanetCount = 0;
        while (reader.Peek() != -1 && (numberOfPlanetsToLoad < 0 || validPlanetCount < numberOfPlanetsToLoad))
        {
            string[] data = reader.ReadLine().Split(',');

            // Parse the necessary data
            if (//!float.TryParse(data[0], out string name) ||
                !float.TryParse(data[3], out float dist) ||
                !float.TryParse(data[4], out float x0) ||
                !float.TryParse(data[5], out float y0) ||
                !float.TryParse(data[6], out float z0) ||
                !float.TryParse(data[7], out float size))
            {
                Debug.LogWarning($"Skipping row with invalid essential numeric data: {string.Join(", ", data)}");
                continue;
            }

            Color starColor = !string.IsNullOrEmpty(data[8]) ? SetColour(data[8][0], data[8].Length > 1 ? data[8][1] : '0') : Color.white; // Default to white

            // Create a new Star object and add it to the list
            Planet planet = new Planet(validPlanetCount, dist, new Vector3(x0, y0, z0), starColor, size);
            planets.Add(planet);

            validPlanetCount++;
        }

        return planets;
    }


    private Color SetColour(char spectralType, char specIndex)
    {
        switch (spectralType)
        {
            case 'O':
                return new Color(0.5f, 0.5f, 1.0f); // Blue
            case 'B':
                return new Color(0.7f, 0.7f, 1.0f); // Blue-white
            case 'A':
                return Color.white; // White
            case 'F':
                return new Color(1.0f, 1.0f, 0.8f); // White (slightly yellowish)
            case 'G':
                return new Color(1.0f, 1.0f, 0.5f); // Yellow-white
            case 'K':
                return new Color(1.0f, 0.8f, 0.5f); // Orange
            case 'M':
                return new Color(1.0f, 0.5f, 0.5f); // Red
            default:
                return Color.white; // Default color
        }
    }
}
