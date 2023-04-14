using System.Collections;
using UnityEngine;
using System.Linq;
using EZCameraShake;
using UnityEngine.SceneManagement;

public enum Controls {
    MousePriority,
    KeyboardPriority
}

public class MovePlayer : MonoBehaviour {
    [HideInInspector] public Vector3 surfaceNormal = Vector3.up;
    [HideInInspector] public Vector3 primaryAxis = Vector3.forward;
    [HideInInspector] public Vector3 secondaryAxis = Vector3.right;
    public Vector3 toVec;

    [SerializeField] private AudioClip burstClip, damageClip;
    public Material camMat;
    [ColorUsageAttribute(false, true)]
    public Color matColor;
    // private KeyCode lockKey = KeyCode.None;
    // private Vector3 lockPrimAxis, lockSecAxis, lockNormal;

    [Header("Keys")]
    public KeyCode upKey;
    public KeyCode downKey;
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode leftRot;
    public KeyCode rightRot;
    public KeyCode upRot;
    public KeyCode downRot;
    public Controls controls;

    [Header("Edge")]
    [SerializeField] private float edgeWidth;
    [SerializeField] private float edgeIntensity;
    [SerializeField] private float edgeSpeed;
    [SerializeField] private AnimationCurve edgeCurve;

    [Header("Initialize")]
    [SerializeField] private Vector2 startingPos;

    [Header("Movement")]
    [SerializeField] private AnimationCurve rotateCurve;
    public float rotateSpeed;

    [Header("Camera")]
    [SerializeField] private float camRotateSpeed;
    public CamRotate cam;


    [Header("Burst Shake")]
    [SerializeField] private float magnitude;
    [SerializeField] private float roughness;
    [SerializeField] private float fadeInTime;
    [SerializeField] private float fadeOutTime;

    [SerializeField] private float toAngleX, toAngleY = -20;
    private Material mat;
    private AudioSource audioSource;
    private float matT = 0;
    public MovePlayer player;
    public bool shield, isMoving = false;
    public float health, damage, hue;
    private Gameplay manager;
    // void OnGUI() {
    //     if (GUI.Button(new Rect(0, 20, 100, 20), "Burst")) {
    //         Burst();
    //     }
    // }

    public void Burst(bool checkShield = true, Material material = null) {
        if (checkShield && shield) return;
        CameraShaker.ShakeAll(magnitude, roughness, fadeInTime, fadeOutTime);
        manager.RestartGame();
        gameObject.SetActive(false);
        foreach (var i in CameraShaker.instanceList.Values) {
            foreach (var j in i.ShakeInstances) {
                j.StartFadeOut(1f);
            }
        }
        audioSource.PlayOneShot(burstClip);
        var burst = Instantiate(Resources.Load("Burst"), transform.position, Quaternion.FromToRotation(Vector3.up, surfaceNormal)) as GameObject;
        burst.GetComponent<ParticleSystemRenderer>().material = material ?? mat;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + primaryAxis * 4f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + secondaryAxis * 4f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + surfaceNormal);
    }

    void Start() {
        transform.localPosition = new Vector3(startingPos.x, Pref.I.size, startingPos.y);
        toVec = transform.position;
        mat = GetComponent<Renderer>().material;
        audioSource = cam.GetComponent<AudioSource>();
        Color.RGBToHSV(matColor, out hue, out _, out _);
        camMat.SetColor("_Color", Color.HSVToRGB(hue, 1, 0));
        mat.SetColor("_Color", matColor);
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<Gameplay>();
        if (Pref.I.twoPlayers) {
            health = 0.1f;
            player = GameObject.FindGameObjectsWithTag("Player").Where(x => x != gameObject).First().GetComponent<MovePlayer>();
        }
    }

    void Update() {
        if (controls == Controls.KeyboardPriority)
            keysPriorityControls();
        else
            mousePriorityControls();
        CheckHealth();
    }

    void CheckHealth() {
        if (!shield && (transform.position.x == Pref.I.size - 1 || transform.position.y == Pref.I.size - 1 || transform.position.z == Pref.I.size - 1 || transform.position.x == 0 || transform.position.y == 0 || transform.position.z == 0)) {
            if (matT < 1) {
                matT += Time.deltaTime * edgeSpeed;
                mat.SetFloat("_Intensity", edgeCurve.Evaluate(matT) * edgeIntensity);
                mat.SetFloat("_w", 1 - edgeCurve.Evaluate(matT) * edgeWidth);
                camMat.SetColor("_Color", Color.HSVToRGB(hue, 1, edgeCurve.Evaluate(matT)));
            }
        } else if (matT > health) {
            matT = Mathf.Max(health, matT - Time.deltaTime * edgeSpeed);
            mat.SetFloat("_Intensity", edgeCurve.Evaluate(matT) * edgeIntensity);
            mat.SetFloat("_w", 1 - edgeCurve.Evaluate(matT) * edgeWidth);
            camMat.SetColor("_Color", Color.HSVToRGB(hue, 1, edgeCurve.Evaluate(matT)));
        } else if (matT < health) {
            matT = Mathf.Min(health, matT + Time.deltaTime * edgeSpeed);
            mat.SetFloat("_Intensity", edgeCurve.Evaluate(matT) * edgeIntensity);
            mat.SetFloat("_w", 1 - edgeCurve.Evaluate(matT) * edgeWidth);
            camMat.SetColor("_Color", Color.HSVToRGB(hue, 1, edgeCurve.Evaluate(matT)));
        }
        if (matT >= 1) {
            matT = 0;
            health = 0;
            Burst(false);
        }
    }

    public bool CheckBelow() {
        bool didHit = Physics.Raycast(transform.position, -surfaceNormal, out RaycastHit hit, 2f);
        if (!didHit || !hit.collider.gameObject.CompareTag("Grid")) {
            return false;
        }
        return true;
    }

    public IEnumerator Roll(Vector3 anchor, Vector3 axis, Vector3 newNormal) {
        isMoving = true;
        bool checkedBelow = false;
        float angle = 90f, _angle = 0f, _rotateSpeed = rotateSpeed, t1 = rotateCurve.Evaluate(0), t;
        Vector3 belowCube = toVec - surfaceNormal;
        toVec = belowCube + newNormal;
        if (toVec.x == Pref.I.size || toVec.y == Pref.I.size || toVec.z == Pref.I.size || toVec.x < 0 || toVec.y < 0 || toVec.z < 0) {
            angle = 180f;
            _rotateSpeed *= 2f;
            cam.SetIntermediateRotation(surfaceNormal);
            if (player && player.toVec == toVec && !player.shield) {
                StartCoroutine(player.Roll(anchor - surfaceNormal, axis, -surfaceNormal));
                player.health += damage;
                audioSource.PlayOneShot(damageClip);
            }
        } else {
            toVec += surfaceNormal;
            if (player && player.toVec == toVec && !player.shield) {
                StartCoroutine(player.Roll(anchor + newNormal, axis, newNormal));
                player.health += damage;
                audioSource.PlayOneShot(damageClip);
            }
        }

        while (true) {
            _angle += _rotateSpeed * Time.deltaTime;
            t = rotateCurve.Evaluate(_angle / angle);
            if (_angle >= angle) break;
            if (!checkedBelow && t > 0.7f) {
                checkedBelow = true;
                if (!CheckBelow()) {
                    Burst();
                }
            }
            transform.RotateAround(anchor, axis, (t - t1) * angle);
            t1 = t;
            if (angle == 180f) surfaceNormal = (transform.position - belowCube).normalized;
            yield return null;
        }
        transform.position = toVec;
        transform.rotation = Quaternion.identity;
        if (angle == 180f)
            surfaceNormal = newNormal;

        yield return null;
        isMoving = false;
    }

    void keysPriorityControls() {
        if (Input.GetKeyDown(leftRot)) toAngleX -= 90f;
        if (Input.GetKeyDown(rightRot)) toAngleX += 90f;

        if (Input.GetKeyDown(upRot)) toAngleY += 20f;
        else if (Input.GetKeyDown(downRot)) toAngleY -= 35f;
        else if (Input.GetKeyUp(upRot) || Input.GetKeyUp(downRot)) toAngleY = 0f;

        cam.turn.x = Mathf.LerpAngle(cam.turn.x, toAngleX, Time.deltaTime * camRotateSpeed);
        cam.turn.y = Mathf.Lerp(cam.turn.y, toAngleY - 20f, Time.deltaTime * camRotateSpeed);

        if (isMoving) return;

        if (Input.GetKey(upKey)) RollUp();
        else if (Input.GetKey(downKey)) RollDown();
        else if (Input.GetKey(rightKey)) RollRight();
        else if (Input.GetKey(leftKey)) RollLeft();
    }

    void mousePriorityControls() {
        if (isMoving) return;
        if (Input.GetKey(upKey)) RollUp();
        else if (Input.GetKey(downKey)) RollDown();
        else if (Input.GetKey(rightKey)) RollRight();
        else if (Input.GetKey(leftKey)) RollLeft();
    }

    // void keysPriorityControls2() {
    //     if (Input.GetKeyUp(upKey) && lockKey == upKey || Input.GetKeyUp(downKey) && lockKey == downKey || Input.GetKeyUp(rightKey) && lockKey == rightKey || Input.GetKeyUp(leftKey) && lockKey == leftKey) {
    //         lockKey = KeyCode.None;
    //         lockPrimAxis = Vector3.zero;
    //     }

    //     if (Input.GetKeyDown(upKey)) {
    //         lockKey = upKey;
    //         lockPrimAxis = primaryAxis; lockSecAxis = secondaryAxis;

    //     } else if (Input.GetKeyDown(downKey)) {
    //         lockKey = downKey;
    //         lockPrimAxis = primaryAxis; lockSecAxis = secondaryAxis;

    //     } else if (Input.GetKeyDown(rightKey)) {
    //         lockKey = rightKey;
    //         lockPrimAxis = primaryAxis; lockSecAxis = secondaryAxis;

    //     } else if (Input.GetKeyDown(leftKey)) {
    //         lockKey = leftKey;
    //         lockPrimAxis = primaryAxis; lockSecAxis = secondaryAxis;
    //     }
    //     if (isMoving) return;

    //     if (surfaceNormal != lockNormal) {
    //         lockPrimAxis = primaryAxis; lockSecAxis = secondaryAxis;
    //     }

    //     if (lockKey == upKey) {
    //         StartCoroutine(Roll(transform.position + lockPrimAxis * 0.5f - surfaceNormal * 0.5f, lockSecAxis, lockPrimAxis));
    //     } else if (lockKey == downKey) {
    //         StartCoroutine(Roll(transform.position - lockPrimAxis * 0.5f - surfaceNormal * 0.5f, -lockSecAxis, -lockPrimAxis));
    //     } else if (lockKey == rightKey) {
    //         StartCoroutine(Roll(transform.position + lockSecAxis * 0.5f - surfaceNormal * 0.5f, -lockPrimAxis, lockSecAxis));
    //     } else if (lockKey == leftKey) {
    //         StartCoroutine(Roll(transform.position - lockSecAxis * 0.5f - surfaceNormal * 0.5f, lockPrimAxis, -lockSecAxis));
    //     }
    //     lockNormal = surfaceNormal;
    // }


    void RollUp() {
        StartCoroutine(Roll(transform.position + primaryAxis * 0.5f - surfaceNormal * 0.5f, secondaryAxis, primaryAxis));
    }
    void RollDown() {
        StartCoroutine(Roll(transform.position - primaryAxis * 0.5f - surfaceNormal * 0.5f, -secondaryAxis, -primaryAxis));
    }
    void RollRight() {
        StartCoroutine(Roll(transform.position + secondaryAxis * 0.5f - surfaceNormal * 0.5f, -primaryAxis, secondaryAxis));
    }
    void RollLeft() {
        StartCoroutine(Roll(transform.position - secondaryAxis * 0.5f - surfaceNormal * 0.5f, primaryAxis, -secondaryAxis));
    }
}