using System;
using nfg.Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public struct NativeMapData : IDisposable {
        public NativeArray<Vector2> n_vecColors;
        public NativeArray<float> n_heightMap;
        public NativeArray<Color> n_colorMap;
        public NativeArray<RegionEntryData> n_regions;
        public NativeCurve n_regionBlendCurve;

        public bool IsCreated { get => n_vecColors.IsCreated; }

        public NativeMapData(SettingsCoherentNoise settingsNoise, RegionEntry[] regions, AnimationCurve regionBlendCurve) : this() {
            n_vecColors = new NativeArray<Vector2>(settingsNoise.Width * settingsNoise.Width, Allocator.Persistent);
            n_heightMap = new NativeArray<float>(settingsNoise.Width * settingsNoise.Width, Allocator.Persistent);
            n_colorMap = new NativeArray<Color>(settingsNoise.Width * settingsNoise.Width, Allocator.Persistent);
            n_regions = new NativeArray<RegionEntryData>(regions.Length, Allocator.Persistent);
            n_regionBlendCurve = new NativeCurve();

            FlattenRegionList(regions);
            n_regionBlendCurve.Update(regionBlendCurve, 256);
        }

        private void FlattenRegionList(RegionEntry[] regions) {
            // Extract RegionEntryData
            for (int regionIdx = 0; regionIdx < n_regions.Length; regionIdx++) {
                n_regions[regionIdx] = regions[regionIdx].entryData;
            }
        }

        public void Dispose() {
            if (!IsCreated) return;

            n_vecColors.Dispose();
            n_heightMap.Dispose();
            n_colorMap.Dispose();
            n_regions.Dispose();
            n_regionBlendCurve.Dispose();
        }
    }

}