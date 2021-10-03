/**
 * ! TODO
 * Things to Consider:
 *   - Paralellize NoiseGen
 *   - Paralellize OctaveGen
 */

// FIXME: Remove all the junk headers from Generation namespace, and move to here and construct the underlying settings Objects Correctly :S
//        Ugly IMO, but it's better for clean code... Separates the Unity integration from the implementation.

// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using System;
using nfg.Collections;
using nfg.gfx;
using nfg.Utils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public enum DisplayMode {
    None,
    FlatGreyScale,
    FlatRegion,
    MeshRegion
}

[ExecuteInEditMode]
[SelectionBase]
public class MapGenerator : MonoBehaviour {
    private bool debug = true;

    public const int CHUNK_SIZE = 241;

    #region -- Editor Options

    // Toggle Objects between Simple and Mesh Maps
    [Header("Unity Objects")]
    [SerializeField] private GameObject simpleMap;
    [SerializeField] private GameObject meshMap;

    [Space]
    [Space]

    [Header("Display Options")]
    [SerializeField] private DisplayMode displayMode = DisplayMode.FlatGreyScale;
    // [SerializeField] private bool blendRegions = false;
    [SerializeField] private AnimationCurve regionBlendCurve;
    [SerializeField] private AnimationCurve heightCurve;

    [Space]
    [Space]

    [Header("Advanced Settings")]
    [SerializeField] private SettingsCoherentNoise settingsNoise;
    [SerializeField] private SettingsMeshGenerator settingsMesh;

    [Space]

    public RegionEntryList regionList;

    [Space]

    public bool autoUpdate;

    #endregion

    #region -- Tracking Fields
    //!FIXME Remove public accessor!
    public JobHandleExtended runningJob = default;
    private Renderer meshRenderer;
    private MeshFilter meshFilter;
    private RegionEntryList lastRegionList;
    private int meshLODSkipVertSize;
    private int meshVerticesWidthSize;
    private bool IsRunning { get => JobHandleStatus.AwaitingCompletion != runningJob.Status; }
    private Action pendingAction;
    #endregion

    #region -- Native Containers
    private NativeArray<Vector2> n_octaveOffsets;
    private NativeArray<float> n_noiseMap;
    private NativeArray<Color> n_colorMap;
    private NativeCurve n_regionBlendCurve;
    private NativeCurve n_heightCurve;
    private NativeArray<RegionEntryData> n_regions;
    private NativeArray<Vector3> n_vertices;
    private NativeArray<int> n_triangles;
    private NativeArray<Vector2> n_uvs;
    #endregion

    #region -- MonoBehavior Lifecycle

    void OnEnable() {
        if (debug) Debug.Log("Started MapGen");

        meshRenderer = meshMap.GetComponent<Renderer>();
        meshFilter = meshMap.GetComponent<MeshFilter>();

        UpdateRegions();

        GenerateMapFromEditor();
    }

    private void OnDisable() {
        runningJob.Complete();

        DisposeNativeContainers();

        n_regions.Dispose();
    }

    private void Update() {
        checkJobQueue();
    }

    #endregion

    #region -- Job Queue

    private void SetupNativeContainers() {
        if (n_octaveOffsets.IsCreated) {
            return;
        }

        if (regionList != lastRegionList) {
            UpdateRegions();
        }

        // LOD is simplified 0-6, where 0 is turned into a scale factor of 1
        //     Greater than 1 will be * 2, to give us actual LOD values (1, 2, 6, 8, 10, 12)
        meshLODSkipVertSize = (0 == settingsMesh.LevelOfDetail) ? 1 : settingsMesh.LevelOfDetail * 2;
        // Num Vertices for the current LOD can be calculated as : ((width - 1) / LODIncrement) + 1
        meshVerticesWidthSize = ((CHUNK_SIZE - 1) / meshLODSkipVertSize) + 1;

        // Init NativeContainers
        n_noiseMap = new NativeArray<float>(CHUNK_SIZE * CHUNK_SIZE, Allocator.Persistent);
        n_colorMap = new NativeArray<Color>(CHUNK_SIZE * CHUNK_SIZE, Allocator.Persistent);
        n_regionBlendCurve = new NativeCurve();
        n_heightCurve = new NativeCurve();

        n_vertices = new NativeArray<Vector3>(meshVerticesWidthSize * meshVerticesWidthSize, Allocator.Persistent);
        n_uvs = new NativeArray<Vector2>(meshVerticesWidthSize * meshVerticesWidthSize, Allocator.Persistent);
        n_triangles = new NativeArray<int>((meshVerticesWidthSize - 1) * (meshVerticesWidthSize - 1) * 6, Allocator.Persistent);

        // Build Curve prefetch data
        n_regionBlendCurve.Update(regionBlendCurve, 256);
        n_heightCurve.Update(heightCurve, 256);

        // Offload OctaveOffset generation
        n_octaveOffsets = CoherentNoiseHelper.GenerateOctaveOffsets(settingsNoise.NumOctaves, settingsNoise.Seed, settingsNoise.Offset);
    }

    private void UpdateRegions() {
        if (n_regions.IsCreated) {
            n_regions.Dispose();
        }

        // Build Regions
        n_regions = new NativeArray<RegionEntryData>(regionList.regions.Length, Allocator.Persistent);
        for (int regionIdx = 0; regionIdx < regionList.regions.Length; regionIdx++) {
            n_regions[regionIdx] = regionList.regions[regionIdx].entryData;
        }

        lastRegionList = regionList;
    }

    private void DisposeNativeContainers() {
        if (!n_noiseMap.IsCreated) {
            return;
        }

        n_noiseMap.Dispose();
        n_colorMap.Dispose();
        n_regionBlendCurve.Dispose();
        n_heightCurve.Dispose();
        n_octaveOffsets.Dispose();
        n_vertices.Dispose();
        n_triangles.Dispose();
        n_uvs.Dispose();

        runningJob = new JobHandle();
    }

    public void GenerateMapFromEditor() {
        if (debug) Debug.Log("Kicking Off New Generate");

        ToggleMapDisplays();

        pendingAction = drawEditorEntities;

        startJobQueue();
    }

    private void startJobQueue() {
        SetupNativeContainers();

        // Start new noiseJob
        JobHandle noiseJob = new CreateCoherentNoiseJob() {
            settingsNoise = settingsNoise,
            n_octaveOffsets = n_octaveOffsets,
            n_noiseMap = n_noiseMap
        }.Schedule(runningJob.handle); // only after the previous runningJob

        // Perform the Color Job
        JobHandle colorJob = new CreateColorMapJob() {
            n_noiseMap = n_noiseMap,
            n_regions = n_regions,
            settingsMesh = settingsMesh,
            displayMode = displayMode,
            n_regionBlendCurve = n_regionBlendCurve,
            n_colorMap = n_colorMap
        }.Schedule(noiseJob); // only after the noiseJob

        // Perform the TerrainMesh Job
        JobHandle meshJob =
            new CreateTerrainMeshJob() {
                settings = settingsMesh,
                meshLODSkipVertSize = meshLODSkipVertSize,
                meshVerticesWidthSize = meshVerticesWidthSize,
                n_regions = n_regions,
                heightMap = n_noiseMap,
                n_heightCurve = n_heightCurve,
                n_vertices = n_vertices,
                n_triangles = n_triangles,
                n_uvs = n_uvs
            }.Schedule(colorJob); // only after the colorJob

        runningJob = meshJob;
    }

    public void checkJobQueue() {
        if (!IsRunning && n_octaveOffsets.IsCreated) {
            // Complete the Job, so the scheduler can move on
            runningJob.Complete();

            pendingAction.Invoke();

            DisposeNativeContainers();

            if (debug) Debug.Log("DONE!");
        }
    }

    #endregion

    #region -- Editor Lifecycle/Functionality

    void OnValidate() {
        // Ensure Noise Settings are valid
        settingsNoise.Width = CHUNK_SIZE;
        settingsNoise.Height = CHUNK_SIZE;
        settingsNoise.Lacunarity = Math.Max(settingsNoise.Lacunarity, 0.01f);
        settingsNoise.NumOctaves = Math.Max(settingsNoise.NumOctaves, 0);
        settingsNoise.Seed = Math.Max(settingsNoise.Seed, 1);
        settingsNoise.Seed = Math.Max(settingsNoise.Seed, 1);

        // Ensure Mesh Generator Settings are valid
        settingsMesh.ChunkSize = CHUNK_SIZE;
        settingsMesh.heightMultiplier = Math.Max(settingsMesh.heightMultiplier, 1);
    }

    private void OnRenderObject() {
        // On Editor Updates
        checkJobQueue();
    }

    private void ToggleMapDisplays() {
        switch (displayMode) {
            case DisplayMode.None:
                simpleMap.SetActive(false);
                meshMap.SetActive(false);
                break;

            case DisplayMode.MeshRegion:
                simpleMap.SetActive(false);
                meshMap.SetActive(true);
                break;

            case DisplayMode.FlatGreyScale:
            case DisplayMode.FlatRegion:
            default:
                simpleMap.SetActive(true);
                meshMap.SetActive(false);
                break;
        }
    }

    private void drawEditorEntities() {
        Debug.Log("Drawing from Editor");

        drawEditorTextures();
        drawEditorMesh();
    }

    private void drawEditorTextures() {
        Texture2D texture = TextureHelper.FromColorMap(CHUNK_SIZE, CHUNK_SIZE, n_colorMap.ToArray());

        Renderer textureRenderer = simpleMap.GetComponent<Renderer>();

        // Set Texture on flatMap view
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(CHUNK_SIZE, 1, CHUNK_SIZE);

        // Also set texture on meshRenderer
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    private void drawEditorMesh() {
        Mesh terrainMesh = MeshHelper.CreateMesh(
            n_vertices.ToArray(),
            n_triangles.ToArray(),
            n_uvs.ToArray()
        );

        // Apply Mesh!
        meshFilter.sharedMesh = terrainMesh;
    }

    #endregion

}
