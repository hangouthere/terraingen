// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using nfg.Unity.Jobs;
using nfg.UnityEngine;
using UnityEngine;

namespace nfg.Unity.TerrainGen.Demo {

    public enum DisplayMode {
        None,
        FlatGreyScale,
        FlatRegion,
        MeshRegion
    }

    [ExecuteInEditMode]
    [SelectionBase]
    public class SettingsEditorGO : MonoBehaviour {

        #region -- Editor Options

        [Header("Display Options")]
        [SerializeField] private Material terrainMaterial;
        [SerializeField] public bool liveUpdate;
        [SerializeField] private TerrainJobQueueUpdateChannel jobQueue;

        [Header("Advanced Settings")]
        [SerializeField] private InnerloopBatchCount parallelLoopBatchCount = InnerloopBatchCount.Count_32;
        [SerializeField] public bool debugMessaging;
        [SerializeField, Tooltip("Leave Empty to AutoGenerate a GameObject, or supply your own!")]
        private GameObject terrainChunkGameObject;

        [Header("ScriptableObject Settings")]
        [SerializeField] private TerrainSettingsSO terrainSaveSettings;

        #endregion

        #region -- Tracking Fields

        private Renderer meshRenderer;
        private MeshFilter meshFilter;
        private double startTime;
        private int startFrame;

        #endregion

        #region -- MonoBehavior Lifecycle

        private void OnEnable() {
            if (debugMessaging) Debug.Log("Started SettingsEditor Script");

            SetupGameObjects();
        }

        private void OnDisable() {
            if (debugMessaging) Debug.Log("Stopping SettingsEditor Script");

            DestroyImmediate(terrainChunkGameObject);

            meshFilter = null;
            meshRenderer = null;
        }

        private void SetupGameObjects() {
            if (debugMessaging) Debug.Log("Generating Settings Terrain Objects");

            terrainChunkGameObject = TerrainDecorator.SetupGameObjects(transform, terrainMaterial);
            terrainChunkGameObject.name = "Settings nfgTerrain";

            meshFilter = terrainChunkGameObject.GetComponent<MeshFilter>();
            meshRenderer = terrainChunkGameObject.GetComponent<MeshRenderer>();
        }

        #endregion


        #region -- Editor UI Methods

        public void GenerateTestChunk() {
            startTime = Time.fixedUnscaledTimeAsDouble;
            startFrame = Time.frameCount;

            TerrainChunkJobRegister jobRegister = new TerrainChunkJobRegister() {
                TerrainChunkJobConfig = new TerrainChunkJobConfig() {
                    TerrainSettings = terrainSaveSettings,
                    ParallelLoopBatchCount = parallelLoopBatchCount,
                },
                OnTerrainData = ChunkBuilt
            };

            jobQueue.RequestChunks(jobRegister);
        }

        private void ChunkBuilt(TerrainData terrainData) {
            drawEditorTextures(terrainData.mapData);
            drawEditorMesh(terrainData.meshData);

            int nowFrame = Time.frameCount;
            double now = Time.fixedUnscaledTimeAsDouble;
            double elapsed = now - startTime;
            double elapsedFrames = nowFrame - startFrame;

            if (debugMessaging) Debug.Log(
                "\t<b>Start at:</b> " + startTime.ToString("F6")
                + "\t<b>Now:</b> " + now.ToString("F6")
                + "\n\t\tDONE in <color=yellow>" + (elapsed * 1000).ToString("F2") + "ms</color>  "
                + "\tDONE in <color=red>" + elapsedFrames + " frames</color>"
            );
        }

        private void drawEditorTextures(MapData mapData) {
            int chunkSize = terrainSaveSettings.NoiseSettings.Width;

            Texture2D texture = TextureHelper.FromColorMap(chunkSize, chunkSize, mapData.colorMap);

            // Set Texture on flatMap view
            meshRenderer.sharedMaterial.mainTexture = texture;
        }

        private void drawEditorMesh(TerrainMeshData meshData) {
            Mesh terrainMesh = MeshHelper.CreateMesh(meshData);

            // Apply Mesh!
            meshFilter.sharedMesh = terrainMesh;
        }

        #endregion

    }

}