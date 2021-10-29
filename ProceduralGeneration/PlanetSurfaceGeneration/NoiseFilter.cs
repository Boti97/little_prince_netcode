using UnityEngine;

public class NoiseFilter
{
    private readonly NoiseSettings noiseSettings;
    private readonly SimplexNoise simplexNoise = new SimplexNoise();

    public NoiseFilter(NoiseSettings noiseSettings)
    {
        this.noiseSettings = noiseSettings;
    }

    public float GenerateNoise(Vector3 pointOnPlanet)
    {
        var noiseValue = 0f;
        var frequency = noiseSettings.baseRoughness;
        var amplitude = 1f;

        for (var i = 0; i < noiseSettings.numberOfLayers; i++)
        {
            var v = simplexNoise.Evaluate(pointOnPlanet * frequency + noiseSettings.noiseCenter);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= noiseSettings.noiseRoughness;
            amplitude *= noiseSettings.persistance;
        }

        noiseValue = Mathf.Max(0, noiseValue - noiseSettings.minimumValue);
        return noiseValue * noiseSettings.noiseAmplitude;
    }
}