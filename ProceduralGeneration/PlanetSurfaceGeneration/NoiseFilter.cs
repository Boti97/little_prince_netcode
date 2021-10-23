using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter
{
    private NoiseSettings noiseSettings;
    private readonly SimplexNoise simplexNoise = new SimplexNoise();

    public NoiseFilter(NoiseSettings noiseSettings)
    {
        this.noiseSettings = noiseSettings;
    }

    public float GenerateNoise(Vector3 pointOnPlanet)
    {
        float noiseValue = 0;
        float frequency = noiseSettings.baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < noiseSettings.numberOfLayers; i++)
        {
            float v = simplexNoise.Evaluate(pointOnPlanet * frequency + noiseSettings.noiseCenter);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= noiseSettings.noiseRoughness;
            amplitude *= noiseSettings.persistance;
        }
        noiseValue = Mathf.Max(0, noiseValue - noiseSettings.minimumValue);
        return noiseValue * noiseSettings.noiseAmplitude;
    }
}
