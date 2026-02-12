using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpriteDisplayerManager : MonoBehaviour
{
    public static SpriteDisplayerManager instance;

    public Image img;
    public Image imgFill;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }
    }
}
