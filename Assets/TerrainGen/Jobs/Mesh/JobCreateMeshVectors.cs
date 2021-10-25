using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateMeshVectors : IJob {
        [ReadOnly]
        public int chunkSize;
        [ReadOnly]
        public int meshLODSkipVertSize;

        public NativeArray<Vector3> n_vertices;

        public void Execute() {
            for (var z = 0; z < chunkSize; z += meshLODSkipVertSize) {
                for (var x = 0; x < chunkSize; x += meshLODSkipVertSize) {
                    n_vertices[x + z * chunkSize] = new Vector3(x, 0, z);
                }
            }
        }
    }

}