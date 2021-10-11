using UnityEngine;

[ExecuteInEditMode]
public class LifecycleManagerGO : MonoBehaviour {
    [SerializeField] private LifecycleUpdateChannelSO[] LifecycleUpdateChannels;

    private void Update() {
        if (null != LifecycleUpdateChannels) {
            foreach (LifecycleUpdateChannelSO channel in LifecycleUpdateChannels) {
                channel.Update();
            }
        }
    }

    // For in-Editor Updates
    private void OnRenderObject() {
        Update();
    }
}