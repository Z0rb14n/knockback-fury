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

        // ensures jumper falls to the ground instead of clinging onto a wall
        public override void TakeDamage(int dmg)
        {
            base.TakeDamage(dmg);
            _jumperMovement.Stun();
        }
    }
}