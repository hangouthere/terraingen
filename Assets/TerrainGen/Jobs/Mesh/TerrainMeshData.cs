using nfg.UnityEngine;

namespace nfg.Unity.TerrainGen {

    public class TerrainMeshData : MeshData {
        public TerrainMeshData(NativeMeshData n_meshData) {
            this.vecMesh = n_meshData.n_vecMesh.ToArray();
            this.triangles = n_meshData.n_triangles.ToArray();
            this.uvs = n_meshData.n_uvs.ToArray();
        }
    }

}