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

public struct MapData : IDisposable {
    public NativeArray<Vector2> n_vecColors;
    public NativeArray<float> n_heightMap;
    public NativeArray<Color> n_colorMap;

    public bool IsCreated { get => n_vecColors.IsCreated; }

    public MapData(int chunkSize) : this() {
        n_vecColors = new NativeArray<Vector2>(chunkSize * chunkSize, Allocator.Persistent);
        n_heightMap = new NativeArray<float>(chunkSize * chunkSize, Allocator.Persistent);
        n_colorMap = new NativeArray<Color>(chunkSize * chunkSize, Allocator.Persistent);
    }

    public void Dispose() {
        if (!IsCreated) return;

        n_vecColors.Dispose();
        n_heightMap.Dispose();
        n_colorMap.Dispose();
    }
}

public struct MeshData : IDisposable {
    public NativeArray<Vector3> n_vecMesh;
    public NativeArray<int> n_triangles;
    public NativeArray<Vector2> n_uvs;

    public bool IsCreated { get => n_vecMesh.IsCreated; }

    public MeshData(int meshVerticesWidthSize) : this() {
        n_vecMesh = new NativeArray<Vector3>(meshVerticesWidthSize * meshVerticesWidthSize, Allocator.Persistent);
        n_uvs = new NativeArray<Vector2>(meshVerticesWidthSize * meshVerticesWidthSize, Allocator.Persistent);
        n_triangles = new NativeArray<int>((meshVerticesWidthSize - 1) * (meshVerticesWidthSize - 1) * 6, Allocator.Persistent);
    }

    public void Dispose() {
        if (!IsCreated) return;

        n_vecMesh.Dispose();
        n_triangles.Dispose();
        n_uvs.Dispose();
    }
}

[ExecuteInEditMode]
[SelectionBase]
public class MapGenerator : MonoBehaviour {

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
    public bool debugMessaging;

    #endregion

    #region -- Tracking Fields
    //!FIXME Remove public accessor!
    public JobHandleExtended runningJob = default;
    private Renderer meshRenderer;
    private MeshFilter meshFilter;
    private RegionEntryList lastRegionList;
    private int meshLODSkipVertSize;
    private int meshVerticesWidthSize;
    private double startTime;
    private int startFrame;
    private bool IsRunning { get => JobHandleStatus.AwaitingCompletion != runningJob.Status; }
    private Action pendingAction;
    #endregion

    #region -- MonoBehavior Lifecycle

    void OnEnable() {
        if (debugMessaging) Debug.Log("Started MapGen");

        if (IsRunning) OnDisable();

        meshRenderer = meshMap.GetComponent<Renderer>();
        meshFilter = meshMap.GetComponent<MeshFilter>();

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

    #region -- Native Containers
    private NativeArray<Vector2> n_vecOctaveOffsets;
    private NativeArray<RegionEntryData> n_regions;
    private NativeCurve n_regionBlendCurve;
    private NativeCurve n_heightCurve;

    private MapData mapData;
    private MeshData meshData;

    #endregion

    #region -- Job Queue

    private void UpdateRegions() {
        if (regionList != lastRegionList) {
            lastRegionList = regionList;

            if (n_regions.IsCreated) {
                n_regions.Dispose();
            }

            n_regions = new NativeArray<RegionEntryData>(regionList.regions.Length, Allocator.Persistent);

            // Extract RegionEntryData
            for (int regionIdx = 0; regionIdx < n_regions.Length; regionIdx++) {
                n_regions[regionIdx] = regionList.regions[regionIdx].entryData;
            }
        }
    }

    private void SetupNativeContainers() {
        if (n_vecOctaveOffsets.IsCreated) {
            return;
        }

        // LOD is simplified 0-6, where 0 is turned into a scale factor of 1
        //     Greater than 1 will be * 2, to give us actual LOD values (1, 2, 6, 8, 10, 12)
        meshLODSkipVertSize = (0 == settingsMesh.LevelOfDetail) ? 1 : settingsMesh.LevelOfDetail * 2;
        // Num Vertices for the current LOD can be calculated as : ((width - 1) / LODIncrement) + 1
        meshVerticesWidthSize = ((CHUNK_SIZE - 1) / meshLODSkipVertSize) + 1;

        mapData = new MapData(CHUNK_SIZE);
        meshData = new MeshData(meshVerticesWidthSize);

        UpdateRegions();

        // Offload OctaveOffset generation
        n_vecOctaveOffsets = new NativeArray<Vector2>(settingsNoise.NumOctaves, Allocator.Persistent);

        // Init NativeContainers
        n_regionBlendCurve = new NativeCurve();
        n_heightCurve = new NativeCurve();


        // Build Curve prefetch data
        n_regionBlendCurve.Update(regionBlendCurve, 256);
        n_heightCurve.Update(heightCurve, 256);
    }

    private void DisposeNativeContainers() {
        if (!mapData.IsCreated) {
            return;
        }

        mapData.Dispose();
        meshData.Dispose();

        n_regionBlendCurve.Dispose();
        n_heightCurve.Dispose();
        n_vecOctaveOffsets.Dispose();
    }

    private void startJobQueue() {
        int PARALLEL_COUNT = 10;

        startTime = Time.fixedUnscaledTimeAsDouble;
        startFrame = Time.frameCount;

        SetupNativeContainers();

        // Generate Vectors
        JobHandle genColorVecsJobs = new JobCreateColorVectors() {
            chunkSize = CHUNK_SIZE,
            n_vertices = mapData.n_vecColors
        }.Schedule(runningJob.handle); // only after the previous runningJob

        JobHandle genMeshVectors = new JobCreateMeshVectors() {
            chunkSize = meshVerticesWidthSize,
            n_vertices = meshData.n_vecMesh,
            meshLODSkipVertSize = meshLODSkipVertSize
        }.Schedule(runningJob.handle); // only after the previous runningJob

        // Start new octaveGenJob
        JobHandle octaveGenJob = new JobCreateOctaveOffsets() {
            seed = settingsNoise.Seed,
            noiseOffset = settingsNoise.NoiseOffset,
            n_vecOctaveOffsets = n_vecOctaveOffsets,
        }.Schedule(runningJob.handle); // only after the previous runningJob

        JobHandle setupJobs = JobHandle.CombineDependencies(genColorVecsJobs, genMeshVectors, octaveGenJob);

        // Start new noiseJob
        JobHandle noiseJob = new JobCreateCoherentNoise() {
            settingsNoise = settingsNoise,
            n_vecOctaveOffsets = n_vecOctaveOffsets,
            n_noiseMap = mapData.n_heightMap,
            n_vecColors = mapData.n_vecColors
        }.ScheduleParallel(mapData.n_heightMap.Length, PARALLEL_COUNT, setupJobs); // only after the previous runningJob

        // Normalize all points
        JobHandle normalizeNoiseJob = new JobNormalizeNoiseMap() {
            n_noiseMap = mapData.n_heightMap,
        }.Schedule(noiseJob); // only after the previous noiseJob

        // Perform the Color Job
        JobHandle colorJob = new JobCreateColorMap() {
            displayMode = displayMode,
            n_colorMap = mapData.n_colorMap,
            n_noiseMap = mapData.n_heightMap,
            n_regionBlendCurve = n_regionBlendCurve,
            n_regions = n_regions
        }.ScheduleParallel(mapData.n_colorMap.Length, PARALLEL_COUNT, normalizeNoiseJob); // only after the noiseJob

        // Perform the TerrainMesh Job
        JobHandle meshJob = new JobCreateTerrainMesh() {
            settingsMesh = settingsMesh,
            meshLODSkipVertSize = meshLODSkipVertSize,
            meshVerticesWidthSize = meshVerticesWidthSize,
            n_regions = n_regions,
            n_heightMap = mapData.n_heightMap,
            n_heightCurve = n_heightCurve,
            meshData = meshData
        }.Schedule(colorJob); // only after the colorJob

        JobHandle.ScheduleBatchedJobs();

        runningJob = meshJob;
    }

    public void checkJobQueue() {
        if (!IsRunning) {
            // Complete the Job, so the scheduler can move on
            runningJob.Complete();

            pendingAction.Invoke();

            DisposeNativeContainers();

            double now = Time.fixedUnscaledTimeAsDouble;
            int nowFrame = Time.frameCount;
            double elapsed = now - startTime;
            double elapsedFrames = nowFrame - startFrame;

            if (debugMessaging) Debug.Log(
                "Start at: " + startTime.ToString("F6")
                + "\tNow: " + now.ToString("F6")
                + "\n\tDONE in " + elapsed.ToString("F4") + "ms"
                + "\tDONE in " + elapsedFrames + " frames"
            );

            startTime = now;
            startFrame = nowFrame;
        }
    }

    #endregion

    #region -- Editor Lifecycle/Functionality

    void OnValidate() {
        // Ensure Noise Settings are valid
        settingsNoise.Width = CHUNK_SIZE;
        settingsNoise.Height = settingsNoise.Width;
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

    public void GenerateMapFromEditor() {
        ToggleMapDisplays();

        if (DisplayMode.None == displayMode) {
            DisposeNativeContainers();
            return;
        }

        pendingAction = drawEditorEntities;

        startJobQueue();
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
        drawEditorTextures();
        drawEditorMesh();
    }

    private void drawEditorTextures() {
        Texture2D texture = TextureHelper.FromColorMap(CHUNK_SIZE, CHUNK_SIZE, mapData.n_colorMap.ToArray());

        Renderer textureRenderer = simpleMap.GetComponent<Renderer>();

        // Set Texture on flatMap view
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(CHUNK_SIZE, 1, CHUNK_SIZE);

        // Also set texture on meshRenderer
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    private void drawEditorMesh() {
        Mesh terrainMesh = MeshHelper.CreateMesh(meshData);

        // Apply Mesh!
        meshFilter.sharedMesh = terrainMesh;
    }

    #endregion

}
