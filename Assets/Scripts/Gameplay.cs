using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PowerupType {
    None,
    SpeedBoost,
    HealthBoost,
    Shield
}
public class Gameplay : MonoBehaviour {
    private Explode explode;

    [SerializeField] private float explodeTime, rebuildMultiplier;
    [SerializeField] private Camera cam1, cam2;
    [SerializeField] private GameObject player1, player2;

    [SerializeField] private int explodeInstances, rebuildInstances;
    private Vector3[] randomVectors = new Vector3[] {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };
    private float[] te, tr, explodeWaitTime, rebuildWaitTime;

    void Awake() {
        if (Pref.I.twoPlayers) {
            cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
            cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            cam2.transform.parent.parent.gameObject.SetActive(true);
            player2.SetActive(true);
        } else {
            cam1.rect = new Rect(0f, 0f, 1f, 1f);
            cam2.transform.parent.parent.gameObject.SetActive(false);
            player2.SetActive(false);
        }

        explode = GetComponent<Explode>();
        explodeWaitTime = new float[explodeInstances];
        rebuildWaitTime = new float[rebuildInstances];
        te = new float[explodeInstances];
        tr = new float[rebuildInstances];

        for (int i = 0; i < explodeInstances; i++) {
            te[i] = 0f;
            explodeWaitTime[i] = Random.Range(0f, explodeTime);
        }
        for (int i = 0; i < rebuildInstances; i++) {
            tr[i] = 0f;
            rebuildWaitTime[i] = Random.Range(0f, explodeTime) * rebuildMultiplier;
        }
    }
    void Update() {
        for (int i = 0; i < explodeInstances; i++) {
            te[i] += Time.deltaTime;
            if (te[i] > explodeWaitTime[i]) {
                StartCoroutine(explode.Shake(true, randomVectors[Random.Range(0, randomVectors.Length)]));
                te[i] = 0f;
                explodeWaitTime[i] = Random.Range(0f, explodeTime);
            }
        }
        for (int i = 0; i < rebuildInstances; i++) {
            tr[i] += Time.deltaTime;
            if (tr[i] > rebuildWaitTime[i]) {
                StartCoroutine(explode.Rebuild(randomVectors[Random.Range(0, randomVectors.Length)]));
                tr[i] = 0f;
                rebuildWaitTime[i] = Random.Range(0f, explodeTime) * rebuildMultiplier;
            }
        }
    }
}
