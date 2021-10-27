using System;
using nfg.Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public struct NativeMapData : IDisposable {
        public NativeArray<Vector3> n_vecNoise;
        public NativeArray<Color> n_colorMap;
        public NativeArray<RegionEntryData> n_regions;
        public NativeCurve n_regionBlendCurve;

        public bool IsCreated { get => n_vecNoise.IsCreated; }

        public NativeMapData(SettingsCoherentNoise settingsNoise, RegionEntry[] regions, AnimationCurve regionBlendCurve) : this() {
            n_vecNoise = new NativeArray<Vector3>(settingsNoise.Width * settingsNoise.Width, Allocator.Persistent);
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

            n_vecNoise.Dispose();
            n_colorMap.Dispose();
            n_regions.Dispose();
            n_regionBlendCurve.Dispose();
        }
    }

}