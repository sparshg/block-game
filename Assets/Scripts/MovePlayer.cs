using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour {
    private Vector3 targetPosition;
    public Vector3 surfaceNormal = Vector3.up;
    public Vector3 primaryAxis = Vector3.forward, secondaryAxis = Vector3.right;


    void Start() {

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            transform.RotateAround(transform.position + primaryAxis * 0.5f - surfaceNormal * 0.5f, secondaryAxis, 90);
            targetPosition += Vector3.up;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            transform.RotateAround(transform.position - primaryAxis * 0.5f - surfaceNormal * 0.5f, secondaryAxis, -90);
            targetPosition -= Vector3.up;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            transform.RotateAround(transform.position + secondaryAxis * 0.5f - surfaceNormal * 0.5f, primaryAxis, -90);
            targetPosition += Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            transform.RotateAround(transform.position - secondaryAxis * 0.5f - surfaceNormal * 0.5f, primaryAxis, 90);
            targetPosition -= Vector3.right;
        }

    }
}
