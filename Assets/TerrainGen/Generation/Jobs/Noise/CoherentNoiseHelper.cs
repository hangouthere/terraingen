using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Static Helper for Coherent Noise
/// </summary>
public static class CoherentNoiseHelper {

    /// <summary>
    /// Generates Octave Offset Vector array for usage in future jobs.
    /// </summary>
    public static NativeArray<Vector2> GenerateOctaveOffsets(
        int numOctaves,
        int seed,
        Vector2 inputOffset
    ) {
        NativeArray<Vector2> octaveOffsets = new NativeArray<Vector2>(numOctaves, Allocator.Persistent);

        var prng = new Unity.Mathematics.Random((uint)seed);

        for (int octaveIdx = 0; octaveIdx < numOctaves; octaveIdx++) {
            float offsetX = prng.NextInt(-100000, 100000) + inputOffset.x;
            float offsetY = prng.NextInt(-100000, 100000) + inputOffset.y;

            octaveOffsets[octaveIdx] = new Vector2(offsetX, offsetY);
        }

        return octaveOffsets;
    }

}