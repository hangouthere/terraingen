using nfg.Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public static class RegionTextureGenerator {
        public static Color GenerateColorForHeight(
            NativeArray<RegionEntryData> n_regions,
            NativeCurve regionBlendCurve,
            float heightVal,
            float worldPerlinValue,
            float scale
        ) {
            float modifiedHeight;

            // Find the correct Region for our current heightVal
            for (int regionIdx = 0; regionIdx < n_regions.Length; regionIdx++) {
                // We want to use some noise to modify the height for color mapping, that way we can have blended textures
                modifiedHeight = heightVal + (worldPerlinValue / (n_regions[regionIdx].BlendModifier * scale));

                if (n_regions[regionIdx].height >= modifiedHeight) {
                    return GenerateRegionColor(
                        n_regions,
                        regionIdx,
                        modifiedHeight,
                        regionBlendCurve
                    );
                }
            }

            return new Color(255, 0, 255, 100);
        }

        private static Color GenerateRegionColor(
            NativeArray<RegionEntryData> n_regions,
            int regionIdx,
            float heightVal,
            NativeCurve regionBlendCurve
        ) {
            RegionEntryData currRegion = n_regions[regionIdx];

            // No Color Lerping requested, just return the Region color!
            if (false == regionBlendCurve.IsCreated) {
                return currRegion.color;
            }

            // Get Last/Next Region Info
            RegionEntryData nextRegion = (regionIdx + 1 < n_regions.Length) ? n_regions[regionIdx + 1] : currRegion;
            RegionEntryData prevRegion = (regionIdx != 0) ? n_regions[regionIdx - 1] : currRegion;

            float regionLength = currRegion.height - prevRegion.height;
            float regionPercent = (heightVal - prevRegion.height) / regionLength;

            float curveVal = regionBlendCurve.Evaluate(regionPercent);
            return Color.Lerp(prevRegion.color, currRegion.color, curveVal);
        }
    }

}