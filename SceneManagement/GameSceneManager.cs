using Unity.Netcode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private NetworkObject planetPrefab;

    [SerializeField]
    private GameObject planetObjectPrefab;

    [SerializeField]
    private GameObject leadSoldierPrefab;

    [SerializeField]
    private GameObject sunPrefab;

    [SerializeField]
    private GameObject objectivePrefab;

    [SerializeField]
    private float solarSystemRadius;

    [SerializeField]
    private int baseSeed;

    public void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        if (SceneNetworkData.choosenJoinMode.Equals(SceneNetworkData.JoinMode.HOST))
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    public void OnDestroy()
    {
        if (NetworkManager.Singleton == null) { return; }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            CreateSolarSystem();
            HandleClientConnected(NetworkManager.Singleton.ServerClientId);
        }

        //StartCoroutine(SpawnPlayer());
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {

        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {

        }
    }


    private void SpawnEnemiesAndObjectives()
    {
        //Vector3 enemySpawnPos;

        //enemySpawnPos = GameObjectManager.Instance.Planets[0].transform.position;
        //enemySpawnPos.x += 30;
        //BoltNetwork.Instantiate(leadSoldierPrefab, enemySpawnPos, Quaternion.identity);

        GameObjectManager.Instance.Planets.ForEach(planet =>
        {
            int numberOfEnemies = Random.Range(1, 4);
            Vector3 enemySpawnPos;
            Vector3 objectiveSpawnPos;
            if (numberOfEnemies > 0)
            {
                enemySpawnPos = planet.transform.position;
                enemySpawnPos.x += 30;
                //BoltNetwork.Instantiate(leadSoldierPrefab, enemySpawnPos, Quaternion.identity);
            }
            if (numberOfEnemies > 1)
            {
                enemySpawnPos = planet.transform.position;
                enemySpawnPos.y += 30;
                //BoltNetwork.Instantiate(leadSoldierPrefab, enemySpawnPos, Quaternion.identity);
            }
            if (numberOfEnemies > 2)
            {
                enemySpawnPos = planet.transform.position;
                enemySpawnPos.z += 30;
                //BoltNetwork.Instantiate(leadSoldierPrefab, enemySpawnPos, Quaternion.identity);
            }

            int probabilityOfObjectives = UnityEngine.Random.Range(1, 101);
            if (probabilityOfObjectives > 50)
            {
                objectiveSpawnPos = planet.transform.position;
                objectiveSpawnPos.z += 30;
                //BoltNetwork.Instantiate(objectivePrefab, objectiveSpawnPos, Quaternion.identity);
            }
        });

        GameObjectManager.Instance.RefreshEnemies();
    }

    //spawn the player to a random planet
    private IEnumerator SpawnPlayer()
    {
        yield return StartCoroutine(GameObjectManager.Instance.RefreshPlanetsCoroutine());
        //yield return StartCoroutine(GameObjectManager.Instance.RefreshEnemiesCoroutine());

        //TODO: change this to random planet, only for debugging purposes
        //int randomPlanetIndex = UnityEngine.Random.Range(0, GameObjectManager.Instance.Planets.Count - 1);
        int randomPlanetIndex = 0;
        //GameObjectManager.Instance.RemoveEnemiesOnPlanet(GameObjectManager.Instance.Planets[randomPlanetIndex].GetComponentInChildren<PlanetNetworkState>().PlanetId);

        Vector3 spawnPos = GameObjectManager.Instance.Planets[randomPlanetIndex].transform.position;
        spawnPos.x += 30;

        //BoltNetwork.Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    private void CreateSolarSystem()
    {
        //GameObjectManager.Instance.Sun = BoltNetwork.Instantiate(sunPrefab).gameObject;

        //set random generation seed for recoverability
        baseSeed = Random.Range(0, 10000);
        Random.InitState(baseSeed);

        List<Vector3> planetPositions = GetPlanetPositions();
        //List<GameObject> planetSurfaces = GetPlanetSurfaces(planetPositions.Count);

        //foreach (var planetPosition in GetPlanetPositions())
        //{
        //    BoltNetwork.Instantiate(planetPrefab, planetPosition, UnityEngine.Random.rotation);
        //}
        NetworkObject planet = Instantiate(planetPrefab, new Vector3(-50f, 0f, 0f), Quaternion.identity);
        planet.SpawnWithOwnership(NetworkManager.Singleton.ServerClientId);

        //SetUpPlanetSurfaceComponents(planetSurfaces, new List<GameObject>());
        //
        //InitiatePlanetObjects(planetPositions, planetSurfaces);


        // Todo: more complex planet placement
        //for (int i = 0; i < planetPositions.Count; i++)
        //{
        //    GameObject planetX = BoltNetwork.Instantiate(planetSurfaces[i], planetPositions[i], Random.rotation).gameObject;
        //}

        GameObjectManager.Instance.RefreshPlanets();
    }

    private void InitiatePlanetObjects(List<Vector3> planetPositions, List<GameObject> planetSurfaces)
    {
        List<GameObject> planetObjects = new List<GameObject>();
        for (int i = 0; i < planetPositions.Count; i++)
        {
            planetPrefab.GetComponentInChildren<MeshFilter>().mesh = planetSurfaces[i].GetComponent<MeshFilter>().mesh;
            planetPrefab.GetComponentInChildren<MeshRenderer>().material = planetSurfaces[i].GetComponent<MeshRenderer>().material;
            planetPrefab.GetComponentInChildren<MeshCollider>().sharedMesh = planetSurfaces[i].GetComponent<MeshCollider>().sharedMesh;

            //GameObject planetObj = Instantiate(planetObjectPrefab, planetPositions[i], Random.rotation);

            //GameObject planet = BoltNetwork.Instantiate(planetPrefab, planetPositions[i], Random.rotation);


            //planetSurfaces[i].transform.parent = planetObject.transform;
            //planetSurfaces[i].transform.localPosition = Vector3.zero;

            //planet.name = "Planet" + i;

            ////BoltNetwork.Instantiate(planetObject);
            //Destroy(planetObj);
        }
        foreach (var planetSurf in planetSurfaces)
        {
            Destroy(planetSurf);
        }
    }

    private void SetUpPlanetSurfaceComponents(List<GameObject> planetSurfaces, List<GameObject> planetObjects)
    {
        for (int i = 0; i < planetSurfaces.Count; i++)
        {
            //assaign unique bolt entity to planet
            //planetSurfaces[i].transform.parent = planetObjects[i].transform;
            //planetSurfaces[i].transform.localPosition = Vector3.zero;

            //set up planet core
            //planetCore.GetComponent<SphereCollider>().radius = 10;
            //planetCore.transform.parent = planetSurfaces[i].transform;

            //set up bolt entity 
            //BoltEntity source = boltEntityPrefab.GetComponent<BoltEntity>();
            //BoltEntity target = planets[i].AddComponent<BoltEntity>();
            //CopyClassValues(source, target);

            //set up network state
            //planetSurfaces[i].AddComponent<PlanetNetworkState>();

            //set up planet behaviour
            //planets[i].AddComponent<PlanetBehaviour>();

            //tag planet
            planetSurfaces[i].tag = "Planet";

            //set Ground layer
            planetSurfaces[i].layer = 8;

            //add Mesh collider
            planetSurfaces[i].AddComponent<MeshCollider>();
        }
    }

    private List<Vector3> GetPlanetPositions()
    {
        return gameObject.GetComponent<PlanetPositionGenerator>().GeneratePlanetPositions();
    }

    private List<GameObject> GetPlanetSurfaces(int numberOfPlanets)
    {
        return gameObject.GetComponent<PlanetSurfaceGenerator>().GeneratePlanets(numberOfPlanets, baseSeed);
    }
}