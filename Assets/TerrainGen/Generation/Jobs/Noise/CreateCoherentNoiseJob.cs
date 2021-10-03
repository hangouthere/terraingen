using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateCoherentNoiseJob : IJob {
    [ReadOnly]
    public SettingsCoherentNoise settingsNoise;
    [ReadOnly]
    public NativeArray<Vector2> n_octaveOffsets;

    public NativeArray<float> n_noiseMap;

    public void Execute() {
        float maxNoiseVal = float.MinValue;
        float minNoiseVal = float.MaxValue;

        for (int y = 0; y < settingsNoise.Height; y++) {
            for (int x = 0; x < settingsNoise.Width; x++) {
                // Track through Octave Iterations, but reset per Coord
                float frequency = 1.0f;
                float amplitude = 1.0f;
                float currNoiseVal = 0;

                for (int octaveIdx = 0; octaveIdx < n_octaveOffsets.Length; octaveIdx++) {
                    currNoiseVal += GenerateNoiseValue(x, y, n_octaveOffsets[octaveIdx], frequency, amplitude);

                    amplitude *= settingsNoise.Persistence;
                    frequency *= settingsNoise.Lacunarity;
                }

                // Keep range of min/max for normalizing later...
                minNoiseVal = Math.Min(minNoiseVal, currNoiseVal);
                maxNoiseVal = Math.Max(maxNoiseVal, currNoiseVal);

                n_noiseMap[x + y * settingsNoise.Width] = currNoiseVal;
            }
        }

        NormalizeNoiseMap(minNoiseVal, maxNoiseVal);
    }

    public float GenerateNoiseValue(int x, int y, Vector2 octaveOffset, float frequency, float amplitude) {
        // Sample Offset for Perlin Calculations
        // Center Noise Generation by offsetting by 0.5*dimensionVec2
        // Scale by settings value, as well as include frequency magnification
        Vector2 samplePoint = new Vector2(
           (x - (settingsNoise.Width / 2)) / settingsNoise.Scale * frequency,
           (y - (settingsNoise.Height / 2)) / settingsNoise.Scale * frequency
        );

        // Perlin Noise at the current Ocatve's Offset, and scaled to allow -1 < val < 1
        samplePoint += octaveOffset;
        float perlinValue = Mathf.PerlinNoise(samplePoint.x, samplePoint.y) * 2 - 1;
        // Even though Unity.Mathematics is meant to be faster and safer, it performs way worse
        // IF YOU DECIDE TO USE: Be sure to enable BURST compile! ~850ms -> ~100ms
        // float perlinValue = Unity.Mathematics.noise.cnoise(samplePoint) * 2 - 1;

        // Shrink by the amplitude as well (in theory should be decel as octaves pass)
        return perlinValue * amplitude;
    }

    public void NormalizeNoiseMap(float minNoiseVal, float maxNoiseVal) {
        for (int idx = 0; idx < n_noiseMap.Length; idx++) {

            // Find percentage of value normalized between the min and max values of noise
            n_noiseMap[idx] = Mathf.InverseLerp(minNoiseVal, maxNoiseVal, n_noiseMap[idx] - 0.000001f);
        }
    }
}
