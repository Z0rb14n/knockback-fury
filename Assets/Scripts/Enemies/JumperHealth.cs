using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(JumperMovement))]
    public class JumperHealth : EntityHealth
    {

        private JumperMovement _jumperMovement;

        protected override void Awake()
        {
            base.Awake();
            _jumperMovement = GetComponent<JumperMovement>();
        }

        public override void TakeDamage(int dmg)
        {
            base.TakeDamage(dmg);
            _jumperMovement.ForceEnableGravity();
        }
    }
}