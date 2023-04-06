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
    public InventorySystem inventorySystem;

    void OnGUI() {
        if (GUI.Button(new Rect(0, 40, 100, 20), "Powerup Effect")) {
            StartCoroutine(PowerupEffects());
        }
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
            inventorySystem = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventorySystem>();
            if (inventorySystem.inventory.Count < inventorySystem.maxInventorySize) {
                inventorySystem.AddItem(other.gameObject);
                Destroy(other.gameObject);
            }
        }
    }
}
