using UnityEngine;

public class RaiseSlowly : MonoBehaviour {
    public float MoveScale = 10;
    public float MaxHeight = 250;

    private Vector3 startPos;

    void Start() {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update() {

        transform.position = startPos + Vector3.up * Mathf.PingPong(MoveScale * Time.time, MaxHeight);

        // if (transform.position.y - startPos.y > MaxHeight) {
        //     transform.position = startPos;
        // } else {
        //     transform.position = transform.position + transform.up * MoveScale * Time.deltaTime;
        // }
    }
}
