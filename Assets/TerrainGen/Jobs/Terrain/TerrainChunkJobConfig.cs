using nfg.Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public struct TerrainChunkJobConfig {
        public TerrainSettingsSO TerrainSettings;
        public InnerloopBatchCount ParallelLoopBatchCount;
    }

}