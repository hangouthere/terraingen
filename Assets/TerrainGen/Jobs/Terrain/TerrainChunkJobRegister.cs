using System;

namespace nfg.Unity.TerrainGen {

    public struct TerrainChunkJobRegister {
        public TerrainChunkJobConfig TerrainChunkJobConfig;
        public Action<TerrainData> OnTerrainData;
    }

}