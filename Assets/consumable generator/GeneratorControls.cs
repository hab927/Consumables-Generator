using ConsumableLib;
using TMPro;
using UnityEngine;

public class GeneratorControls : MonoBehaviour
{
    public TextMeshProUGUI mt_sliderText;
    public TextMeshProUGUI rr_sliderText;
    public TextMeshProUGUI uc_sliderText;

    public void MaxTierSlider(float value) {        // slider that affects max tier
        ConsumablesManager.ingredients.MaxTier = (int)value;
        IngredientsPersistence.SaveIngredientsToJSON();
        if (ConsumablesManager.ingredients.MaxTier == 0) {
            mt_sliderText.text = "No Tiers";
        }
        else {
            string tier = value.ToString();
            mt_sliderText.text = tier;
        }
    }

    public void RaritySlider(float value) {         // the one for rarity
        ConsumablesManager.ingredients.RaritySkew = (int)value;
        IngredientsPersistence.SaveIngredientsToJSON();
        if (ConsumablesManager.ingredients.RaritySkew == 0) {
            rr_sliderText.text = "Uniform";
        }
        else {
            string skew = value.ToString();
            rr_sliderText.text = skew;
        }
    }

    public void UntieredProbability(float value) {  // the one that affects tiered/untiered probability (80% on this means 80% likely to be untiered)
        ConsumablesManager.ingredients.UntieredChance = Mathf.Round(value) / 100.0f;
        IngredientsPersistence.SaveIngredientsToJSON();
        string chance = value.ToString() + "%";
        uc_sliderText.text = chance;
    }
}
