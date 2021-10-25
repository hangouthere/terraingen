using UnityEngine;

namespace nfg.UnityEngine {

    public static class MeshDecorator {

        public static void SetupGameObjects(GameObject parentObject, Material baseMaterial) {
            MeshRenderer meshRenderer = parentObject.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = parentObject.GetComponent<MeshFilter>();

            if (!meshRenderer) {
                meshRenderer = parentObject.AddComponent<MeshRenderer>();
                meshRenderer.material = baseMaterial;
            }

            if (!meshFilter) {
                meshFilter = parentObject.AddComponent<MeshFilter>();
            }
        }

    }

}