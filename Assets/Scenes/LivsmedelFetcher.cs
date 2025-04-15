using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

public class LivsmedelFetcher : MonoBehaviour
{
    string        localPath => Path.Combine(Application.persistentDataPath, "livsmedel.json");

    void Start()
    {
        if (File.Exists(localPath))
        {
            Debug.Log("Laddar från cache...");
        }
        else
        {
            Debug.Log("Hämtar från API...");
            StartCoroutine(DownloadAndCacheAll());
        }
    }

    IEnumerator DownloadAndCacheAll()
    {
        string url = "https://dataportal.livsmedelsverket.se/livsmedel/api/v1/livsmedel?offset=0&limit=2500&sprak=1";
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Accept", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllText(localPath, request.downloadHandler.text);
            Debug.Log("Sparad till cache.");
        }
        else
        {
            Debug.LogError($"Fel vid hämtning: {request.error}");
        }
    }
    
    public void LoadFromCache(string query, int maxResults = 10)
    {
        string json = File.ReadAllText(localPath);
        var root = JSON.Parse(json);
        var items = root["livsmedel"];

        List<(int nummer, string namn, int relevans, int längd)> matchningar = new();

        for (int i = 0; i < items.Count; i++)
        {
            string namn = items[i]["namn"];
            int nummer = items[i]["nummer"].AsInt;

            int index = namn.ToLower().IndexOf(query.ToLower());
            if (index >= 0)
            {
                matchningar.Add((nummer, namn, index, namn.Length));
            }
        }

        // Sortera först på relevans, sedan på längd
        matchningar.Sort((a, b) =>
        {
            int relevansJämförelse = a.relevans.CompareTo(b.relevans);
            return relevansJämförelse != 0
                ? relevansJämförelse
                : a.längd.CompareTo(b.längd);
        });

        int antal = Mathf.Min(maxResults, matchningar.Count);
        for (int i = 0; i < antal; i++)
        {
            Debug.Log($"{matchningar[i].nummer}: {matchningar[i].namn}");
        }
    }
}

[System.Serializable]
public class Livsmedel
{
    public int    nummer;
    public string namn;
}

[System.Serializable]
public class LivsmedelLista
{
    public List<Livsmedel> livsmedel;
}
