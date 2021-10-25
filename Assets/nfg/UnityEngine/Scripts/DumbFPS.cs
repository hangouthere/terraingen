using TMPro;
using UnityEngine;

namespace nfg.UnityEngine.Scripts {

    public class DumbFPS : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI fpsDisplay;
        [SerializeField] private TextMeshProUGUI averageFPSDisplay;
        [SerializeField] private TextMeshProUGUI minFPSDisplay;
        [SerializeField] private TextMeshProUGUI maxFPSDisplay;

        private int framesPassed = 0;
        private float fpsTotal = 0f;
        private float minFPS = Mathf.Infinity;
        private float maxFPS = 0f;

        void Update() {
            float fps = 1 / Time.unscaledDeltaTime;
            fpsDisplay.text = "FPS: " + (int)fps;

            fpsTotal += fps;
            framesPassed++;
            averageFPSDisplay.text = "Avg FPS: " + (int)(fpsTotal / framesPassed);

            if (fps > maxFPS && framesPassed > 10) {
                maxFPS = fps;
                maxFPSDisplay.text = "Max: " + (int)maxFPS;
            }
            if (fps < minFPS && framesPassed > 10) {
                minFPS = fps;
                minFPSDisplay.text = "Min: " + (int)minFPS;
            }
        }

    }

}