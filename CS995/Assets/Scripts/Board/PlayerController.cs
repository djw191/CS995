using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Board
{
    public class PlayerController : MonoBehaviour, IMoveableObject
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        [SerializeField] private float moveSpeed;

        [field: SerializeField]
        [DefaultValue(1)]
        public int DefensePower { get; set; }

        private Animator _animator;
        private bool _canMove;
        private InputAction _move;
        private SpriteRenderer _spriteRenderer;
        private GameManager _gameManager;
        public int ActionsTakenThisLevel { get; private set; }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _move = InputSystem.actions.FindAction("Move");
            _animator = GetComponent<Animator>();
            _gameManager = GameManager.Instance;
            CurrentMovementPoints = MovementPoints;
            _canMove = true;

            InputSystem.actions.FindAction("Pause").performed += HandlePauseInput;
            GameManager.Instance.BoardManager.OnMovementEnded += SetCanMoveTrue;
            ActionsTakenThisLevel = 0;
        }

        private void Update()
        {
            if (!_move.WasPressedThisFrame() || GameManager.Instance.Paused || !_canMove || _gameManager.IsGameOver) return;

            CurrentMovementPoints--;
            _canMove = false;

            //Get Direction and flip sprite
            var direction = GetDirectionAndSetFlip();

            if (GameManager.Instance.BoardManager.RequestMove(new BoardManager.ActionData(Position, Position + direction, this,
                _animator, true, BoardManager.ActionData.ActionType.Move)))
                ActionsTakenThisLevel++;
        }

        private void OnDestroy()
        {
            InputSystem.actions.FindAction("Pause").performed -= HandlePauseInput;
            GameManager.Instance.BoardManager.OnMovementEnded -= SetCanMoveTrue;
        }

        [field: SerializeField]
        [DefaultValue(1)]
        public int MovementPoints { get; set; }

        public int CurrentMovementPoints { get; private set; }

        public GameObject GameObject => gameObject;
        [field: SerializeField] public int AttackPower { get; set; }
        public Vector2Int Position { get; set; }

        public void Move()
        {
            GameManager.Instance.UpdateMovementFoodPlayer();
            if (CurrentMovementPoints <= 0)
            {
                CurrentMovementPoints = MovementPoints;
                GameManager.Instance.TurnManager.Tick();
            }
            else
            {
                _canMove = true;
            }
        }

        private Vector2Int GetDirectionAndSetFlip()
        {
            var direction = Vector2Int.RoundToInt(_move.ReadValue<Vector2>());

            //Set X Flip L/R
            if (direction.Equals(new Vector2Int(-1, 0)))
                _spriteRenderer.flipX = true;
            else if (direction.Equals(new Vector2Int(1, 0)))
                _spriteRenderer.flipX = false;
            return direction;
        }

        private void HandlePauseInput(InputAction.CallbackContext _)
        {
            GameManager.Instance.UIManager.TogglePauseMenu();
        }

        private void SetCanMoveTrue()
        {
            _canMove = true;
        }

        public void AlertObservers(string message)
        {
            switch (message)
            {
                case "AttackAnimationEnded":
                    _animator.SetBool(Attack, false);
                    GameManager.Instance.BoardManager.FinishAttacking();
                    Move();
                    break;
            }
        }

        public void Spawn(Vector2Int cellPosition, BoardManager board)
        {
            Position = cellPosition;
            transform.position = GameManager.Instance.BoardManager.CellToWorld(cellPosition);
        }
    }
}