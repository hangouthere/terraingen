using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateColorVectors : IJob {
        [ReadOnly]
        public int chunkSize;

        public NativeArray<Vector2> n_vertices;

        public void Execute() {
            for (var y = 0; y < chunkSize; y++) {
                for (var x = 0; x < chunkSize; x++) {
                    n_vertices[x + y * chunkSize] = new Vector2(x, y);
                }
            }
        }
    }

}