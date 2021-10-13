using nfg.Unity.Utils;
using UnityEditor;
using UnityEngine;

namespace nfg.Unity.TerrainGen.Demo {

    [CustomEditor(typeof(SettingsEditorGO))]
    public class EditorSettingsEditor : Editor {
        private Debouncer debouncer;

        SettingsEditorGO settingsEditor { get => (SettingsEditorGO)target; }

        override public bool RequiresConstantRepaint() { return settingsEditor.liveUpdate; }

        private void OnEnable() {
            debouncer = new Debouncer(0.05f);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Generate Now")) {
                settingsEditor.GenerateTestChunk();
                return;
            }

            if (settingsEditor.liveUpdate) {
                debouncer.Debounce(settingsEditor.GenerateTestChunk);
            }

            debouncer.DebounceCheck();
        }
    }

}