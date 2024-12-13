using System;
using System.ComponentModel;
using System.Reflection;
using Board;
using UnityEngine;
using Object = System.Object;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManagerPrefab;
    [SerializeField] private PlayerController playerControllerPrefab;

    [SerializeField] private UIManager uiManagerPrefab;

    [field: SerializeField]
    [DefaultValue(100)]
    public int FoodAmount { get; private set; }

    public float FoodMultiplier = 1.0f;
    public float BonusMultiplier = 1.0f;
    public float WallMultiplier = 1.0f;
    public int WaypointTarget = 0;

    private Camera _camera;
    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager { get; private set; }
    public PlayerController Player { get; private set; }
    public TurnManager TurnManager { get; private set; }
    public UIManager UIManager { get; private set; }
    [DefaultValue(0)] public int CurrentLevel { get; private set; }
    public float Score { get; private set; }

    public bool Paused { get; private set; }
    public bool IsGameOver { get; private set; }

    private int goalDistance = int.MaxValue;

    //TODO have other multiplier affect gameplay
    // Setup Singleton in Awake
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _camera = Camera.main;

        Paused = false;
        UIManager = Instantiate(uiManagerPrefab);
        BoardManager = Instantiate(boardManagerPrefab);
        Player = Instantiate(playerControllerPrefab);
        TurnManager = new TurnManager();
    }

    public event Action OnLevelComplete;

    // Increases the size of the Camera area until the entire board fits in the viewport
    private void SetCameraSize()
    {
        if (_camera == null) return;

        var boardSize = BoardManager.boardSize;
        _camera.transform.position = new Vector3(boardSize.x / 2.0f, boardSize.y / 2.0f, _camera.transform.position.z);

        _camera.orthographicSize = 0;
        var tilemapBounds = BoardManager.Tilemap.cellBounds;
        while (true)
        {
            var viewportMin = _camera.WorldToViewportPoint(tilemapBounds.min);
            var viewportMax = _camera.WorldToViewportPoint(tilemapBounds.max);

            if (viewportMin is { x: >= 0, y: >= 0 } && viewportMax is { x: <= 1, y: <= 1 })
                break;

            _camera.orthographicSize++;
        }

        _camera.orthographicSize++;
    }

    public void TogglePause()
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        Paused = !Paused;
    }

    // Score is ideal path / actions taken per level, approaching 0 the more moves you take
    private void TallyScore()
    {
        Score += goalDistance / (float)Player.ActionsTakenThisLevel;
    }

    public void NewLevel()
    {
        var isFirstLevel = CurrentLevel == 0;
        BoardManager.Clean();

        if (!isFirstLevel)
        {
            TallyScore();
            BoardManager.boardSize = new Vector2Int(BoardManager.boardSize.x + 1, BoardManager.boardSize.y + 1);
            BoardManager.TargetWalls += (1 - BoardManager.TargetWalls) / 25;
            BoardManager.TargetFood -= BoardManager.TargetFood / 25;
            BoardManager.targetEnemies += 0.5f;
        }

        CurrentLevel++;

        BoardManager.Init();
        SetCameraSize();
        Player.Spawn(new Vector2Int(1, 1), BoardManager);
        goalDistance = BoardManager.GetGoalDistance(Player.AttackPower);
        OnLevelComplete?.Invoke();
    }

    public void AttackPlayer(int amount)
    {
        ChangeFood(-amount - Player.DefensePower);
    }

    public void ConsumeFoodPlayer(int amount)
    {
        ChangeFood(amount);
        //UIManager.DisplayMessageBox($"You consumed {amount} food.");
    }

    public void UpdateMovementFoodPlayer()
    {
        ChangeFood(-1);
    }

    private void ChangeFood(int amount)
    {
        FoodAmount += amount;
        UIManager.UpdateFoodLabel(FoodAmount);

        if (FoodAmount > 0) return;

        TallyScore();
        IsGameOver = true;
        UIManager.ToggleGameOverPanel();
    }
}