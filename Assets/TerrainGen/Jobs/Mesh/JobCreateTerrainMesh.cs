using nfg.Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateTerrainMesh : IJob {
        [ReadOnly]
        public SettingsMeshGenerator settingsMesh;
        [ReadOnly]
        public NativeArray<Vector3> n_vecNoise;
        [ReadOnly]
        public NativeCurve n_heightCurve;
        [ReadOnly]
        public int lodVerticeIncrement;
        [ReadOnly]
        public int lodVerticesSize;

        public NativeArray<Vector3> n_vecMesh;
        public NativeArray<int> n_triangles;
        public NativeArray<Vector2> n_uvs;

        private int triangleIndex;

        public void Execute() {
            triangleIndex = 0;
            int vertexIndex = 0;
            float chunkCenterOffset = (settingsMesh.ChunkSize - 1) / -2f;

            for (int z = 0; z < settingsMesh.ChunkSize; z += lodVerticeIncrement) {
                for (int x = 0; x < settingsMesh.ChunkSize; x += lodVerticeIncrement) {

                    // Get Height Val, apply the HeightCurve expression, and scale by HeightMultiplier
                    float heightVal = n_vecNoise[x + z * settingsMesh.ChunkSize].y;
                    float curvedVal = n_heightCurve.Evaluate(heightVal);
                    heightVal *= curvedVal * settingsMesh.heightMultiplier;

                    // Build current Vertex with newlycal'd heightVal, but offset so the terrain is built from the center
                    n_vecMesh[vertexIndex] = new Vector3(chunkCenterOffset + x, heightVal, -chunkCenterOffset - z);
                    n_uvs[vertexIndex] = new Vector2(x / (float)settingsMesh.ChunkSize, z / (float)settingsMesh.ChunkSize);

                    if (x < settingsMesh.ChunkSize - 1 && z < settingsMesh.ChunkSize - 1) {
                        AddQuadFromVertexIndex(vertexIndex);
                    }

                    vertexIndex++;
                }
            }
        }

        // Adds an entire Quad (aka, 2x Triangles) from the current vertexIndex
        public void AddQuadFromVertexIndex(int vertexIndex) {
            AddTriangle(vertexIndex, vertexIndex + lodVerticesSize + 1, vertexIndex + lodVerticesSize);
            AddTriangle(vertexIndex + lodVerticesSize + 1, vertexIndex, vertexIndex + 1);
        }

        // Adds a single Triangle from the given a triple of vertexIndices
        public void AddTriangle(int a, int b, int c) {
            n_triangles[triangleIndex] = a;
            n_triangles[triangleIndex + 1] = b;
            n_triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

}