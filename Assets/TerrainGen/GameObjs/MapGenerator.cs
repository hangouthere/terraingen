// FIXME: Remove all the junk headers from Generation namespace, and move to here and construct the underlying settings Objects Correctly :S
//        Ugly IMO, but it's better for clean code... Separates the Unity integration from the implementation.

// Lots of info https://medium.com/@yvanscher/playing-with-perlin-noise-generating-realistic-archipelagos-b59f004d8401

using System;
using nfg.gfx;
using UnityEngine;

public enum DisplayMode {
    FlatGreyScale,
    FlatRegion,
    MeshRegion
}

[SelectionBase]
public class MapGenerator : MonoBehaviour {
    // Toggle Objects between Simple and Mesh Maps
    [SerializeField] private GameObject simpleMap;
    [SerializeField] private GameObject meshMap;

    [SerializeField] private DisplayMode displayMode = DisplayMode.FlatGreyScale;
    [SerializeField] private bool blendRegions = false;
    [SerializeField] private AnimationCurve regionBlendCurve;
    [SerializeField] private CoherentNoiseSettings noiseSettings;
    [SerializeField] private MeshGeneratorSettings meshGenSettings;

    public bool autoUpdate;

    const int CHUNK_SIZE = 241;

    void OnValidate() {
        // Ensure Noise Settings are valid
        noiseSettings.Width = CHUNK_SIZE;
        noiseSettings.Height = CHUNK_SIZE;
        noiseSettings.Lacunarity = Math.Max(noiseSettings.Lacunarity, 0.01f);
        noiseSettings.Octaves = Math.Max(noiseSettings.Octaves, 0);

        // Ensure Mesh Generator Settings are valid
        meshGenSettings.heightMultiplier = Math.Max(meshGenSettings.heightMultiplier, 1);
    }

    void Start() {
        GenerateMap();
    }

    public void GenerateMap() {
        float[,] heightMap = new CoherentNoise(noiseSettings).GenerateMap();

        TextureGenerator textureGenerator;

        AnimationCurve usedCurve = blendRegions ? regionBlendCurve : null;

        switch (displayMode) {
            case DisplayMode.FlatRegion:
                textureGenerator = new RegionTextureGenerator(CHUNK_SIZE, meshGenSettings.regions, usedCurve);

                simpleMap.SetActive(true);
                meshMap.SetActive(false);

                break;

            case DisplayMode.MeshRegion:
                textureGenerator = new RegionTextureGenerator(CHUNK_SIZE, meshGenSettings.regions, usedCurve);

                DrawMesh(heightMap);

                simpleMap.SetActive(false);
                meshMap.SetActive(true);

                break;

            case DisplayMode.FlatGreyScale:
            default:
                textureGenerator = new GreyscaleTextureGenerator(CHUNK_SIZE, CHUNK_SIZE);

                simpleMap.SetActive(true);
                meshMap.SetActive(false);
                break;
        }

        DrawTextures(textureGenerator, heightMap);
    }

    private void DrawMesh(float[,] heightMap) {
        TerrainMeshGenerator meshGenerator = new TerrainMeshGenerator(meshGenSettings);
        MeshData meshData = meshGenerator.FromHeightMap(heightMap);

        // Apply Mesh!
        meshGenSettings.meshFilter.sharedMesh = meshData.CreateMesh();
    }

    public void DrawTextures(TextureGenerator textureGenerator, float[,] heightMap) {
        Texture2D texture = textureGenerator.FromHeightMap(heightMap);

        noiseSettings.textureRenderer.sharedMaterial.mainTexture = texture;
        noiseSettings.textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);

        meshGenSettings.meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
