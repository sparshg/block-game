using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EZCameraShake;
using PathCreation;
using UnityEngine;

public class Explode : MonoBehaviour {

    [SerializeField] private GenerateGrid grid;
    [SerializeField] private AudioClip[] explodeClips;
    [SerializeField] private AudioClip rebuildClip;
    [SerializeField][Range(0.0f, 0.3f)] private float shakeAmount = 0.1f;

    [Header("Explosion Shake")]
    [SerializeField] private float magnitude;
    [SerializeField] private float roughness;
    [SerializeField] private float fadeInTime;
    [SerializeField] private float fadeOutTime;

    [SerializeField] private AnimationCurve rebuildCurve, colorCurve;
    [Header("Explosion Color")]
    [SerializeField] private float colorSpeed;
    [SerializeField] private float colorIntensity;

    [Header("Explosion")]
    [SerializeField] private float explosionSpeed1;
    [SerializeField] private float explosionSpeed2;
    [SerializeField] private float explosionRotation;
    [SerializeField] private float explosionDrag;
    [SerializeField] private float rebuildSpeed;

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

    // void OnGUI() {
    //     if (GUI.Button(new Rect(100, 0, 100, 20), "Explode")) {
    //         StartCoroutine(Shake(true, player.surfaceNormal));
    //     }
    //     if (GUI.Button(new Rect(100, 20, 100, 20), "Rebuild")) {
    //         StartCoroutine(Rebuild(player.surfaceNormal));
    //     }
    // }
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
    public IEnumerator Shake(bool explode, Vector3 normal) {
        Vector2 toShake = new Vector2(Random.Range(1, Pref.I.size - 1), Random.Range(1, Pref.I.size - 1));
        Transform[] children = grid.GetComponentsInChildren<Transform>().Where(t => {
            if (normal.x == 1 || normal.x == -1) {
                return t.localPosition.y == toShake.x && t.localPosition.z == toShake.y && t != transform;
            } else if (normal.y == 1 || normal.y == -1) {
                return t.localPosition.x == toShake.x && t.localPosition.z == toShake.y && t != transform;
            } else {
                return t.localPosition.x == toShake.x && t.localPosition.y == toShake.y && t != transform;
            }
        }).ToArray();
        if (children.Length == 0) yield break;


        Material[] mats = new Material[children.Length];
        for (int i = 0; i < children.Length; i++) {
            mats[i] = children[i].GetComponent<Renderer>().material;
        }
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime * colorSpeed;
            foreach (var mat in mats) {
                mat.SetFloat("_Intensity", colorCurve.Evaluate(t) * colorIntensity);
            }
            Vector3 shake = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
            foreach (var child in children) {
                child.localPosition += shake * colorCurve.Evaluate(t);
            }
            yield return null;
            foreach (var child in children) {
                child.localPosition = Vector3Int.RoundToInt(child.localPosition);
            }
        }

        if (explode) {
            StartCoroutine(Explosion(children, normal));
            // voids[map[normal]].Add(toShake);
        }
    }
    IEnumerator Explosion(Transform[] children, Vector3 normal) {
        int max = int.MinValue, min = int.MaxValue;
        Transform maxObject = null, minObject = null;

        // Find top grid block
        foreach (var child in children) {
            if (Vector3.Dot(normal, child.localPosition) > max) {
                max = (int)Vector3.Dot(normal, child.localPosition);
                maxObject = child;
            }
            if (Vector3.Dot(normal, child.localPosition) < min) {
                min = (int)Vector3.Dot(normal, child.localPosition);
                minObject = child;
            }
        }
        if (maxObject == null || minObject == null) {
            Debug.Log(children.Length);

            yield break;
        }
        Vector3 maxChild = maxObject.localPosition, minChild = minObject.localPosition;
        // Cache Rigidbody
        Rigidbody[] rbs = new Rigidbody[children.Length];
        Vector3[][] way = new Vector3[children.Length][];
        for (int i = 0; i < children.Length; i++) {
            rbs[i] = children[i].GetComponent<Rigidbody>();
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
        CameraShaker.ShakeAll(magnitude, roughness, fadeInTime, fadeOutTime);
        var r = explodeClips[Random.Range(0, explodeClips.Length)];
        var a1 = maxObject.GetComponent<AudioSource>();
        var a2 = minObject.GetComponent<AudioSource>();
        a1.enabled = true;
        a2.enabled = true;
        a1.PlayOneShot(r);
        a2.PlayOneShot(r);

        bool loop = true;
        while (loop) {
            loop = false;
            for (int i = 0; i < children.Length; i++) {
                if (Vector3.Dot(normal, maxChild - children[i].localPosition) >= 0 && Vector3.Dot(normal, minChild - children[i].localPosition) <= 0) {
                    if (Physics.Raycast(maxChild, normal, out RaycastHit h, 2f)) {
                        if (h.collider.CompareTag("Player")) {
                            h.collider.GetComponent<MovePlayer>().Burst(material: children[i].GetComponent<Renderer>().material);
                        } else if (h.collider.tag == "Powerup") {
                            Destroy(h.collider.gameObject);
                        }
                    } else if (Physics.Raycast(minChild, -normal, out RaycastHit h2, 2f)) {
                        if (h2.collider.CompareTag("Player")) {
                            h2.collider.GetComponent<MovePlayer>().Burst(material: children[i].GetComponent<Renderer>().material);
                        } else if (h2.collider.tag == "Powerup") {
                            Destroy(h2.collider.gameObject);
                        }
                    }
                    loop = true;
                } else if (rbs[i].drag == 0) {
                    rbs[i].drag = explosionDrag;
                    rbs[i].AddForce(Vector3.Cross(Random.insideUnitSphere, normal)
 * explosionSpeed2, ForceMode.Impulse);
                    rbs[i].AddTorque(Random.insideUnitSphere * explosionRotation, ForceMode.Impulse);
                    rbs[i].maxAngularVelocity = explosionRotation;
                    rbs[i].rotation = Random.rotation;
                    rbs[i].GetComponent<BoxCollider>().isTrigger = false;
                }
            }
            yield return null;
        }
    }

    public IEnumerator Rebuild(Vector3 normal) {
        if (detachedCubes[map[normal]].Count == 0) yield break;
        int rand = Random.Range(0, detachedCubes[map[normal]].Count);
        Transform[] children = detachedCubes[map[normal]][rand];
        Vector3[][] way = detachedCubesSplines[map[normal]][rand];
        detachedCubes[map[normal]].RemoveAt(rand);
        detachedCubesSplines[map[normal]].RemoveAt(rand);

        PathCreator[] pc = new PathCreator[children.Length];
        Rigidbody[] rbs = new Rigidbody[children.Length];
        Material[] mats = new Material[children.Length];

        for (int i = 0; i < children.Length; i++) {
            rbs[i] = children[i].GetComponent<Rigidbody>();
            mats[i] = children[i].GetComponent<Renderer>().material;
            way[i][way[i].Length - 1] = children[i].localPosition;
            children[i].GetComponent<BoxCollider>().isTrigger = true;
            rbs[i].isKinematic = true;
            rbs[i].drag = 0;
            GameObject g = new GameObject();
            g.SetActive(false);
            var path = g.AddComponent<PathCreator>().bezierPath = new BezierPath(way[i], false, PathSpace.xyz);
            pc[i] = g.GetComponent<PathCreator>();
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
        children[0].GetComponent<AudioSource>().PlayOneShot(rebuildClip);
        children[children.Length - 1].GetComponent<AudioSource>().PlayOneShot(rebuildClip);
        float t = 1;
        bool finished = false;
        while (t >= 0) {
            for (int i = 0; i < children.Length; i++) {
                try {
                    children[i].position = pc[i].path.GetPointAtTime(rebuildCurve.Evaluate(t), EndOfPathInstruction.Stop);
                } catch (System.Exception e) {
                    Debug.Log(way[i]);
                    Debug.Log(e);
                }
                children[i].rotation = pc[i].path.GetRotation(rebuildCurve.Evaluate(t), EndOfPathInstruction.Stop);
                mats[i].SetFloat("_Intensity", rebuildCurve.Evaluate(t) * colorIntensity);
            }
            yield return null;
            t -= rebuildSpeed * Time.deltaTime;
            if (t < 0 && !finished) {
                t = 0;
                finished = true;
            }
        }
        for (int i = 0; i < children.Length; i++) {
            children[i].localPosition = Vector3Int.RoundToInt(children[i].localPosition);

            mats[i].SetFloat("_Intensity", 0f);
            Destroy(pc[i].gameObject);
        }
    }
}
