using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour {
    public GameObject powerUpPrefab;
    public int maxPowerups = 10; // the maximum number of powerups to spawn
    public float spawnInterval = 5f; // the time interval between spawns
    public float hoverHeight = 0.25f; // the height above the cube to hover
    private int powerupCount = 0; // the current number of spawned powerups

    void Start() {
        StartCoroutine(SpawnPowerups());
    }

    IEnumerator SpawnPowerups() {
        while (powerupCount < maxPowerups) {
            // generate a random location within the grid
            Vector3 spawnPos = new Vector3(Random.Range(0, Pref.I.size), Random.Range(0, Pref.I.size), Random.Range(0, Pref.I.size));
            int a = Random.Range(0, 6);
            if (a == 0)  // top face
                spawnPos.y = Pref.I.size - 1 + hoverHeight;
            else if (a == 1)  // bottom face
                spawnPos.y = -hoverHeight;
            else if (a == 2)  // back face
                spawnPos.z = Pref.I.size - 1 + hoverHeight;
            else if (a == 3)  // front face
                spawnPos.z = -hoverHeight;
            else if (a == 4)  // right face
                spawnPos.x = Pref.I.size - 1 + hoverHeight;
            else if (a == 5)  // left face
                spawnPos.x = -hoverHeight;


            Collider[] colliders = Physics.OverlapSphere(spawnPos, 0.1f);
            Debug.Log(colliders.Length);
            Debug.Log(colliders[0].gameObject.transform.position);

            // check if the location already has a powerup
            if (colliders.Length == 0) {
                // spawn the powerup prefab at the location
                if (a == 0) {
                    Instantiate(powerUpPrefab, spawnPos, Quaternion.Euler(0, 0, 0), transform);
                } else if (a == 1) {
                    Instantiate(powerUpPrefab, spawnPos, Quaternion.Euler(180, 0, 0), transform);
                } else if (a == 2) {
                    Instantiate(powerUpPrefab, spawnPos, Quaternion.Euler(90, 0, 0), transform);
                } else if (a == 3) {
                    Instantiate(powerUpPrefab, spawnPos, Quaternion.Euler(270, 0, 0));
                } else if (a == 4) {
                    Instantiate(powerUpPrefab, spawnPos, Quaternion.Euler(0, 0, 270), transform);
                } else if (a == 5) {
                    Instantiate(powerUpPrefab, spawnPos, Quaternion.Euler(0, 0, 90), transform);
                }
                powerupCount++;
            } else {
                Debug.Log("Powerup already exists at " + spawnPos);
            }

            // wait for the spawn interval
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
