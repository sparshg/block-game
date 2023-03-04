using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour {

    public Vector3 surfaceNormal = Vector3.up;
    public Vector3 primaryAxis = Vector3.forward, secondaryAxis = Vector3.right;
    [SerializeField] private AnimationCurve rotateCurve;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private CamRotate cam;

    private bool isMoving = false;
    // private Vector3 lockPrimAxis, lockSecAxis;
    // private KeyCode lockKey;

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
        float angle = 90f, _angle = 0f, _rotateSpeed = rotateSpeed, t1 = rotateCurve.Evaluate(0), t;
        Vector3 belowCube = transform.position - surfaceNormal;
        Vector3 toVec = belowCube + newNormal;

        if (toVec.x == Pref.I.size || toVec.y == Pref.I.size || toVec.z == Pref.I.size || toVec.x < 0 || toVec.y < 0 || toVec.z < 0) {
            angle = 180f;
            _rotateSpeed *= 2f;
            cam.SetIntermediateRotation(surfaceNormal);
        } else {
            toVec += surfaceNormal;
        }
        while (true) {
            _angle += _rotateSpeed * Time.deltaTime;
            t = rotateCurve.Evaluate(_angle / angle);
            if (_angle >= angle) break;
            transform.RotateAround(anchor, axis, (t - t1) * angle);
            t1 = t;
            if (angle == 180f) surfaceNormal = (transform.position - belowCube).normalized;
            yield return null;
        }
        transform.position = toVec;
        transform.rotation = Quaternion.identity;
        if (angle == 180f)
            surfaceNormal = newNormal;

        yield return null;
        isMoving = false;
    }

    //     void keysPriorityControls() {

    //         if (Input.GetKeyUp(KeyCode.UpArrow) && lockKey == KeyCode.UpArrow) {
    //             lockKey = KeyCode.None;
    //             lockPrimAxis = Vector3.zero;
    //         } else if (Input.GetKeyUp(KeyCode.DownArrow) && lockKey == KeyCode.DownArrow) {
    //             lockKey = KeyCode.None;
    //             lockPrimAxis = Vector3.zero;
    //         } else if (Input.GetKeyUp(KeyCode.RightArrow) && lockKey == KeyCode.RightArrow) {
    //             lockKey = KeyCode.None;
    //             lockPrimAxis = Vector3.zero;
    //         } else if (Input.GetKeyUp(KeyCode.LeftArrow) && lockKey == KeyCode.LeftArrow) {
    //             lockKey = KeyCode.None;
    //             lockPrimAxis = Vector3.zero;
    //         }

    //         if (Input.GetKeyDown(KeyCode.UpArrow)) {
    //             lockKey = KeyCode.UpArrow;
    //         } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
    //             lockKey = KeyCode.DownArrow;
    //         } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
    //             lockKey = KeyCode.RightArrow;
    //         } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
    //             lockKey = KeyCode.LeftArrow;
    //         }
    //         if (isMoving) return;

    //         if (lockPrimAxis == Vector3.zero) {
    //             lockPrimAxis = primaryAxis; lockSecAxis = secondaryAxis;
    //         }

    //         if (lockKey == KeyCode.UpArrow) {
    //             StartCoroutine(Roll(transform.position + lockPrimAxis * 0.5f - surfaceNormal * 0.5f, lockSecAxis, lockPrimAxis));
    //         } else if (lockKey == KeyCode.DownArrow) {
    //             StartCoroutine(Roll(transform.position - lockPrimAxis * 0.5f - surfaceNormal * 0.5f, -lockSecAxis, -lockPrimAxis));
    //         } else if (lockKey == KeyCode.RightArrow) {
    //             StartCoroutine(Roll(transform.position + lockSecAxis * 0.5f - surfaceNormal * 0.5f, -lockPrimAxis, lockSecAxis));
    //         } else if (lockKey == KeyCode.LeftArrow) {
    //             StartCoroutine(Roll(transform.position - lockSecAxis * 0.5f - surfaceNormal * 0.5f, lockPrimAxis, -lockSecAxis));
    //         }

    //     }
}
