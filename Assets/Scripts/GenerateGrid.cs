using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGrid : MonoBehaviour {
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int size;
    // Start is called before the first frame update
    void Start() {
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    Instantiate(cubePrefab, new Vector3(x, y, z), Quaternion.identity, transform);
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
