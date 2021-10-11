// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using System;
using nfg.Jobs;
using Unity.Jobs;

public enum InnerloopBatchCount {
    Count_1 = 1,
    Count_8 = 8,
    Count_16 = 16,
    Count_32 = 32,
    Count_64 = 64,
    Count_128 = 128
}

public enum NormalSpace {
    Global,
    Local
}

public class TerrainChunkBuilder : IDisposable {

    public const int CHUNK_SIZE = 241;

    #region -- Tracking Fields

    public bool CanComplete { get => jobHandle.CanComplete; }

    private TerrainSettingsSO terrainSettings;
    private JobHandleExtended jobHandle = default;

    private NativeTerrain n_terrain;

    #endregion

    #region -- Job Queue

    public TerrainChunkBuilder(TerrainSettingsSO terrainSettings, InnerloopBatchCount parallelLoopBatchCount) {
        this.terrainSettings = terrainSettings;

        // Forcibly Override Chunk Sizes
        terrainSettings.NoiseSettings.Width
            = terrainSettings.NoiseSettings.Height
            = terrainSettings.MeshSettings.ChunkSize
            = CHUNK_SIZE;

        GenerateMapData(parallelLoopBatchCount);
    }

    private void GenerateMapData(InnerloopBatchCount parallelLoopBatchCount) {
        n_terrain = new NativeTerrain(terrainSettings);

        // Generate Color Vectors
        JobHandle genColorVecsJobs = new JobCreateColorVectors() {
            // In
            chunkSize = CHUNK_SIZE,
            // Out
            n_vertices = n_terrain.n_mapData.n_vecColors
        }.Schedule(jobHandle.handle); // only after the previous runningJob

        // Generate Mesh Vectors
        JobHandle genMeshVectors = new JobCreateMeshVectors() {
            // In
            chunkSize = n_terrain.n_meshData.lodVerticesSize,
            meshLODSkipVertSize = n_terrain.n_meshData.lodVerticeIncrement,
            // Out
            n_vertices = n_terrain.n_meshData.n_vecMesh,
        }.Schedule(jobHandle.handle); // only after the previous runningJob

        // Start new octaveGenJob
        JobHandle octaveGenJob = new JobCreateOctaveOffsets() {
            // In
            settingsNoise = terrainSettings.NoiseSettings,
            // Out
            n_vecOctaveOffsets = n_terrain.n_mapData.n_vecOctaveOffsets,
        }.Schedule(jobHandle.handle); // only after the previous runningJob

        JobHandle setupJobs = JobHandle.CombineDependencies(genColorVecsJobs, genMeshVectors, octaveGenJob);

        // Start new noiseJob
        JobHandle noiseJob = new JobCreateCoherentNoise() {
            // In
            settingsNoise = terrainSettings.NoiseSettings,
            n_vecColors = n_terrain.n_mapData.n_vecColors,
            n_vecOctaveOffsets = n_terrain.n_mapData.n_vecOctaveOffsets,
            // Out
            n_heightMap = n_terrain.n_mapData.n_heightMap
        }.ScheduleParallel(n_terrain.n_mapData.n_heightMap.Length, (int)parallelLoopBatchCount, setupJobs); // only after the previous setupJobs Combo

        // Normalize all points as a height map
        JobHandle normalizeHeightMapJob = new JobNormalizeHeightMap() {
            // In
            settingsNoise = terrainSettings.NoiseSettings,
            // Out
            n_noiseMap = n_terrain.n_mapData.n_heightMap,
        }.Schedule(noiseJob); // only after the previous colorJob

        // Perform the Color Job
        JobHandle colorJob = new JobCreateColorMap() {
            // In
            displayMode = DisplayMode.MeshRegion,
            n_heightMap = n_terrain.n_mapData.n_heightMap,
            n_regionBlendCurve = n_terrain.n_mapData.n_regionBlendCurve,
            n_regions = n_terrain.n_mapData.n_regions,
            // Out
            n_colorMap = n_terrain.n_mapData.n_colorMap
        }.ScheduleParallel(n_terrain.n_mapData.n_colorMap.Length, (int)parallelLoopBatchCount, normalizeHeightMapJob); // only after the noiseJob

        // Perform the TerrainMesh Job
        JobHandle meshJob = new JobCreateTerrainMesh() {
            // In
            settingsMesh = terrainSettings.MeshSettings,
            n_heightMap = n_terrain.n_mapData.n_heightMap,
            lodVerticeIncrement = n_terrain.n_meshData.lodVerticeIncrement,
            lodVerticesSize = n_terrain.n_meshData.lodVerticesSize,
            n_heightCurve = n_terrain.n_meshData.n_heightCurve,
            // Out
            n_triangles = n_terrain.n_meshData.n_triangles,
            n_uvs = n_terrain.n_meshData.n_uvs,
            n_vecMesh = n_terrain.n_meshData.n_vecMesh
        }.Schedule(colorJob); // only after the normalizeHeightMapJob

        // Kick off Scheduler
        JobHandle.ScheduleBatchedJobs();

        // Keep reference of the jobQueue to tack other jobs onto
        // (since we can't cancel jobs, and don't want concurrent
        // jobs accessing the same NativeContainers)
        jobHandle = meshJob;
    }

    public void Complete() {
        jobHandle.Complete();
    }

    public void ForceQueueCompletion() {
        jobHandle.Complete();
        Dispose();
    }

    public TerrainData ToTerrainData() {
        return new TerrainData(n_terrain);
    }

    public void Dispose() {
        n_terrain.Dispose();
        n_terrain = null;
    }

    #endregion

}
