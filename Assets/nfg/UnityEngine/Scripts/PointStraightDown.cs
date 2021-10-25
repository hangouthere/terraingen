using UnityEngine;

namespace nfg.UnityEngine.Scripts {

    public class PointStraightDown : MonoBehaviour {
        [System.Obsolete]
        private void Update() {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

}