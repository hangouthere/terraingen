using System.Collections.Generic;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    internal struct TerrainChunkEntryAssociation {
        public TerrainChunk chunk;
        public TerrainJobQueueEntry queueEntry;
    }

    [SelectionBase]
    public class EndlessTerrainGO : MonoBehaviour {
        [Header("View Settings")]
        [Range(10, 1000)]
        [SerializeField] private float ViewDistance = 300;
        [Range(300, 5000)]
        [SerializeField] private float DestroyBufferDistance = 3000;
        [SerializeField] private Transform viewpoint;

        [Header("Terrain Settings")]
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private TerrainSettingsSO terrainSettingsSO;
        [SerializeField] private TerrainJobQueueUpdateChannel jobQueue;

        [Header("Debug Settings")]
        [SerializeField] private bool DebugMessaging;
        [SerializeField] private bool ViewDistances;
        [SerializeField] private bool ChunkBounds;
        [SerializeField] private bool ShowClosestToChunk;
        [SerializeField] public bool liveUpdate;
        [Range(1f, 5f)]
        [SerializeField] public float liveUpdateSpeed;

        private Transform _transformCache;

        private Dictionary<Vector2, TerrainChunkEntryAssociation> chunkAssocCoordMap = new Dictionary<Vector2, TerrainChunkEntryAssociation>();
        private List<TerrainChunkEntryAssociation> chunkAssocList = new List<TerrainChunkEntryAssociation>();

        private int distToRecalc;
        private int chunkSize;
        private int chunksVisible;
        private int destroyChunkSize;
        private Vector3 lastViewerUpdatePos;

        private void Awake() {
            _transformCache = transform;
        }

        void Start() {
            DebugMessaging = jobQueue.DebugMessaging;
            chunkSize = JobQueueTerrainChunkBuilder.CHUNK_SIZE - 1;
        }

        private void OnValidate() {
            jobQueue.DebugMessaging = DebugMessaging;
        }

        private void OnDestroy() {
            DestroyAllChunks();
        }

        private void OnDisable() {
            DestroyAllChunks();
        }

        public void DestroyAllChunks() {
            for (int assocIdx = chunkAssocList.Count - 1; assocIdx > 0; assocIdx--) {
                DestroyChunk(chunkAssocList[assocIdx]);
            }

            chunkAssocList.Clear();
            chunkAssocCoordMap.Clear();

            lastViewerUpdatePos = new Vector3(int.MaxValue, 0, int.MaxValue);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (0 == chunkAssocList.Count) {
                return;
            }

            if (true == ViewDistances) {
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(viewpoint.position, Vector3.up, ViewDistance);
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(viewpoint.position, Vector3.up, ViewDistance + DestroyBufferDistance);
            }

            foreach (TerrainChunkEntryAssociation chunkAssoc in chunkAssocList) {
                if (true == ShowClosestToChunk) {
                    Vector3 boundsPoint = chunkAssoc.chunk.bounds.ClosestPoint(viewpoint.position);
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(viewpoint.position, boundsPoint);
                }

                if (true == ChunkBounds) {
                    chunkAssoc.chunk.OnDrawGizmos();
                }
            }
        }
#endif

        private void Update() {
            float distFromLastUpdate = (lastViewerUpdatePos - viewpoint.position).magnitude;

            // No need to recalc yet, we haven't traveled very far
            if (distFromLastUpdate < distToRecalc) {
                return;
            }

            ManageChunks();
        }

        private void ManageChunks() {
            Vector2 viewerCoord = new Vector2(
                Mathf.RoundToInt(viewpoint.position.x / chunkSize),
                Mathf.RoundToInt(viewpoint.position.z / chunkSize)
            );

            chunksVisible = Mathf.RoundToInt(ViewDistance / chunkSize);
            distToRecalc = (int)ViewDistance / 9;
            float destroyDist = ViewDistance + DestroyBufferDistance;
            destroyChunkSize = Mathf.RoundToInt(destroyDist / chunkSize);

            CleanPreviousChunks(viewerCoord);
            EnableChunksInRange(viewerCoord);

            lastViewerUpdatePos = viewpoint.position;
        }

        private void EnableChunksInRange(Vector2 viewerCoord) {
            for (int y = -chunksVisible; y <= chunksVisible; y++) {
                for (int x = -chunksVisible; x <= chunksVisible; x++) {
                    Vector2 chunkCoord = viewerCoord + new Vector2(x, y); // viewCoordinate + chunkOffset
                    TerrainChunkEntryAssociation chunkAssoc = GetOrCreateChunkEntryAssoc(chunkCoord);
                    chunkAssoc.chunk.isVisible = true;
                }
            }
        }

        private TerrainChunkEntryAssociation GetOrCreateChunkEntryAssoc(Vector2 chunkCoord) {
            // Check Cache and Update, or instantiate if necessary
            if (chunkAssocCoordMap.ContainsKey(chunkCoord)) {
                return chunkAssocCoordMap[chunkCoord];
            } else {
                return CreateNewChunk(chunkCoord);
            }
        }

        private void CleanPreviousChunks(Vector2 viewerCoord) {
            for (int chunkIdx = chunkAssocList.Count - 1; chunkIdx >= 0; chunkIdx--) {
                TerrainChunkEntryAssociation chunkAssoc = chunkAssocList[chunkIdx];

                int chunkDistance = (int)(chunkAssoc.chunk.Coordinate - viewerCoord).magnitude;

                if (chunkDistance > destroyChunkSize) {
                    DestroyChunk(chunkAssoc);
                }
            }
        }

        private TerrainChunkEntryAssociation CreateNewChunk(Vector2 chunkCoord) {
            // Copy input TSO so we can manipulate the PositionOffset for it's Coordinate, and not modify the original SO
            TerrainSettingsSO customTSO = Instantiate(terrainSettingsSO);
            TerrainChunk newChunk = new TerrainChunk(chunkCoord, chunkSize, _transformCache, terrainMaterial);
            newChunk.SetParent(transform);

            // Assign offset for noise for our chunk coordinates
            customTSO.NoiseSettings.PositionOffset += chunkCoord * chunkSize;

            TerrainJobQueueEntry queueEntry = jobQueue.RequestChunk(new TerrainChunkJobRegister() {
                OnTerrainData = newChunk.DrawTerrain,
                TerrainChunkJobConfig = new TerrainChunkJobConfig() {
                    TerrainSettings = customTSO
                }
            });

            TerrainChunkEntryAssociation chunkAssoc = new TerrainChunkEntryAssociation() {
                chunk = newChunk,
                queueEntry = queueEntry
            };

            chunkAssocCoordMap.Add(chunkCoord, chunkAssoc);
            chunkAssocList.Add(chunkAssoc);

            return chunkAssoc;
        }

        private void DestroyChunk(TerrainChunkEntryAssociation chunkAssoc) {
            jobQueue.TryCancel(chunkAssoc.queueEntry);
            chunkAssoc.chunk.DestroyChunk();
            chunkAssocList.Remove(chunkAssoc);
            chunkAssocCoordMap.Remove(chunkAssoc.chunk.Coordinate);
        }

    }

}
