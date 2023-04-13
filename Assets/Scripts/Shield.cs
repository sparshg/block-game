using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    public Transform camPos;
    Material mat;
    [SerializeField] AnimationCurve curve;
    [SerializeField] float speed;
    public MovePlayer player;
    [SerializeField] private float down;
    private void Awake() {
        mat = GetComponent<Renderer>().material;
    }
    void Update() {
        transform.forward = camPos.position - transform.position;
        transform.position = player.transform.position - player.surfaceNormal * down;
    }

    public IEnumerator DisolveShield(bool target, bool removeShield = false) {
        float t = 1 - mat.GetFloat("_Disolve");
        while (target && t < 1) {
            mat.SetFloat("_Disolve", 1 - curve.Evaluate(t));
            t += Time.deltaTime * speed;
            yield return null;
        }
        while (!target && t > 0) {
            mat.SetFloat("_Disolve", 1 - curve.Evaluate(t));
            t -= Time.deltaTime * speed;
            yield return null;
        }
        if (!target) {
            if (removeShield) player.shield = false;
            if (!player.isMoving && !player.CheckBelow()) player.Burst();
            Destroy(gameObject);
        }
    }
}
