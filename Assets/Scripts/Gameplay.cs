using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
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
    [SerializeField] private MovePlayer player1, player2;

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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Screen.SetResolution(1440, 900, true);
        if (Pref.I.twoPlayers) {
            cam1.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            cam2.rect = new Rect(0f, 0f, 0.5f, 1f);
            player1.upKey = KeyCode.I;
            player1.downKey = KeyCode.K;
            player1.leftKey = KeyCode.J;
            player1.rightKey = KeyCode.L;
            player1.leftRot = KeyCode.U;
            player1.rightRot = KeyCode.O;
            player1.upRot = KeyCode.Alpha8;
            player1.downRot = KeyCode.Comma;
            player1.controls = Controls.KeyboardPriority;
            cam2.transform.parent.parent.gameObject.SetActive(true);
            player2.gameObject.SetActive(true);
        } else {
            cam1.rect = new Rect(0f, 0f, 1f, 1f);
            cam2.transform.parent.parent.gameObject.SetActive(false);
            player2.gameObject.SetActive(false);
        }
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;

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
    public void RestartGame() {
        StartCoroutine(Restart());
    }

    public IEnumerator Restart() {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void Update() {
        if (player1.pushedOther) {
            player2.ControlledUpdate();
            player1.ControlledUpdate();
        } else {
            player1.ControlledUpdate();
            player2.ControlledUpdate();
        }
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
