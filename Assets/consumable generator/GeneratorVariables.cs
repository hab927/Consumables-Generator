using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ConsumableLib {
    public struct Consumable {
        public string name;                                 // name of the consumable
        public string shape;                                // the shape (if it's a potion, flask, or vial)
        public string type;                                 // the type (syringe, pill, flask, potion, vial, or viand)
        public string effect;                               // the name of the effect given
        public KeyValuePair<string, Color32> color;         // the color of the consumable (give it a hex code)
        public int duration;                                // how long the consumable's effect lasts (in seconds)
        public int tier;                                    // the tier of the consumable
        public float rarity;                                // the rarity (%) of the consumable, generally, the higher the stronger

        public Consumable(string name, string shape, string type, string effect, KeyValuePair<string, Color32> color, int duration, int tier, float rarity) {
            this.name = name;
            this.shape = shape;
            this.type = type;
            this.effect = effect;
            this.color = color;
            this.duration = duration;
            this.tier = tier;
            this.rarity = rarity;
        }
    }

    [Serializable]
    public class NCPair {
        public string name;
        public Color32 color;
    }

    [Serializable]
    public class Ingredients : ISerializationCallbackReceiver {
        public List<string> BottleShapes = new();
        public List<string> Types = new();
        public List<string> TieredEffects = new();
        public List<string> UntieredEffects = new();
        public List<NCPair> SerializableColors = new();
        public Dictionary<string, Color32> NamesColors = new();
        public int MaxTier = 5;
        public int Spt = 15;
        public bool ScaleDown = false;
        public int RandMin = 10;
        public int RandMax = 30;
        public int RaritySkew = 0;      // 0 - 10, 0 being uniform and 10 being tier 1 is really common/max tier is really rare
        public float UntieredChance = 0.25f;

        public void OnAfterDeserialize() {      // have to do this because C# doesn't like serializing dictionaries
            // reset dictionary and refill it based on what's in the serializable colors
            NamesColors.Clear();
            foreach (NCPair pair in SerializableColors) {
                NamesColors[pair.name] = pair.color;
            }
        }

        public void OnBeforeSerialize() {
            // reset list of serializable colors and refill it based on what's in the NamesColors dictionary
            SerializableColors.Clear();
            foreach (KeyValuePair<string, Color32> nc in NamesColors) {
                SerializableColors.Add(new NCPair { name = nc.Key, color = nc.Value });
            }
        }
    }

    public class Functions {
        public static int CalculateDuration(int tier) {
            Ingredients i = ConsumablesManager.ingredients;
            int duration = ((i.MaxTier + (tier * (int)Mathf.Pow(-1, i.ScaleDown ? 0 : 1))) * i.Spt + Random.Range(i.RandMin, i.RandMax + 1));
            return duration;
        }

        public static Consumable GenerateConsumable() {
            string randomColorName;
            Color randomColor32;
            string shape = null;
            string type;
            string effect;
            int tier = 0;
            float rarity;

            Ingredients i = ConsumablesManager.ingredients;

            // pick from any color
            randomColorName = i.NamesColors.Keys.ToList()[Random.Range(0, i.NamesColors.Count)];
            randomColor32 = i.NamesColors[randomColorName];

            // type of consumable
            type = i.Types[Random.Range(0, i.Types.Count)];
            if (type == "Flask" || type == "Potion" || type == "Vial") {
                shape = i.BottleShapes[Random.Range(0, i.BottleShapes.Count)];
            }

            // choose between a tiered and untiered effect and set the tier accordingly
            float UT_roll = Random.Range(0.0f, 1.0f);

            if (UT_roll >= i.UntieredChance) {
                effect = i.TieredEffects[Random.Range(0, i.TieredEffects.Count)];

                if (i.MaxTier > 0) {
                    tier = RandomFunctions.TierFromDistribution(i.MaxTier, i.RaritySkew, out rarity);
                    rarity *= (1 - i.UntieredChance);
                }
                else {
                    tier = 0;
                    rarity = (1 - i.UntieredChance) / i.TieredEffects.Count;
                }
            }
            else {
                int index = Random.Range(0, i.UntieredEffects.Count);
                // the rarity of one of these will be the probability of untiered being picked / number of untiered effects
                rarity = i.UntieredChance / i.UntieredEffects.Count;
                effect = i.UntieredEffects[index];
            }

            // generate duration from tier using scaling formula
            int duration = CalculateDuration(tier);

            Consumable consumableStruct = new() {
                name = randomColorName + (shape == null ? " " : " " + shape + " ") + type + " of " + effect + (tier == 0 ? "" : " " + tier),
                shape = shape,
                type = type,
                effect = effect,
                color = new KeyValuePair<string, Color32>(randomColorName, randomColor32),
                duration = duration,
                tier = tier,
                rarity = rarity
            };

            return consumableStruct;
        }

        public static void GenerateInUI() {
            Consumable c = GenerateConsumable();

            string desc = c.color.Key +
                            (c.shape == null ? " " : " " + c.shape + " ") +
                            c.type +
                            " of " +
                            c.effect +
                            (c.tier == 0 ? "" : " " + c.tier);
            ItemDescription.description.text = desc;
            ItemDescription.duration.text = (c.duration / 60).ToString("D2") + ":" + (c.duration % 60).ToString("D2");
            ItemDescription.rarity.text = (c.rarity * 100.0f).ToString("F2") + "%";

            // load the image onto our sprite displayer
            Texture2D tex = (Texture2D)Resources.Load(c.type);
            SpriteDisplayerManager.instance.img.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100.0f
            );
            Texture2D fillTex = (Texture2D)Resources.Load(c.type + "Fill");
            SpriteDisplayerManager.instance.imgFill.sprite = Sprite.Create(
                fillTex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100.0f
            );
            SpriteDisplayerManager.instance.imgFill.color = c.color.Value;
        }
    }

    public class IngredientsPersistence {       // for saving and loading to json
        private static string GetSavePath() =>
            Path.Combine(Application.persistentDataPath, "ConsumablesConfig.json");

        public static Ingredients GetIngredientsFromJSON() {
            string path = GetSavePath();
            if (File.Exists(path)) {
                string json = File.ReadAllText(path);
                if (ConsumablesManager.encryptionOn) {
                    json = Encryption.EncryptDecrypt(json);
                }
                return JsonUtility.FromJson<Ingredients>(json);
            }
            TextAsset t = Resources.Load<TextAsset>("ConsumablesConfig");
            return JsonUtility.FromJson<Ingredients>(t.text);
        }

        public static void SaveIngredientsToJSON() {
            string json = JsonUtility.ToJson(ConsumablesManager.ingredients, true);
            if (ConsumablesManager.encryptionOn) {
                json = Encryption.EncryptDecrypt(json);
            }
            File.WriteAllText(GetSavePath(), json);
        }

        public static void DeleteJSON() {        // function to reset all values to default
            string path = GetSavePath();
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    public class RandomFunctions {
        public static int TierFromDistribution(int maxTier, int skew, out float rarity) {

            if (maxTier <= 1) {         // no reason to do this if the max tier is 1
                rarity = 1.0f;
                return 1;
            }

            float[] probabilities = new float[maxTier];

            if (skew == 0) {            // just use a uniform distribution, tiers 1 to maxTier will all be equally likely
                float uniformProb = 1.0f / maxTier;
                for (int i = 0; i < maxTier; i++) {
                    probabilities[i] = uniformProb;
                }
            }

            // now we get to do the geometric distribution
            float r = (float)Math.Exp(-0.05 * skew);
            float sum = 0;

            for (int i = 0; i < maxTier; i++) {
                probabilities[i] = (float)Math.Pow(r, i);
                sum += probabilities[i];
            }

            for (int i = 0; i < maxTier; i++) {
                probabilities[i] /= sum;
            }

            // roll, and consecutively add until the cumulative sum exceeds the roll (i think we did this in class)
            float roll = Random.Range(0.0f, 1.0f);
            float cumulative = 0.0f;

            for (int i = 0; i < maxTier; i++) {
                cumulative += probabilities[i];
                if (roll < cumulative) {
                    rarity = probabilities[i];
                    return i + 1;
                }
            }
            rarity = probabilities[maxTier - 1];
            return maxTier;
        }
    }
}
