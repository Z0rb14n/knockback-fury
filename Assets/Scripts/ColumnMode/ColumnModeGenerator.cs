using System;
using System.Collections.Generic;
using GameEnd;
using Player;
using UnityEngine;
using Util;

namespace ColumnMode
{
    [DisallowMultipleComponent]
    public class ColumnModeGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject[] columnPrefabs;
        [SerializeField, Min(0)] private float diffBeforeDeletion = 20;
        [SerializeField, Min(0)] private float diffBetween = 10;
        [SerializeField, Min(0)] private float generationStart = 15;
        
        
        private Queue<ColumnSection> _sections = new();
        private float _minHeight = -float.MinValue;
        private float _maxHeight = -float.MinValue;
        private PlayerMovementScript _player;
        private GameEndCanvas _gameEnd;

        private void Awake()
        {
            GameObject go = Instantiate(columnPrefabs.GetRandom(), Vector3.zero, Quaternion.identity, transform);
            _sections.Enqueue(go.GetComponent<ColumnSection>());
            _minHeight = 0;
            _maxHeight = 0;
            _player = PlayerMovementScript.Instance;
            _gameEnd = GameEndCanvas.Instance;
        }

        private void FixedUpdate()
        {
            while (_player.Pos.y + generationStart > _maxHeight)
            {
                GameObject go = Instantiate(columnPrefabs.GetRandom(), new Vector3(0, _maxHeight+diffBetween), Quaternion.identity, transform);
                _sections.Enqueue(go.GetComponent<ColumnSection>());
                _maxHeight += diffBetween;
            }

            while (_player.Pos.y > _minHeight + diffBeforeDeletion)
            {
                Destroy(_sections.Dequeue().gameObject);
                _minHeight += diffBetween;
            }

            _gameEnd.endData.maxHeight = Mathf.Max(_gameEnd.endData.maxHeight, _player.Pos.y);
        }
    }
}