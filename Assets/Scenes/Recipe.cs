using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Recipe
{
    public string                 Name;
    public List<RecipeIngredient> Ingredients = new();

    public Recipe(string name)
    {
        Name = name;
    }

    public void AddIngredient(FoodItem item, float amount, Unit unit, float density = -1f)
    {
        Ingredients.Add(new RecipeIngredient(item, amount, unit, density));
    }

    public float TotalEnergy  => Ingredients.Sum(i => i.Energy);
    public float TotalProtein => Ingredients.Sum(i => i.Protein);
    public float TotalFat     => Ingredients.Sum(i => i.Fat);
    public float TotalCarbs   => Ingredients.Sum(i => i.Carbs);

    public void PrintSummary()
    {
        Debug.Log($"🍽️ {Name} innehåller:");
        foreach (var ing in Ingredients)
            Debug.Log($"- {ing}");

        Debug.Log($"🔢 Totalt:\n  Energi: {TotalEnergy} kcal\n  Protein: {TotalProtein} g\n  Fett: {TotalFat} g\n  Kolhydrater: {TotalCarbs} g");
    }
}


[System.Serializable]
public class RecipeIngredient
{
    public FoodItem Item;

    public float OriginalAmount;
    public Unit  OriginalUnit;

    public float WeightInGrams;

    public RecipeIngredient(FoodItem item, float amount, Unit unit, float density = -1f)
    {
        Item = item;
        OriginalAmount = amount;
        OriginalUnit = unit;
        WeightInGrams = UnitConverter.ToGrams(amount, unit, density);
    }

    public float Energy  => Item.EnergyKcalPer100g * WeightInGrams / 100f;
    public float Protein => Item.ProteinPer100g * WeightInGrams / 100f;
    public float Fat     => Item.FatPer100g * WeightInGrams / 100f;
    public float Carbs   => Item.CarbsPer100g * WeightInGrams / 100f;

    public override string ToString()
    {
        return $"{OriginalAmount} {OriginalUnit} {Item.Name}";
    }
}

