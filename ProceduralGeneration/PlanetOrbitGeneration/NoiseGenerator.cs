using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float GenerateNoise(float xPos, float yPos, float noiseAmplitude, float noiseRoughness, float noiseSeed)
    {
        return 1 + (noiseAmplitude * GenerateFirstLayerNoise(xPos, noiseRoughness) * GenerateSecondLayerNoise(yPos, noiseSeed, noiseRoughness));
    }

    private static float GenerateFirstLayerNoise(float xPos, float noiseRoughness)
    {
        return Mathf.Sin(xPos * noiseRoughness);
    }

    private static float GenerateSecondLayerNoise(float yPos, float noiseSeed, float noiseRoughness)
    {
        return Mathf.PerlinNoise(noiseSeed, yPos * noiseRoughness);
    }
}