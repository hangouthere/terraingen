using System;

//FIXME: Document this better, but this should only be created in the main thread, since it manages TerrainSettingsSO
// Alternatively, consider a bridge class to generate the native regions externally rather than delegated deeper

public class NativeTerrain : IDisposable {
    public NativeMapData n_mapData;
    public NativeMeshData n_meshData;

    public bool IsCreated { get => n_mapData.IsCreated; }

    public NativeTerrain(TerrainSettingsSO terrainSettings) {
        n_mapData = new NativeMapData(
            terrainSettings.NoiseSettings,
            terrainSettings.regions,
            terrainSettings.regionBlendCurve
        );

        n_meshData = new NativeMeshData(
            terrainSettings.MeshSettings,
            terrainSettings.heightCurve
        );
    }

    public void Dispose() {
        if (!IsCreated) return;

        n_mapData.Dispose();
        n_meshData.Dispose();
    }
}
