using nfg.Util;
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
        public NativeArray<Vector2> n_vecColors;
        [ReadOnly]
        public NativeFastNoiseLite fastNoiseGen;
        [ReadOnly]
        public float minimumNoise;
        [ReadOnly]
        public float maximumNoise;

        public NativeArray<float> n_heightMap;

        public void Execute(int index) {
            n_heightMap[index] = GenerateNoiseValue(n_vecColors[index]);
        }

        public float GenerateNoiseValue(Vector2 position) {
            // Sample Offset for Perlin Calculations
            // Center Noise Generation by offsetting by 0.5*dimensionVec2
            // Scale by settings value, as well as include frequency magnification
            Vector2 centerPoint = new Vector2(settingsNoise.Width, settingsNoise.Height) / 2;
            Vector2 samplePoint = (position - centerPoint + settingsNoise.PositionOffset) / settingsNoise.Scale;

            float noiseSample = fastNoiseGen.GetNoise(samplePoint.x, samplePoint.y);

            // Return a Normalized value Lerp'd between a Min/Max value (avoids a resample)
            return Mathf.InverseLerp(minimumNoise, maximumNoise, noiseSample);
        }
    }

}