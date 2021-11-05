using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameObjectManager : MonoBehaviour
{
    private static readonly object Padlock = new object();
    private static GameObjectManager _instance;
    [SerializeField] private NetworkObject headstonePrefab;

    public Slider StaminaBar { get; private set; }
    public Slider HealthBar { get; private set; }
    public Slider ThrustBar { get; private set; }
    public Slider LoadingBar { get; private set; }
    public GameObject GameOverText { get; private set; }
    public GameObject YouWonText { get; private set; }
    public CinemachineFreeLook CinemachineVirtualCamera { get; private set; }
    public List<GameObject> Planets { get; private set; }
    public List<GameObject> Players { get; private set; }
    private List<GameObject> Enemies { get; set; }
    private int Score { get; set; }
    private TMP_Text ScoreText { get; set; }

    public static GameObjectManager Instance
    {
        get
        {
            lock (Padlock)
            {
                if (_instance == null)
                {
                    _instance = new GameObjectManager();
                }

                return _instance;
            }
        }
    }

    public void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        StaminaBar = GameObject.FindGameObjectWithTag("StaminaBar").GetComponent<Slider>();
        ThrustBar = GameObject.FindGameObjectWithTag("ThrustBar").GetComponent<Slider>();
        HealthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Slider>();
        LoadingBar = GameObject.FindGameObjectWithTag("LoadingBar").GetComponent<Slider>();
        GameOverText = GameObject.FindGameObjectWithTag("GameOver");
        YouWonText = GameObject.FindGameObjectWithTag("YouWon");
        CinemachineVirtualCamera =
            GameObject.FindGameObjectWithTag("ThirdPersonCamera").GetComponent<CinemachineFreeLook>();

        Planets = new List<GameObject>();
        Planets.AddRange(GameObject.FindGameObjectsWithTag("Planet"));

        Players = new List<GameObject>();
        Players.AddRange(GameObject.FindGameObjectsWithTag("Player"));

        Enemies = new List<GameObject>();
        Enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

        ScoreText = GameObject.FindWithTag("Score").GetComponent<TMP_Text>();

        DeactivateUnnecessaryGameObjects();
    }

    //----------------------------------- PLAYER RELATED METHODS -----------------------------------

    public void RefreshPlayers()
    {
        Players.Clear();
        Players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
    }

    public IEnumerator RefreshPlayersCoroutine()
    {
        Players.Clear();
        while (Players.Count.Equals(0))
        {
            yield return new WaitForSeconds(1f);
            Players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        }
    }

    public void RemovePlayer(ulong playerNetworkId)
    {
        var deadPlayer = GetPlayerById(playerNetworkId);
        Instantiate(headstonePrefab, deadPlayer.transform.position, Quaternion.identity);
        Players.Remove(deadPlayer);
        Destroy(deadPlayer);
    }

    public bool IsPlayerOwned(ulong id)
    {
        return GetOwnedPlayerId() == id;
    }

    public GameObject GetPlayerById(ulong id)
    {
        return Players.Find(player => player.GetComponent<NetworkObject>().NetworkObjectId == id);
    }

    public GameObject GetOwnedPlayerById(ulong ownerId)
    {
        return Players.Find(player => player.GetComponent<NetworkObject>().OwnerClientId == ownerId);
    }

    private ulong GetOwnedPlayerId()
    {
        return Players.Find(player => player.GetComponent<NetworkObject>().IsOwner)
            .GetComponent<CharacterNetworkState>().NetworkObjectId;
    }

    public bool IsOwnedPlayerAlive()
    {
        return Players.Find(player => player.GetComponent<NetworkObject>().IsOwner) != null;
    }

    public List<GameObject> FindPlayersOnPlanet(ulong planetId)
    {
        return Players.FindAll(player => player.GetComponent<PlayerBehaviour>().planetId == planetId);
    }

    public void DisableLocalPlayer()
    {
        CinemachineVirtualCamera.gameObject.SetActive(false);
    }

    public void EnableLocalPlayer()
    {
        CinemachineVirtualCamera.gameObject.SetActive(true);
    }

    public void DisableLocalPlayerMovement()
    {
        CinemachineVirtualCamera.gameObject.SetActive(false);
        CinemachineVirtualCamera.Follow.gameObject.GetComponent<PlayerBehaviour>().enabled = false;
    }

    public void EnableLocalPlayerMovement()
    {
        CinemachineVirtualCamera.gameObject.SetActive(true);
        CinemachineVirtualCamera.Follow.gameObject.GetComponent<PlayerBehaviour>().enabled = true;
    }

    public void CreateHeadstoneForPlayer(ulong playerId)
    {
        var player = GetPlayerById(playerId);
        if (player == null)
        {
            Debug.LogWarning("Dead player doesn't exist in scene.");
        }
        else
        {
            CreateHeadstoneOnPositions(player.transform.position);
        }
    }

    public void DisablePlayerById(ulong playerId)
    {
        var player = GetPlayerById(playerId);
        if (player == null)
        {
            Debug.LogWarning("Player doesn't exist in scene.");
        }
        else
        {
            player.SetActive(false);
        }
    }

    public void CreateHeadstoneOnPositions(Vector3 position)
    {
        var headstone = Instantiate(headstonePrefab, position, Quaternion.identity);
        headstone.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
    }

    public ulong GetLocalPlayerId()
    {
        return CinemachineVirtualCamera.Follow.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
    }

    public GameObject GetLocalPlayer()
    {
        return GetPlayerById(GetLocalPlayerId());
    }

    //----------------------------------- ENEMY RELATED METHODS -----------------------------------
    public IEnumerator RefreshEnemiesCoroutine()
    {
        while (Enemies.Count.Equals(0))
        {
            yield return new WaitForSeconds(1f);
            Enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        }
    }

    private List<GameObject> FindEnemiesOnPlanet(ulong planetId)
    {
        return Enemies.FindAll(enemy => enemy.GetComponent<EnemyBehaviour>().planetId == planetId);
    }

    public void RemoveEnemiesOnPlanet(ulong planetId)
    {
        FindEnemiesOnPlanet(planetId).ForEach(enemy =>
        {
            Enemies.Remove(enemy);
            Destroy(enemy);
        });
    }

    public void RefreshEnemies()
    {
        Enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
    }

    //----------------------------------- PLANET RELATED METHODS -----------------------------------
    public IEnumerator RefreshPlanetsCoroutine()
    {
        Planets.Clear();
        while (Planets.Count.Equals(0))
        {
            yield return new WaitForSeconds(1f);
            Planets.AddRange(GameObject.FindGameObjectsWithTag("Planet"));
        }
    }

    public void RefreshPlanets()
    {
        Planets.Clear();
        Planets.AddRange(GameObject.FindGameObjectsWithTag("Planet"));
    }

    public GameObject GetPlanetById(ulong planetNetworkId)
    {
        return Planets.Find(planet => planet.GetComponent<NetworkObject>().NetworkObjectId == planetNetworkId);
    }

    public List<Vector3> GetPlanetPositions()
    {
        var planetPositions = new List<Vector3>();
        Planets.ForEach(planet => planetPositions.Add(planet.transform.position));
        return planetPositions;
    }

    //----------------------------------- OTHER METHODS -----------------------------------
    public bool IsGameOver()
    {
        return GameOverText.activeSelf || YouWonText.activeSelf;
    }

    public void EnableUnnecessaryPlayerUIObjects()
    {
        ThrustBar.gameObject.SetActive(true);
        HealthBar.gameObject.SetActive(true);
        StaminaBar.gameObject.SetActive(true);
        ScoreText.gameObject.SetActive(true);
    }

    public void DisableUnnecessaryPlayerUIObjects()
    {
        ThrustBar.gameObject.SetActive(false);
        HealthBar.gameObject.SetActive(false);
        StaminaBar.gameObject.SetActive(false);
        ScoreText.gameObject.SetActive(false);
    }

    public void IncreaseScore()
    {
        //TODO: implement check and game end

        Score++;
        ScoreText.text = "Score: " + Score;
    }

    //----------------------------------- PRIVATE METHODS -----------------------------------
    private void DeactivateUnnecessaryGameObjects()
    {
        GameOverText.SetActive(false);
        YouWonText.SetActive(false);
    }
}