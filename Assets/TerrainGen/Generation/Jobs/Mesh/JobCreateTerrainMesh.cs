using nfg.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct JobCreateTerrainMesh : IJob {
    [ReadOnly]
    public SettingsMeshGenerator settingsMesh;
    [ReadOnly]
    public NativeArray<RegionEntryData> n_regions;
    [ReadOnly]
    public int meshLODSkipVertSize;
    [ReadOnly]
    public int meshVerticesWidthSize;
    [ReadOnly]
    public NativeArray<float> n_heightMap;
    [ReadOnly]
    public NativeCurve n_heightCurve;

    public MeshData meshData;

    private int triangleIndex;

    public void Execute() {
        triangleIndex = 0;
        int vertexIndex = 0;
        float chunkCenterOffset = (settingsMesh.ChunkSize - 1) / -2f;

        for (int z = 0; z < settingsMesh.ChunkSize; z += meshLODSkipVertSize) {
            for (int x = 0; x < settingsMesh.ChunkSize; x += meshLODSkipVertSize) {

                // Get Height Val, apply the HeightCurve expression, and scale by HeightMultiplier
                float heightVal = n_heightMap[x + z * settingsMesh.ChunkSize];
                float curvedVal = n_heightCurve.Evaluate(heightVal);
                heightVal *= curvedVal * settingsMesh.heightMultiplier;

                // Build current Vertex with newlycal'd heightVal, but offset so the terrain is built from the center
                meshData.n_vecMesh[vertexIndex] = new Vector3(chunkCenterOffset + x, heightVal, -chunkCenterOffset - z);
                meshData.n_uvs[vertexIndex] = new Vector2(x / (float)settingsMesh.ChunkSize, z / (float)settingsMesh.ChunkSize);

                if (x < settingsMesh.ChunkSize - 1 && z < settingsMesh.ChunkSize - 1) {
                    AddQuadFromVertexIndex(vertexIndex);
                }

                vertexIndex++;
            }
        }
    }

    // Adds an entire Quad (aka, 2x Triangles) from the current vertexIndex
    public void AddQuadFromVertexIndex(int vertexIndex) {
        AddTriangle(vertexIndex, vertexIndex + meshVerticesWidthSize + 1, vertexIndex + meshVerticesWidthSize);
        AddTriangle(vertexIndex + meshVerticesWidthSize + 1, vertexIndex, vertexIndex + 1);
    }

    // Adds a single Triangle from the given a triple of vertexIndices
    public void AddTriangle(int a, int b, int c) {
        meshData.n_triangles[triangleIndex] = a;
        meshData.n_triangles[triangleIndex + 1] = b;
        meshData.n_triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }
}