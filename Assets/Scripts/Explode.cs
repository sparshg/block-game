using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathCreation;
using UnityEngine;

public class Explode : MonoBehaviour {

    [SerializeField] private GenerateGrid grid;
    [SerializeField][Range(0.0f, 0.3f)] private float shakeAmount = 0.1f;
    [SerializeField][Range(0.0f, 0.1f)] private float shakeIntervals = 0.02f;
    [SerializeField][Range(0, 200)] private int shakeCount = 50;
    [SerializeField] private AnimationCurve rebuildCurve;
    [SerializeField] private float explosionSpeed1, explosionSpeed2, explosionRotation, explosionDrag, rebuildSpeed;

    List<Transform[]>[] detachedCubes = new List<Transform[]>[3];
    List<Vector3[][]>[] detachedCubesSplines = new List<Vector3[][]>[3];
    Dictionary<Vector3, int> map = new Dictionary<Vector3, int>() {
        {Vector3.right, 0},
        {Vector3.left, 0},
        {Vector3.back, 1},
        {Vector3.forward, 1},
        {Vector3.up, 2},
        {Vector3.down, 2},
    };
    private MovePlayer player;

    void OnGUI() {
        if (GUI.Button(new Rect(0, 0, 100, 20), "Shake")) {
            StartCoroutine(Shake(false));
        }
        if (GUI.Button(new Rect(100, 0, 100, 20), "Explode")) {
            StartCoroutine(Shake(true));
        }
        if (GUI.Button(new Rect(100, 20, 100, 20), "Rebuild")) {
            StartCoroutine(Rebuild());
        }
    }
    void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<MovePlayer>();
    }
    void Start() {
        for (int i = 0; i < 3; i++) {
            detachedCubes[i] = new List<Transform[]>();
            detachedCubesSplines[i] = new List<Vector3[][]>();
        }
    }

    // only call when normal is one of the 6 directions
    IEnumerator Shake(bool explode) {
        Vector2 toShake = new Vector2(Random.Range(1, Pref.I.size - 1), Random.Range(1, Pref.I.size - 1));
        Vector3 normal = player.surfaceNormal;
        Vector3 primaryAxis = player.primaryAxis;
        Vector3 secondaryAxis = player.secondaryAxis;
        Transform[] children = grid.GetComponentsInChildren<Transform>().Where(t => {
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
        int max = int.MinValue;
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
        Vector3[][] way = new Vector3[children.Length][];
        for (int i = 0; i < children.Length; i++) {
            rbs[i] = children[i].gameObject.GetComponent<Rigidbody>();
            rbs[i].isKinematic = false;
            // if closer to minChild
            if (Vector3.Dot(-normal, minChild - children[i].localPosition) < Vector3.Dot(normal, maxChild - children[i].localPosition)) {
                rbs[i].AddForce(-normal * explosionSpeed1, ForceMode.Impulse);
                if (children[i].localPosition != minChild) {
                    way[i] = new Vector3[3];
                    way[i][1] = minChild;
                } else {
                    way[i] = new Vector3[2];
                }
            } else {
                rbs[i].AddForce(normal * explosionSpeed1, ForceMode.Impulse);
                if (children[i].localPosition != maxChild) {
                    way[i] = new Vector3[3];
                    way[i][1] = maxChild;
                } else {
                    way[i] = new Vector3[2];
                }
            }
            way[i][0] = children[i].localPosition;
        }
        detachedCubes[map[normal]].Add(children);
        detachedCubesSplines[map[normal]].Add(way);

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

    IEnumerator Rebuild() {
        Vector3 normal = player.surfaceNormal;
        int rand = Random.Range(0, detachedCubes[map[normal]].Count);
        Transform[] children = detachedCubes[map[normal]][rand];
        Vector3[][] way = detachedCubesSplines[map[normal]][rand];
        detachedCubes[map[normal]].RemoveAt(rand);
        detachedCubesSplines[map[normal]].RemoveAt(rand);

        GameObject[] pc = new GameObject[children.Length];
        Rigidbody[] rbs = new Rigidbody[children.Length];

        for (int i = 0; i < children.Length; i++) {
            rbs[i] = children[i].gameObject.GetComponent<Rigidbody>();
            way[i][way[i].Length - 1] = children[i].localPosition;
            children[i].gameObject.GetComponent<BoxCollider>().enabled = false;
            rbs[i].isKinematic = true;
            rbs[i].drag = 0;
            pc[i] = new GameObject();
            pc[i].SetActive(false);
            var path = pc[i].AddComponent<PathCreator>().bezierPath = new BezierPath(way[i], false, PathSpace.xyz);
            path.ControlPointMode = BezierPath.ControlMode.Free;
            if (path.NumAnchorPoints > 2) {
                path.SetPoint(1, (way[i][0] + way[i][1]) * 0.5f);
                path.SetPoint(2, (way[i][0] + way[i][1]) * 0.5f);
                path.SetPoint(4, way[i][1] + normal * Vector3.Dot(way[i][2] - way[i][1], normal) * 0.5f);
            } else {
                path.SetPoint(1, way[i][0] + normal * Vector3.Dot(way[i][1] - way[i][0], normal) * 0.5f);
                path.SetPoint(2, path.GetPoint(1));
            }
        }

        float t = 1;
        bool finished = false;
        while (t >= 0) {
            for (int i = 0; i < children.Length; i++) {
                var pathCreator = pc[i].GetComponent<PathCreator>();
                children[i].position = pathCreator.path.GetPointAtTime(rebuildCurve.Evaluate(t), EndOfPathInstruction.Stop);
                children[i].rotation = pathCreator.path.GetRotation(rebuildCurve.Evaluate(t), EndOfPathInstruction.Stop);
            }
            yield return null;
            t -= rebuildSpeed * Time.deltaTime;
            if (t < 0 && !finished) {
                t = 0;
                finished = true;
            }
        }
        foreach (var child in children) {
            child.localPosition = new Vector3(
                Mathf.RoundToInt(child.localPosition.x),
                Mathf.RoundToInt(child.localPosition.y),
                Mathf.RoundToInt(child.localPosition.z)
            );
        }
        foreach (var p in pc) Destroy(p);
    }
}
