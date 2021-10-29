using UnityEngine;

public class ShapeGenerator
{
    private NoiseFilter[] noiseFilters;
    private ShapeSettings shapeSettings;

    public MinMax ElevationMinMax { get; private set; }

    public void UpdateSettings(ShapeSettings setting)
    {
        shapeSettings = setting;
        noiseFilters = new NoiseFilter[shapeSettings.noiseLayers.Length];
        for (var i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = new NoiseFilter(shapeSettings.noiseLayers[i].noiseSettings);
        }

        ElevationMinMax = new MinMax();
    }

    public Vector3 GenerateShape(Vector3 pointOnPlanet)
    {
        float elevation = 0;
        float firstLayerValue = 0;

        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].GenerateNoise(pointOnPlanet);
            if (shapeSettings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }

        for (var i = 1; i < noiseFilters.Length; i++)
        {
            if (!shapeSettings.noiseLayers[i].enabled) continue;
            
            var mask = (shapeSettings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
            elevation += noiseFilters[i].GenerateNoise(pointOnPlanet) * mask;
        }

        elevation = shapeSettings.radius * (1 + elevation);
        ElevationMinMax.AddValue(elevation);
        return pointOnPlanet * elevation;
    }
}