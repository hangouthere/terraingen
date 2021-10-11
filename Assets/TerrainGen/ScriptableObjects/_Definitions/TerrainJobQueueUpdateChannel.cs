using System;
using System.Collections.Generic;
using UnityEngine;

struct TerrainJobQueueEntry {
    public Vector2[] chunkPositions;
    public TerrainSettingsSO terrainSettings;
    public Action<TerrainData> callback;
    public TerrainChunkBuilder chunkBuilder;
    public InnerloopBatchCount parallelLoopBatchCount;

    public void FinalizeJob() {
        // Complete's ChunkBuilder job
        chunkBuilder.Complete();
        // Calls the callback Action with TerrainData
        callback.Invoke(chunkBuilder.ToTerrainData());
        // Clear ChunkBuilder's memory
        chunkBuilder.Dispose();
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
            currentEntry.chunkBuilder.ForceQueueCompletion();
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
        if (currentEntry.chunkBuilder.CanComplete) {
            // Finalize the Job
            currentEntry.FinalizeJob();
            // Reset to default for next in queue/noop
            currentEntry = default(TerrainJobQueueEntry);
        }
    }

    private void ProcessNextInQueue() {
        currentEntry = jobQueue.Dequeue();

        //! FIXME: Do something with currentEntry.chunkPositions 
        TerrainChunkBuilder chunkBuilder = new TerrainChunkBuilder(currentEntry.terrainSettings, currentEntry.parallelLoopBatchCount);
        currentEntry.chunkBuilder = chunkBuilder;
    }

    public void RequestChunks(Vector2[] chunkPositions, TerrainSettingsSO terrainSettings, InnerloopBatchCount parallelLoopBatchCount, Action<TerrainData> callback) {
        jobQueue.Enqueue(new TerrainJobQueueEntry() {
            terrainSettings = terrainSettings,
            chunkPositions = chunkPositions,
            parallelLoopBatchCount = parallelLoopBatchCount,
            callback = callback
        });
    }

}
