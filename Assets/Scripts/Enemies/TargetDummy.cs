using System.Collections;
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

        private float _runningSum;
        protected override void DoTakeDamage(int dmg)
        {
            base.DoTakeDamage(dmg);
            health += dmg;
            _runningSum += dmg;
            StartCoroutine(OnDamageCoroutine(dmg));
            display.text = _runningSum + "\n\u2248" + (_runningSum / maxTime).ToString("N2") + " DPS";
        }

        private IEnumerator OnDamageCoroutine(int dmg)
        {
            yield return new WaitForSeconds(maxTime);
            _runningSum -= dmg;
        }
    }
}