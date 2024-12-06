using System.ComponentModel;
using UnityEngine;

namespace Board
{
    public class PowerupObject : CellObject
    {
        [DefaultValue(0)] [SerializeField] private int strength;
        [DefaultValue(0)] [SerializeField] private int defense;
        [DefaultValue(0)] [SerializeField] private int movement;

        public override void Entered(bool isPlayer, IMoveableObject moveableObject)
        {
            moveableObject.AttackPower += strength;
            if (isPlayer && moveableObject is PlayerController pc)
            {
                pc.MovementPoints += movement;
                pc.DefensePower += defense;
            }

            Destroy(gameObject);
        }
    }
}