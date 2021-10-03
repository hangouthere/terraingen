using Unity.Collections;
using UnityEngine;

namespace nfg.gfx {
    public static class GreyscaleTextureGenerator {
        public static void GenerateColorMap(NativeArray<float> heightMap, NativeArray<Color> colorMap) {
            for (int heightIdx = 0; heightIdx < heightMap.Length; heightIdx++) {
                float heightVal = heightMap[heightIdx];
                colorMap[heightIdx] = Color.Lerp(Color.black, Color.white, heightMap[heightIdx]);
            }
        }
    }
}