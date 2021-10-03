using nfg.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct CreateTerrainMeshJob : IJob {
    [ReadOnly]
    public SettingsMeshGenerator settings;
    [ReadOnly]
    public NativeArray<RegionEntryData> n_regions;
    [ReadOnly]
    public int meshLODSkipVertSize;
    [ReadOnly]
    public int meshVerticesWidthSize;
    [ReadOnly]
    public NativeArray<float> heightMap;
    [ReadOnly]
    public NativeCurve n_heightCurve;

    public NativeArray<Vector3> n_vertices;
    public NativeArray<int> n_triangles;
    public NativeArray<Vector2> n_uvs;

    private int triangleIndex;

    public void Execute() {
        triangleIndex = 0;
        int vertexIndex = 0;
        float chunkCenterOffset = (settings.ChunkSize - 1) / -2f;

        for (int z = 0; z < settings.ChunkSize; z += meshLODSkipVertSize) {
            for (int x = 0; x < settings.ChunkSize; x += meshLODSkipVertSize) {

                // Get Height Val, apply the HeightCurve expression, and scale by HeightMultiplier
                float heightVal = heightMap[x + z * settings.ChunkSize];
                float curvedVal = n_heightCurve.Evaluate(heightVal);
                heightVal *= curvedVal * settings.heightMultiplier;

                // Build current Vertex with newlycal'd heightVal, but offset so the terrain is built from the center
                n_vertices[vertexIndex] = new Vector3(chunkCenterOffset + x, heightVal, -chunkCenterOffset - z);
                n_uvs[vertexIndex] = new Vector2(x / (float)settings.ChunkSize, z / (float)settings.ChunkSize);

                if (x < settings.ChunkSize - 1 && z < settings.ChunkSize - 1) {
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
        n_triangles[triangleIndex] = a;
        n_triangles[triangleIndex + 1] = b;
        n_triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }
}