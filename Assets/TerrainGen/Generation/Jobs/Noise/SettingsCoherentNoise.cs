using UnityEngine;

[System.Serializable]
public struct SettingsCoherentNoise {
    [HideInInspector]
    public int Width;
    [HideInInspector]
    public int Height;
    [Range(0.1f, 1000f)]
    public float Scale;
    [Range(1f, 25f)]
    public int NumOctaves;
    [Range(0.01f, 5f)]
    public float Lacunarity;
    [Range(0.01f, 1f)]
    public float Persistence;
    public int Seed;
    public Vector2 NoiseOffset;
    public Vector2 PositionOffset;
}