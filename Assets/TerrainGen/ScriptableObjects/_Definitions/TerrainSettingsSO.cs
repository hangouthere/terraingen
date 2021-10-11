using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainSettings", menuName = "TerrainGen/TerrainSettings")]
public class TerrainSettingsSO : ScriptableObject {
    public SettingsCoherentNoise NoiseSettings;
    public SettingsMeshGenerator MeshSettings;

    //FIXME: Test if this can go in NoiseSettings.... this is annoying to have them split
    public RegionEntry[] regions;
    public AnimationCurve regionBlendCurve;
    public AnimationCurve heightCurve;
}
