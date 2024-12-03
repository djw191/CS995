﻿using UnityEngine;

namespace Board
{
    public class Enemy : CellObject
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Damaged = Animator.StringToHash("Damaged");
        [SerializeField] private int damage;
        [SerializeField] private int maxHitPoints;
        private Animator _animator;

        private int _hitPoints;
        private PlayerController _player;
        private SpriteRenderer _spriteRenderer; // ReSharper disable Unity.PerformanceAnalysis
        private BoardManager _boardManager;

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
        }

        public override void Init(Vector2Int cell)
        {
            base.Init(cell);
            _hitPoints = maxHitPoints;
            _player = GameManager.Instance.Player;
            CurrentMovementPoints = MovementPoints;
            GameManager.Instance.TurnManager.OnTick += Turn;
        }

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

            // var xDiff = _player.Position.x - Position.x;
            // var yDiff = _player.Position.y - Position.y;
            //
            // _spriteRenderer.flipX = xDiff > 0;
            
            Vector2Int nextCell = _boardManager.GetNextCell(Position, _player.Position);
            
            var diff = new Vector2Int(nextCell.x - Position.x, nextCell.y - Position.y);
            
            _spriteRenderer.flipX = diff.x < 0;

            if (_player.Position == nextCell)
            {
                GameManager.Instance.AttackPlayer(damage);
                GameManager.Instance.BoardManager.RequestMove(new BoardManager.ActionData(Position,
                    Mathf.Abs(diff.x) > Mathf.Abs(diff.y)
                        ? new Vector2Int(Position.x + (diff.x > 0 ? 1 : -1), Position.y)
                        : new Vector2Int(Position.x, Position.y + (diff.y > 0 ? 1 : -1)),
                    this, _animator, false, BoardManager.ActionData.ActionType.Attack));
            }
            else
            {
                GameManager.Instance.BoardManager.RequestMove(new BoardManager.ActionData(Position,
                    Mathf.Abs(diff.x) > Mathf.Abs(diff.y)
                        ? new Vector2Int(Position.x + (diff.x > 0 ? 1 : -1), Position.y)
                        : new Vector2Int(Position.x, Position.y + (diff.y > 0 ? 1 : -1)),
                    this, _animator, false, BoardManager.ActionData.ActionType.Move));
            }
            
            // if ((Mathf.Abs(xDiff) <= 1 && Mathf.Abs(yDiff) == 0) || (Mathf.Abs(yDiff) <= 1 && Mathf.Abs(xDiff) == 0)) //If in attack range
            // {
            //     GameManager.Instance.AttackPlayer(damage);
            //     GameManager.Instance.BoardManager.RequestMove(new BoardManager.ActionData(Position,
            //         Mathf.Abs(xDiff) > Mathf.Abs(yDiff)
            //             ? new Vector2Int(Position.x + (xDiff > 0 ? 1 : -1), Position.y)
            //             : new Vector2Int(Position.x, Position.y + (yDiff > 0 ? 1 : -1)),
            //         this, _animator, false, BoardManager.ActionData.ActionType.Attack));
            // }
            // else
            // {
            //     GameManager.Instance.BoardManager.RequestMove(new BoardManager.ActionData(Position,
            //         Mathf.Abs(xDiff) > Mathf.Abs(yDiff)
            //             ? new Vector2Int(Position.x + (xDiff > 0 ? 1 : -1), Position.y)
            //             : new Vector2Int(Position.x, Position.y + (yDiff > 0 ? 1 : -1)),
            //         this, _animator, false, BoardManager.ActionData.ActionType.Move));
            // }
        }

        public override bool AttemptEnter(int attackPower)
        {
            if (_hitPoints <= 0)
            {
                Destroy(gameObject);
                return true;
            }

            _hitPoints -= attackPower;
            _animator.SetTrigger(Damaged);
            return false;
        }

        private void OnDestroy()
        {
            GameManager.Instance.TurnManager.OnTick -= Turn;
        }
    }
}