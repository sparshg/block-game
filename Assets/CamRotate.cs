using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour {
    [SerializeField] private float sensitivity;
    [SerializeField] private MovePlayer player;
    [SerializeField] private float radius;
    private Vector2 turn;

    void Start() {

    }

    void Update() {
        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.x %= 360;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        turn.y = Mathf.Clamp(turn.y, -70, -20);
        transform.localPosition = new Vector3(
            Mathf.Sin(turn.x * Mathf.Deg2Rad), 0, Mathf.Cos(turn.x * Mathf.Deg2Rad)
        ) * -radius;
        transform.localRotation = Quaternion.Euler(-turn.y, turn.x, 0);

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, player.surfaceNormal);
        if (player.surfaceNormal.x != 0) {
            if (Mathf.Abs(forward.y) > Mathf.Abs(forward.z)) {
                player.primaryAxis = Vector3.up * Mathf.Sign(forward.y);
                player.secondaryAxis = -Vector3.forward * Mathf.Sign(forward.y);
            } else {
                player.primaryAxis = Vector3.forward * Mathf.Sign(forward.z);
                player.secondaryAxis = Vector3.up * Mathf.Sign(forward.z);
            }
        } else if (player.surfaceNormal.y != 0) {
            if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z)) {
                player.primaryAxis = Vector3.right * Mathf.Sign(forward.x);
                player.secondaryAxis = -Vector3.forward * Mathf.Sign(forward.x);
            } else {
                player.primaryAxis = Vector3.forward * Mathf.Sign(forward.z);
                player.secondaryAxis = Vector3.right * Mathf.Sign(forward.z);
            }
        } else if (player.surfaceNormal.z != 0) {
            if (Mathf.Abs(forward.x) > Mathf.Abs(forward.y)) {
                player.primaryAxis = Vector3.right * Mathf.Sign(forward.x);
                player.secondaryAxis = -Vector3.up * Mathf.Sign(forward.x);
            } else {
                player.primaryAxis = Vector3.up * Mathf.Sign(forward.y);
                player.secondaryAxis = Vector3.right * Mathf.Sign(forward.y);
            }
        }
    }
}
