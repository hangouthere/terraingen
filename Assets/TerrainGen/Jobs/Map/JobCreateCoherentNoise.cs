using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateCoherentNoise : IJobFor {
        [ReadOnly]
        public SettingsCoherentNoise settingsNoise;
        [ReadOnly]
        public NativeArray<Vector2> n_vecOctaveOffsets;
        [ReadOnly]
        public NativeArray<Vector2> n_vecColors;

        public NativeArray<float> n_heightMap;

        public void Execute(int index) {
            // Track through Octave Iterations, but reset per Coord
            float frequency = 1.0f;
            float amplitude = 1.0f;
            float currNoiseVal = 0;

            // Iterate all Octaves, generate the final noise value
            for (int octaveIdx = 0; octaveIdx < n_vecOctaveOffsets.Length; octaveIdx++) {
                currNoiseVal += GenerateNoiseValue(n_vecColors[index], n_vecOctaveOffsets[octaveIdx], frequency, amplitude);

                amplitude *= settingsNoise.Persistence;
                frequency *= settingsNoise.Lacunarity;
            }

            n_heightMap[index] = currNoiseVal;
        }

        public float GenerateNoiseValue(Vector2 position, Vector2 octaveOffset, float frequency, float amplitude) {
            // Sample Offset for Perlin Calculations
            // Center Noise Generation by offsetting by 0.5*dimensionVec2
            // Scale by settings value, as well as include frequency magnification
            Vector2 centerPoint = new Vector2(settingsNoise.Width, settingsNoise.Height) / 2;
            Vector2 samplePoint = (position - centerPoint + settingsNoise.PositionOffset) / settingsNoise.Scale * frequency;

            // Perlin Noise at the current Ocatve's Offset, and scaled to allow -1 < val < 1
            samplePoint += octaveOffset;
            float perlinValue = Mathf.PerlinNoise(samplePoint.x, samplePoint.y) * 2 - 1;
            // Even though Unity.Mathematics is meant to be faster and safer, it performs way worse
            // IF YOU DECIDE TO USE: Be sure to enable BURST compile!
            // float perlinValue = Unity.Mathematics.noise.cnoise(samplePoint) * 2 - 1;

            // Shrink by the amplitude as well (in theory should be decel as octaves pass)
            return perlinValue * amplitude;
        }
    }

}