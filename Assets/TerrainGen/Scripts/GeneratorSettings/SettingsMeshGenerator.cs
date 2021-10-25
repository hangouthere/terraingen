using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [System.Serializable]
    public struct SettingsMeshGenerator {
        [HideInInspector]
        public int ChunkSize;
        public float heightMultiplier;
        [Range(0, 6)] // Simple representation of LOD 0-6;
        public int LevelOfDetail;
    }

    [System.Serializable]
    public struct RegionEntryData {
        public float height;
        public Color color;
        [Range(0.001f, 20f)]
        public float BlendModifier;
    }

    // Nested Struct to keep managed string out of necessary data for jobs
    [System.Serializable]
    public struct RegionEntry {
        public string label;
        public RegionEntryData entryData;
    }

}