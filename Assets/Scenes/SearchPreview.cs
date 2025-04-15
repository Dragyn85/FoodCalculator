using TMPro;
using UnityEngine;

public class SearchPreview : MonoBehaviour
{
    private FoodItem foodItem;
    private TMP_Text foodText;
    
    public void Initialize(FoodItem foodItem)
    {
        this.foodItem = foodItem;
        foodText = GetComponentInChildren<TMP_Text>();
        foodText.text = $"{foodItem.Name}: {foodItem.EnergyKcalPer100g}kcal";
    }
    
}
