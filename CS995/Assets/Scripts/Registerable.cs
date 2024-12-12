
using System;
using UnityEngine;

public class Registerable : MonoBehaviour
{
        public string guid;

        private void Awake()
        {
                Attrs.Register(gameObject);
        }
}