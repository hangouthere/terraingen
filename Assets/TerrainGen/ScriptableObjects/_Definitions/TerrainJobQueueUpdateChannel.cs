using System.Collections.Generic;
using nfg.Unity.Utils.Lifecycle;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    struct TerrainJobQueueEntry {
        public TerrainChunkJobRegister JobRegister;
        public TerrainChunkBuilder ChunkBuilder;

        public void FinalizeJob() {
            // Complete's ChunkBuilder job
            ChunkBuilder.Complete();
            // Calls the callback Action with TerrainData
            JobRegister.OnTerrainData.Invoke(ChunkBuilder.ToTerrainData());
            // Clear ChunkBuilder's memory
            ChunkBuilder.Dispose();
        }
    }

    // Linked to LifecycleManager as a LifecycleUpdateChannelSO 
    [CreateAssetMenu(fileName = "TerrainJobQueueUpdateChannel", menuName = "TerrainGen/Terrain Job Queue Channel")]
    public class TerrainJobQueueUpdateChannel : LifecycleUpdateChannelSO {
        private readonly Queue<TerrainJobQueueEntry> jobQueue = new Queue<TerrainJobQueueEntry>();
        private TerrainJobQueueEntry currentEntry;

        private bool HasCurrentEntry { get => !currentEntry.Equals(default(TerrainJobQueueEntry)); }

        private void OnDestroy() {
            if (HasCurrentEntry) {
                currentEntry.ChunkBuilder.ForceQueueCompletion();
            }

            jobQueue.Clear();
        }

        // Called via LifecycleManagerGO
        public override void Update() {
            // Default or "empty" entry means we haven't selected a currentEntry
            if (!HasCurrentEntry) {
                if (jobQueue.Count > 0) {
                    // We have more jobs to run!
                    ProcessNextInQueue();
                } else {
                    // No job running, and none left to run, so we just noop
                    return;
                }
            }

            // Current job can be completed...
            if (currentEntry.ChunkBuilder.CanComplete) {
                // Finalize the Job
                currentEntry.FinalizeJob();
                // Reset to default for next in queue/noop
                currentEntry = default(TerrainJobQueueEntry);
            }
        }

        private void ProcessNextInQueue() {
            currentEntry = jobQueue.Dequeue();

            TerrainChunkBuilder chunkBuilder = new TerrainChunkBuilder(currentEntry.JobRegister.TerrainChunkJobConfig);

            currentEntry.ChunkBuilder = chunkBuilder;
        }

        public void RequestChunks(TerrainChunkJobRegister jobRegister) {
            jobQueue.Enqueue(new TerrainJobQueueEntry() {
                JobRegister = jobRegister
            });
        }

    }

}