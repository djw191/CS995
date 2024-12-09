using UnityEngine;

namespace Board
{
    public class FoodObject : CellObject
    {
        [field: SerializeField] public int FoodAmount {get; private set;}

        public override void Entered(bool isPlayer, IMoveableObject moveableObject)
        {
            if (isPlayer)
                GameManager.Instance.ConsumeFoodPlayer(FoodAmount);
            Destroy(gameObject);
        }
    }
}