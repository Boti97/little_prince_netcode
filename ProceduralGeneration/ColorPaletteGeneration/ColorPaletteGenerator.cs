using System.Collections.Generic;
using UnityEngine;

public static class ColorPaletteGenerator
{
    public static List<KeyValuePair<Color, int>> GenerateColorPalette(int baseSeed, int sizeOfColorPalette)
    {
        var colorPaletteWithSeed = new List<KeyValuePair<Color, int>>();
        Random.InitState(baseSeed);
        Debug.Log("Base Seed: " + Random.seed);
        for (var i = 0; i < sizeOfColorPalette; i++)
        {
            //generate color seed with base seed, to get different colors
            var colorSeed = Random.Range(0, 10000);
            Random.InitState(colorSeed);
            Debug.Log("Color Seed: " + Random.seed);

            colorPaletteWithSeed.Add(new KeyValuePair<Color, int>(GenerateRandomButLimitedColor(), colorSeed));
        }

        Random.InitState(baseSeed);
        Debug.Log("Base Seed: " + Random.seed);

        return colorPaletteWithSeed;
    }

    private static Color GenerateRandomButLimitedColor()
    {
        Debug.Log("Color Seed: " + Random.seed);
        var minHue = Random.Range(0f, 1f);
        var maxHue = Random.Range(minHue, 1f);
        var minSaturation = Random.Range(0.3f, 0.7f);
        var maxSaturation = Random.Range(minSaturation, 0.7f);
        var minValue = Random.Range(0.3f, 0.5f);
        var maxValue = Random.Range(minValue, 0.5f);

        return Random.ColorHSV(minHue, maxHue, minSaturation, maxSaturation, minValue, maxValue);
    }
}