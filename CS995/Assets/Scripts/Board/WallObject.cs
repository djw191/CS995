using UnityEngine;
using UnityEngine.Tilemaps;

namespace Board
{
    public class WallObject : CellObject
    {
        [SerializeField] private Tile obstacleTile;
        [SerializeField] private Tile damagedObstacleTile;

        [SerializeField] private int maxHitPoints;
        public int HitPoints { get; private set; }
        private Tile _originalTile;

        public override void Init(Vector2Int position)
        {
            base.Init(position);
            HitPoints = maxHitPoints;
            _originalTile = GameManager.Instance.BoardManager.GetCellTile(position);
            GameManager.Instance.BoardManager.SetCellTile(position, obstacleTile);
        }

        public override bool AttemptEnter(IMoveableObject moveableObject)
        {
            if (HitPoints <= 0) return true;


            HitPoints -= moveableObject.AttackPower;
            if (HitPoints <= 1)
                GameManager.Instance.BoardManager.SetCellTile(Position, damagedObstacleTile);

            if (HitPoints > 0) return false;

            GameManager.Instance.BoardManager.SetCellTile(Position, _originalTile);
            Destroy(gameObject);
            return false;
        }
    }
}