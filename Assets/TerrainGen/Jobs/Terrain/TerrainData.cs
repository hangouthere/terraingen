using UnityEngine;

public struct MapData {
    public Color[] colorMap;
    public float[] heightMap;

    public MapData(NativeMapData n_mapData) {
        this.colorMap = n_mapData.n_colorMap.ToArray();
        this.heightMap = n_mapData.n_heightMap.ToArray();
    }
}

public struct MeshData {
    public Vector3[] vecMesh;
    public int[] triangles;
    public Vector2[] uvs;

    public MeshData(NativeMeshData n_meshData) {
        this.vecMesh = n_meshData.n_vecMesh.ToArray();
        this.triangles = n_meshData.n_triangles.ToArray();
        this.uvs = n_meshData.n_uvs.ToArray();
    }
}

public struct TerrainData {
    public MapData mapData;
    public MeshData meshData;

    public TerrainData(NativeTerrain n_terrain) {
        this.mapData = new MapData(n_terrain.n_mapData);
        this.meshData = new MeshData(n_terrain.n_meshData);
    }
}
