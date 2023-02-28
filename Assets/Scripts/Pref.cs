using UnityEngine;

public class Pref : MonoBehaviour {
    public static Pref I { get; private set; }
    public int size;
    private void Awake() {
        if (I != null && I != this) {
            Destroy(this);
        } else {
            I = this;
        }
    }
}
