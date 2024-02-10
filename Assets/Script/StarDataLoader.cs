using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class StarDataLoader : MonoBehaviour
{
    public class Star
    {
        // Variables to define the star in the game.
        public float hipparcosNumber;
        public float distanceFromSol;
        public Vector3 position;
        public Color colour;
        public float size;
        public float absoluteMagnitude;
        public float relativeMagnitude;
        public Vector3 velocity;

        // Constructor for the new dataset
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

    public List<Star> LoadData()
    {
        List<Star> stars = new List<Star>();
        const string filename = "athyg"; // Change this to your new dataset file name
        TextAsset textAsset = Resources.Load(filename) as TextAsset;

        if (textAsset == null)
        {
            Debug.LogError($"Could not load {filename} from Resources folder.");
            return stars;
        }

        StringReader reader = new StringReader(textAsset.text);

        // Read and discard the header line if it exists
        string headerLine = reader.ReadLine();

        int validStarCount = 0;
        while (reader.Peek() != -1 && validStarCount < 100)
        {
            string[] data = reader.ReadLine().Split(',');

            if (data.Length < 31) // Ensure the row has enough columns
            {
                Debug.LogWarning($"Skipping row with insufficient columns: {string.Join(", ", data)}");
                continue;
            }

            if (!float.TryParse(data[15], out float distanceFromSol) ||
                !float.TryParse(data[16], out float x0) ||
                !float.TryParse(data[17], out float y0) ||
                !float.TryParse(data[18], out float z0) ||
                !float.TryParse(data[20], out float magnitude))
            {
                Debug.LogWarning($"Skipping row with invalid numeric data: {string.Join(", ", data)}");
                continue;
            }

            // Additional columns for the new dataset
            Star star = new Star(0, distanceFromSol, new Vector3(x0, y0, z0), Color.white, 1f, 0f, 0f, Vector3.zero);

            if (data[30].Length > 0)
            {
                star.colour = star.SetColour((byte)data[30][0], 0);
            }
            else
            {
                Debug.LogWarning($"Skipping row with empty spectral type: {string.Join(", ", data)}");
                continue;
            }

            star.size = star.SetSize((short)magnitude);
            star.CalculateAbsoluteMagnitude(distanceFromSol, magnitude);

            stars.Add(star);
            validStarCount++;
        }

        return stars;
    }


}
