using UnityEngine;
using UnityEngine.Tilemaps;

namespace Board
{
    public class ExitCellObject : CellObject
    {
        public Tile endTile;

        public override void Init(Vector2Int position)
        {
            base.Init(position);
            GameManager.Instance.BoardManager.SetCellTile(position, endTile);
        }

        public override void Entered(bool isPlayer, IMoveableObject moveableObject)
        {
            base.Entered(false, moveableObject);
            GameManager.Instance.NewLevel();
        }
    }
}