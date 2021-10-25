using nfg.UnityEngine;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public static class TerrainDecorator {
        public static GameObject SetupGameObjects(Transform parent, Material terrainMaterial) {
            GameObject terrainGameObj = new GameObject("nfgTerrain");

            // Parent our new GO to whatever we're decorating!
            terrainGameObj.transform.parent = parent;

            MeshDecorator.SetupGameObjects(terrainGameObj, terrainMaterial);

            return terrainGameObj;
        }

        public static void ApplyTerrainData(
            MeshRenderer meshRenderer,
            MeshFilter meshFilter,
            TerrainData terrainData
        ) {
            drawMapTexture(meshRenderer, terrainData.ChunkSize, terrainData.MapData);
            drawTerrainMesh(meshFilter, terrainData.MeshData);
        }

        private static void drawMapTexture(MeshRenderer meshRenderer, int chunkSize, MapData mapData) {
            Texture2D texture = TextureHelper.FromColorMap(chunkSize, chunkSize, mapData.colorMap);

            // Set Texture on Renderer
            if (Application.isPlaying) {
                meshRenderer.material.mainTexture = texture;
            } else {
                meshRenderer.sharedMaterial.mainTexture = texture;
            }
        }

        private static void drawTerrainMesh(MeshFilter meshFilter, TerrainMeshData meshData) {
            Mesh terrainMesh = MeshHelper.CreateMesh(meshData);

            // Apply Mesh!
            meshFilter.mesh = terrainMesh;
        }
    }

}