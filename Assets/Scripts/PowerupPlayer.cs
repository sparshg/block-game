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
    private InventorySystem inventorySystem;
    private MovePlayer player;

    void OnGUI() {
        if (GUI.Button(new Rect(0, 40, 100, 20), "Powerup Effect")) {
            StartCoroutine(PowerupEffects());
        }
        if (GUI.Button(new Rect(100, 40, 100, 20), "Shield")) {
            StartCoroutine(SpawnShield());
        }
    }

    void Awake() {
        inventorySystem = GetComponent<InventorySystem>();
        player = GetComponent<MovePlayer>();
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
