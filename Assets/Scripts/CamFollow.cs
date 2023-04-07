using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour {
    [SerializeField] private MovePlayer player;
    public float elevation, followSpeed;
    void LateUpdate() {
        Vector3 targetPosition = player.transform.position + player.surfaceNormal * elevation;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

}
