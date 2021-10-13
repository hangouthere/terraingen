using nfg.Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateColorMap : IJobFor {
        [ReadOnly]
        public NativeArray<float> n_heightMap;
        [ReadOnly]
        public NativeArray<RegionEntryData> n_regions;
        [ReadOnly]
        public NativeCurve n_regionBlendCurve;

        public NativeArray<Color> n_colorMap;

        public void Execute(int index) {
            Color color;
            float heightVal = n_heightMap[index];

            color = RegionTextureGenerator.GenerateColorForHeight(
                n_regions,
                n_regionBlendCurve,
                heightVal
            );
            // color = Color.Lerp(Color.black, Color.white, heightVal);

            n_colorMap[index] = color;
        }
    }

}