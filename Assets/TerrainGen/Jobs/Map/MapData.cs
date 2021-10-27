using System.Linq;
using UnityEngine;

namespace nfg.Unity.TerrainGen {

    public struct MapData {
        public Color[] colorMap;
        public float[] heightMap;

        public MapData(NativeMapData n_mapData) {
            this.colorMap = n_mapData.n_colorMap.ToArray();
            this.heightMap = n_mapData.n_vecNoise.Select(v => v.y).ToArray();
        }
    }

}