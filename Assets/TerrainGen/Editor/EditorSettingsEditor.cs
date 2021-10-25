using nfg.Unity.Utils;
using UnityEditor;
using UnityEngine;

namespace nfg.Unity.TerrainGen.Demo {

    [CustomEditor(typeof(SettingsEditorGO))]
    public class EditorSettingsEditor : Editor {
        private Debouncer debouncer;

        SettingsEditorGO settingsEditor { get => (SettingsEditorGO)target; }

        override public bool RequiresConstantRepaint() { return settingsEditor.LiveUpdate; }

        private void OnEnable() {
            // Don't go below 0.1f to avoid over-processing while
            // fidgeting with settings
            debouncer = new Debouncer(0.1f);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUILayout.Button("Generate Now")) {
                settingsEditor.GenerateTestChunk();
                settingsEditor.ForceComplete();
                return;
            }

            if (settingsEditor.LiveUpdate) {
                debouncer.Debounce(settingsEditor.GenerateTestChunk);
            }

            debouncer.DebounceCheck();
        }
    }

}