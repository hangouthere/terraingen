using Unity.Collections;
using UnityEngine;

namespace nfg.gfx {

    public class TextureHelper {
        public static Texture2D FromColorMap(int width, int height, Color[] colorMap) {
            Texture2D texture = new Texture2D(width, height);

            // Make it sharper and not texture wrap
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            // SetMap and Apply
            texture.SetPixels(colorMap);
            texture.Apply();

            return texture;
        }
    }

}