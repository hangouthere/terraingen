using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraSwitcher", menuName = "NerdFoundry/CameraSwitcher")]
public class CameraSwitcher : ScriptableObject {
    private Camera[] Cameras;
    private int _CameraIndex;

    public int CameraIndex {
        get => _CameraIndex;

        set {
            int lastCameraIndex = _CameraIndex;
            _CameraIndex = WrapIndex(value);

            SwapCamera(lastCameraIndex, _CameraIndex);
        }
    }

    private void Start() {
        if (!Application.isPlaying) {
            return;
        }

        ReloadCameras();
        _CameraIndex = Cameras.Length - 1;
    }

    public void ReloadCameras() {
        // Debug.Log("reloading cams: " + Camera.allCamerasCount);

        if (0 == Camera.allCamerasCount) {
            return;
        }

        // Quickly re-enable so all cameras are detected
        foreach (Camera cam in Cameras) {
            if (cam) cam.enabled = true;
        }

        Cameras = Camera.allCameras;

        if (_CameraIndex > 0) {
            Camera currentCam = Camera.current;
            CameraIndex = ArrayUtility.FindIndex(Cameras, (cam) => cam.Equals(currentCam));
        }
    }

    public void PreviousCamera() {
        CameraIndex--;
    }

    public void NextCamera() {
        CameraIndex++;
    }

    private int WrapIndex(int index) {
        int mod = index % Cameras.Length;
        return mod < 0 ? mod + Cameras.Length : mod;
    }

    private void SwapCamera(int lastCameraIndex, int nextCameraIndex) {
        // Debug.Log($"Moving from {lastCameraIndex} -> {nextCameraIndex}");

        Cameras[lastCameraIndex].enabled = false;
        Cameras[nextCameraIndex].enabled = true;
    }
}