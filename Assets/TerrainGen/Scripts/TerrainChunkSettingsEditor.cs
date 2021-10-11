// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using nfg.gfx;
using UnityEngine;

public enum DisplayMode {
    None,
    FlatGreyScale,
    FlatRegion,
    MeshRegion
}

[ExecuteInEditMode]
[SelectionBase]
public class TerrainChunkSettingsEditor : MonoBehaviour {

    #region -- Editor Options

    [Header("Display Options")]
    [SerializeField] private Material terrainMaterial;
    [SerializeField] public bool liveUpdate;
    [SerializeField] private TerrainJobQueueUpdateChannel jobQueue;

    [Header("Advanced Settings")]
    // !FIXME: reimplement this... was annoyed at warning
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

    void OnEnable() {
        if (debugMessaging) Debug.Log("Started TerrainChunkSettingsEditor Script");

        SetupGameObjects();
    }

    private void SetupGameObjects() {
        if (!terrainChunkGameObject) {
            if (debugMessaging) Debug.Log("Generating Base GameObject");

            terrainChunkGameObject = new GameObject("Settings TerrainChunk");
        }

        terrainChunkGameObject.transform.parent = transform;
        meshRenderer = terrainChunkGameObject.GetComponent<MeshRenderer>();
        meshFilter = terrainChunkGameObject.GetComponent<MeshFilter>();

        if (!meshRenderer) {
            if (debugMessaging) Debug.Log("Generating MeshRenderer Component");
            meshRenderer = terrainChunkGameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = terrainMaterial;
        }

        if (!meshFilter) {
            if (debugMessaging) Debug.Log("Generating MeshFilter Component");
            meshFilter = terrainChunkGameObject.AddComponent<MeshFilter>();
        }
    }

    #endregion


    #region -- Editor UI Methods

    public void GenerateTestChunk() {
        startTime = Time.fixedUnscaledTimeAsDouble;
        startFrame = Time.frameCount;

        Vector2[] positions = new Vector2[] { new Vector2(0, 0) };

        jobQueue.RequestChunks(positions, terrainSaveSettings, parallelLoopBatchCount, ChunkBuilt);
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

    private void drawEditorMesh(MeshData meshData) {
        Mesh terrainMesh = MeshHelper.CreateMesh(meshData);

        // Apply Mesh!
        meshFilter.sharedMesh = terrainMesh;
    }

    #endregion

}
