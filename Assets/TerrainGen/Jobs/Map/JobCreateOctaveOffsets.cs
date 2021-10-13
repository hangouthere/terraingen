using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Mathematics = Unity.Mathematics;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateOctaveOffsets : IJob {
        [ReadOnly]
        public SettingsCoherentNoise settingsNoise;

        public NativeArray<Vector2> n_vecOctaveOffsets;

        public void Execute() {
            var prng = new Mathematics.Random((uint)settingsNoise.Seed);

            for (int octaveIdx = 0; octaveIdx < n_vecOctaveOffsets.Length; octaveIdx++) {
                float offsetX = prng.NextInt(-100000, 100000) + settingsNoise.NoiseOffset.x;
                float offsetY = prng.NextInt(-100000, 100000) + settingsNoise.NoiseOffset.y;

                n_vecOctaveOffsets[octaveIdx] = new Vector2(offsetX, offsetY);
            }
        }
    }

}