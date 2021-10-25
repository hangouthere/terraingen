using nfg.Unity.Jobs;

namespace nfg.Unity.TerrainGen {

    public struct TerrainChunkJobConfig {
        public TerrainSettingsSO TerrainSettings;
        public InnerloopBatchCount ParallelLoopBatchCount;
    }

}