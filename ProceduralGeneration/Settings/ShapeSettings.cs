using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeSettings
{
    [Range(5, 10)]
    public float radius;
    public NoiseLayer[] noiseLayers;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;
        public bool useFirstLayerAsMask;
        public NoiseSettings noiseSettings;
    }
}
