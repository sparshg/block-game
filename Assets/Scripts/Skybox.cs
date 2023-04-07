using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox : MonoBehaviour {
    [SerializeField] private float rotSpeed, colorSpeed;

    [SerializeField] private Color initColor, quakeColor, rebuildColor;
    [SerializeField] private float rebuildSpeed, quakeSpeed;
    public AnimationCurve curve;

    void Start() {
        RenderSettings.skybox.SetColor("_Tint", initColor);
    }
    public void QuakeEffect() {
        StopAllCoroutines();
        StartCoroutine(ColorChange(quakeColor, quakeSpeed));
    }
    public void RebuildEffect() {
        StopAllCoroutines();
        StartCoroutine(ColorChange(rebuildColor, rebuildSpeed));
    }

    public void ResetColor() {
        StopAllCoroutines();
        StartCoroutine(ColorChange(initColor, colorSpeed));
    }

    IEnumerator ColorChange(Color end, float speed) {
        float t = 0;
        Color startingColor = RenderSettings.skybox.GetColor("_Tint");
        while (t <= 1) {
            RenderSettings.skybox.SetColor("_Tint", Color.Lerp(startingColor, end, curve.Evaluate(t)));
            t += Time.deltaTime * speed;
            yield return null;
        }
        RenderSettings.skybox.SetColor("_Tint", end);
    }
    void Update() {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotSpeed);
    }

}
