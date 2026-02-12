using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ConsumableLib;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorUI : MonoBehaviour
{
    public static EditorUI instance;

    private void Awake() {
        if (instance != null && instance != this) {
            return;
        }
        else {
            instance = this;
        }
    }

    // stuff in effects editor
    public TMP_InputField tieredField;
    public TMP_InputField untieredField;

    // stuff in colors editor
    public TMP_InputField colorField;

    // stuff in durations editor
    public TextMeshProUGUI durationFormula;

    public TMP_InputField sptField;
    public TMP_InputField randMinField;
    public TMP_InputField randMaxField;
    public Toggle scaleDirectionToggle;

    // sliders
    public Slider mt_slider;
    public Slider tr_slider;
    public Slider uc_slider;

    public void GenerateButton() {
        Functions.GenerateInUI();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenEditor(Canvas editor) {
        editor.gameObject.SetActive(true);
    }

    public void CloseEditor() {
        transform.parent.parent.gameObject.SetActive(false);
    }

    // effects menu
    public void TieredEffectsInput() {
        List<string> tieredArray = ConsumablesManager.ingredients.TieredEffects;
        tieredArray.Sort();
        string tieredEffectsString = String.Join(", ", tieredArray);
        tieredField.text = tieredEffectsString;
    }

    public void UntieredEffectsInput() {
        List<string> untieredArray = ConsumablesManager.ingredients.UntieredEffects;
        untieredArray.Sort();
        string untieredEffectsString = String.Join(", ", untieredArray);
        untieredField.text = untieredEffectsString;
    }

    public void SaveEffects() {
        ConsumablesManager.ingredients.TieredEffects = Regex.Split(tieredField.text, @"\s*,\s*").ToList();
        ConsumablesManager.ingredients.UntieredEffects = Regex.Split(untieredField.text, @"\s*,\s*").ToList();
        IngredientsPersistence.SaveIngredientsToJSON();
    }

    //colors menu
    public void ShowColorsInput() {
        Dictionary<string, Color32> nc_dictionary = ConsumablesManager.ingredients.NamesColors;
        List<string> shownText = new();

        foreach (KeyValuePair<string, Color32> entry in nc_dictionary) {
            shownText.Add( "(" + entry.Key + "," + ColorUtility.ToHtmlStringRGBA(entry.Value) + ")" );
        }

        colorField.text = string.Join(", ", shownText);
    }

    public void SaveColors() {
        ConsumablesManager.ingredients.NamesColors.Clear();
        string input = colorField.text;
        string pattern = @"(?:()[a-zA-Z\s]+\s*,\s*[0-9a-fA-F]+(?:))";
        MatchCollection matches = Regex.Matches(input, pattern);

        foreach (Match match in matches) {
            string[] pair = Regex.Split(match.Value, @"\s*,\s*");
            if (ColorUtility.TryParseHtmlString("#" + pair[1].ToUpper(), out Color color32)) {
                ConsumablesManager.ingredients.NamesColors[pair[0]] = color32;
            }
        }

        IngredientsPersistence.SaveIngredientsToJSON();
    }

    // duration menu
    public void SetDurationFields() {
        scaleDirectionToggle.isOn = !ConsumablesManager.ingredients.ScaleDown;
        sptField.text = ConsumablesManager.ingredients.Spt.ToString();
        randMinField.text = ConsumablesManager.ingredients.RandMin.ToString();
        randMaxField.text = ConsumablesManager.ingredients.RandMax.ToString();
    }

    public void ShowScalingFormula() {
        durationFormula.text = "(" + ConsumablesManager.ingredients.MaxTier +
                                (!ConsumablesManager.ingredients.ScaleDown ? " - " : " + ") + "tier)" +
                                " * " + ConsumablesManager.ingredients.Spt +
                                " + random(" + ConsumablesManager.ingredients.RandMin + " to " + ConsumablesManager.ingredients.RandMax + ")";
    }

    public void SaveScalingFormula() {
        ConsumablesManager.ingredients.Spt = int.Parse(sptField.text);
        ConsumablesManager.ingredients.ScaleDown = !scaleDirectionToggle.isOn;
        ConsumablesManager.ingredients.RandMin = int.Parse(randMinField.text);
        ConsumablesManager.ingredients.RandMax = int.Parse(randMaxField.text);
        IngredientsPersistence.SaveIngredientsToJSON();
        ShowScalingFormula();
    }

    public void CopyJSONDirToClipboard() {
        string dir = Path.Combine(Application.persistentDataPath, "ConsumablesConfig.json");
        GUIUtility.systemCopyBuffer = dir;
    }

    public void ResetButton() {
        IngredientsPersistence.DeleteJSON();
        ConsumablesManager.ingredients = IngredientsPersistence.GetIngredientsFromJSON();
        mt_slider.value = ConsumablesManager.ingredients.MaxTier;
        tr_slider.value = ConsumablesManager.ingredients.RaritySkew;
        uc_slider.value = ConsumablesManager.ingredients.UntieredChance * 100;
        SaveEffects();
        SaveColors();
        SaveScalingFormula();
        IngredientsPersistence.SaveIngredientsToJSON();
    }
}