using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxController : MonoBehaviour
{
    public Material SkyboxMat;
    public float speed;
    private float t = 0;
    public Color StartingColor, EndColor;
    public AnimationCurve Curve;
    void OnGUI()
    {
        if (GUI.Button(new Rect(200, 0, 100, 20), "ColorChange"))
        {
            StartCoroutine(ColorChange());
        }
    }

    IEnumerator ColorChange()
    {

        while (t <= 1)
        {
            SkyboxMat.SetColor("_Color", EndColor);
            t += Time.deltaTime * speed;
            yield return null;
        }
    }
}
