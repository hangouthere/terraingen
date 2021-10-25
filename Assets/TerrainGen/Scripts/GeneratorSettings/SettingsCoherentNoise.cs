using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [System.Serializable]
    public struct SettingsCoherentNoise {
        [HideInInspector]
        public int Width;
        [HideInInspector]
        public int Height;
        [Range(0.1f, 10f)]
        public float Scale;
        [Range(1, 25)]
        public int NumOctaves;
        [Range(0.01f, 5f)]
        public float Lacunarity;
        [Range(0.01f, 1f)]
        public float Persistence;
        public int Seed;
        [Range(0.01f, 10f)]
        public float CeilingModifier;
        [Range(0.01f, 10f)]
        public float FloorModifier;
        public Vector2 PositionOffset;
        public NormalSpace NormalSpace;
    }

}