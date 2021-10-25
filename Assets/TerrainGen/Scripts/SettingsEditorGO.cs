// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using nfg.Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        [SerializeField] private Material TerrainMaterial;
        [SerializeField] public bool LiveUpdate;
        [SerializeField] private TerrainJobQueueUpdateChannel JobQueue;

        [Header("Advanced Settings")]
        [SerializeField] private InnerloopBatchCount ParallelLoopBatchCount = InnerloopBatchCount.Count_32;
        [SerializeField] public bool DebugMessaging;
        [SerializeField, Tooltip("Leave Empty to AutoGenerate a GameObject, or supply your own!")]
        private GameObject TerrainChunkGameObject;

        [Header("ScriptableObject Settings")]
        [SerializeField] private TerrainSettingsSO TerrainSaveSettings;

        #endregion

        #region -- Tracking Fields

        private MeshRenderer MeshRenderer;
        private MeshFilter MeshFilter;
        private TerrainJobQueueEntry JobQueueEntry;

        #endregion

        #region -- MonoBehavior Lifecycle

        private void OnEnable() {
            if (DebugMessaging) Debug.Log("Started SettingsEditor Script");

            SetupGameObjects();
        }

        private void OnDestroy() {
            OnDisable();
        }

        private void OnValidate() {
            if (JobQueue) {
                JobQueue.DebugMessaging = DebugMessaging;
            }
        }

        private void OnDisable() {
            if (DebugMessaging) Debug.Log("Stopping SettingsEditor Script");

            DestroyImmediate(TerrainChunkGameObject);

            MeshFilter = null;
            MeshRenderer = null;
        }

        private void SetupGameObjects() {
            if (null == TerrainChunkGameObject) {
                if (DebugMessaging) Debug.Log("Generating Settings Terrain Objects");

                TerrainChunkGameObject = TerrainDecorator.SetupGameObjects(transform, TerrainMaterial);
                TerrainChunkGameObject.name = "Settings nfgTerrain";
            }

            MeshFilter = TerrainChunkGameObject.GetComponent<MeshFilter>();
            MeshRenderer = TerrainChunkGameObject.GetComponent<MeshRenderer>();
        }

        #endregion

        #region -- Editor UI Methods

        public void GenerateTestChunk() {
            TerrainChunkJobRegister jobRegister = new TerrainChunkJobRegister() {
                TerrainChunkJobConfig = new TerrainChunkJobConfig() {
                    TerrainSettings = TerrainSaveSettings,
                    ParallelLoopBatchCount = ParallelLoopBatchCount,
                },
                OnTerrainData = ChunkBuilt
            };

            JobQueueEntry = JobQueue.RequestChunk(jobRegister);
        }

        public void ForceComplete() {
            JobQueueEntry.FinalizeJob();
        }

        private void ChunkBuilt(TerrainData terrainData) {
            // Quick failsafe for async bailout if the GO is deleted/disabled
            if (!MeshRenderer) {
                return;
            }

            TerrainDecorator.ApplyTerrainData(MeshRenderer, MeshFilter, terrainData);
        }

        #endregion

    }

}