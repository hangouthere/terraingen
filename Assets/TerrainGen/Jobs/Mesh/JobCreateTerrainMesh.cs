using nfg.Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [BurstCompile]
    public struct JobCreateTerrainMesh : IJobFor {
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
        [NativeDisableParallelForRestriction]
        public NativeArray<int> n_triangles;
        public NativeArray<Vector2> n_uvs;

        private int triangleIndex;

        public void Execute(int index) {
            // Not an LOD vertex, so we don't really care about processing it
            if (0 != index % lodVerticeIncrement) {
                return;
            }

            Vector3 origVecMesh = n_vecMesh[index];

            // Since the last index of a row isn't added as a Quad (it's the literal edge, there's no quad to add),
            // we want to reduce the Triangle Index counter by the rowNumber to track properly
            int rowNum = index / lodVerticesSize;
            // Our Index is divisible by the LOD vertex jump size, minus the row count, and 6 vertices per quad
            triangleIndex = ((index / lodVerticeIncrement) - rowNum) * 6;

            float chunkCenterOffset = (settingsMesh.ChunkSize - 1) / -2f;

            // Get Height Val, apply the HeightCurve expression, and scale by HeightMultiplier
            float heightVal = n_vecNoise[index].y;
            float curvedVal = n_heightCurve.Evaluate(heightVal);
            heightVal *= curvedVal * settingsMesh.heightMultiplier;

            // Rebuild current Vertex with newly calculated heightVal, but offset so the terrain is built from the center of chunk
            n_vecMesh[index] = new Vector3(chunkCenterOffset + origVecMesh.x, heightVal, -chunkCenterOffset - origVecMesh.z);
            // Set the UV for the current index, which is the coordinate's position as a percentage of the overall chunksize
            n_uvs[index] = new Vector2(origVecMesh.x / (float)settingsMesh.ChunkSize, origVecMesh.z / (float)settingsMesh.ChunkSize);

            // Related to the `rowNum` information above, we only add a Quad if there are more vertices
            // to potentially add (ie, not the far border vertices)
            if (origVecMesh.x < settingsMesh.ChunkSize - 1 && origVecMesh.z < settingsMesh.ChunkSize - 1) {
                AddQuadFromVertexIndex(index);
            }
        }

        // Adds an entire Quad (aka, 2x Triangles) from the current vertexIndex
        private void AddQuadFromVertexIndex(int vertexIndex) {
            AddTriangle(vertexIndex, vertexIndex + lodVerticesSize + 1, vertexIndex + lodVerticesSize);
            AddTriangle(vertexIndex + lodVerticesSize + 1, vertexIndex, vertexIndex + 1);
        }

        // Adds a single Triangle from the given a triple of vertexIndices
        private void AddTriangle(int a, int b, int c) {
            n_triangles[triangleIndex] = a;
            n_triangles[triangleIndex + 1] = b;
            n_triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

}