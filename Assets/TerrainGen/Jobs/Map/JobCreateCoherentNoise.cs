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
        public NativeFastNoiseLite fastNoiseGen;
        [ReadOnly]
        public float minimumNoise;
        [ReadOnly]
        public float maximumNoise;

        public NativeArray<Vector3> n_vecNoise;

        public void Execute(int index) {
            Vector3 vecNoise = n_vecNoise[index] + (Vector3.up * GenerateNoiseValue(n_vecNoise[index]));

            n_vecNoise[index] = vecNoise;
        }

        public float GenerateNoiseValue(Vector3 position) {
            // Sample Offset for Perlin Calculations
            // Center Noise Generation by offsetting by 0.5*dimensionVec2
            // Scale by settings value, as well as include frequency magnification
            Vector3 centerPoint = new Vector3(settingsNoise.Width, 0, settingsNoise.Height) / 2;
            Vector3 samplePoint = (position - centerPoint +
                new Vector3(
                    settingsNoise.PositionOffset.x,
                    0,
                    settingsNoise.PositionOffset.y
                )
            ) / settingsNoise.Scale;

            float noiseSample = fastNoiseGen.GetNoise(samplePoint.x, samplePoint.z);

            // Return a Normalized value Lerp'd between a Min/Max value (avoids a resample)
            return Mathf.InverseLerp(minimumNoise, maximumNoise, noiseSample);
        }
    }

}