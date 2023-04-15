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
    private Vector3 toVec, anchor, axis, toNorm;
    private float angle = 90f, _angle = 0f;

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
    public bool shield, isMoving = false, pushedOther = false;
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
        toNorm = surfaceNormal;
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

    public void ControlledUpdate() {
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
        _angle = 0f;
        float _rotateSpeed = rotateSpeed, t1 = rotateCurve.Evaluate(0), t;
        Vector3 belowCube = toVec - newNormal;
        if (toVec.x == Pref.I.size || toVec.y == Pref.I.size || toVec.z == Pref.I.size || toVec.x < 0 || toVec.y < 0 || toVec.z < 0) {
            toNorm = newNormal;
            _rotateSpeed *= 2f;
            cam.SetIntermediateRotation(surfaceNormal);
            if (player && player.toVec == toVec && !player.shield) {
                pushedOther = true;
                player.pushedOther = false;
                player.InterruptRoll(anchor - surfaceNormal, axis, -surfaceNormal);
                player.health += damage;
                audioSource.PlayOneShot(damageClip);
            }
        } else {
            toVec += toNorm;
            if (player && player.toVec == toVec && !player.shield) {
                pushedOther = true;
                player.pushedOther = false;
                player.InterruptRoll(anchor + newNormal, axis, newNormal);
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
            // Quaternion.RotateTowards(transform.rotation, Quaternion.identity, _angle)
            transform.RotateAround(anchor, axis, (t - t1) * angle);
            t1 = t;
            if (toNorm != surfaceNormal) surfaceNormal = (transform.position - belowCube).normalized;
            yield return null;
        }
        transform.position = toVec;
        surfaceNormal = toNorm;
        transform.rotation = Quaternion.identity;

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

        mousePriorityControls();
    }

    void mousePriorityControls() {
        if (isMoving) return;
        Vector3 _newNormal;
        if (Input.GetKey(upKey)) RollUp(out anchor, out axis, out _newNormal);
        else if (Input.GetKey(downKey)) RollDown(out anchor, out axis, out _newNormal);
        else if (Input.GetKey(rightKey)) RollRight(out anchor, out axis, out _newNormal);
        else if (Input.GetKey(leftKey)) RollLeft(out anchor, out axis, out _newNormal);
        else return;
        toVec += _newNormal - surfaceNormal;
        if (toVec.x == Pref.I.size || toVec.y == Pref.I.size || toVec.z == Pref.I.size || toVec.x < 0 || toVec.y < 0 || toVec.z < 0) angle = 180f;
        else angle = 90f;
        StartCoroutine(Roll(anchor, axis, _newNormal));
    }

    void InterruptRoll(Vector3 _anchor, Vector3 _axis, Vector3 _newNormal) {
        float _newAngle = 90f;
        toVec += _newNormal - toNorm;
        if (toVec.x == Pref.I.size || toVec.y == Pref.I.size || toVec.z == Pref.I.size || toVec.x < 0 || toVec.y < 0 || toVec.z < 0) {
            _newAngle = 180f;
        }
        if (isMoving) {
            StopAllCoroutines();
            (Quaternion.AngleAxis(_newAngle, _axis) * Quaternion.AngleAxis(angle - rotateCurve.Evaluate(_angle / angle) * angle, axis)).ToAngleAxis(out _newAngle, out _axis);
            if (axis != -_axis) anchor += _newNormal * 0.5f;
        } else {
            anchor = _anchor;
        }

        angle = _newAngle;
        axis = _axis;
        StartCoroutine(Roll(anchor, axis, _newNormal));

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


    void RollUp(out Vector3 _anchor, out Vector3 _axis, out Vector3 _newNormal) {
        _anchor = toVec + primaryAxis * 0.5f - surfaceNormal * 0.5f;
        _axis = secondaryAxis;
        _newNormal = primaryAxis;
    }
    void RollDown(out Vector3 _anchor, out Vector3 _axis, out Vector3 _newNormal) {
        _anchor = toVec - primaryAxis * 0.5f - surfaceNormal * 0.5f;
        _axis = -secondaryAxis;
        _newNormal = -primaryAxis;
    }
    void RollRight(out Vector3 _anchor, out Vector3 _axis, out Vector3 _newNormal) {
        _anchor = toVec + secondaryAxis * 0.5f - surfaceNormal * 0.5f;
        _axis = -primaryAxis;
        _newNormal = secondaryAxis;
    }
    void RollLeft(out Vector3 _anchor, out Vector3 _axis, out Vector3 _newNormal) {
        _anchor = toVec - secondaryAxis * 0.5f - surfaceNormal * 0.5f;
        _axis = primaryAxis;
        _newNormal = -secondaryAxis;
    }
}