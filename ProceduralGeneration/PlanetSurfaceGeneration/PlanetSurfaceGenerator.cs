
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using static ShapeSettings;

public class PlanetSurfaceGenerator : MonoBehaviour
{
    public int planetMinRange;
    public int planetMaxRange;
    public Shader planetShader;
    public List<Color> colorPalette;
    public List<int> planetSeeds = new List<int>();

    private ShapeSettings shapeSettings;

    [Range(2, 256)]
    private int resolution = 100;
    private int planetSeed;

    private List<GameObject> planets = new List<GameObject>();

    private MeshFilter[] meshFilters;
    private TerrainFace[] terrainFaces;
    private ShapeGenerator shapeGenerator = new ShapeGenerator();
    private ColorGenerator colorGenerator = new ColorGenerator();

    private Gradient currentGradient;

    private Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    private String[] directionStrings = { "Up", "Down", "Left", "Right", "Forward", "Back" };

    public List<GameObject> GeneratePlanets(int numberOfPlanets, int baseSeed)
    {
        this.colorPalette = new ColorPaletteGenerator().GenerateColorPalette(colorPalette.Count);
        //set base seed
        Random.InitState(baseSeed);
        for (int i = 0; i < numberOfPlanets; i++)
        {
            planetSeed = Random.Range(0, 10000);
            planetSeeds.Add(planetSeed);
            //generate planet seed with base seed, to get different planets
            Random.InitState(planetSeed);
            GenerateInput();
            planets.Add(GeneratePlanet(i));
        }
        //set base seed back
        Random.InitState(baseSeed);
        return planets;
    }

    // ----------------------------- INITIALIZATORS ----------------------------------

    private void GenerateInput()
    {
        currentGradient = SetColorGradient();

        shapeSettings = new ShapeSettings();
        shapeSettings.radius = Random.Range(planetMinRange, planetMaxRange);
        shapeSettings.noiseLayers = new NoiseLayer[3];

        for (int i = 0; i < shapeSettings.noiseLayers.Length; i++)
        {
            NoiseSettings noiseSettings = new NoiseSettings();
            NoiseLayer noiseLayer = new NoiseLayer();

            shapeSettings.noiseLayers[i] = noiseLayer;

            noiseLayer.noiseSettings = noiseSettings;
            noiseLayer.enabled = true;

            // generating the continents
            if (i == 0)
            {
                noiseLayer.useFirstLayerAsMask = false;
                noiseSettings.noiseAmplitude = Random.Range(0.1f, 0.2f);
                noiseSettings.numberOfLayers = Random.Range(2, 4);
                noiseSettings.noiseRoughness = Random.Range(0.2f, 1f);
                noiseSettings.persistance = Random.Range(0.8f, 1f);
                noiseSettings.baseRoughness = Random.Range(0.5f, 1.5f);
                if (noiseSettings.numberOfLayers == 2)
                {
                    noiseSettings.minimumValue = Random.Range(0.5f, 1f);
                }
                else
                {
                    noiseSettings.minimumValue = Random.Range(0.8f, 1.5f);
                }
            }
            // generating the mountains
            else if (i == 1)
            {
                noiseSettings.numberOfLayers = Random.Range(2, 4);
                noiseSettings.noiseAmplitude = Random.Range(0.1f, 0.5f);
                noiseSettings.persistance = Random.Range(1.5f, 2f);
                noiseSettings.noiseRoughness = Random.Range(1f, 2f);
                noiseLayer.useFirstLayerAsMask = true;
                noiseSettings.minimumValue = Random.Range(1.5f, 2f);
                noiseSettings.baseRoughness = Random.Range(1f, 2f);
            }
            else
            {
                noiseSettings.numberOfLayers = Random.Range(2, 4);
                if (noiseSettings.numberOfLayers == 2)
                {
                    noiseSettings.noiseAmplitude = Random.Range(1f, 3f);
                    noiseSettings.noiseRoughness = Random.Range(1f, 2f);
                    noiseSettings.minimumValue = Random.Range(1f, 1.5f);
                }
                else
                {
                    noiseSettings.noiseAmplitude = Random.Range(0.1f, 0.8f);
                    noiseSettings.noiseRoughness = Random.Range(1f, 1.5f);
                    noiseSettings.minimumValue = Random.Range(2f, 3f);
                }

                noiseSettings.persistance = Random.Range(1.5f, 2f);
                noiseLayer.useFirstLayerAsMask = true;
                noiseSettings.baseRoughness = Random.Range(1f, 3f);
            }
            noiseSettings.noiseCenter = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }

    private Gradient SetColorGradient()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[6];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[6];
        for (int i = 0; i < colorKey.Length; i++)
        {
            //get a random, NOT USED color from color palette
            int numberOfColorInPalette = Random.Range(0, colorPalette.Count - 1);
            colorKey[i].color = colorPalette[numberOfColorInPalette];
        }

        colorKey[0].time = 0.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[0].alpha = 1.0f;

        colorKey[1].time = 0.018f;
        alphaKey[1].time = 0.018f;
        alphaKey[1].alpha = 1.0f;

        colorKey[2].time = 0.079f;
        alphaKey[2].time = 0.079f;
        alphaKey[2].alpha = 1.0f;

        colorKey[3].time = 0.438f;
        alphaKey[3].time = 0.438f;
        alphaKey[3].alpha = 1.0f;

        colorKey[4].time = 0.8f;
        alphaKey[4].time = 0.8f;
        alphaKey[4].alpha = 1.0f;

        colorKey[5].time = 1.0f;
        alphaKey[5].time = 1.0f;
        alphaKey[5].alpha = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        return gradient;
    }

    // ----------------------------- PLANET GENERATION METHODS ---------------------------------------------
    public GameObject GeneratePlanet(int number)
    {
        GameObject planet = new GameObject("Planet" + number + "Surface");
        Material planetMaterial = new Material(planetShader);

        Initialize(planet, number, planetMaterial);
        planet.AddComponent<MeshFilter>().mesh.CombineMeshes(GenerateMeshes());
        planet.AddComponent<MeshRenderer>().material = planetMaterial;

        colorGenerator.UpdateElevation(planetMaterial, shapeGenerator.ElevationMinMax);
        colorGenerator.UpdateColors(planetMaterial, currentGradient);

        int childs = planet.transform.childCount;
        for (var i = childs - 1; i >= 0; i--)
        {
            Destroy(planet.transform.GetChild(i).gameObject);
        }

        return planet;
    }

    private void Initialize(GameObject planet, int number, Material material)
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings();
        meshFilters = new MeshFilter[6];
        terrainFaces = new TerrainFace[6];

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("TerrainFace" + number + directionStrings[i]);
                meshObj.transform.parent = planet.transform;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().material = material;
            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directions[i], resolution, shapeGenerator);
        }
    }

    private CombineInstance[] GenerateMeshes()
    {
        CombineInstance[] combineInstances = new CombineInstance[6];
        for (int i = 0; i < terrainFaces.Length; i++)
        {
            combineInstances[i].mesh = terrainFaces[i].ConstructMesh();
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        return combineInstances;
    }
}