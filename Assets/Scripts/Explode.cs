using System.Collections;
using System.Linq;
using PathCreation;
using UnityEngine;

public class Explode : MonoBehaviour {

    [SerializeField] private GenerateGrid grid;
    [SerializeField][Range(0.0f, 0.3f)] private float shakeAmount = 0.1f;
    [SerializeField][Range(0.0f, 0.1f)] private float shakeIntervals = 0.02f;
    [SerializeField][Range(0, 200)] private int shakeCount = 50;
    [SerializeField] private AnimationCurve explosionCurve;
    [SerializeField] private float explosionSpeed1, explosionSpeed2, explosionRotation, explosionDrag;

    Transform[] children;
    private MovePlayer player;

    void OnGUI() {
        if (GUI.Button(new Rect(0, 0, 100, 20), "Shake")) {
            StartCoroutine(Shake(false));
        }
        if (GUI.Button(new Rect(100, 0, 100, 20), "Explode")) {
            StartCoroutine(Shake(true));
        }
        if (GUI.Button(new Rect(100, 20, 100, 20), "Rebuild")) {
            // StartCoroutine(Rebuild());
        }
    }
    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MovePlayer>();
    }
    IEnumerator Shake(bool explode) {
        Vector2 toShake = new Vector2(Random.Range(1, Pref.I.size - 1), Random.Range(1, Pref.I.size - 1));
        Vector3 normal = player.surfaceNormal;
        Vector3 primaryAxis = player.primaryAxis;
        Vector3 secondaryAxis = player.secondaryAxis;
        var children = grid.GetComponentsInChildren<Transform>().Where(t => {
            if (normal.x == 1 || normal.x == -1) {
                return t.localPosition.y == toShake.x && t.localPosition.z == toShake.y && t != transform;
            } else if (normal.y == 1 || normal.y == -1) {
                return t.localPosition.x == toShake.x && t.localPosition.z == toShake.y && t != transform;
            } else {
                return t.localPosition.x == toShake.x && t.localPosition.y == toShake.y && t != transform;
            }
        }).ToArray();
        for (int i = 0; i < shakeCount; i++) {
            Vector3 shake = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
            foreach (var child in children) {
                child.localPosition += shake;
            }
            yield return new WaitForSeconds(shakeIntervals);
            foreach (var child in children) {
                child.localPosition = new Vector3(
                    Mathf.RoundToInt(child.localPosition.x),
                    Mathf.RoundToInt(child.localPosition.y),
                    Mathf.RoundToInt(child.localPosition.z)
                );
            }
        }
        if (explode) {
            StartCoroutine(Explosion(children, normal, primaryAxis, secondaryAxis));
        }
    }
    IEnumerator Explosion(Transform[] children, Vector3 normal, Vector3 primaryAxis, Vector3 secondaryAxis) {
        int max = 0;
        Vector3 maxChild = Vector3.zero, minChild;

        // Find top grid block
        foreach (var child in children) {
            if (Vector3.Dot(normal, child.localPosition) > max) {
                max = (int)Vector3.Dot(normal, child.localPosition);
                maxChild = child.localPosition;
            }
        }

        minChild = maxChild - normal * (Pref.I.size - 1);

        // Cache Rigidbody
        Rigidbody[] rbs = new Rigidbody[children.Length];
        for (int i = 0; i < children.Length; i++) {
            rbs[i] = children[i].gameObject.GetComponent<Rigidbody>();
            rbs[i].isKinematic = false;
            // if closer to minChild
            if (Vector3.Dot(-normal, minChild - children[i].localPosition) < Vector3.Dot(normal, maxChild - children[i].localPosition)) {
                rbs[i].AddForce(-normal * explosionSpeed1, ForceMode.Impulse);
            } else {
                rbs[i].AddForce(normal * explosionSpeed1, ForceMode.Impulse);
            }
        }

        bool loop = true;
        while (loop) {
            loop = false;
            for (int i = 0; i < children.Length; i++) {
                if (Vector3.Dot(normal, maxChild - children[i].localPosition) >= 0 && Vector3.Dot(normal, minChild - children[i].localPosition) <= 0) {
                    loop = true;
                } else if (rbs[i].drag == 0) {
                    rbs[i].drag = explosionDrag;
                    rbs[i].AddForce((primaryAxis * Random.Range(-1f, 1f) + secondaryAxis * Random.Range(-1f, 1f)) * explosionSpeed2, ForceMode.Impulse);
                    rbs[i].AddTorque(Random.insideUnitSphere * explosionRotation, ForceMode.Impulse);
                    rbs[i].maxAngularVelocity = explosionRotation;
                    rbs[i].rotation = Random.rotation;
                    rbs[i].gameObject.GetComponent<BoxCollider>().enabled = true;
                }
            }
            yield return null;
        }
    }
}
