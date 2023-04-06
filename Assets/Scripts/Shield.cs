using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    Camera _cam;
    Material mat;
    [SerializeField] AnimationCurve curve;
    [SerializeField] float speed;
    public MovePlayer player;
    [SerializeField] private float down;
    private void Awake() {
        _cam = Camera.main;
        mat = GetComponent<Renderer>().material;
    }
    void Update() {
        transform.forward = _cam.transform.position - transform.position;
        transform.position = player.transform.position - player.surfaceNormal * down;
        // if (Input.GetKeyDown(KeyCode.F)) {
        //     player.shield = !player.shield;
        //     StartCoroutine(DisolveShield(player.shield));
        // }
    }

    public IEnumerator DisolveShield(bool target) {
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
            player.shield = false;
            if (!player.isMoving && !player.CheckBelow()) player.Burst();
            Destroy(gameObject);
        }
    }
}
