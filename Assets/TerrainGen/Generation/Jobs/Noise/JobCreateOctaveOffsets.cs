using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct JobCreateOctaveOffsets : IJob {
    [ReadOnly]
    public int seed;
    [ReadOnly]
    public Vector2 noiseOffset;

    public NativeArray<Vector2> n_vecOctaveOffsets;

    public void Execute() {
        var prng = new Unity.Mathematics.Random((uint)seed);

        for (int octaveIdx = 0; octaveIdx < n_vecOctaveOffsets.Length; octaveIdx++) {
            float offsetX = prng.NextInt(-100000, 100000) + noiseOffset.x;
            float offsetY = prng.NextInt(-100000, 100000) + noiseOffset.y;

            n_vecOctaveOffsets[octaveIdx] = new Vector2(offsetX, offsetY);
        }
    }
}