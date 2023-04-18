using UnityEngine;

public class Pref : MonoBehaviour {
    public static Pref I;
    public int size;
    public bool twoPlayers;
    private void Awake() {
        if (I != null) {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
}
