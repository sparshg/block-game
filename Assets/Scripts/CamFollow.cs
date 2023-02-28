using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour {
    [SerializeField] private MovePlayer player;
    [SerializeField] private float elevation, followSpeed;
    void LateUpdate() {
        Vector3 targetPosition = player.transform.position + player.surfaceNormal * elevation;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public IEnumerator Rotate(Vector3 axis, Vector3 point, float rotateSpeed) {
        float _angle = 0f;
        // while (true) {
        //     _angle += rotateSpeed * Time.deltaTime;
        //     if (_angle >= angle) break;
        //     transform.RotateAround(anchor, axis, rotateSpeed * Time.deltaTime);
        //     yield return null;
        // }
        // transform.position = toVec;
        // transform.rotation = Quaternion.identity;
        // isMoving = false;
        yield return null;
    }

}
