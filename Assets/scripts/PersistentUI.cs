using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    void Awake() { DontDestroyOnLoad(this.gameObject); }


}
