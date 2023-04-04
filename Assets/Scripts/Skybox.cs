using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox : MonoBehaviour {
    [SerializeField] private float rotSpeed, colorSpeed;
    public Color initColor, endColor;
    public AnimationCurve curve;

    void Start() {
        RenderSettings.skybox.SetColor("_Tint", initColor);
    }
    void OnGUI() {
        if (GUI.Button(new Rect(200, 0, 100, 20), "ColorChange")) {
            StartCoroutine(ColorChange());
        }
    }

    IEnumerator ColorChange() {
        float t = 0;
        Color startingColor = RenderSettings.skybox.GetColor("_Tint");
        while (t <= 1) {
            RenderSettings.skybox.SetColor("_Tint", Color.Lerp(startingColor, endColor, curve.Evaluate(t)));
            t += Time.deltaTime * colorSpeed;
            yield return null;
        }
        RenderSettings.skybox.SetColor("_Tint", endColor);
    }
    void Update() {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotSpeed);
    }

}
