using UnityEngine;

namespace Board
{
    public interface IMoveableObject
    {
        public Vector2Int Position { get; set; }
        public GameObject GameObject { get; }
        public int MovementPoints { get; }
        public int CurrentMovementPoints { get; }
        public void Move();
    }
}