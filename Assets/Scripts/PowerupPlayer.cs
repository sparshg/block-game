using System.Collections;
using System.Collections.Generic;
using EZCameraShake;
using UnityEngine;

public class PowerupPlayer : MonoBehaviour {

    // public CameraShakeInstance cameraShakePresets;
    [Header("Powerup Shake")]
    [SerializeField] private float magnitude;
    [SerializeField] private float roughness;
    [SerializeField] private float fadeInTime;
    [SerializeField] private float fadeOutTime;

    [Header("Powerup Speed")]
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float fovSpeed;
    [SerializeField] private float fovMultiplier;

    [Header("Powerup Earthquake")]
    [SerializeField] private int quakeCount;
    [SerializeField] private float quakeWaitDuration;
    [Header("Powerup Rebuild")]
    [SerializeField] private int rebuildCount;
    [SerializeField] private float rebuildWaitDuration;

    private InventorySystem inventorySystem;
    private MovePlayer player;
    private Camera cam;
    private Explode explode;
    private Vector3[] randomVectors = new Vector3[] {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };
    void OnGUI() {
        if (GUI.Button(new Rect(0, 40, 100, 20), "Powerup Effect")) {
            StartCoroutine(PowerupEffects());
        }
        if (GUI.Button(new Rect(100, 40, 100, 20), "Shield")) {
            StartCoroutine(SpawnShield());
        }
        if (GUI.Button(new Rect(200, 40, 100, 20), "Speed")) {
            StartCoroutine(Speed());
        }
        if (GUI.Button(new Rect(0, 60, 100, 20), "Earthquake")) {
            StartCoroutine(Earthquake());
        }
        if (GUI.Button(new Rect(100, 60, 100, 20), "RebuildPowerup")) {
            StartCoroutine(Rebuild());
        }
    }

    void Awake() {
        explode = GameObject.FindGameObjectWithTag("GameController").GetComponent<Explode>();
        inventorySystem = GetComponent<InventorySystem>();
        player = GetComponent<MovePlayer>();
        cam = Camera.main;
    }



    IEnumerator Rebuild() {
        CameraShakeInstance s = CameraShaker.Instance.StartShake(2f, 20f, 2f);
        for (int i = 0; i < rebuildCount; i++) {
            StartCoroutine(explode.Rebuild(randomVectors[Random.Range(0, randomVectors.Length)]));
            yield return new WaitForSeconds(rebuildWaitDuration);
        }
        yield return new WaitForSeconds(2f);
        s.StartFadeOut(2f);

    }

    IEnumerator Earthquake() {
        CameraShaker.Instance.Shake(CameraShakePresets.Earthquake);
        for (int i = 0; i < quakeCount; i++) {
            StartCoroutine(explode.Shake(true, randomVectors[Random.Range(0, randomVectors.Length)]));
            yield return new WaitForSeconds(quakeWaitDuration);
        }
    }

    IEnumerator Speed() {
        player.rotateSpeed *= speedMultiplier;
        float init = cam.fieldOfView, final = init * fovMultiplier;
        while (cam.fieldOfView < final - 1f) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, final, Time.deltaTime * fovSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(5f);
        player.rotateSpeed /= speedMultiplier;
        while (cam.fieldOfView > init + 1f) {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, init, Time.deltaTime * fovSpeed);
            yield return null;
        }
        cam.fieldOfView = init;
    }

    IEnumerator SpawnShield() {
        var shield = (Instantiate(Resources.Load("Shield")) as GameObject).GetComponent<Shield>();
        shield.player = player;
        player.shield = true;
        StartCoroutine(shield.DisolveShield(true));
        yield return new WaitForSeconds(5f);
        StartCoroutine(shield.DisolveShield(false));
    }

    IEnumerator PowerupEffects() {
        CameraShaker.Instance.ShakeOnce(magnitude, roughness, fadeInTime, fadeOutTime);
        yield return new WaitForSeconds(2f);
        // CameraShaker.Instance.Shake(CameraShakePresets.Bump);
        // yield return new WaitForSeconds(2f);
        // CameraShaker.Instance.Shake(CameraShakePresets.Earthquake);
        // yield return new WaitForSeconds(2f);
        // CameraShaker.Instance.Shake(CameraShakePresets.Explosion);
        // yield return new WaitForSeconds(2f);
        // CameraShaker.Instance.Shake(CameraShakePresets.Vibration);
        // yield return new WaitForSeconds(2f);
        // CameraShaker.Instance.Shake(CameraShakePresets.RoughDriving);
        // yield return new WaitForSeconds(2f);
        // CameraShaker.Instance.Shake(CameraShakePresets.HandheldCamera);
    }
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Powerup")) {
            if (inventorySystem.inventory.Count < inventorySystem.maxInventorySize) {
                // inventorySystem.AddItem(other.gameObject);
                Destroy(other.gameObject);
            }
        }
    }
}
