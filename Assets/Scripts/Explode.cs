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
    [SerializeField] private float explosionSpeed;

    Transform[] children;
    private MovePlayer player;

    void OnGUI() {
        if (GUI.Button(new Rect(0, 0, 100, 20), "Shake")) {
            StartCoroutine(Shake(false));
        }
        if (GUI.Button(new Rect(100, 0, 100, 20), "Explode")) {
            StartCoroutine(Shake(true));
        }
    }
    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MovePlayer>();
    }
    IEnumerator Shake(bool explode) {
        Vector2 toShake = new Vector2(Random.Range(0, Pref.I.size), Random.Range(0, Pref.I.size));
        Vector3 normal = player.surfaceNormal;
        Vector3 primaryAxis = player.primaryAxis;
        Vector3 secondaryAxis = player.secondaryAxis;
        var children = grid.GetComponentsInChildren<Transform>().Where(t => {
            if (normal.x == 1) {
                return t.localPosition.y == toShake.x && t.localPosition.z == toShake.y;
            } else if (normal.y == 1) {
                return t.localPosition.x == toShake.x && t.localPosition.z == toShake.y;
            } else {
                return t.localPosition.x == toShake.x && t.localPosition.y == toShake.y;
            }
        }).Skip(1).ToArray();
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
        int best = 0;
        Vector3 bestChild = Vector3.zero;

        // Find top grid block
        foreach (var child in children) {
            if (Vector3.Dot(normal, child.localPosition) > best) {
                best = (int)Vector3.Dot(normal, child.localPosition);
                bestChild = child.localPosition;
            }
        }

        // Cache Rigidbody
        Rigidbody[] rbs = new Rigidbody[children.Length];
        for (int i = 0; i < children.Length; i++) {
            rbs[i] = children[i].gameObject.GetComponent<Rigidbody>();
            rbs[i].isKinematic = false;
            rbs[i].AddForce(normal * 10f * Time.deltaTime, ForceMode.Impulse);
        }

        bool loop = true;
        while (loop) {
            loop = false;
            for (int i = 0; i < children.Length; i++) {
                if (Vector3.Dot(normal, children[i].localPosition) >= Vector3.Dot(normal, bestChild)) {
                    loop = true;
                } else if (rbs[i].drag == 0) {
                    rbs[i].drag = 1;
                    rbs[i].AddForce((normal * 10f + primaryAxis * Random.Range(-3f, 3f) + secondaryAxis * Random.Range(-3f, 3f)) * Time.deltaTime, ForceMode.Impulse);
                }
            }
            yield return null;
        }

        // Cache Paths
        // PathCreator[] pathCreators = new PathCreator[children.Length];

        // Set path for each grid block
        // for (int i = 0; i < 1; i++) {
        //     Vector2 r = Random.insideUnitCircle;
        //     Vector3[] waypoints = new Vector3[4];
        //     waypoints[0] = children[i].localPosition;
        //     waypoints[1] = bestChild;
        //     waypoints[2] = bestChild + 2f * normal + 1.5f * (primaryAxis * r.x + secondaryAxis * r.y);
        //     waypoints[3] = bestChild + 4f * normal + 2f * (primaryAxis * r.x + secondaryAxis * r.y);
        //     pathCreators[i] = children[i].gameObject.GetComponent<PathCreator>();
        //     pathCreators[i].bezierPath = new BezierPath(waypoints, false, PathSpace.xyz);
        // }

        // Follow path for each grid block
        // float t = 0, y = 0;
        // while (t < 1) {
        //     t += explosionSpeed * Time.deltaTime;
        //     y = explosionCurve.Evaluate(t);
        //     for (int i = 0; i < children.Length; i++) {
        //         children[i].localPosition = pathCreators[i].path.GetPointAtDistance(y, EndOfPathInstruction.Stop);
        //         children[i].localRotation = pathCreators[i].path.GetRotationAtDistance(y, EndOfPathInstruction.Stop);
        //         // transform.rotation = pc.path.GetRotationAtDistance(y, EndOfPathInstruction.Stop);
        //     }
        // yield return null;
        // }
    }
}
