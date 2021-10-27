# TODO

* Play with converting the Ceil/Floor Modifiers into actual limits for simplicity-sake
* Refactor Jobs to be more functional (ie, no more nested object reaching such as with noiseSettings, etc)
* LOD is messed up?????
* Configure EditorConfig to enforce PascalCase for Fields, Props, and Functions!
* DOCUMENT ALL FILES EVERYWHERE!
* Check for initial settings in a new project env
  * Likely need to set up OnValidate() to do checks, as well as Start??

----

# Job Queue:

## Create Noise Vectors

First, we need to create base Vectors for the Noise calculations to live in. 

> The X and Z components of the Vector will represent their Coordinate layout, while the Y component will represent the normalized noise value.

Vector initialization are parallelized for each row, building the coordinate system completely before continuing with further Jobs.

## Generate Noise

Prior to actually generating Noise, we want to establish a *minimum* and *maximum* value based on some parameters passed into the JobQueueRequest. These settings should be tweaked in the UI beforehand to eliminate frustration.

Using the previously generated Vector array for Noise, we can iterate through the Vectors and set the Y-value as the noise, normalized to the min/max values calculated.

> FastNoiseLite has been ported to utilize `NativeContainers` and implemented for this project to make Noise generation extremely fast! As a fortunate side-effect, various settings such as the *Octave* and *lacunarity* fractalization is handled internally to FNL for customization.

## Texture Map Color Data

Once our Noise is fully generated, we're able to create a `Color` array that will later be fed into a helper to get a `Texture2D`, that will then be used to apply to a `Renderer`.

During this process, we select a `RegionEntryData` based on its height value, and through some good ol' Perlin Noise and Color LERP'ing we recieve the color map for our texture.

## Terrain Mesh

Now that our Texture has been generated