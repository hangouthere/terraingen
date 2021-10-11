using System;
using nfg.Jobs;
using Unity.Collections;
using UnityEngine;

public struct NativeMeshData : IDisposable {
    public NativeArray<Vector3> n_vecMesh;
    public NativeArray<int> n_triangles;
    public NativeArray<Vector2> n_uvs;

    public NativeCurve n_heightCurve;

    public int lodVerticeIncrement;
    public int lodVerticesSize;

    public bool IsCreated { get => n_vecMesh.IsCreated; }

    public NativeMeshData(SettingsMeshGenerator settingsMesh, AnimationCurve heightCurve) : this() {
        // LOD is simplified 0-6, where 0 is turned into a scale factor of 1
        //     Greater than 1 will be * 2, to give us actual LOD values (1, 2, 6, 8, 10, 12)
        lodVerticeIncrement = (0 == settingsMesh.LevelOfDetail) ? 1 : settingsMesh.LevelOfDetail * 2;
        // Num Vertices for the current LOD can be calculated as : ((width - 1) / LODIncrement) + 1
        lodVerticesSize = ((settingsMesh.ChunkSize - 1) / lodVerticeIncrement) + 1;

        n_vecMesh = new NativeArray<Vector3>(lodVerticesSize * lodVerticesSize, Allocator.Persistent);
        n_uvs = new NativeArray<Vector2>(lodVerticesSize * lodVerticesSize, Allocator.Persistent);
        n_triangles = new NativeArray<int>((lodVerticesSize - 1) * (lodVerticesSize - 1) * 6, Allocator.Persistent);
        n_heightCurve = new NativeCurve();

        n_heightCurve.Update(heightCurve, 256);
    }

    public void Dispose() {
        if (!IsCreated) return;

        n_heightCurve.Dispose();

        n_vecMesh.Dispose();
        n_triangles.Dispose();
        n_uvs.Dispose();
    }
}