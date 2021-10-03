using nfg.Utils;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
    Debouncer debouncer;

    override public bool RequiresConstantRepaint() { return debouncer?.IsPending ?? false; }

    private void OnEnable() {
        debouncer = new Debouncer(0.125f);
    }

    public override void OnInspectorGUI() {
        MapGenerator mapGen = (MapGenerator)target;

        serializedObject.Update();

        if (DrawDefaultInspector()) {
            if (mapGen.autoUpdate) {
                debouncer.Debounce(mapGen.GenerateMapFromEditor);
            }
        }

        if (GUILayout.Button("Generate Threaded")) {
            mapGen.GenerateMapFromEditor();
        }

        if (GUILayout.Button("Generate Now")) {
            mapGen.GenerateMapFromEditor();
            mapGen.runningJob.handle.Complete();
            mapGen.checkJobQueue();
        }

        debouncer.DebounceCheck();
    }
}