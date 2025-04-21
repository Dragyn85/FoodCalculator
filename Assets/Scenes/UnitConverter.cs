public enum Unit
{
    Gram,
    Kilogram,
    Ounce,
    Pound,

    Milliliter,
    Liter,
    Teaspoon,
    Tablespoon,
    Cup,
    FluidOunce
}

public static class UnitConverter
{
    public static float DefaultDensity = 1.0f; // g/ml, default is water

    public static float ToGrams(float value, Unit unit, float density = -1f)
    {
        if (density <= 0) density = DefaultDensity;

        return unit switch
        {
            // Metric weight
            Unit.Gram => value,
            Unit.Kilogram => value * 1000f,

            // Imperial weight
            Unit.Ounce => value * 28.3495f,
            Unit.Pound => value * 453.592f,

            // Metric volume
            Unit.Milliliter => value * density,
            Unit.Liter => value * 1000f * density,

            // Volume with fixed metric equivalents
            Unit.Teaspoon => value * 4.92892f * density,
            Unit.Tablespoon => value * 14.7868f * density,
            Unit.Cup => value * 240f * density,
            Unit.FluidOunce => value * 29.5735f * density,

            _ => throw new System.NotSupportedException($"Unit {unit} not supported")
        };
    }

    public static float FromGrams(float grams, Unit unit, float density = -1f)
    {
        if (density <= 0) density = DefaultDensity;

        return unit switch
        {
            Unit.Gram => grams,
            Unit.Kilogram => grams / 1000f,
            Unit.Ounce => grams / 28.3495f,
            Unit.Pound => grams / 453.592f,

            Unit.Milliliter => grams / density,
            Unit.Liter => grams / (1000f * density),

            Unit.Teaspoon => grams / (4.92892f * density),
            Unit.Tablespoon => grams / (14.7868f * density),
            Unit.Cup => grams / (240f * density),
            Unit.FluidOunce => grams / (29.5735f * density),

            _ => throw new System.NotSupportedException($"Unit {unit} not supported")
        };
    }
}