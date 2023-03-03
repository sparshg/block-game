using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour {

    public Vector3 surfaceNormal = Vector3.up;
    public Vector3 primaryAxis = Vector3.forward, secondaryAxis = Vector3.right;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private CamRotate cam;

    private bool isMoving = false;

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + primaryAxis * 4f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + secondaryAxis * 4f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + surfaceNormal);
    }

    void Update() {
        if (isMoving) return;
        if (Input.GetKey(KeyCode.UpArrow)) {
            StartCoroutine(Roll(transform.position + primaryAxis * 0.5f - surfaceNormal * 0.5f, secondaryAxis, primaryAxis));
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            StartCoroutine(Roll(transform.position - primaryAxis * 0.5f - surfaceNormal * 0.5f, -secondaryAxis, -primaryAxis));
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            StartCoroutine(Roll(transform.position + secondaryAxis * 0.5f - surfaceNormal * 0.5f, -primaryAxis, secondaryAxis));
        } else if (Input.GetKey(KeyCode.LeftArrow)) {
            StartCoroutine(Roll(transform.position - secondaryAxis * 0.5f - surfaceNormal * 0.5f, primaryAxis, -secondaryAxis));
        }
    }

    IEnumerator Roll(Vector3 anchor, Vector3 axis, Vector3 newNormal) {
        isMoving = true;
        float angle = 90f, _angle = 0f, _rotateSpeed = rotateSpeed;
        Vector3 belowCube = transform.position - surfaceNormal;
        Vector3 toVec = belowCube + newNormal;

        if (toVec.x == Pref.I.size || toVec.y == Pref.I.size || toVec.z == Pref.I.size || toVec.x < 0 || toVec.y < 0 || toVec.z < 0) {
            angle = 180f;
            _rotateSpeed *= 2;
            cam.SetIntermediateRotation(surfaceNormal);
        } else {
            toVec += surfaceNormal;
        }
        while (true) {
            _angle += _rotateSpeed * Time.deltaTime;
            if (_angle >= angle) break;
            transform.RotateAround(anchor, axis, _rotateSpeed * Time.deltaTime);
            if (angle == 180f) surfaceNormal = (transform.position - belowCube).normalized;
            yield return null;
        }
        transform.position = toVec;
        transform.rotation = Quaternion.identity;
        if (angle == 180f) {
            surfaceNormal = newNormal;
        }
        isMoving = false;
        yield return null;
    }
}
