// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using System;
using nfg.Unity.Jobs;
using nfg.Util;
using Unity.Jobs;

namespace nfg.Unity.TerrainGen {

    public enum NormalSpace {
        Global,
        Local
    }

    public class JobQueueTerrainChunkBuilder : IDisposable {

        public const int CHUNK_SIZE = 241;

        #region -- Tracking Fields

        public bool CanComplete { get => jobHandle.CanComplete; }

        private TerrainChunkJobConfig terrainChunkJobConfig;
        private JobHandleExtended jobHandle = default;

        private NativeTerrainData n_terrain;

        private NativeFastNoiseLiteContainerHydrator fastNoiseHydrator;
        private NativeFastNoiseLite fastNoiseGen;
        private float maximumNoise;
        private float minimumNoise;
        private bool _isDisposed;

        #endregion

        #region -- Dispose Pattern

        ~JobQueueTerrainChunkBuilder() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_isDisposed) {
                return;
            }

            if (disposing) {
                n_terrain.Dispose();
                fastNoiseHydrator.Dispose();
                n_terrain = null;
                fastNoiseHydrator = null;
            }

            _isDisposed = true;
        }

        #endregion

        #region -- Job Queue

        public JobQueueTerrainChunkBuilder(TerrainChunkJobConfig terrainChunkConfig) {
            this.terrainChunkJobConfig = terrainChunkConfig;

            // Forcibly Override Chunk Sizes
            terrainChunkConfig.TerrainSettings.NoiseSettings.Width
                = terrainChunkConfig.TerrainSettings.NoiseSettings.Height
                = terrainChunkConfig.TerrainSettings.MeshSettings.ChunkSize
                = CHUNK_SIZE;

            // Negate Y offset as it translates to the actual Z offset when
            // translated to 3D mesh, and Unity has Z-positive
            terrainChunkConfig.TerrainSettings.NoiseSettings.PositionOffset.y *= -1;

            SetupFastNoiseGen();
            GenerateMapData();
        }

        private void SetupFastNoiseGen() {
            SettingsCoherentNoise noiseSettings = terrainChunkJobConfig.TerrainSettings.NoiseSettings;

            fastNoiseHydrator = new NativeFastNoiseLiteContainerHydrator();
            fastNoiseGen = new NativeFastNoiseLite(fastNoiseHydrator.Containers);

            fastNoiseGen.SetFractalType(FractalType.FBm);
            fastNoiseGen.SetNoiseType(NoiseType.Perlin);
            fastNoiseGen.SetSeed(noiseSettings.Seed);
            fastNoiseGen.SetFractalOctaves(noiseSettings.NumOctaves);
            fastNoiseGen.SetFractalLacunarity(noiseSettings.Lacunarity);
            fastNoiseGen.SetFractalGain(noiseSettings.Persistence);

            maximumNoise = 1f;
            float amplitude = 1;

            for (int octave = 0; octave < noiseSettings.NumOctaves; octave++) {
                maximumNoise += amplitude;
                amplitude *= noiseSettings.Persistence;
            }

            minimumNoise = -maximumNoise / noiseSettings.FloorModifier;
            maximumNoise /= noiseSettings.CeilingModifier;
        }

        private void GenerateMapData() {
            n_terrain = new NativeTerrainData(terrainChunkJobConfig.TerrainSettings);

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

            JobHandle setupJobs = JobHandle.CombineDependencies(
                genColorVecsJobs,
                genMeshVectors
            );

            // Start new noiseJob
            JobHandle noiseJob = new JobCreateCoherentNoise() {
                // In
                settingsNoise = terrainChunkJobConfig.TerrainSettings.NoiseSettings,
                n_vecColors = n_terrain.n_mapData.n_vecColors,
                fastNoiseGen = fastNoiseGen,
                minimumNoise = minimumNoise,
                maximumNoise = maximumNoise,
                // Out
                n_heightMap = n_terrain.n_mapData.n_heightMap
            }.ScheduleParallel(
                n_terrain.n_mapData.n_heightMap.Length,
                (int)terrainChunkJobConfig.ParallelLoopBatchCount,
                setupJobs
            ); // only after the previous setupJobs Combo

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
            }.Schedule(noiseJob); // only after the noiseJob

            // Perform the Color Job
            JobHandle colorJob = new JobCreateColorMap() {
                // In
                settingsNoise = terrainChunkJobConfig.TerrainSettings.NoiseSettings,
                n_vecMesh = n_terrain.n_meshData.n_vecMesh,
                n_heightMap = n_terrain.n_mapData.n_heightMap,
                n_regionBlendCurve = n_terrain.n_mapData.n_regionBlendCurve,
                n_regions = n_terrain.n_mapData.n_regions,
                fastNoiseGen = fastNoiseGen,
                // Out
                n_colorMap = n_terrain.n_mapData.n_colorMap
            }.ScheduleParallel(
                n_terrain.n_mapData.n_colorMap.Length,
                (int)terrainChunkJobConfig.ParallelLoopBatchCount,
                meshJob
            ); // only after the meshJob

            // Kick off Scheduler
            JobHandle.ScheduleBatchedJobs();

            // Keep reference of the jobQueue to tack other jobs onto
            // (since we can't cancel jobs, and don't want concurrent
            // jobs accessing the same NativeContainers)
            jobHandle = colorJob;
        }

        public void Complete() {
            jobHandle.Complete();
        }

        public void ForceQueueCompletion() {
            jobHandle.Complete();
            Dispose();
        }

        public TerrainData ToTerrainData() {
            return new TerrainData(
                CHUNK_SIZE,
                n_terrain
            );
        }

        #endregion

    }

}