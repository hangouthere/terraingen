using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using nfg.Unity.Utils.Lifecycle;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace nfg.Unity.TerrainGen {

    public struct TerrainJobQueueEntry {
        public TerrainChunkJobRegister JobRegister;
        public JobQueueTerrainChunkBuilder ChunkBuilder;

        public void FinalizeJob() {
            if (null == ChunkBuilder) {
                return;
            }

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
        public bool DebugMessaging;

        private Queue<TerrainJobQueueEntry> jobQueue = new Queue<TerrainJobQueueEntry>();
        private Queue<TerrainJobQueueEntry> cancelQueue = new Queue<TerrainJobQueueEntry>();
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
            ClearCancelledEntries();
            CheckQueueProgress();
            CheckCurrentEntryProgress();
        }

        public void ClearCancelledEntries() {
            if (jobQueue.Count == 0) {
                return;
            }

            jobQueue = new Queue<TerrainJobQueueEntry>(
                jobQueue.Where(entry => !currentEntry.Equals(entry) && !cancelQueue.Contains(entry))
            );

            cancelQueue.Clear();
        }

        Stopwatch totalElapse = new Stopwatch();
        Stopwatch chunkElapse;

        private void logElapsed(string prefix, Stopwatch sw) {
            sw.Stop();

            if (!DebugMessaging) {
                return;
            }

            TimeSpan ts = sw.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}h:{1:00}m:{2:00}s:{3:00}ms",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            Debug.Log(prefix + elapsedTime);
        }

        private void CheckQueueProgress() {
            // Currently processing a Job, or no Jobs left...
            if (HasCurrentEntry) {
                return;
            }

            if (0 == jobQueue.Count) {
                if (totalElapse.IsRunning) {
                    logElapsed("JobQueue Time: ", totalElapse);
                }

                return;
            }

            // Restart if we stopped for some reason
            if (!totalElapse.IsRunning) {
                totalElapse = new Stopwatch();
            }

            chunkElapse = new Stopwatch();
            chunkElapse.Start();
            totalElapse.Start();

            currentEntry = jobQueue.Dequeue();
            currentEntry.ChunkBuilder = new JobQueueTerrainChunkBuilder(currentEntry.JobRegister.TerrainChunkJobConfig);
        }

        public void CheckCurrentEntryProgress() {
            // No Job, or Current job cannot be completed...
            if (!HasCurrentEntry || !currentEntry.ChunkBuilder.CanComplete) {
                return;
            }

            // Finalize the Job
            currentEntry.FinalizeJob();
            // Reset to default for next in queue/noop
            currentEntry = default(TerrainJobQueueEntry);

            if (chunkElapse.IsRunning) {
                logElapsed("Chunk Rendertime: ", chunkElapse);
            }
        }

        public TerrainJobQueueEntry RequestChunk(TerrainChunkJobRegister jobRegister) {
            // One Stop Hack-Shop: Ensure we're working with a COPY of the SO, so we don't modify settings as we operate!
            jobRegister.TerrainChunkJobConfig.TerrainSettings = Instantiate(jobRegister.TerrainChunkJobConfig.TerrainSettings);

            TerrainJobQueueEntry jobQueueEntry = new TerrainJobQueueEntry() {
                JobRegister = jobRegister
            };

            jobQueue.Enqueue(jobQueueEntry);

            return jobQueueEntry;
        }

        public void TryCancel(TerrainJobQueueEntry queueEntry) {
            if (jobQueue.Contains(queueEntry)) {
                cancelQueue.Enqueue(queueEntry);
            }
        }
    }

}