using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour {
    [SerializeField] private float sensitivity;
    [SerializeField] private MovePlayer player;
    [SerializeField] private float radius;
    private Vector2 turn;
    private Quaternion history = Quaternion.identity;
    private Vector3 fromNormal = Vector3.up;

    public void SetIntermediateRotation(Vector3 normal) {
        history = Quaternion.FromToRotation(fromNormal, normal) * history;
        fromNormal = normal;
    }

    void Update() {
        turn.x += Input.GetAxis("Mouse X") * sensitivity;
        turn.x %= 360;
        turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        turn.y = Mathf.Clamp(turn.y, -60, 10);

        Quaternion rotation = Quaternion.FromToRotation(fromNormal, player.surfaceNormal) * history;
        transform.localPosition = rotation * new Vector3(
            Mathf.Sin(turn.x * Mathf.Deg2Rad), 0, Mathf.Cos(turn.x * Mathf.Deg2Rad)
        ) * -radius;

        transform.localRotation = rotation * Quaternion.Euler(-turn.y, turn.x, 0);
        if (player.surfaceNormal.x != 0) {
            if (Mathf.Abs(transform.forward.y) > Mathf.Abs(transform.forward.z))
                player.primaryAxis = Vector3.up * Mathf.Sign(transform.forward.y);
            else
                player.primaryAxis = Vector3.forward * Mathf.Sign(transform.forward.z);
        } else if (player.surfaceNormal.y != 0) {
            if (Mathf.Abs(transform.forward.x) > Mathf.Abs(transform.forward.z))
                player.primaryAxis = Vector3.right * Mathf.Sign(transform.forward.x);
            else
                player.primaryAxis = Vector3.forward * Mathf.Sign(transform.forward.z);
        } else if (player.surfaceNormal.z != 0) {
            if (Mathf.Abs(transform.forward.x) > Mathf.Abs(transform.forward.y))
                player.primaryAxis = Vector3.right * Mathf.Sign(transform.forward.x);
            else
                player.primaryAxis = Vector3.up * Mathf.Sign(transform.forward.y);
        }
        player.secondaryAxis = Vector3.Cross(player.surfaceNormal, player.primaryAxis);
    }
}
