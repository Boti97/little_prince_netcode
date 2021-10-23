using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGenerator
{
    private Texture2D texture;
    const int textureResolution = 50;

    public void UpdateSettings()
    {
        texture = new Texture2D(textureResolution, 1);
    }

    public void UpdateElevation(Material material, MinMax elevationMinMax)
    {
        material.SetVector("_elevation", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }

    public void UpdateColors(Material material, Gradient gradient)
    {
        Color[] colors = new Color[textureResolution];
        for (int i = 0; i < textureResolution; i++)
        {
            colors[i] = gradient.Evaluate(i / (textureResolution - 1f));
        }
        texture.SetPixels(colors);
        texture.Apply();
        material.SetTexture("_texture", texture);
    }
}
