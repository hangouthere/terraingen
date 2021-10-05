using UnityEngine;

namespace nfg.gfx {

    public static class MeshHelper {
        public static Mesh CreateMesh(MeshData meshData) {
            Mesh mesh = new Mesh();

            mesh.vertices = meshData.n_vecMesh.ToArray();
            mesh.triangles = meshData.n_triangles.ToArray();
            mesh.uv = meshData.n_uvs.ToArray();

            mesh.RecalculateNormals();

            return mesh;
        }
    }

}