using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public sealed class GameObjectManager : MonoBehaviour
{
    private static readonly object padlock = new object();
    private static GameObjectManager instance = null;
    [SerializeField] private GameObject headstonePrefab;
    private CinemachineFreeLook cinemachineVirtualCamera;
    private List<GameObject> enemies;
    private GameObject gameOverText;
    private Slider healthBar;
    private List<GameObject> planets;
    private List<GameObject> players;

    private Slider staminaBar;
    private GameObject sun;
    private Slider thrustBar;
    private GameObject youWonText;

    public static GameObjectManager Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new GameObjectManager();
                }

                return instance;
            }
        }
    }

    public Slider StaminaBar
    {
        get => staminaBar;
        set => staminaBar = value;
    }

    public Slider HealthBar
    {
        get => healthBar;
        set => healthBar = value;
    }

    public GameObject GameOverText
    {
        get => gameOverText;
        set => gameOverText = value;
    }

    public GameObject YouWonText
    {
        get => youWonText;
        set => youWonText = value;
    }

    public CinemachineFreeLook CinemachineVirtualCamera
    {
        get => cinemachineVirtualCamera;
        set => cinemachineVirtualCamera = value;
    }

    public Slider ThrustBar
    {
        get => thrustBar;
        set => thrustBar = value;
    }

    public List<GameObject> Planets
    {
        get => planets;
        set => planets = value;
    }

    public List<GameObject> Players
    {
        get => players;
        set => players = value;
    }

    public List<GameObject> Enemies
    {
        get => enemies;
        set => enemies = value;
    }

    public GameObject Sun
    {
        get => sun;
        set => sun = value;
    }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        StaminaBar = GameObject.FindGameObjectWithTag("StaminaBar").GetComponent<Slider>();
        ThrustBar = GameObject.FindGameObjectWithTag("ThrustBar").GetComponent<Slider>();
        HealthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Slider>();
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

        Sun = GameObject.FindGameObjectWithTag("Sun");

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
        GameObject deadPlayer = GetPlayerById(playerNetworkId);
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

    public ulong GetOwnedPlayerId()
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

    //----------------------------------- ENEMY RELATED METHODS -----------------------------------
    public IEnumerator RefreshEnemiesCoroutine()
    {
        while (Enemies.Count.Equals(0))
        {
            yield return new WaitForSeconds(1f);
            Enemies.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        }
    }

    public List<GameObject> FindEnemiesOnPlanet(ulong planetId)
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
        Planets.AddRange(GameObject.FindGameObjectsWithTag("Planet"));
    }

    public GameObject GetPlanetById(ulong planetNetworkId)
    {
        return Planets.Find(planet => planet.GetComponent<NetworkObject>().NetworkObjectId == planetNetworkId);
    }

    public List<Vector3> GetPlanetPositions()
    {
        List<Vector3> planetPositions = new List<Vector3>();
        Planets.ForEach(planet => planetPositions.Add(planet.transform.position));
        return planetPositions;
    }

    //----------------------------------- OTHER METHODS -----------------------------------
    public bool IsGameOver()
    {
        return GameOverText.activeSelf;
    }

    //----------------------------------- PRIVATE METHODS -----------------------------------
    private void DeactivateUnnecessaryGameObjects()
    {
        GameOverText.SetActive(false);
        YouWonText.SetActive(false);
    }
}