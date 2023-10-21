using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Enemies
{
    /// <summary>
    /// A target dummy.
    /// </summary>
    [DisallowMultipleComponent]
    public class TargetDummy : EntityHealth
    {
        [SerializeField] private TextMeshPro display;
        [SerializeField, Min(0)] private float maxTime = 1.5f;
        [SerializeField, Min(0)] private float timeBeforeHidden = 3.5f;

        private readonly Queue<(float, float)> _prevDamage = new();
        private float _lastDamage = 0;

        private void FixedUpdate()
        {
            if (Time.time - _lastDamage > timeBeforeHidden) display.text = "";
        }

        protected override void DoTakeDamage(int dmg)
        {
            base.DoTakeDamage(dmg);
            health += dmg;
            _prevDamage.Enqueue((dmg, Time.time));
            while (_prevDamage.Peek().Item2 + maxTime < Time.time)
            {
                _prevDamage.Dequeue();
            }

            float totalDamage = _prevDamage.Select(pair => pair.Item1).Sum();
            float totalTime = Time.time - _prevDamage.Peek().Item2;
            if (totalTime < 1) totalTime = 1;
            display.text = totalDamage + "\n\u2248" + (totalDamage / totalTime).ToString("N2") + " DPS";
            _lastDamage = Time.time;
        }
    }
}