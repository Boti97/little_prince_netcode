using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ColorPaletteGenerator
{
    public List<Color> GenerateColorPalette(int sizeOfColorPalette)
    {
        List<Color> colorPalette = new List<Color>();
        Debug.Log(Random.seed);
        for (int i = 0; i < sizeOfColorPalette; i++)
        {
            colorPalette.Add(GenerateRandomButLimitedColor());
        }
        return colorPalette;
    }

    private Color GenerateRandomButLimitedColor()
    {
        float minHue = Random.Range(0f, 1f);
        float maxHue = Random.Range(minHue, 1f);
        float minSaturation = Random.Range(0.3f, 0.7f);
        float maxSaturation = Random.Range(minSaturation, 0.7f);
        float minValue = Random.Range(0.3f, 0.5f);
        float maxValue = Random.Range(minValue, 0.5f);

        return Random.ColorHSV(minHue, maxHue, minSaturation, maxSaturation, minValue, maxValue);
    }
}
