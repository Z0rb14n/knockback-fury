using System;
using Player;
using UnityEngine;

namespace NewFloorGen
{
    public class ComputerRoom : MonoBehaviour
    {
        public ComputerRoom[] neighbors;
        public Vector2 spawnPoint;


        public void SpawnPlayer()
        {
            PlayerMovementScript.Instance.Pos = transform.TransformPoint(spawnPoint);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(spawnPoint), 1);
        }
    }
}