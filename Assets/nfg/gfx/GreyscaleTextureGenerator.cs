using UnityEngine;

namespace nfg.gfx {

    public class GreyscaleTextureGenerator : TextureGenerator {
        public GreyscaleTextureGenerator(int width, int height) : base(width, height) { }

        public override Texture2D FromHeightMap(float[,] heightMap) {
            Color[] colorMap = new Color[this.Width * this.Height];

            for (int y = 0; y < this.Height; y++) {
                for (int x = 0; x < this.Width; x++) {
                    colorMap[x + y * this.Width] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                }
            }

            return FromColorMap(colorMap);
        }
    }

}