using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using static BaseFunctionValueGenerator;

public class PlanetPositionGenerator : MonoBehaviour
{
    [SerializeField] private Camera localCamera;

    [SerializeField] private float planetDensity;
    private int baseCircleRadius;

    private LineRenderer functionView;

    private int lineRendererDivisionNum;
    private float noiseAmplitude;
    private float noiseRoughness;
    private float noiseSeed;

    public List<Vector3> GeneratePlanetPositions(int baseSeed)
    {
        GenerateInput(baseSeed);

        SetupFieldValues();

        CreateFunctionRepresentation();

        List<Vector3> planetPositions =
            PlanetPositionFinder.FindPlanetPositions(functionView, lineRendererDivisionNum, planetDensity);

        Destroy(functionView.gameObject);

        return planetPositions;
    }

    private void GenerateInput(int baseSeed)
    {
        Random.InitState(baseSeed);
        Debug.Log("Input Base Seed: " + Random.seed);

        noiseSeed = Random.Range(0f, 5f);
        Debug.Log("Noise Seed: " + noiseSeed);

        baseCircleRadius = Random.Range(300, 500);
        Debug.Log("Base Circle Radius: " + (baseCircleRadius / 1000f));

        noiseAmplitude = Random.Range(0.2f, 0.6f) / (baseCircleRadius / 500f);
        Debug.Log("Noise Amplitude: " + noiseAmplitude);

        noiseRoughness = Random.Range(0.01f, 0.1f) / (baseCircleRadius / 500f);
        Debug.Log("Noise Roughness: " + noiseRoughness);

        lineRendererDivisionNum = 1000;
    }

    private void SetupFieldValues()
    {
        functionView = Instantiate(new GameObject()).AddComponent<LineRenderer>();

        if (lineRendererDivisionNum <= 0) lineRendererDivisionNum = 0;
        if (lineRendererDivisionNum % 2 == 0) lineRendererDivisionNum++;
    }

    private void CreateFunctionRepresentation()
    {
        CreateFunctionLineRenderer();

        float angle = 360f / (lineRendererDivisionNum - 1);
        for (int i = 0; i < lineRendererDivisionNum; i++)
        {
            FloatPoint point = BaseFunction(baseCircleRadius, angle * i);
            float noise = NoiseGenerator.GenerateNoise(point.X, point.Y, noiseAmplitude, noiseRoughness, noiseSeed);
            functionView.SetPosition(i, new Vector3(point.X, point.Y, 0f) * noise);
        }
    }

    private void CreateFunctionLineRenderer()
    {
        functionView.material = new Material(Shader.Find("Sprites/Default"));
        functionView.widthMultiplier = 0.01f * baseCircleRadius;
        functionView.positionCount = lineRendererDivisionNum;
        functionView.startColor = Color.cyan;
        functionView.endColor = Color.cyan;
    }
}