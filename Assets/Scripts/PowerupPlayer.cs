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
    private CamFollow camFollow;
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
        cam = player.cam.GetComponent<Camera>();
        camFollow = player.cam.transform.parent.GetComponent<CamFollow>();
    }



    IEnumerator Rebuild() {
        // CameraShaker.ShakeAll(CameraShakePresets);
        for (int i = 0; i < rebuildCount; i++) {
            StartCoroutine(explode.Rebuild(randomVectors[Random.Range(0, randomVectors.Length)]));
            yield return new WaitForSeconds(rebuildWaitDuration);
        }

    }

    IEnumerator Earthquake() {
        List<CameraShakeInstance> s = new List<CameraShakeInstance>();
        foreach (var i in CameraShaker.instanceList.Values) {
            s.Add(i.StartShake(2f, 20f, 2f));
        }
        for (int i = 0; i < quakeCount; i++) {
            StartCoroutine(explode.Shake(true, randomVectors[Random.Range(0, randomVectors.Length)]));
            yield return new WaitForSeconds(quakeWaitDuration);
        }
        yield return new WaitForSeconds(2f);
        foreach (var i in s) i.StartFadeOut(1.5f);

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
        shield.camPos = cam.transform;
        player.shield = true;
        StartCoroutine(shield.DisolveShield(true));
        yield return new WaitForSeconds(5f);
        StartCoroutine(shield.DisolveShield(false));
    }

    IEnumerator PowerupEffects() {
        CameraShaker.ShakeAll(magnitude, roughness, fadeInTime, fadeOutTime);
        camFollow.transform.position -= player.surfaceNormal;
        camFollow.followSpeed /= 2f;
        yield return new WaitForSeconds(1f);
        camFollow.followSpeed *= 2f;


    }
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Powerup")) {
            if (inventorySystem.inventory.Count < inventorySystem.maxInventorySize) {
                StartCoroutine(PowerupEffects());
                switch (Random.Range(0, 4)) {
                    case 0:
                        StartCoroutine(SpawnShield());
                        break;
                    case 1:
                        StartCoroutine(Speed());
                        break;
                    case 2:
                        StartCoroutine(Earthquake());
                        break;
                    case 3:
                        StartCoroutine(Rebuild());
                        break;
                }
                // inventorySystem.AddItem(other.gameObject);
                Destroy(other.gameObject);
            }
        }
    }
}
