using nfg.Collections;
using nfg.gfx;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateColorMapJob : IJob {
    [ReadOnly]
    public DisplayMode displayMode;
    [ReadOnly]
    public SettingsMeshGenerator settingsMesh;
    [ReadOnly]
    public NativeArray<RegionEntryData> n_regions;
    [ReadOnly]
    public NativeArray<float> n_noiseMap;
    [ReadOnly]
    public NativeCurve n_regionBlendCurve;

    public NativeArray<Color> n_colorMap;

    public void Execute() {
        // TextureGen will popuplate the colorMap array internally

        switch (displayMode) {
            case DisplayMode.FlatRegion:
            case DisplayMode.MeshRegion:
                RegionTextureGenerator.GenerateColorMap(
                    n_regions,
                    n_regionBlendCurve,
                    n_noiseMap,
                    n_colorMap
                );
                break;

            case DisplayMode.FlatGreyScale:
            default:
                GreyscaleTextureGenerator.GenerateColorMap(n_noiseMap, n_colorMap);
                break;
        }

    }
}