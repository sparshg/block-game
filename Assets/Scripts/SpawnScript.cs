using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScript : MonoBehaviour
{
    public GameObject[] powerUpPrefab;
    public int maxPowerups = 10; // the maximum number of powerups to spawn
    public float spawnInterval = 5f; // the time interval between spawns
    public float hoverHeight = 0.25f; // the height above the cube to hover

    private int powerupCount = 0; // the current number of spawned powerups

    void Start()
    {
        // Check that the powerUpPrefab array is not empty
        if (powerUpPrefab == null || powerUpPrefab.Length == 0)
        {
            Debug.LogError("Powerup prefab array is empty or null. No powerups will spawn.");
            return;
        }

        // Calculate positions for each cube within the 8x8x8 cube
        StartCoroutine(SpawnPowerups());
    }



    IEnumerator SpawnPowerups()
    {
        while (powerupCount < maxPowerups)
        {
            // generate a random location within the grid
            float x = 0, y =0 , z=0;
            int a = Random.Range(0, 6);
            if (a == 0){ // top face - done
                x = Random.Range(0, 8);
                y = 7.25f + hoverHeight;
                z = Random.Range(0, 8);
            } else if (a == 1){ // bottom face - done
                x = Random.Range(0, 8);
                y = -0.25f - hoverHeight;
                z = Random.Range(0, 8);
            } else if (a == 2){ // back face - done
                x = Random.Range(0, 8);
                y = Random.Range(0, 8);
                z = 7.25f + hoverHeight;
            } else if (a == 3){ // front face - done
                x = Random.Range(0, 8);
                y = Random.Range(0, 8);
                z = -0.25f - hoverHeight;
            } else if (a == 4){ // right face
                x = 7.25f + hoverHeight;
                y = Random.Range(0, 8);
                z = Random.Range(0, 8);
            } else if (a == 5){ // left face
                x = -0.25f - hoverHeight;
                y = Random.Range(0, 8);
                z = Random.Range(0, 8);
            }

            Vector3 spawnPos = new Vector3(x, y, z);
            Collider[] colliders = Physics.OverlapSphere(spawnPos, 0.25f);

            // check if the location already has a powerup
            if (colliders.Length == 0)
            {
                // spawn the powerup prefab at the location
                if (a == 0){
                    Instantiate(powerUpPrefab[Random.Range(0, powerUpPrefab.Length)], spawnPos, Quaternion.Euler(0, 0, 0));
                } else if (a == 1){
                    Instantiate(powerUpPrefab[Random.Range(0, powerUpPrefab.Length)], spawnPos, Quaternion.Euler(180, 0, 0));
                } else if (a == 2){
                    Instantiate(powerUpPrefab[Random.Range(0, powerUpPrefab.Length)], spawnPos, Quaternion.Euler(90, 0, 0));
                } else if (a == 3){
                    Instantiate(powerUpPrefab[Random.Range(0, powerUpPrefab.Length)], spawnPos, Quaternion.Euler(270, 0, 0));
                } else if (a == 4){
                    Instantiate(powerUpPrefab[Random.Range(0, powerUpPrefab.Length)], spawnPos, Quaternion.Euler(0, 0, 270));
                } else if (a == 5){
                    Instantiate(powerUpPrefab[Random.Range(0, powerUpPrefab.Length)], spawnPos, Quaternion.Euler(0, 0, 90));
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
