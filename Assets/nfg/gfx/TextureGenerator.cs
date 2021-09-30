using System;
using UnityEngine;

namespace nfg.gfx {

    public class TextureGenerator {
        protected int Width;
        protected int Height;

        public TextureGenerator(int width, int height) {
            this.Width = width;
            this.Height = height;
        }

        public Texture2D FromColorMap(Color[] colorMap) {
            Texture2D texture = new Texture2D(this.Width, this.Height);

            // Make it sharper and not texture wrap
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            // SetMap and Apply
            texture.SetPixels(colorMap);
            texture.Apply();

            return texture;
        }

        public virtual Texture2D FromHeightMap(float[,] heightMap) {
            throw new NotImplementedException();
        }
    }
}