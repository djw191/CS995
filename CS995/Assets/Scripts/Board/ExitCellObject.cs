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
            if(moveableObject is PlayerController)
                GameManager.Instance.UIManager.DisplayNotification( $"You reached the exit of the level!!", Color.green);
            GameManager.Instance.NewLevel();
        }
    }
}