using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodSearch : MonoBehaviour
{
    [SerializeField] TMP_InputField searchField;
    [SerializeField] Button searchByNameButton;
    [SerializeField] OpenFoodFactsFetcher fetcher;
    [SerializeField] SearchPreview searchPreviewPrefab;
    [SerializeField] Transform searchPreviewContainer;
    void Start()
    {
        searchByNameButton.onClick.AddListener(SearchByNameButton);
    }
    
    public void ClearPreviews()
    {
        foreach (Transform child in searchPreviewContainer)
        {
            Destroy(child.gameObject);
        }
    }
    async void SearchByNameButton()
    {
        string query = searchField.text;
        var FoodItems  = await fetcher.SearchAndSortFoodAsync(query);
        if (FoodItems.Count <= 0)
        {
            return;
        }

        foreach (var foodItem in FoodItems)
        {
            AddPreview(foodItem);
        }
    }

    public void AddPreview(FoodItem foodItem)
    {
        SearchPreview preview = Instantiate(searchPreviewPrefab, searchPreviewContainer);
        preview.Initialize(foodItem);
    }
}
