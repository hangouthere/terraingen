using nfg.UnityEngine;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public static class TerrainDecorator {
        public static GameObject SetupGameObjects(Transform parent, Material terrainMaterial) {
            GameObject terrainGameObj = new GameObject("nfgTerrain");

            // Parent our new GO to whatever we're decorating!
            terrainGameObj.transform.parent = parent;

            MeshDecorator.SetupGameObjects(terrainGameObj, terrainMaterial);

            return terrainGameObj;
        }
    }

}