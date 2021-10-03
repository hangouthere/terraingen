using UnityEngine;

[System.Serializable]
public struct SettingsCoherentNoise {
    public int Width;
    public int Height;
    [Range(0.1f, 1000f)]
    public float Scale;
    [Range(1f, 25f)]
    public int NumOctaves;
    [Range(0.01f, 5f)]
    public float Lacunarity;
    [Range(0, 1)]
    public float Persistence;
    public int Seed;
    public Vector2 Offset;

    public SettingsCoherentNoise(
        int width = 10,
        int height = 10,
        float scale = 0.0001f,
        int numOctaves = 1,
        float lacunarity = 2f,
        float persistence = 0.5f,
        int seed = 10,
        Vector2 offset = default(Vector2)
    ) {
        Width = width;
        Height = height;
        Scale = scale;
        NumOctaves = numOctaves;
        Lacunarity = lacunarity;
        Persistence = persistence;
        Seed = seed;
        Offset = offset;
    }
}