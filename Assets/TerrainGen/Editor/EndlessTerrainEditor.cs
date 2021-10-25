using nfg.Unity.Utils;
using UnityEditor;
using UnityEngine;

namespace nfg.Unity.TerrainGen.Demo {

    [CustomEditor(typeof(EndlessTerrainGO))]
    public class EndlessTerrainEditor : Editor {
        private Debouncer debouncer;

        EndlessTerrainGO endlessTerrain { get => (EndlessTerrainGO)target; }

        override public bool RequiresConstantRepaint() { return endlessTerrain.liveUpdate; }

        private void OnEnable() {
            debouncer = new Debouncer(3f);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Regenerate Now")) {
                endlessTerrain.DestroyAllChunks();
                return;
            }

            if (endlessTerrain.liveUpdate) {
                debouncer.Debounce(endlessTerrain.DestroyAllChunks);
            }

            debouncer.DebounceCheck();
        }
    }

}