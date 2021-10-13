using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobNormalizeHeightMap : IJob {
        [ReadOnly]
        public SettingsCoherentNoise settingsNoise;

        public NativeArray<float> n_noiseMap;

        private float minNoiseVal;
        private float maxNoiseVal;

        public void Execute() {
            if (NormalSpace.Local == settingsNoise.NormalSpace) {
                calcLocalExtremes();
            } else {
                calcGlobalExtremes();
            }
        }

        private void calcLocalExtremes() {
            minNoiseVal = float.MaxValue;
            maxNoiseVal = float.MinValue;

            // Keep range of min/maxing...
            for (int idx = 0; idx < n_noiseMap.Length; idx++) {
                minNoiseVal = Math.Min(minNoiseVal, n_noiseMap[idx]);
                maxNoiseVal = Math.Max(maxNoiseVal, n_noiseMap[idx]);
            }

            // Find percentage of value normalized between the min and max values
            // of noise and normalize value
            for (int idx = 0; idx < n_noiseMap.Length; idx++) {
                n_noiseMap[idx] = Mathf.InverseLerp(minNoiseVal, maxNoiseVal, n_noiseMap[idx] - 0.000001f);
                // n_noiseMap[idx] = Mathf.InverseLerp(minNoiseVal, maxNoiseVal, n_noiseMap[idx]);
            }
        }

        private void calcGlobalExtremes() {
            float normalizedHeight;
            float maxHeight = 1f;
            float amplitude = 1;

            for (int octave = 0; octave < settingsNoise.NumOctaves; octave++) {
                maxHeight += amplitude;
                amplitude *= settingsNoise.Persistence;
            }

            for (int idx = 0; idx < n_noiseMap.Length; idx++) {
                // normalizedHeight = (n_noiseMap[idx] + 1.5f) / (maxHeight * 0.825f);
                normalizedHeight = Mathf.InverseLerp(-maxHeight / 2.5f, maxHeight / 2.75f, n_noiseMap[idx]);
                n_noiseMap[idx] = Mathf.Clamp01(normalizedHeight);
            }
        }
    }

}