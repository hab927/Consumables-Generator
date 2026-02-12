using TMPro;
using UnityEngine;

public class ItemDescription : MonoBehaviour
{
    public static ItemDescription instance;

    public static TextMeshProUGUI description;
    public static TextMeshProUGUI duration;
    public static TextMeshProUGUI rarity;

    private void Start() {
        description = GetComponent<TextMeshProUGUI>();
        duration = GameObject.Find("D tag").GetComponent<TextMeshProUGUI>();
        rarity = GameObject.Find("R tag").GetComponent<TextMeshProUGUI>();
    }
}
