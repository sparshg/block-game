using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {
    private MovePlayer player;
    private Vector3 gridMid, normal;
    private float t = 0;
    [SerializeField] private float height, speed;
    [SerializeField] private AnimationCurve curve;
    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MovePlayer>();
    }
    void Start() {
        gridMid = new Vector3((Pref.I.size - 1) * 0.5f, (Pref.I.size - 1) * 0.5f, (Pref.I.size - 1) * 0.5f);
        normal = player.surfaceNormal;

        transform.localPosition = normal * height + gridMid;
        transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
    }

    void Update() {
        if (t >= 1) {
            Destroy(gameObject);
            return;
        }
        transform.localPosition -= 2f * height * (curve.Evaluate(t + Time.deltaTime * speed) - curve.Evaluate(t)) * normal;
        t += Time.deltaTime * speed;

    }
}
