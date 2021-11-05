using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneLoadData;
using Random = UnityEngine.Random;

public class GameSceneManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject planetPrefab;
    [SerializeField] private NetworkObject leadSoldierPrefab;
    [SerializeField] private NetworkObject objectivePrefab;
    [SerializeField] private NetworkObject roomInfoManagerPrefab;

    private int baseSeed;
    private bool isRoomLive = false;

    private List<Vector3> planetPositions;
    private List<GameObject> planetSurfaces;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        //we don't want to see or do anything with the player until the room is set up
        GameObjectManager.Instance.DisableLocalPlayer();
        GameObjectManager.Instance.DisableUnnecessaryPlayerUIObjects();

        if (chosenJoinMode.Equals(JoinMode.Host))
        {
            StartHost();
        }
        else
        {
            StartClient();
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    private void StartHost()
    {
        try
        {
            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            Debug.Log("Error during starting host: " + e);
        }

        StartCoroutine(RoomHealthWatcher(JoinMode.Host));
    }

    private void StartClient()
    {
        try
        {
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.Log("Error during connecting to host: " + e);
        }

        StartCoroutine(RoomHealthWatcher(JoinMode.Client));
    }

    private IEnumerator RoomHealthWatcher(JoinMode joinMode)
    {
        while (true)
        {
            yield return new WaitForSeconds(5);

            if (RoomInfoManager.Instance == null || !RoomInfoManager.Instance.RoomNetworkState.IsRoomLive)
            {
                Debug.LogWarning("Room is not alive.");
                Destroy(GameObject.FindWithTag("NetworkManager"));
                if (joinMode.Equals(JoinMode.Client))
                {
                    ReasonForSceneLoad = "Unable to connect to room.";
                }
                else
                {
                    ReasonForSceneLoad = "Unable to start room hosting.";
                }

                SceneManager.LoadScene("Start");
            }
            else
            {
                Debug.Log("Room is alive.");
            }
        }
    }

    //----------------------------------- CALLBACK METHODS -----------------------------------
    private void HandleServerStarted()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            return;
        }

        StartCoroutine(SetUpServer());
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }

        StartCoroutine(SetUpClient());
    }

    private IEnumerator SetUpServer()
    {
        var roomInfoManagerObj = Instantiate(roomInfoManagerPrefab, Vector3.zero, Quaternion.identity);
        roomInfoManagerObj.SpawnWithOwnership(NetworkManager.Singleton.ServerClientId);

        RoomInfoManager.Instance.RoomNetworkState.SetIsRoomLiveServerRpc(true);

        //create new random seed
        baseSeed = Random.Range(0, 100000);
        //send new seed to server
        RoomInfoManager.Instance.RoomNetworkState.SetRoomSeedServerRpc(baseSeed);

        //generate planet positions, and planet surfaces
        yield return StartCoroutine(GetPlanetPositions());
        yield return StartCoroutine(GetPlanetSurfaces());

        InitiatePlanetObjects(planetPositions);

        GameObjectManager.Instance.RefreshPlanets();

        var planetsOrderedById =
            GameObjectManager.Instance.Planets
                .OrderBy(planet => planet.GetComponent<NetworkObject>().NetworkObjectId)
                .ToList();
        //add name and surface to planets
        SetUpPlanets(planetsOrderedById, planetSurfaces);

        var playerLocation = SetPlayerLocation();

        RoomInfoManager.Instance.RoomNetworkState.ReportPlayerJoinedServerRpc(GameObjectManager.Instance
            .GetLocalPlayerId());
        //RoomInfoManager.Instance.ReportJoinedPlayer(GameObjectManager.Instance.GetLocalPlayerId());

        SpawnEnemiesAndObjectives(playerLocation);
    }

    private IEnumerator SetUpClient()
    {
        //init randomizer for generation
        baseSeed = RoomInfoManager.Instance.RoomNetworkState.RoomSeed;

        //refresh planets, at this point Netcode should synchronize them already, but without surface
        GameObjectManager.Instance.RefreshPlanets();
        planetPositions = GameObjectManager.Instance.GetPlanetPositions();

        //generate planet positions, and planet surfaces
        yield return StartCoroutine(GetPlanetSurfaces());

        var planetsOrderedById =
            GameObjectManager.Instance.Planets
                .OrderBy(planet => planet.GetComponent<NetworkObject>().NetworkObjectId)
                .ToList();
        //add name and surface to planets
        SetUpPlanets(planetsOrderedById, planetSurfaces);

        SetPlayerLocation();

        RoomInfoManager.Instance.RoomNetworkState.ReportPlayerJoinedServerRpc(GameObjectManager.Instance
            .GetLocalPlayerId());
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        //if the host kicked us out
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SceneManager.LoadScene("Start");
        }
    }
    //----------------------------------- PLAYER SETUP METHODS -----------------------------------

    /// <summary>
    /// Move player to a random planet
    /// </summary>
    /// <returns>the network id of the planet the player was moved to</returns>
    private ulong SetPlayerLocation()
    {
        GameObjectManager.Instance.EnableLocalPlayer();
        GameObjectManager.Instance.LoadingBar.gameObject.SetActive(false);
        GameObjectManager.Instance.EnableUnnecessaryPlayerUIObjects();

        GameObjectManager.Instance.RefreshPlanets();
        GameObjectManager.Instance.RefreshPlayers();

        var player = GameObjectManager.Instance.GetLocalPlayer();

        var planetsOrderById =
            GameObjectManager.Instance.Planets.OrderBy(planet => planet.GetComponent<NetworkObject>().NetworkObjectId)
                .ToList();
        var minPlanetId = planetsOrderById[0].GetComponent<NetworkObject>().NetworkObjectId;
        var maxPlanetId = planetsOrderById[planetsOrderById.Count - 1].GetComponent<NetworkObject>().NetworkObjectId;
        //TODO: not used because with minPlanetId it is easier to debug, use after debugging
        //var randomPlanetIndex = (ulong) Random.Range(minPlanetId, maxPlanetId);
        var randomPlanetIndex = minPlanetId;

        //TODO: uncomment when enemies added
        //GameObjectManager.Instance.RemoveEnemiesOnPlanet(GameObjectManager.Instance.Planets[randomPlanetIndex].GetComponentInChildren<PlanetNetworkState>().PlanetId);

        var spawnPos = GameObjectManager.Instance.Planets
            .Find(planet => planet.GetComponent<NetworkObject>().NetworkObjectId == randomPlanetIndex).transform
            .position;
        spawnPos.x += 35;

        player.transform.position = spawnPos;

        return randomPlanetIndex;
    }

    //----------------------------------- ENEMY SETUP METHODS -----------------------------------
    private void SpawnEnemiesAndObjectives(ulong planetIdToSkip)
    {
        GameObjectManager.Instance.Planets
            .Where(planet => planet.GetComponent<NetworkObject>().NetworkObjectId != planetIdToSkip)
            .ToList()
            .ForEach(planet =>
            {
                var planetPosition = planet.transform.position;
                var enemySpawnPos = planetPosition;
                enemySpawnPos.x += 35;
                var enemy = Instantiate(leadSoldierPrefab, enemySpawnPos, Quaternion.identity);
                enemy.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);

                var objectiveSpawnPos = planetPosition;
                objectiveSpawnPos.x += 35;
                var objective = Instantiate(objectivePrefab, objectiveSpawnPos, Quaternion.identity);
                objective.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
            });

        GameObjectManager.Instance.Planets
            .Where(planet => planet.GetComponent<NetworkObject>().NetworkObjectId == planetIdToSkip)
            .ToList()
            .ForEach(planet =>
            {
                var planetPosition = planet.transform.position;

                for (var i = 0; i < 30; i++)
                {
                    var objectiveSpawnPos = planetPosition;
                    objectiveSpawnPos.y += 35;
                    var objective = Instantiate(objectivePrefab, objectiveSpawnPos, Quaternion.identity);
                    objective.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
                }
            });

        GameObjectManager.Instance.RefreshEnemies();
    }

    //----------------------------------- PLANET SETUP METHODS -----------------------------------
    private void InitiatePlanetObjects(IReadOnlyList<Vector3> planetPositions)
    {
        foreach (var planetPosition in planetPositions)
        {
            var planet = Instantiate(planetPrefab, planetPosition, Quaternion.identity);
            planet.SpawnWithOwnership(NetworkManager.Singleton.ServerClientId);
        }
    }

    private void SetUpPlanets(IReadOnlyList<GameObject> planets, IReadOnlyList<GameObject> planetSurfaces)
    {
        for (var i = 0; i < planets.Count; i++)
        {
            planetSurfaces[i].transform.parent = planets[i].transform;
            planetSurfaces[i].transform.localPosition = Vector3.zero;
            planets[i].name = "Planet" + i;
        }
    }

    //----------------------------------- GENERATOR CALLS  -----------------------------------
    private IEnumerator GetPlanetPositions()
    {
        planetPositions = gameObject.GetComponent<PlanetPositionGenerator>().GeneratePlanetPositions(baseSeed);
        yield break;
    }

    private IEnumerator GetPlanetSurfaces()
    {
        yield return StartCoroutine(gameObject.GetComponent<PlanetSurfaceGenerator>()
            .GeneratePlanets(planetPositions.Count, baseSeed));
        planetSurfaces = gameObject.GetComponent<PlanetSurfaceGenerator>().Planets;
    }
}