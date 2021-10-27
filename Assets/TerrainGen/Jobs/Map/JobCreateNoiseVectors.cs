using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateNoiseVectors : IJob {
        [ReadOnly]
        public int chunkSize;

        public NativeArray<Vector3> n_vertices;

        public void Execute() {
            for (var z = 0; z < chunkSize; z++) {
                for (var x = 0; x < chunkSize; x++) {
                    n_vertices[x + z * chunkSize] = new Vector3(x, 0, z);
                }
            }
        }
    }

}