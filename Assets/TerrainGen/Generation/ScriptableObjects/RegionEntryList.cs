using UnityEngine;

[CreateAssetMenu(fileName = "WorldRegionEntries", menuName = "TerrainGen/RegionEntryList", order = 0)]
public class RegionEntryList : ScriptableObject {
    public RegionEntry[] regions;
}
