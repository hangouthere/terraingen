using UnityEngine;

namespace nfg.gfx {

    public static class MeshHelper {
        public static Mesh CreateMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs) {
            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateNormals();

            return mesh;
        }
    }

}