using System;
using UnityEngine;

namespace Board
{
    public interface IMoveableObject
    {
        public int AttackPower { get; set; }
        public Vector2Int Position { get; set; }
        public GameObject GameObject { get; }
        public int ActionsTakenThisLevel { get; }
        public void Move();
    }
}