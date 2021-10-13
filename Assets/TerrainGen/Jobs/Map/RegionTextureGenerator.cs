using nfg.Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public static class RegionTextureGenerator {
        public static Color GenerateColorForHeight(
            NativeArray<RegionEntryData> n_regions,
            NativeCurve regionBlendCurve,
            float heightVal
        ) {
            // Find the correct Region for our current heightVal
            for (int regionIdx = 0; regionIdx < n_regions.Length; regionIdx++) {
                if (heightVal < n_regions[regionIdx].height) {
                    return GenerateRegionColor(n_regions, regionIdx, heightVal, regionBlendCurve);
                }
            }

            return new Color();
        }

        private static Color GenerateRegionColor(
            NativeArray<RegionEntryData> n_regions,
            int regionIdx,
            float heightVal,
            NativeCurve regionBlendCurve
        ) {
            RegionEntryData region = n_regions[regionIdx];

            // No Color Lerping requested, just return the Region color!
            if (false == regionBlendCurve.IsCreated) {
                return region.color;
            }

            // Get Last/Next Region Info
            RegionEntryData nextRegion = (regionIdx + 1 < n_regions.Length) ? n_regions[regionIdx + 1] : region;
            RegionEntryData prevRegion = (regionIdx != 0) ? n_regions[regionIdx - 1] : new RegionEntryData {
                height = 0,
                color = n_regions[0].color
            };

            float regionLength = region.height - prevRegion.height;
            float regionPercent = (heightVal - prevRegion.height) / regionLength;
            float curveVal = regionBlendCurve.Evaluate(regionPercent - 0.0001f);
            return Color.Lerp(prevRegion.color, region.color, curveVal);
        }
    }

}