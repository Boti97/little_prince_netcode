using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject planetPrefab;
    [SerializeField] private GameObject leadSoldierPrefab;
    [SerializeField] private GameObject objectivePrefab;
    [SerializeField] private NetworkObject roomInfoManagerPrefab;

    [SerializeField] private int baseSeed;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        StartCoroutine(SceneNetworkData.chosenJoinMode.Equals(SceneNetworkData.JoinMode.Host)
            ? StartHost()
            : StartClient());
    }

    public override void OnDestroy()
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

    //----------------------------------- CALLBACK METHODS -----------------------------------
    private void HandleServerStarted()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            return;
        }

        var roomInfoManager = Instantiate(roomInfoManagerPrefab, Vector3.zero, Quaternion.identity);
        roomInfoManager.SpawnWithOwnership(NetworkManager.Singleton.ServerClientId);

        //create new random seed
        baseSeed = Random.Range(0, 100000);
        //send new seed to server
        RoomInfoManager.Instance.SetRoomSeed(baseSeed);

        //generate planet positions, and planet surfaces
        var planetPositions = GetPlanetPositions();
        var planetSurfaces = GetPlanetSurfaces(planetPositions.Count);

        InitiatePlanetObjects(planetPositions);

        GameObjectManager.Instance.RefreshPlanets();

        var planetsOrderedById =
            GameObjectManager.Instance.Planets
                .OrderBy(planet => planet.GetComponent<NetworkObject>().NetworkObjectId)
                .ToList();
        //add name and surface to planets
        SetUpPlanets(planetsOrderedById, planetSurfaces);

        SetPlayerLocation();

        RoomInfoManager.Instance.IncreaseNumberOfLivePlayers(NetworkManager.Singleton.LocalClientId);
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }

        //init randomizer for generation
        baseSeed = RoomInfoManager.Instance.RoomNetworkState.RoomSeed;

        //refresh planets, at this point Netcode should synchronize them already, but without surface
        GameObjectManager.Instance.RefreshPlanets();

        //generate planet positions, and planet surfaces
        var planetSurfaces = GetPlanetSurfaces(GameObjectManager.Instance.Planets.Count);

        var planetsOrderedById =
            GameObjectManager.Instance.Planets
                .OrderBy(planet => planet.GetComponent<NetworkObject>().NetworkObjectId)
                .ToList();
        //add name and surface to planets
        SetUpPlanets(planetsOrderedById, planetSurfaces);

        SetPlayerLocation();

        RoomInfoManager.Instance.IncreaseNumberOfLivePlayers(NetworkManager.Singleton.LocalClientId);
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

    //move player to a random starting position
    private void SetPlayerLocation()
    {
        GameObjectManager.Instance.RefreshPlanets();
        GameObjectManager.Instance.RefreshPlayers();

        var player = GameObjectManager.Instance.GetOwnedPlayerById(NetworkManager.Singleton.LocalClientId);

        var planetsOrderById =
            GameObjectManager.Instance.Planets.OrderBy(planet => planet.GetComponent<NetworkObject>().NetworkObjectId)
                .ToList();
        var minPlanetId = planetsOrderById[0].GetComponent<NetworkObject>().NetworkObjectId;
        var maxPlanetId = planetsOrderById[planetsOrderById.Count - 1].GetComponent<NetworkObject>().NetworkObjectId;
        var randomPlanetIndex = (ulong) Random.Range(minPlanetId, maxPlanetId);

        //TODO: uncomment when enemies added
        //GameObjectManager.Instance.RemoveEnemiesOnPlanet(GameObjectManager.Instance.Planets[randomPlanetIndex].GetComponentInChildren<PlanetNetworkState>().PlanetId);

        var spawnPos = GameObjectManager.Instance.Planets
            .Find(planet => planet.GetComponent<NetworkObject>().NetworkObjectId == randomPlanetIndex).transform
            .position;
        spawnPos.x += 30;

        player.transform.position = spawnPos;
    }

    //----------------------------------- ENEMY SETUP METHODS -----------------------------------
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
                var objectiveSpawnPos = planet.transform.position;
                objectiveSpawnPos.z += 30;
                //BoltNetwork.Instantiate(objectivePrefab, objectiveSpawnPos, Quaternion.identity);
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
    private List<Vector3> GetPlanetPositions()
    {
        return gameObject.GetComponent<PlanetPositionGenerator>().GeneratePlanetPositions(baseSeed);
    }

    private List<GameObject> GetPlanetSurfaces(int numberOfPlanets)
    {
        return gameObject.GetComponent<PlanetSurfaceGenerator>().GeneratePlanets(numberOfPlanets, baseSeed);
    }
}