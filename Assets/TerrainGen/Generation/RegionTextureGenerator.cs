using nfg.gfx;
using UnityEngine;

public class RegionTextureGenerator : TextureGenerator {

    private AnimationCurve regionBlendCurve;
    private RegionEntry[] regions;

    public RegionTextureGenerator(int chunkSize, RegionEntry[] regions, AnimationCurve regionBlendCurve) : base(chunkSize, chunkSize) {
        this.regions = regions;
        this.regionBlendCurve = regionBlendCurve;
    }

    public override Texture2D FromHeightMap(float[,] heightMap) {
        Color[] colorMap = new Color[this.Width * this.Height];

        for (int y = 0; y < this.Height; y++) {
            for (int x = 0; x < this.Width; x++) {
                float heightVal = heightMap[x, y];

                for (int regionIdx = 0; regionIdx < regions.Length; regionIdx++) {

                    if (heightVal < regions[regionIdx].height) {
                        colorMap[x + y * this.Width] = GenerateTextureColor(regionIdx, heightVal);
                        break;
                    }
                }
            }
        }

        return FromColorMap(colorMap);
    }

    private Color GenerateTextureColor(int regionIdx, float heightVal) {
        RegionEntry region = regions[regionIdx];

        // Get Last/Next Region Info
        RegionEntry nextRegion = (regionIdx + 1 < regions.Length) ? regions[regionIdx + 1] : region;
        RegionEntry lastRegion = (regionIdx != 0) ? regions[regionIdx - 1] : new RegionEntry {
            label = "Floor Base Level",
            height = 0,
            color = regions[0].color
        };

        float regionLength = region.height - lastRegion.height;
        float regionPercent = (heightVal - lastRegion.height) / regionLength;
        float lerpPercent, curveVal;


        // Bottom Lerp Range
        if (null != regionBlendCurve) {
            lerpPercent = regionPercent / 0.5f;
            curveVal = regionBlendCurve.Evaluate(lerpPercent);
            return Color.Lerp(lastRegion.color, region.color, curveVal);
            // } else if (null != regionBlendCurve && regionPercent >= 0.5f) {
            //     // Top Lerp Range
            //     lerpPercent = (regionPercent - 0.5f) / 0.5f;
            //     curveVal = regionBlendCurve.Evaluate(lerpPercent);
            //     return Color.Lerp(region.color, nextRegion.color, curveVal);
        } else {
            // Middle of region, let's just use the color!
            return regions[regionIdx].color;
        }
    }
}