using nfg.Collections;
using Unity.Collections;
using UnityEngine;

public static class RegionTextureGenerator {
    public static void GenerateColorMap(NativeArray<RegionEntryData> regions, NativeCurve regionBlendCurve, NativeArray<float> heightMap, NativeArray<Color> colorMap) {
        for (int heightIdx = 0; heightIdx < heightMap.Length; heightIdx++) {
            float heightVal = heightMap[heightIdx];

            for (int regionIdx = 0; regionIdx < regions.Length; regionIdx++) {
                if (heightVal < regions[regionIdx].height) {
                    colorMap[heightIdx] = GenerateTextureColor(regions, regionIdx, heightVal, regionBlendCurve);
                    break;
                }
            }
        }
    }

    private static Color GenerateTextureColor(NativeArray<RegionEntryData> regions, int regionIdx, float heightVal, NativeCurve regionBlendCurve) {
        RegionEntryData region = regions[regionIdx];

        // No Color Lerping requested, just return the Region color!
        if (false == regionBlendCurve.IsCreated) {
            return region.color;
        }

        // Get Last/Next Region Info
        RegionEntryData nextRegion = (regionIdx + 1 < regions.Length) ? regions[regionIdx + 1] : region;
        RegionEntryData prevRegion = (regionIdx != 0) ? regions[regionIdx - 1] : new RegionEntryData {
            height = 0,
            color = regions[0].color
        };

        float regionLength = region.height - prevRegion.height;
        float regionPercent = (heightVal - prevRegion.height) / regionLength;
        float curveVal;

        curveVal = regionBlendCurve.Evaluate(regionPercent);
        return Color.Lerp(prevRegion.color, region.color, curveVal);
    }
}