using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameSceneManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject planetPrefab;
    [SerializeField] private GameObject leadSoldierPrefab;
    [SerializeField] private GameObject objectivePrefab;
    [SerializeField] private float solarSystemRadius;

    [SerializeField] private int baseSeed;
    private ServerNetworkState serverNetworkState;

    public void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;

        serverNetworkState = transform.GetComponent<ServerNetworkState>();

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        if (SceneNetworkData.choosenJoinMode.Equals(SceneNetworkData.JoinMode.HOST))
        {
            StartCoroutine(StartHost());
        }
        else
        {
            StartCoroutine(StartClient());
        }
    }

    public void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    private IEnumerator StartHost()
    {
        //TODO: implement
        yield return new WaitForSeconds(0f);
        NetworkManager.Singleton.StartHost();
    }

    private IEnumerator StartClient()
    {
        //TODO: implement
        yield return new WaitForSeconds(0f);
        NetworkManager.Singleton.StartClient();
    }

    private void HandleServerStarted()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            return;
        }

        //create new random seed
        baseSeed = Random.Range(0, 100000);
        //send new seed to server
        serverNetworkState.SetRoomSeedServerRpc(baseSeed);

        //generate planet positions, and planet surfaces
        var planetPositions = GetPlanetPositions();
        var planetSurfaces = GetPlanetSurfaces(planetPositions.Count);

        InitiatePlanetObjects(planetPositions);

        GameObjectManager.Instance.RefreshPlanets();

        //add name and surface to planets
        SetUpPlanets(GameObjectManager.Instance.Planets, planetSurfaces);

        SetPlayerLocation();
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }

        //init randomizer for generation
        Debug.LogWarning(serverNetworkState.RoomSeed);
        baseSeed = serverNetworkState.RoomSeed;

        //refresh planets, at this point Netcode should synchronize them already, but without surface
        GameObjectManager.Instance.RefreshPlanets();

        //generate planet positions, and planet surfaces
        var planetSurfaces = GetPlanetSurfaces(GameObjectManager.Instance.Planets.Count);

        //add name and surface to planets
        SetUpPlanets(GameObjectManager.Instance.Planets, planetSurfaces);

        SetPlayerLocation();
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count == 1 &&
                NetworkManager.Singleton.ConnectedClients[0].ClientId == NetworkManager.Singleton.LocalClientId)
            {
                GameObjectManager.Instance.YouWonText.SetActive(true);
                GameObjectManager.Instance.CinemachineVirtualCamera.gameObject.SetActive(false);
            }
        }
    }

    private void SetPlayerLocation()
    {
        GameObjectManager.Instance.RefreshPlanets();
        GameObjectManager.Instance.RefreshPlayers();

        GameObject player = GameObjectManager.Instance.GetOwnedPlayerById(NetworkManager.Singleton.LocalClientId);

        //TODO: change this to random planet, only for debugging purposes
        //int randomPlanetIndex = UnityEngine.Random.Range(0, GameObjectManager.Instance.Planets.Count - 1);
        var randomPlanetIndex = 0;

        //GameObjectManager.Instance.RemoveEnemiesOnPlanet(GameObjectManager.Instance.Planets[randomPlanetIndex].GetComponentInChildren<PlanetNetworkState>().PlanetId);

        var spawnPos = GameObjectManager.Instance.Planets[randomPlanetIndex].transform.position;
        spawnPos.x += 30;

        player.transform.position = spawnPos;
    }

    private void SpawnEnemiesAndObjectives()
    {
        //Vector3 enemySpawnPos;

        //enemySpawnPos = GameObjectManager.Instance.Planets[0].transform.position;
        //enemySpawnPos.x += 30;
        //BoltNetwork.Instantiate(leadSoldierPrefab, enemySpawnPos, Quaternion.identity);

        GameObjectManager.Instance.Planets.ForEach(planet =>
        {
            var numberOfEnemies = Random.Range(1, 4);
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

            var probabilityOfObjectives = Random.Range(1, 101);
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
        var randomPlanetIndex = 0;
        //GameObjectManager.Instance.RemoveEnemiesOnPlanet(GameObjectManager.Instance.Planets[randomPlanetIndex].GetComponentInChildren<PlanetNetworkState>().PlanetId);

        var spawnPos = GameObjectManager.Instance.Planets[randomPlanetIndex].transform.position;
        spawnPos.x += 30;

        //BoltNetwork.Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    private void InitiatePlanetObjects(List<Vector3> planetPositions)
    {
        for (var i = 0; i < planetPositions.Count; i++)
        {
            var planet = Instantiate(planetPrefab, planetPositions[i], Quaternion.identity);
            planet.SpawnWithOwnership(NetworkManager.Singleton.ServerClientId);
        }
    }

    private void SetUpPlanets(List<GameObject> planets, List<GameObject> planetSurfaces)
    {
        for (var i = 0; i < planets.Count; i++)
        {
            planetSurfaces[i].transform.parent = planets[i].transform;
            planetSurfaces[i].transform.localPosition = Vector3.zero;
            planets[i].name = "Planet" + i;
        }
    }

    private List<Vector3> GetPlanetPositions()
    {
        return gameObject.GetComponent<PlanetPositionGenerator>().GeneratePlanetPositions(baseSeed);
    }

    private List<GameObject> GetPlanetSurfaces(int numberOfPlanets)
    {
        return gameObject.GetComponent<PlanetSurfaceGenerator>().GeneratePlanets(numberOfPlanets, baseSeed);
    }
}