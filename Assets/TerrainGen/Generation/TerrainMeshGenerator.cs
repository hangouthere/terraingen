using UnityEngine;

[System.Serializable]
public class MeshGeneratorSettings {
    [HideInInspector]
    public int ChunkSize = 241;

    public AnimationCurve heightCurve;
    public float heightMultiplier = 10.0f;

    // Simple representation of LOD 1-6;
    [Range(0, 6)]
    public int LevelOfDetail = 0;

    public Renderer meshRenderer;
    public MeshFilter meshFilter;
    public RegionEntry[] regions;
}

[System.Serializable]
public struct RegionEntry {
    public string label;
    public float height;
    public Color color;
}

public class TerrainMeshGenerator {
    private MeshGeneratorSettings settings;

    public TerrainMeshGenerator(MeshGeneratorSettings settings) {
        this.settings = settings;
    }

    public MeshData FromHeightMap(float[,] heightMap) {
        int vertexIndex = 0;
        float chunkCenterOffset = (settings.ChunkSize - 1) / -2f;

        // LOD is simplified 0-6, where 0 is turned into a scale factor of 1
        //     Greater than 1 will be * 2, to give us actual LOD values (1, 2, 6, 8, 10, 12)
        int meshLODSkipVertSize = (0 == settings.LevelOfDetail) ? 1 : settings.LevelOfDetail * 2;
        // Num Vertices for the current LOD can be calculated as : ((width - 1) / LODIncrement) + 1
        int verticesWidthSize = ((settings.ChunkSize - 1) / meshLODSkipVertSize) + 1;

        MeshData meshData = new MeshData(verticesWidthSize);

        for (int z = 0; z < settings.ChunkSize; z += meshLODSkipVertSize) {
            for (int x = 0; x < settings.ChunkSize; x += meshLODSkipVertSize) {

                // Get Height Val, apply the HeightCurve expression, and scale by HeightMultiplier
                float heightVal = heightMap[x, z];
                float curvedVal = settings.heightCurve.Evaluate(heightVal);
                heightVal *= curvedVal * settings.heightMultiplier;

                // Build current Vertex with newlycal'd heightVal, but offset so the terrain is built from the center
                meshData.vertices[vertexIndex] = new Vector3(chunkCenterOffset + x, heightVal, -chunkCenterOffset - z);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)settings.ChunkSize, z / (float)settings.ChunkSize);

                if (x < settings.ChunkSize - 1 && z < settings.ChunkSize - 1) {
                    meshData.AddQuadFromVertexIndex(vertexIndex);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData {
    public int ChunkSize;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    private int triangleIndex;

    public MeshData(int chunkSize) {
        ChunkSize = chunkSize;

        vertices = new Vector3[chunkSize * chunkSize];
        uvs = new Vector2[chunkSize * chunkSize];
        triangles = new int[(chunkSize - 1) * (chunkSize - 1) * 6];
    }

    // Adds an entire Quad (aka, 2x Triangles) from the current vertexIndex
    public void AddQuadFromVertexIndex(int vertexIndex) {
        AddTriangle(vertexIndex, vertexIndex + ChunkSize + 1, vertexIndex + ChunkSize);
        AddTriangle(vertexIndex + ChunkSize + 1, vertexIndex, vertexIndex + 1);
    }

    // Adds a single Triangle from the given a triple of vertexIndices
    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        return mesh;
    }

}