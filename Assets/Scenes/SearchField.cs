using TMPro;
using UnityEngine;

public class SearchField : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] private OpenFoodFactsFetcher fetcher;
    
    float searchDelay = 0.5f;

    float searchTimer = 0f;
        private void Start()
    {
        searchField.onValueChanged.AddListener(OnSearchFieldChanged);
    }

    void OnSearchFieldChanged(string arg0)
    {
        searchTimer = 0f;
    }
    private void Update()
    {
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchDelay)
        {
            string query = searchField.text;
            if (!string.IsNullOrEmpty(query))
            {
                //StartCoroutine(fetcher.SearchAndSortFood(query));
            }
            searchTimer = 0f;
        }
    }
}
