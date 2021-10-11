using nfg.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct JobCreateColorMap : IJobFor {
    [ReadOnly]
    public DisplayMode displayMode;
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

        switch (displayMode) {
            case DisplayMode.FlatRegion:
            case DisplayMode.MeshRegion:
                color = RegionTextureGenerator.GenerateColorForHeight(
                    n_regions,
                    n_regionBlendCurve,
                    heightVal
                );
                break;

            case DisplayMode.FlatGreyScale:
            default:
                color = Color.Lerp(Color.black, Color.white, heightVal);
                break;
        }

        n_colorMap[index] = color;
    }
}