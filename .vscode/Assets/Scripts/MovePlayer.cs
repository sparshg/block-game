using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum Controls {
    MousePriority,
    KeyboardPriority
}

public class MovePlayer : MonoBehaviour {
    [ReadOnly] public Vector3 surfaceNormal = Vector3.up;
    [ReadOnly] public Vector3 primaryAxis = Vector3.forward;
    [ReadOnly] public Vector3 secondaryAxis = Vector3.right;
    // private KeyCode lockKey = KeyCode.None;
    // private Vector3 lockPrimAxis, lockSecAxis, lockNormal;

    [Header("Keys")]
    [SerializeField] private KeyCode upKey;
    [SerializeField] private KeyCode downKey;
    [SerializeField] private KeyCode leftKey;
    [SerializeField] private KeyCode rightKey;
    [SerializeField] private KeyCode leftRot;
    [SerializeField] private KeyCode rightRot;
    [SerializeField] private KeyCode upRot;
    [SerializeField] private KeyCode downRot;
    public Controls controls;

    [Header("Initialize")]
    [SerializeField] private Vector2 startingPos;

    [Header("Movement")]
    [SerializeField] private AnimationCurve rotateCurve;
    [SerializeField] private float rotateSpeed;
    [Header("Camera")]
    [SerializeField] private float camRotateSpeed;
    [SerializeField] private CamRotate cam;
    private float toAngleX, toAngleY;

    private bool isMoving = false;

    void OnGUI() {
        if (GUI.Button(new Rect(0, 20, 100, 20), "Burst")) {
            StartCoroutine(Burst());
        }
    }

    IEnumerator Burst() {
        yield return new WaitForSeconds(2f);
        Instantiate(Resources.Load("Burst"), transform.position, Quaternion.FromToRotation(Vector3.up, surfaceNormal));
        gameObject.SetActive(false);
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
    }

    void Update() {
        if (controls == Controls.KeyboardPriority) {
            keysPriorityControls();
        } else
            mousePriorityControls();
    }

    IEnumerator Roll(Vector3 anchor, Vector3 axis, Vector3 newNormal) {
        isMoving = true;
        float angle = 90f, _angle = 0f, _rotateSpeed = rotateSpeed, t1 = rotateCurve.Evaluate(0), t;
        Vector3 belowCube = transform.position - surfaceNormal;
        Vector3 toVec = belowCube + newNormal;

        if (toVec.x == Pref.I.size || toVec.y == Pref.I.size || toVec.z == Pref.I.size || toVec.x < 0 || toVec.y < 0 || toVec.z < 0) {
            angle = 180f;
            _rotateSpeed *= 2f;
            cam.SetIntermediateRotation(surfaceNormal);
        } else {
            toVec += surfaceNormal;
        }
        while (true) {
            _angle += _rotateSpeed * Time.deltaTime;
            t = rotateCurve.Evaluate(_angle / angle);
            if (_angle >= angle) break;
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
        if (Input.GetKeyDown(leftRot)) {
            toAngleX -= 90f;
        }
        if (Input.GetKeyDown(rightRot)) {
            toAngleX += 90f;
        }
        if (Input.GetKeyDown(upRot)) {
            toAngleY += 20f;
        } else if (Input.GetKeyDown(downRot)) {
            toAngleY -= 35f;
        } else if (Input.GetKeyUp(upRot) || Input.GetKeyUp(downRot)) {
            toAngleY = 0f;
        }
        cam.turn.x = Mathf.LerpAngle(cam.turn.x, toAngleX, Time.deltaTime * camRotateSpeed);
        cam.turn.y = Mathf.Lerp(cam.turn.y, toAngleY, Time.deltaTime * camRotateSpeed);
        if (isMoving) return;
        if (Input.GetKey(upKey)) {
            StartCoroutine(Roll(transform.position + primaryAxis * 0.5f - surfaceNormal * 0.5f, secondaryAxis, primaryAxis));
        } else if (Input.GetKey(downKey)) {
            StartCoroutine(Roll(transform.position - primaryAxis * 0.5f - surfaceNormal * 0.5f, -secondaryAxis, -primaryAxis));
        } else if (Input.GetKey(rightKey)) {
            StartCoroutine(Roll(transform.position + secondaryAxis * 0.5f - surfaceNormal * 0.5f, -primaryAxis, secondaryAxis));
        } else if (Input.GetKey(leftKey)) {
            StartCoroutine(Roll(transform.position - secondaryAxis * 0.5f - surfaceNormal * 0.5f, primaryAxis, -secondaryAxis));
        }
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

    void mousePriorityControls() {
        if (isMoving) return;
        if (Input.GetKey(upKey)) {
            StartCoroutine(Roll(transform.position + primaryAxis * 0.5f - surfaceNormal * 0.5f, secondaryAxis, primaryAxis));
        } else if (Input.GetKey(downKey)) {
            StartCoroutine(Roll(transform.position - primaryAxis * 0.5f - surfaceNormal * 0.5f, -secondaryAxis, -primaryAxis));
        } else if (Input.GetKey(rightKey)) {
            StartCoroutine(Roll(transform.position + secondaryAxis * 0.5f - surfaceNormal * 0.5f, -primaryAxis, secondaryAxis));
        } else if (Input.GetKey(leftKey)) {
            StartCoroutine(Roll(transform.position - secondaryAxis * 0.5f - surfaceNormal * 0.5f, primaryAxis, -secondaryAxis));
        }
    }
}


// [ReadOnly] inspector attribute

public class ReadOnlyAttribute : PropertyAttribute {
}
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label) {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}