using UnityEngine;

public class RotateAroundAndPointAt : MonoBehaviour {
    [SerializeField] private Transform pointAt; //the target object
    [Range(0.001f, 100f)]
    [SerializeField] private float rotateSpeed = 10.0f; //a speed modifier

    void Update() {
        transform.LookAt(pointAt);
        transform.RotateAround(pointAt.position, Vector3.up, rotateSpeed * Time.deltaTime);
    }
}