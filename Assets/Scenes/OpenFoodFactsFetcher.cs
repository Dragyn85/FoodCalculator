using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;

public class OpenFoodFactsFetcher : MonoBehaviour
{
    private CancellationTokenSource currentSearchToken;
    private Dictionary<string, List<FoodItem>> _searchCache = new();
    private string cacheFilePath => Path.Combine(Application.persistentDataPath, "food_cache.json");
    
    public async Task<List<FoodItem>> SearchAndSortFoodAsync(string query, int maxResults = 5)
    {
        query = query.ToLower().Trim();

        if (_searchCache.TryGetValue(query, out var cachedList))
        {
            Debug.Log($"📦 Loaded from cache: {query}");
            return cachedList.GetRange(0,Mathf.Min(maxResults, cachedList.Count));
        }

        string translatedQuery = await TranslateToEnglishAsync(query);
        Debug.Log($"🔁 \"{query}\" → \"{translatedQuery}\"");

        var originalResults = await SearchFood(query);
        var translatedResults = await SearchFood(translatedQuery);

        Dictionary<string, (FoodItem item, int score)> merged = new();

        // Add original matches with high priority
        foreach (var r in originalResults)
        {
            if (!merged.ContainsKey(r.item.Name))
                merged[r.item.Name] = (r.item, r.relevance * 2); // priority boost
        }

        // Add translated matches if not already included
        foreach (var r in translatedResults)
        {
            if (!merged.ContainsKey(r.item.Name))
                merged[r.item.Name] = (r.item, r.relevance * 4); // higher number = lower priority
        }

        // Sort by score (lower is better), then name length
        var sorted = new List<(FoodItem item, int score)>(merged.Values);
        sorted.Sort((a, b) =>
        {
            int c = a.score.CompareTo(b.score);
            return c != 0 ? c : a.item.Name.Length.CompareTo(b.item.Name.Length);
        });

        return sorted.ConvertAll(x => x.item).GetRange(0, Mathf.Min(maxResults, sorted.Count));
    }

    private async Task<List<(FoodItem item, int relevance)>> SearchFood(string query)
    {
        string lang = GetDeviceLanguageCode();
        string cc = GetCountryCodeFromLanguage();
        string url = $"https://world.openfoodfacts.org/cgi/search.pl?search_terms={UnityWebRequest.EscapeURL(query)}&search_simple=1&action=process&json=1&page_size=100&lc={lang}&cc={cc}";

        using UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Accept", "application/json");
        var op = www.SendWebRequest();

        while (!op.isDone)
            await Task.Yield();

        var results = new List<(FoodItem item, int relevance)>();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Nätverksfel: " + www.error);
            return results;
        }

        JSONNode root = JSON.Parse(www.downloadHandler.text);
        JSONArray products = root["products"].AsArray;
        string[] queryWords = query.ToLower().Split(' ');

        for (int i = 0; i < products.Count; i++)
        {
            JSONNode p = products[i];
            string name = p["product_name"];
            if (string.IsNullOrEmpty(name)) continue;

            string nameLower = name.ToLower();
            bool allMatch = true;
            int minIndex = int.MaxValue;

            foreach (var word in queryWords)
            {
                int idx = nameLower.IndexOf(word);
                if (idx < 0) { allMatch = false; break; }
                minIndex = Mathf.Min(minIndex, idx);
            }

            if (!allMatch) continue;

            var nutr = p["nutriments"];
            float energy = nutr.HasKey("energy-kcal_100g") ? nutr["energy-kcal_100g"].AsFloat : 0f;
            float protein = nutr.HasKey("proteins_100g") ? nutr["proteins_100g"].AsFloat : 0f;
            float fat = nutr.HasKey("fat_100g") ? nutr["fat_100g"].AsFloat : 0f;
            float carbs = nutr.HasKey("carbohydrates_100g") ? nutr["carbohydrates_100g"].AsFloat : 0f;

            var food = new FoodItem(name, energy, protein, fat, carbs);
            results.Add((food, minIndex));
        }
        
        if (results.Count > 0)
        {
            _searchCache[query] = results.ConvertAll(r => r.item);
            SaveCacheToDisk();
        }

        return results;
    }
    void LoadCacheFromDisk()
    {
        if (!File.Exists(cacheFilePath)) return;

        string json = File.ReadAllText(cacheFilePath);
        var raw = JSON.Parse(json);

        foreach (var key in raw.Keys)
        {
            var list = new List<FoodItem>();
            for (var index = 0; index < raw[key].AsArray.Count; index++)
            {
                var item = raw[key].AsArray[(Index)index];
                var f = new FoodItem(
                    item["Name"],
                    item["EnergyKcalPer100g"].AsFloat,
                    item["ProteinPer100g"].AsFloat,
                    item["FatPer100g"].AsFloat,
                    item["CarbsPer100g"].AsFloat
                );
                list.Add(f);
            }

            _searchCache[key] = list;
        }

        Debug.Log($"🔄 Cache loaded: {_searchCache.Count} entries.");
    }
    
    void SaveCacheToDisk()
    {
        var root = new JSONObject();
        foreach (var kvp in _searchCache)
        {
            JSONArray arr = new JSONArray();
            foreach (var item in kvp.Value)
            {
                JSONObject f = new JSONObject();
                f["Name"] = item.Name;
                f["EnergyKcalPer100g"] = item.EnergyKcalPer100g;
                f["ProteinPer100g"] = item.ProteinPer100g;
                f["FatPer100g"] = item.FatPer100g;
                f["CarbsPer100g"] = item.CarbsPer100g;
                arr.Add(f);
            }
            root[kvp.Key] = arr;
        }

        File.WriteAllText(cacheFilePath, root.ToString());
        Debug.Log($"💾 Cache saved: {_searchCache.Count} entries.");
    }
    
    public async Task<string> TranslateToEnglishAsync(string input)
    {
        string sourceLang = GetDeviceLanguageCode();
        if (sourceLang == "en") return input; // skip if already English

        string url = "https://libretranslate.com/translate";
        string jsonPayload = JsonUtility.ToJson(new TranslationRequest
        {
            q = input,
            source = sourceLang,
            target = "en",
            format = "text"
        });

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Översättning misslyckades: " + request.error);
            return input;
        }

        JSONNode result = JSON.Parse(request.downloadHandler.text);
        return result["translatedText"];
    }
    
    public async Task<FoodItem> GetProductByBarcodeAsync(string barcode)
    {
        string url = $"https://world.openfoodfacts.org/api/v0/product/{barcode}.json";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Accept", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Nätverksfel: " + request.error);
            return null;
        }

        JSONNode root = JSON.Parse(request.downloadHandler.text);

        int status = root["status"].AsInt;
        if (status != 1)
        {
            Debug.LogWarning($"❌ Produkt med streckkod {barcode} hittades inte.");
            return null;
        }

        JSONNode product = root["product"];

        string name =
            !string.IsNullOrEmpty(product["product_name"]) ? product["product_name"] :
            !string.IsNullOrEmpty(product["product_name_sv"]) ? product["product_name_sv"] :
            !string.IsNullOrEmpty(product["product_name_en"]) ? product["product_name_en"] :
            "(namnlös produkt)";

        JSONNode nutr = product["nutriments"];
        float energy = nutr.HasKey("energy-kcal_100g") ? nutr["energy-kcal_100g"].AsFloat : 0f;
        float protein = nutr.HasKey("proteins_100g") ? nutr["proteins_100g"].AsFloat : 0f;
        float fat = nutr.HasKey("fat_100g") ? nutr["fat_100g"].AsFloat : 0f;
        float carbs = nutr.HasKey("carbohydrates_100g") ? nutr["carbohydrates_100g"].AsFloat : 0f;

        return new FoodItem(name, energy, protein, fat, carbs);
    }

    public string GetDeviceLanguageCode()
    {
        return Application.systemLanguage switch
        {
            SystemLanguage.Swedish => "sv",
            SystemLanguage.English => "en",
            SystemLanguage.German => "de",
            SystemLanguage.Spanish => "es",
            SystemLanguage.French => "fr",
            SystemLanguage.Italian => "it",
            SystemLanguage.Polish => "pl",
            SystemLanguage.Danish => "da",
            SystemLanguage.Norwegian => "no",
            SystemLanguage.Finnish => "fi",
            _ => "en" // fallback to English
        };
    }
    string GetCountryCodeFromLanguage()
    {
        return Application.systemLanguage switch
        {
            SystemLanguage.Swedish => "se",
            SystemLanguage.English => "gb", // or "us"
            SystemLanguage.German => "de",
            SystemLanguage.French => "fr",
            SystemLanguage.Spanish => "es",
            SystemLanguage.Italian => "it",
            SystemLanguage.Danish => "dk",
            SystemLanguage.Norwegian => "no",
            SystemLanguage.Finnish => "fi",
            _ => "us"
        };
    }

    void Awake()
    {
        LoadCacheFromDisk();
    }
}

[System.Serializable]
public class FoodItem
{
    public string Name;
    public float  EnergyKcalPer100g;
    public float  ProteinPer100g;
    public float  FatPer100g;
    public float  CarbsPer100g;

    public FoodItem(string name, float energy, float protein, float fat, float carbs)
    {
        Name = name;
        EnergyKcalPer100g = energy;
        ProteinPer100g = protein;
        FatPer100g = fat;
        CarbsPer100g = carbs;
    }
}

[System.Serializable]
public class TranslationRequest
{
    public string q;
    public string source;
    public string target;
    public string format;
}
