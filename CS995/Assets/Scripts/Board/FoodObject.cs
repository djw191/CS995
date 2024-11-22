using UnityEngine;

namespace Board
{
    public class FoodObject : CellObject
    {
        [SerializeField] private int foodAmount;

        public override void Entered(bool isPlayer, IMoveableObject moveableObject)
        {
            if (isPlayer)
                GameManager.Instance.ConsumeFoodPlayer(foodAmount);
            Destroy(gameObject);
        }
    }
}