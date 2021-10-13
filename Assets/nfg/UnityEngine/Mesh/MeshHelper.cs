using nfg.Unity.TerrainGen;
using UnityEngine;

namespace nfg.UnityEngine {

    public static class MeshHelper {
        public static Mesh CreateMesh(MeshData meshData) {
            Mesh mesh = new Mesh();

            mesh.vertices = meshData.vecMesh;
            mesh.triangles = meshData.triangles;
            mesh.uv = meshData.uvs;

            mesh.RecalculateNormals();

            return mesh;
        }
    }

}