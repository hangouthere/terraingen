using System;

namespace nfg.Unity.TerrainGen {

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

}