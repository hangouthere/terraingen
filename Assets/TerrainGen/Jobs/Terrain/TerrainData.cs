namespace nfg.Unity.TerrainGen {

    public struct TerrainData {
        public MapData mapData;
        public TerrainMeshData meshData;

        public TerrainData(NativeTerrain n_terrain) {
            this.mapData = new MapData(n_terrain.n_mapData);
            this.meshData = new TerrainMeshData(n_terrain.n_meshData);
        }
    }

}