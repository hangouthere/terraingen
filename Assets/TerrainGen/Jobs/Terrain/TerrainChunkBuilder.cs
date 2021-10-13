// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using System;
using nfg.Unity.Jobs;
using Unity.Jobs;

namespace nfg.Unity.TerrainGen {

    public enum NormalSpace {
        Global,
        Local
    }

    public class TerrainChunkBuilder : IDisposable {

        public const int CHUNK_SIZE = 241;

        #region -- Tracking Fields

        public bool CanComplete { get => jobHandle.CanComplete; }

        private TerrainChunkJobConfig terrainChunkJobConfig;
        private JobHandleExtended jobHandle = default;

        private NativeTerrain n_terrain;

        #endregion

        #region -- Job Queue

        public TerrainChunkBuilder(TerrainChunkJobConfig terrainChunkConfig) {
            this.terrainChunkJobConfig = terrainChunkConfig;

            // Forcibly Override Chunk Sizes
            this.terrainChunkJobConfig.TerrainSettings.NoiseSettings.Width
                = this.terrainChunkJobConfig.TerrainSettings.NoiseSettings.Height
                = this.terrainChunkJobConfig.TerrainSettings.MeshSettings.ChunkSize
                = CHUNK_SIZE;

            GenerateMapData();
        }

        private void GenerateMapData() {
            n_terrain = new NativeTerrain(terrainChunkJobConfig.TerrainSettings);

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
                settingsNoise = terrainChunkJobConfig.TerrainSettings.NoiseSettings,
                // Out
                n_vecOctaveOffsets = n_terrain.n_mapData.n_vecOctaveOffsets,
            }.Schedule(jobHandle.handle); // only after the previous runningJob

            JobHandle setupJobs = JobHandle.CombineDependencies(
                genColorVecsJobs,
                genMeshVectors,
                octaveGenJob
            );

            // Start new noiseJob
            JobHandle noiseJob = new JobCreateCoherentNoise() {
                // In
                settingsNoise = terrainChunkJobConfig.TerrainSettings.NoiseSettings,
                n_vecColors = n_terrain.n_mapData.n_vecColors,
                n_vecOctaveOffsets = n_terrain.n_mapData.n_vecOctaveOffsets,
                // Out
                n_heightMap = n_terrain.n_mapData.n_heightMap
            }.ScheduleParallel(
                n_terrain.n_mapData.n_heightMap.Length,
                (int)terrainChunkJobConfig.ParallelLoopBatchCount,
                setupJobs
            ); // only after the previous setupJobs Combo

            // Normalize all points as a height map
            JobHandle normalizeHeightMapJob = new JobNormalizeHeightMap() {
                // In
                settingsNoise = terrainChunkJobConfig.TerrainSettings.NoiseSettings,
                // Out
                n_noiseMap = n_terrain.n_mapData.n_heightMap,
            }.Schedule(noiseJob); // only after the previous colorJob

            // Perform the Color Job
            JobHandle colorJob = new JobCreateColorMap() {
                // In
                n_heightMap = n_terrain.n_mapData.n_heightMap,
                n_regionBlendCurve = n_terrain.n_mapData.n_regionBlendCurve,
                n_regions = n_terrain.n_mapData.n_regions,
                // Out
                n_colorMap = n_terrain.n_mapData.n_colorMap
            }.ScheduleParallel(
                n_terrain.n_mapData.n_colorMap.Length,
                (int)terrainChunkJobConfig.ParallelLoopBatchCount,
                normalizeHeightMapJob
            ); // only after the noiseJob

            // Perform the TerrainMesh Job
            JobHandle meshJob = new JobCreateTerrainMesh() {
                // In
                settingsMesh = terrainChunkJobConfig.TerrainSettings.MeshSettings,
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

}