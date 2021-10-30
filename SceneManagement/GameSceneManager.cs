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
        GameObjectManager.Instance.DisablePlayerBars();

        if (SceneLoadData.chosenJoinMode.Equals(SceneLoadData.JoinMode.Host))
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
        NetworkManager.Singleton.StartHost();
        StartCoroutine(RoomHealthWatcher());
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        StartCoroutine(RoomHealthWatcher());
    }

    private IEnumerator RoomHealthWatcher()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);

            if (RoomInfoManager.Instance == null || !RoomInfoManager.Instance.IsRoomLive())
            {
                Debug.LogWarning("Room is not alive.");
                Destroy(NetworkManager.Singleton);
                SceneLoadData.ReasonForSceneLoad = "Unable to connect to room.";
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

        RoomInfoManager.Instance.SetIsRoomLive(true);

        //create new random seed
        baseSeed = Random.Range(0, 100000);
        //send new seed to server
        RoomInfoManager.Instance.SetRoomSeed(baseSeed);

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

        SetPlayerLocation();

        RoomInfoManager.Instance.IncreaseNumberOfLivePlayers(NetworkManager.Singleton.LocalClientId);
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
        GameObjectManager.Instance.EnableLocalPlayer();
        GameObjectManager.Instance.LoadingBar.gameObject.SetActive(false);
        GameObjectManager.Instance.EnablePlayerBars();

        GameObjectManager.Instance.RefreshPlanets();
        GameObjectManager.Instance.RefreshPlayers();

        var player = GameObjectManager.Instance.GetOwnedPlayerById(NetworkManager.Singleton.LocalClientId);

        var planetsOrderById =
            GameObjectManager.Instance.Planets.OrderBy(planet => planet.GetComponent<NetworkObject>().NetworkObjectId)
                .ToList();
        var minPlanetId = planetsOrderById[0].GetComponent<NetworkObject>().NetworkObjectId;
        var maxPlanetId = planetsOrderById[planetsOrderById.Count - 1].GetComponent<NetworkObject>().NetworkObjectId;
        //TODO: not used becouse with minPlanetId it is easier to debug, use after debugging
        var randomPlanetIndex = (ulong) Random.Range(minPlanetId, maxPlanetId);

        //TODO: uncomment when enemies added
        //GameObjectManager.Instance.RemoveEnemiesOnPlanet(GameObjectManager.Instance.Planets[randomPlanetIndex].GetComponentInChildren<PlanetNetworkState>().PlanetId);

        var spawnPos = GameObjectManager.Instance.Planets
            .Find(planet => planet.GetComponent<NetworkObject>().NetworkObjectId == minPlanetId).transform
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