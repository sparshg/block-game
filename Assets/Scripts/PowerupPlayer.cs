using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupPlayer : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Powerup")) {
            Destroy(other.gameObject);
        }
    }
}
