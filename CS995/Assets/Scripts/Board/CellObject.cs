using System.ComponentModel;
using UnityEngine;

namespace Board
{
    public class CellObject : MonoBehaviour, IMoveableObject
    {
        public Vector2Int Position { get; set; }
        public GameObject GameObject => gameObject;

        [field: SerializeField]
        [DefaultValue(1)]
        public int MovementPoints { get; protected set; }

        public int CurrentMovementPoints { get; protected set; }

        public virtual void Move()
        {
        }

        public virtual void Init(Vector2Int cell)
        {
            Position = cell;
        }

        public virtual void Entered(bool isPlayer, IMoveableObject moveableObject)
        {
        }

        public virtual bool AttemptEnter(int attackPower)
        {
            return true;
        }
    }
}