namespace nfg.Unity.TerrainGen {

    public struct TerrainData {
        public int ChunkSize;
        public MapData MapData;
        public TerrainMeshData MeshData;

        public TerrainData(int ChunkSize, NativeTerrainData n_terrain) {
            this.ChunkSize = ChunkSize;
            this.MapData = new MapData(n_terrain.n_mapData);
            this.MeshData = new TerrainMeshData(n_terrain.n_meshData);
        }
    }

}