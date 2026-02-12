using UnityEngine;
using ConsumableLib;
using UnityEngine.UI;

public class ConsumablesManager : MonoBehaviour
{
    public static ConsumablesManager instance;
    public static Ingredients ingredients;
    public Slider maxTierSlider;
    public Slider raritySlider;
    public Slider untieredSlider;
    public static bool encryptionOn = false;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }
    }

    void Start()
    {
        ingredients = IngredientsPersistence.GetIngredientsFromJSON();
        maxTierSlider.value = ingredients.MaxTier;
        raritySlider.value = ingredients.RaritySkew;
        untieredSlider.value = Mathf.Round(ingredients.UntieredChance * 100);
    }

    void Update()
    {

    }
}
