using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSettings
{
    [Range(0.1f, 5f)]
    public float noiseAmplitude = 0f;
    [Range(0.02f, 3f)]
    public float noiseRoughness = 0f;
    public Vector3 noiseCenter;
    [Range(1, 8)]
    public int numberOfLayers = 1;
    [Range(0.5f, 3f)]
    public float baseRoughness;
    public float persistance;
    [Range(0.5f, 3f)]
    public float minimumValue = 1;
}
