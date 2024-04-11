using System;
using Player;
using UnityEngine;
using Util;

namespace NewFloorGen
{
    public class ComputerRoom : MonoBehaviour
    {
        public BreakableFileData[] fileDatas;
        public ComputerRoom[] neighbors;
        public Vector2 spawnPoint;

        private void Awake()
        {
            BreakableFile[] files = GetComponentsInChildren<BreakableFile>();
            foreach (BreakableFile file in files)
            {
                if (file.randomize) file.SetData(fileDatas.GetRandom());
            }
        }


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