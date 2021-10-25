using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public class TerrainChunk {
        public readonly GameObject terrainGO;
        public readonly Bounds bounds;

        public Vector2 Coordinate { get; private set; }
        private readonly Vector3 position;

        private bool _isVisible;

        public bool isVisible {
            get => _isVisible;
            set {
                _isVisible = value;
                terrainGO.SetActive(value);
            }
        }

        public TerrainChunk(
            Vector2 chunkCoord,
            int chunkSize,
            Transform parentTransform,
            Material material
        ) {
            this.Coordinate = chunkCoord;
            position = new Vector3(chunkCoord.x, 0, chunkCoord.y) * chunkSize;
            bounds = new Bounds(position, new Vector3(chunkSize, 0, chunkSize));
            terrainGO = TerrainDecorator.SetupGameObjects(parentTransform, material);

            isVisible = false;
        }

        public void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        public void DestroyChunk() {
            Object.Destroy(terrainGO);
        }

        public void DrawTerrain(TerrainData terrainData) {
            if (!terrainGO) return;

            MeshRenderer meshRenderer = terrainGO.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = terrainGO.GetComponent<MeshFilter>();

            terrainGO.transform.position = position;

            TerrainDecorator.ApplyTerrainData(meshRenderer, meshFilter, terrainData);
        }

        public void SetParent(Transform parent) {
            terrainGO.transform.parent = parent;
        }

        public float GetSqrDistanceFrom(Vector3 fromPoint) {
            return bounds.SqrDistance(fromPoint);
        }

    }

}