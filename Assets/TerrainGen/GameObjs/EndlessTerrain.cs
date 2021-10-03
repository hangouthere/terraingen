using System;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {
    [Range(10, 1000)]
    public float ViewDistance = 300;

    public Transform viewpoint;

    private Dictionary<Vector2, TerrainChunk> terrainCache = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> currentChunks = new List<TerrainChunk>();

    private int chunkSize;
    private int chunksVisible;
    private float viewDistSqr;

    void Start() {
        chunkSize = MapGenerator.CHUNK_SIZE - 1;
    }

    void Update() {
        chunksVisible = Mathf.RoundToInt(ViewDistance / chunkSize);
        viewDistSqr = ViewDistance * ViewDistance;

        ManageChunks();
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(viewpoint.position, Vector3.up, ViewDistance);

        foreach (TerrainChunk chunk in currentChunks) {
            Vector3 boundsPoint = chunk.bounds.ClosestPoint(viewpoint.position);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(viewpoint.position, chunk.mesh.transform.position);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(viewpoint.position, boundsPoint);

            chunk.OnDrawGizmos();
        }
    }
#endif

    void ManageChunks() {
        Vector2 viewerCoord = new Vector2(
                Mathf.RoundToInt(viewpoint.position.x / chunkSize),
                Mathf.RoundToInt(viewpoint.position.z / chunkSize)
            );

        removeOldChunks();

        for (int y = -chunksVisible; y <= chunksVisible; y++) {
            for (int x = -chunksVisible; x <= chunksVisible; x++) {
                Vector2 chunkCoord = viewerCoord + new Vector2(x, y); // viewCoordinate + chunkOffset

                // Check Cache and Update, or instantiate if necessary
                if (terrainCache.ContainsKey(chunkCoord)) {
                    DetermineChunkViewStatus(chunkCoord);
                } else {
                    CreateNewChunk(chunkCoord);
                }
            }
        }
    }

    private void CreateNewChunk(Vector2 chunkCoord) {
        TerrainChunk newChunk = new TerrainChunk(chunkCoord, chunkSize);

        // newChunk.isVisible = true;
        newChunk.mesh.transform.parent = transform;

        terrainCache.Add(chunkCoord, newChunk);
    }

    void removeOldChunks() {
        for (int chunkIdx = 0; chunkIdx < currentChunks.Count; chunkIdx++) {
            currentChunks[chunkIdx].isVisible = false;
        }

        currentChunks.Clear();
    }

    void DetermineChunkViewStatus(Vector2 chunkCoord) {
        TerrainChunk chunk = terrainCache[chunkCoord];
        float sqrDist = chunk.bounds.SqrDistance(viewpoint.position);
        chunk.isVisible = sqrDist <= viewDistSqr;

        if (true == chunk.isVisible) {
            currentChunks.Add(chunk);
        }
    }
}

public class TerrainChunk {
    public readonly GameObject mesh;
    public readonly Bounds bounds;
    private readonly Vector2 chunkCoord;
    private readonly Vector2 chunkPosition;

    private bool _isVisible;

    public bool isVisible {
        get => _isVisible;
        set {
            _isVisible = value;
            mesh.SetActive(value);
        }
    }

    public TerrainChunk(Vector2 chunkCoord, int chunkSize) {
        this.chunkCoord = chunkCoord;
        chunkPosition = chunkCoord * chunkSize;
        Vector3 chunkPositionV3 = new Vector3(chunkPosition.x, 0, chunkPosition.y);

        bounds = new Bounds(chunkPositionV3, new Vector3(chunkSize, 0, chunkSize));

        mesh = GameObject.CreatePrimitive(PrimitiveType.Plane);
        mesh.transform.position = chunkPositionV3;
        // ! FIXME: this will go away, i hope!
        mesh.transform.localScale = Vector3.one * chunkSize / 10f;

        isVisible = false;
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    public void DestroyChunk() {
        UnityEngine.Object.Destroy(mesh);
    }
}