using UnityEngine;

namespace nfg.Unity.TerrainGen {

    [CreateAssetMenu(fileName = "NewTerrainSettings", menuName = "TerrainGen/TerrainSettings")]
    public class TerrainSettingsSO : ScriptableObject {
        public SettingsCoherentNoise NoiseSettings;
        public SettingsMeshGenerator MeshSettings;

        // These must remain in the Main Thread, as we need to convert 
        // these to Native formats before submitting to Jobs
        public RegionEntry[] regions;
        public AnimationCurve regionBlendCurve;
        public AnimationCurve heightCurve;
    }

}