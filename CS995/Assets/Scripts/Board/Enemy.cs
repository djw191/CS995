using System;
using UnityEngine;

namespace Board
{
    [NotPathable]
    public class Enemy : CellObject, IMoveableObject
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Damaged = Animator.StringToHash("Damaged");
        [SerializeField] private int maxHitPoints;
        private Animator _animator;

        private int _hitPoints;
        private PlayerController _player;
        private SpriteRenderer _spriteRenderer; // ReSharper disable Unity.PerformanceAnalysis
        private BoardManager _boardManager;
        public int ActionsTakenThisLevel { get; private set; }

        public void AlertObservers(string message)
        {
            if (message.Equals("AttackAnimationEnded"))
            {
                _animator.SetBool(Attack, false);
                GameManager.Instance.BoardManager.FinishAttacking();
                Move();
            }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boardManager = GameManager.Instance.BoardManager;
            ActionsTakenThisLevel = 0;
        }

        public override void Init(Vector2Int cell)
        {
            base.Init(cell);
            _hitPoints = maxHitPoints;
            _player = GameManager.Instance.Player;
            CurrentMovementPoints = MovementPoints;
            GameManager.Instance.TurnManager.OnTick += Turn;
        }

        [field: SerializeField] public int AttackPower { get; set; }

        public override void Move()
        {
            Turn();
        }

        private void Turn()
        {
            if (CurrentMovementPoints <= 0)
            {
                CurrentMovementPoints = MovementPoints;
                return;
            }

            CurrentMovementPoints--;

            var nextCell = _boardManager.GetNextCell(Position, _player.Position, AttackPower);
            //TODO with multiple enemies, they can both pathfind into the same square, must do this sequentially

            var diff = new Vector2Int(nextCell.x - Position.x, nextCell.y - Position.y);

            _spriteRenderer.flipX = diff.x > 0;

            var moveApproved = false;
            if (_player.Position == nextCell)
            {
                GameManager.Instance.AttackPlayer(AttackPower);
                moveApproved = GameManager.Instance.BoardManager.RequestMove(new BoardManager.ActionData(Position,
                    Mathf.Abs(diff.x) > Mathf.Abs(diff.y)
                        ? new Vector2Int(Position.x + (diff.x > 0 ? 1 : -1), Position.y)
                        : new Vector2Int(Position.x, Position.y + (diff.y > 0 ? 1 : -1)),
                    this, _animator, false, BoardManager.ActionData.ActionType.Attack));
            }
            else
            {
                moveApproved = GameManager.Instance.BoardManager.RequestMove(new BoardManager.ActionData(Position,
                    Mathf.Abs(diff.x) > Mathf.Abs(diff.y)
                        ? new Vector2Int(Position.x + (diff.x > 0 ? 1 : -1), Position.y)
                        : new Vector2Int(Position.x, Position.y + (diff.y > 0 ? 1 : -1)),
                    this, _animator, false, BoardManager.ActionData.ActionType.Move));
            }

            if (moveApproved) ActionsTakenThisLevel++;
        }

        public override bool AttemptEnter(IMoveableObject moveableObject)
        {
            if (_hitPoints <= 0)
            {
                Destroy(gameObject);
                return true;
            }

            _hitPoints -= moveableObject.AttackPower;
            _animator.SetTrigger(Damaged);
            return false;
        }

        private void OnDestroy()
        {
            GameManager.Instance.TurnManager.OnTick -= Turn;
        }
    }
}