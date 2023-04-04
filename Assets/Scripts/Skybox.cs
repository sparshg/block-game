using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox : MonoBehaviour
{
    [SerializeField] private float speed;
    void Update() {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * speed);
    }
}
