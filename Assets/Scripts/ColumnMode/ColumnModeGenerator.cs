using System;
using System.Collections.Generic;
using System.Linq;
using GameEnd;
using Player;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace ColumnMode
{
    [DisallowMultipleComponent]
    public class ColumnModeGenerator : MonoBehaviour
    {
        [SerializeField] private ColumnPrefab[] columnPrefabs;
        [SerializeField, Min(0)] private float diffBeforeDeletion = 20;
        [SerializeField, Min(0)] private float diffBetween = 10;
        [SerializeField, Min(0)] private float generationStart = 15;
        [SerializeField] private float maxGeneration = 750;
        
        private readonly Queue<ColumnSection> _sections = new();
        private GameObject[] _prefabs;
        private float[] _weights;
        private float _maxHeight = -float.MinValue;
        private PlayerMovementScript _player;
        private GameEndCanvas _gameEnd;

        private void Awake()
        {
            _prefabs = columnPrefabs.Select(prefab => prefab.go).ToArray();
            _weights = columnPrefabs.Select(prefab => prefab.weight).ToArray();
            _player = PlayerMovementScript.Instance;
            _gameEnd = GameEndCanvas.Instance;
            _maxHeight = 0;
            AddSection(0);
        }

        private void FixedUpdate()
        {
            while (_player.Pos.y + generationStart > _maxHeight)
            {
                AddSection(_maxHeight + diffBetween);
            }

            while (_sections.TryPeek(out ColumnSection s) && _player.Pos.y > s.transform.position.y + s.height + diffBeforeDeletion)
            {
                ColumnSection section = _sections.Dequeue();
                Destroy(section.gameObject);
            }

            _gameEnd.endData.maxHeight = Mathf.Max(_gameEnd.endData.maxHeight, _player.Pos.y);
        }

        private void AddSection(float yPos)
        {
            GameObject go = Instantiate(_prefabs.GetRandomWeighted(_weights, out int index),
                new Vector3(0, yPos), Quaternion.identity, transform);
            if (columnPrefabs[index].canFlip && Random.Range(0, 2) == 0)
                go.transform.localScale = new Vector3(-1, 1, 1);
            ColumnSection section = go.GetComponent<ColumnSection>();
            section.Initialize();
            _sections.Enqueue(section);
            _maxHeight = section.transform.position.y + section.height + diffBetween;
            if (_maxHeight >= maxGeneration) _maxHeight = float.MaxValue;
        }

        private void OnValidate()
        {
            _prefabs = columnPrefabs.Select(prefab => prefab.go).ToArray();
            _weights = columnPrefabs.Select(prefab => prefab.weight).ToArray();
        }

        [Serializable]
        public struct ColumnPrefab
        {
            public GameObject go;
            [Min(0)] public float weight;
            public bool canFlip;
        }
    }
}