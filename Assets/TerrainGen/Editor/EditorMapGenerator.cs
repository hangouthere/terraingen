using nfg.Utils;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainChunkSettingsEditor))]
public class TerrainChunkSettingsEditorEditor : Editor {
    private Debouncer debouncer;

    TerrainChunkSettingsEditor settingsEditor { get => (TerrainChunkSettingsEditor)target; }

    override public bool RequiresConstantRepaint() { return settingsEditor.liveUpdate; }

    private void OnEnable() {
        debouncer = new Debouncer(0.125f);
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Now")) {
            settingsEditor.GenerateTestChunk();
        }

        if (settingsEditor.liveUpdate) {
            debouncer.Debounce(settingsEditor.GenerateTestChunk);
        }

        debouncer.DebounceCheck();
    }
}