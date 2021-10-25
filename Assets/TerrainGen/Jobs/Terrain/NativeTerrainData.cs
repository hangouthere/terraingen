using System;

namespace nfg.Unity.TerrainGen {

    public class NativeTerrainData : IDisposable {
        public NativeMapData n_mapData;
        public NativeMeshData n_meshData;

        public bool IsCreated { get => n_mapData.IsCreated; }

        private bool _isDisposed;

        ~NativeTerrainData() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public NativeTerrainData(TerrainSettingsSO terrainSettings) {
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

        public void Dispose(bool disposing) {
            if (_isDisposed) return;

            if (disposing && IsCreated) {
                n_mapData.Dispose();
                n_meshData.Dispose();
                n_mapData = default;
                n_meshData = default;
            }

            _isDisposed = true;
        }

    }

}