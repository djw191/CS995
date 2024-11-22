using UnityEngine;
using UnityEngine.Tilemaps;

namespace Board
{
    public class WallObject : CellObject
    {
        [SerializeField] private Tile obstacleTile;
        [SerializeField] private Tile damagedObstacleTile;

        [SerializeField] private int maxHitPoints;
        private int _hitPoints;
        private Tile _originalTile;

        public override void Init(Vector2Int position)
        {
            base.Init(position);
            _hitPoints = maxHitPoints;
            _originalTile = GameManager.Instance.BoardManager.GetCellTile(position);
            GameManager.Instance.BoardManager.SetCellTile(position, obstacleTile);
        }

        public override bool AttemptEnter(int attackPower)
        {
            if (_hitPoints <= 0) return true;


            _hitPoints -= attackPower;
            if (_hitPoints <= 1)
                GameManager.Instance.BoardManager.SetCellTile(Position, damagedObstacleTile);

            if (_hitPoints > 0) return false;

            GameManager.Instance.BoardManager.SetCellTile(Position, _originalTile);
            Destroy(gameObject);
            return false;
        }
    }
}