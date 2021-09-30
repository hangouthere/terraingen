using System;
using UnityEngine;

namespace nfg.gfx {

    [System.Serializable]
    public class CoherentNoiseSettings {
        public Renderer textureRenderer;
        public int Width = 10;
        public int Height = 10;
        public float Scale = 0.0001f;
        [Range(1, 25)]
        public int Octaves = 1;
        public float Lacunarity = 2f;
        // TODO: Consider changing this name to Dampening or Decelerate?
        [Range(0, 1)]
        public float Persistence = 0.5f;
        public Vector2 Offset;
        public int Seed = 10;
    }

    public class CoherentNoise {
        public CoherentNoiseSettings Settings { get; }
        public float[,] Values { get; private set; }

        private Vector2[] octaveOffsets;

        public CoherentNoise(CoherentNoiseSettings settings = null) {
            if (settings == null) {
                settings = new CoherentNoiseSettings();
            }

            Settings = settings;
        }

        protected virtual void GenerateOctaveOffsets() {
            System.Random prng = new System.Random(Settings.Seed);

            octaveOffsets = new Vector2[Settings.Octaves];

            for (int octaveIdx = 0; octaveIdx < Settings.Octaves; octaveIdx++) {
                float offsetX = prng.Next(-100000, 100000) + Settings.Offset.x;
                float offsetY = prng.Next(-100000, 100000) + Settings.Offset.y;

                octaveOffsets[octaveIdx] = new Vector2(offsetX, offsetY);
            }
        }

        public float[,] GenerateMap() {
            GenerateOctaveOffsets();

            Values = new float[Settings.Width, Settings.Height];

            float maxNoiseVal = float.MinValue;
            float minNoiseVal = float.MaxValue;

            for (int y = 0; y < Settings.Height; y++) {
                for (int x = 0; x < Settings.Width; x++) {
                    // Track through Octave Iterations, but reset per Coord
                    float frequency = 1.0f;
                    float amplitude = 1.0f;
                    float currNoiseVal = 0;

                    for (int octaveIdx = 0; octaveIdx < octaveOffsets.Length; octaveIdx++) {
                        currNoiseVal += GenerateNoiseValue(x, y, octaveOffsets[octaveIdx], frequency, amplitude);

                        amplitude *= Settings.Persistence;
                        frequency *= Settings.Lacunarity;
                    }

                    // Keep range of min/max for normalizing later...
                    minNoiseVal = Math.Min(minNoiseVal, currNoiseVal);
                    maxNoiseVal = Math.Max(maxNoiseVal, currNoiseVal);

                    Values[x, y] = currNoiseVal;
                }
            }

            NormalizeNoiseMap(minNoiseVal, maxNoiseVal);

            return Values;
        }

        protected virtual float GenerateNoiseValue(int x, int y, Vector2 octaveOffset, float frequency, float amplitude) {
            // Sample Offset for Perlin Calculations
            // Center Noise Generation by offsetting by 0.5*dimensionVec2
            // Scale by settings value, as well as include frequency magnification
            float sampleX = (x - (Settings.Width / 2)) / Settings.Scale * frequency;
            float sampleY = (y - (Settings.Height / 2)) / Settings.Scale * frequency;

            // Perlin Noise at the current Ocatve's Offset, and scaled to allow -1 < val < 1
            float perlinValue = Mathf.PerlinNoise(octaveOffset.x + sampleX, octaveOffset.y + sampleY) * 2 - 1;

            // Shrink by the amplitude as well (in theory should be decel as octaves pass)
            return perlinValue * amplitude;
        }

        protected void NormalizeNoiseMap(float minNoise, float maxNoise) {
            for (int y = 0; y < Settings.Height; y++) {
                for (int x = 0; x < Settings.Width; x++) {
                    // Find percentage of value normalized between the min and max values of noise
                    Values[x, y] = Mathf.InverseLerp(minNoise, maxNoise, Values[x, y]);
                }
            }
        }
    }

}