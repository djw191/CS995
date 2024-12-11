using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Board
{
    public class BoardManager : MonoBehaviour
    {
        private static readonly int Moving = Animator.StringToHash("Moving");
        private static readonly int Attack = Animator.StringToHash("Attack");

        [SerializeField] private Tile[] groundTiles;
        [SerializeField] private Tile[] wallTiles;
        [SerializeField] private ExitCellObject exitCellPrefab;
        [SerializeField] private Tile outline;
        [field: SerializeField] public Tile DebugTile { get; private set; }

        public Vector2Int boardSize = new(10, 10);
        [SerializeField] private Grid grid;

        [SerializeField] private FoodObject[] foodPrefabs;
        [Range(0, 1)] public float initTargetFood;
        [NonSerialized] public float TargetFood;
        [Range(0, 1)] [SerializeField] private float foodVariance;

        [SerializeField] private WallObject[] wallPrefabs;
        [Range(0, 1)] public float initTargetWalls;
        [NonSerialized] public float TargetWalls;
        [Range(0, 1)] [SerializeField] private float wallsVariance;


        [SerializeField] private PowerupObject[] powerupPrefabs;
        public int TargetPowerups { get; set; }

        [SerializeField] private Enemy[] enemyPrefabs;
        [SerializeField] public int targetEnemies;

        [SerializeField] private float movementSpeed;
        private readonly List<Vector2Int> _emptyTiles = new();
        private readonly List<ActionData> _objectsToAct = new();
        private bool _allowedToMove = true;
        private CellData[,] _boardData;
        private float _currentMovementSpeed;

        [field: SerializeField] public Tilemap Tilemap { get; private set; }
        [field: SerializeField] public Tilemap OutlineTilemap { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            TargetFood = initTargetFood;
            TargetWalls = initTargetWalls;
            TargetPowerups = 0;
            _currentMovementSpeed = movementSpeed;
            GameManager.Instance.OnLevelComplete += ClearMovement;
            if (PlayerPrefs.GetInt("IM", 0) == 1) _currentMovementSpeed = float.MaxValue;
        }

        private void Update()
        {
            if (!_allowedToMove) return;
            _objectsToAct.RemoveAll(match => match.MoveableObject == null);
            if (_objectsToAct.Count <= 0)
            {
                OnMovementEnded?.Invoke();
                return;
            }

            var obj = _objectsToAct[0];
            if (GetCell(obj.To).ContainedObject is INotPathable && obj.MoveableObject is INotPathable)
            {
                _objectsToAct.RemoveAt(0);
            }
            else if (obj.ActionType_ == ActionData.ActionType.Move && obj.Animator != null)
            {
                obj.Animator.SetBool(Moving, true);
                obj.MoveableObject.Position = obj.To;

                obj.MoveableObject.GameObject.transform.position = Vector3.MoveTowards(
                    obj.MoveableObject.GameObject.transform.position, CellToWorld(obj.To),
                    _currentMovementSpeed * Time.deltaTime);

                if (obj.MoveableObject.GameObject.transform.position != CellToWorld(obj.To)) return;

                _objectsToAct.Remove(obj);
                obj.MoveableObject.Move();
                obj.Animator.SetBool(Moving, false);

                var data = GetCell(obj.MoveableObject.Position);
                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
                if (data.ContainedObject != null)
                    data.ContainedObject.Entered(obj.IsPlayer, obj.MoveableObject);

                if (obj.IsPlayer) return;

                var toCell = _boardData[obj.To.x, obj.To.y];
                var fromCell = _boardData[obj.From.x, obj.From.y];

                toCell.ContainedObject = fromCell.ContainedObject;
                fromCell.ContainedObject = null;
            }
            else if (obj.ActionType_ == ActionData.ActionType.Attack && obj.Animator != null)
            {
                obj.Animator.SetBool(Attack, true);
                _allowedToMove = false;
                _objectsToAct.Remove(obj);
            }
        }

        public event Action OnMovementEnded;

        public void Init()
        {
            _emptyTiles.Clear();
            _boardData = new CellData[boardSize.x, boardSize.y];
            for (var i = 0; i < boardSize.x; i++)
            for (var j = 0; j < boardSize.y; j++)
            {
                if (i == 0 || j == 0 || i == boardSize.x - 1 || j == boardSize.y - 1)
                {
                    Tilemap.SetTile(new Vector3Int(i, j, 0), wallTiles[Random.Range(0, wallTiles.Length)]);
                    _boardData[i, j] = new CellData(false);
                }
                else
                {
                    Tilemap.SetTile(new Vector3Int(i, j, 0), groundTiles[Random.Range(0, groundTiles.Length)]);
                    _boardData[i, j] = new CellData(true);
                    _emptyTiles.Add(new Vector2Int(i, j));
                }

                OutlineTilemap.SetTile(new Vector3Int(i, j, 0), outline);
            }

            _emptyTiles.Remove(new Vector2Int(1, 1));

            var endPosition = new Vector2Int(boardSize.x - 2, boardSize.y - 2);
            AddObject(Instantiate(exitCellPrefab), endPosition);
            _emptyTiles.Remove(endPosition);

            GenerateFood();
            GenerateWalls();
            GenerateEnemies();
            GeneratePowerups();
        }

        private void ClearMovement()
        {
            _objectsToAct.Clear();
            _allowedToMove = true;
        }

        public void ToggleMovementSpeed()
        {
            var isDefaultSpeed = _currentMovementSpeed <= movementSpeed;
            _currentMovementSpeed = isDefaultSpeed ? float.MaxValue : movementSpeed;
            PlayerPrefs.SetInt("IM", isDefaultSpeed ? 1 : 0);
        }

        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeNullComparison")]
        public bool RequestMove(ActionData actionData)
        {
            switch (actionData.ActionType_)
            {
                case ActionData.ActionType.Move:
                {
                    var cell = GetCell(actionData.To);

                    // Check for null, passable, and if it is already in the acting objects
                    if (cell is null || !cell.Passable || _objectsToAct.Any(item =>
                            item.MoveableObject.GameObject == actionData.MoveableObject.GameObject)) return false;

                    if (GetCell(actionData.To).ContainedObject != null &&
                        !GetCell(actionData.To).ContainedObject.AttemptEnter(actionData.MoveableObject))
                        actionData.ActionType_ = ActionData.ActionType.Attack;
                    break;
                }
                case ActionData.ActionType.Attack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _objectsToAct.Add(actionData);
            return true;
        }

        //A Star that returns just next target cell
        public Vector2Int GetNextCell(Vector2Int start, Vector2Int goal, int attack)
        {
            var astarPathFinder = new AStarPathfinder(_boardData, attack);
            return astarPathFinder.GetNextCell(start, goal);
        }

        public int GetTraversalCost(Vector2Int start, Vector2Int goal, int attack)
        {
            var astarPathFinder = new AStarPathfinder(_boardData, attack);
            return astarPathFinder.GetTotalCost(start, goal, -foodPrefabs.Min(f => f.FoodAmount));
        }

        public int GetGoalDistance(int attack)
        {
            return GetTraversalCost(GameManager.Instance.Player.Position,
                new Vector2Int(boardSize.x - 2, boardSize.y - 2), attack);
        }

        public void FinishAttacking()
        {
            _allowedToMove = true;
        }

        public void Clean()
        {
            if (_boardData == null) return;

            for (var y = 0; y < _boardData.GetLength(1); y++)
            for (var x = 0; x < _boardData.GetLength(0); x++)
            {
                var cellData = _boardData[x, y];
                // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison
                if (cellData.ContainedObject != null) Destroy(cellData.ContainedObject.gameObject);
                SetCellTile(new Vector2Int(x, y), null);
            }
        }

        private int GetNumberOfTilesToSpawn(float target, float variance)
        {
            var realTiles = (boardSize.x - 2) * (boardSize.y - 2);
            var t = (int)(realTiles * target);
            var v = (int)(target * variance);
            return Random.Range(t - v, t + v);
        }

        private void GeneratePowerups()
        {
            for (var i = 0; i < TargetPowerups; i++)
            {
                var position = _emptyTiles[Random.Range(0, _emptyTiles.Count)];
                _emptyTiles.Remove(position);
                var powerup = Instantiate(powerupPrefabs[Random.Range(0, powerupPrefabs.Length)]);
                AddObject(powerup, position);
            }
        }

        private void GenerateEnemies()
        {
            for (var i = 0; i < targetEnemies; i++)
            {
                var position = _emptyTiles[Random.Range(0, _emptyTiles.Count)];
                _emptyTiles.Remove(position);

                var enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)]);
                AddObject(enemy, position);
            }
        }

        private void GenerateFood()
        {
            var amount = Mathf.Min(GetNumberOfTilesToSpawn(TargetFood, foodVariance), _emptyTiles.Count);
            for (var i = 0; i < amount; i++)
            {
                var position = _emptyTiles[Random.Range(0, _emptyTiles.Count)];
                _emptyTiles.Remove(position);

                var food = Instantiate(foodPrefabs[Random.Range(0, foodPrefabs.Length)]);
                AddObject(food, position);
            }
        }

        private void GenerateWalls()
        {
            var amount = Mathf.Min(GetNumberOfTilesToSpawn(TargetWalls, wallsVariance), _emptyTiles.Count);
            for (var i = 0; i < amount; i++)
            {
                var position = _emptyTiles[Random.Range(0, _emptyTiles.Count)];
                _emptyTiles.Remove(position);

                var wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]);
                AddObject(wall, position);
            }
        }

        private void AddObject(CellObject cellObject, Vector2Int position)
        {
            var cell = _boardData[position.x, position.y];
            cellObject.transform.position = CellToWorld(position);
            cell.ContainedObject = cellObject;
            cellObject.Init(position);
        }

        public Vector3 CellToWorld(Vector2Int position)
        {
            return grid.GetCellCenterWorld((Vector3Int)position);
        }

        private CellData GetCell(Vector2Int position)
        {
            if (position.x >= _boardData.GetLength(0) || position.y > _boardData.GetLength(1) || position.x < 0 ||
                position.y < 0) return null;
            return _boardData[position.x, position.y];
        }

        public Tile GetCellTile(Vector2Int position)
        {
            return Tilemap.GetTile<Tile>(new Vector3Int(position.x, position.y, 0));
        }

        public void SetCellTile(Vector2Int cell, Tile tile)
        {
            Tilemap.SetTile(new Vector3Int(cell.x, cell.y, 0), tile);
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnLevelComplete -= ClearMovement;
        }

        public class ActionData
        {
            public readonly Animator Animator;
            public readonly bool IsPlayer;
            public readonly IMoveableObject MoveableObject;
            public ActionType ActionType_;
            public Vector2Int From;
            public Vector2Int To;

            public ActionData(Vector2Int from, Vector2Int to, IMoveableObject moveableObject, Animator animator,
                bool isPlayer, ActionType actionType)
            {
                From = from;
                To = to;
                MoveableObject = moveableObject;
                Animator = animator;
                IsPlayer = isPlayer;
                ActionType_ = actionType;
            }

            public enum ActionType
            {
                Move,
                Attack
            }
        }
    }

    public class CellData
    {
        public readonly bool Passable;
        public CellObject ContainedObject;

        public CellData(bool passable)
        {
            Passable = passable;
        }
    }
}