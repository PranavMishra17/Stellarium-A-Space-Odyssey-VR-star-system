using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class StarDataLoader : MonoBehaviour
{
    public class Star
    {
        public float hipparcosNumber;
        public float distanceFromSol;
        public Vector3 position;
        public Color colour;
        public float size;
        public float absoluteMagnitude;
        public float relativeMagnitude;
        public Vector3 velocity;
        public Vector3 originalPosition;

        public Star(float hipparcosNumber, float distanceFromSol, Vector3 position,
                    Color colour, float size, float absoluteMagnitude, float relativeMagnitude, Vector3 velocity)
        {
            this.hipparcosNumber = hipparcosNumber;
            this.distanceFromSol = distanceFromSol;
            this.position = position;
            this.colour = colour;
            this.size = size;
            this.absoluteMagnitude = absoluteMagnitude;
            this.relativeMagnitude = relativeMagnitude;
            this.velocity = velocity;
            this.originalPosition = position;
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
            // OBAFGKM colours from: https://arxiv.org/pdf/2101.06254.pdf
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

        public void CalculateAbsoluteMagnitude(float distanceFromSol, float relativeMagnitude)
        {
            // Implement your absolute magnitude calculation logic based on distanceFromSol and relativeMagnitude
            // For example, a simplistic calculation:
            // Assuming you have a formula like AbsoluteMagnitude = ApparentMagnitude - 5 * log10(Distance / 10 parsecs)
            // You can adjust this formula based on your actual requirements.
            absoluteMagnitude = relativeMagnitude - 5 * Mathf.Log10(distanceFromSol / 10f);
        }

        public void CalculateVelocity(float vx, float vy, float vz)
        {
            // Set the velocity vector based on the provided components
            velocity = new Vector3(vx, vy, vz);
        }
    }

    public List<Star> LoadData(int numberOfStarsToLoad)
    {
        List<Star> stars = new List<Star>();
        const string filename = "cleaned_stardata"; // Updated file name
        TextAsset textAsset = Resources.Load<TextAsset>(filename);

        if (textAsset == null)
        {
            Debug.LogError($"Could not load {filename} from Resources folder.");
            return stars;
        }

        StringReader reader = new StringReader(textAsset.text);
        string headerLine = reader.ReadLine(); // Read and discard the header line

        int validStarCount = 0;
        while (reader.Peek() != -1 && (numberOfStarsToLoad < 0 || validStarCount < numberOfStarsToLoad))
        {
            string[] data = reader.ReadLine().Split(',');

            // Check that we have all needed data
            if (data.Length < 11)
            {
                Debug.LogWarning($"Skipping row with insufficient columns: {string.Join(", ", data)}");
                continue;
            }

            // Parse the necessary data
            if (!float.TryParse(data[0], out float hip) ||
                !float.TryParse(data[1], out float dist) ||
                !float.TryParse(data[2], out float x0) ||
                !float.TryParse(data[3], out float y0) ||
                !float.TryParse(data[4], out float z0) ||
                !float.TryParse(data[5], out float absMag) ||
                !float.TryParse(data[6], out float mag) ||
                !float.TryParse(data[7], out float vx) ||
                !float.TryParse(data[8], out float vy) ||
                !float.TryParse(data[9], out float vz))
            {
                Debug.LogWarning($"Skipping row with invalid numeric data: {string.Join(", ", data)}");
                continue;
            }

            Color starColor = Color.white;

            if (!string.IsNullOrEmpty(data[10]) && data[10].Length > 0)
            {
                starColor = SetColour(data[10][0], data[10].Length > 1 ? data[10][1] : '0');

            }
            else
            {
                Debug.LogWarning($"Skipping row with missing or invalid spectral type: {string.Join(", ", data)}");
                continue;
            }

            // Create a new Star object and add it to the list
            Star star = new Star(hip, dist, new Vector3(x0, y0, z0), starColor, 1f, absMag, mag, new Vector3(vx, vy, vz));
            stars.Add(star);

            validStarCount++;
        }

        return stars;
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
