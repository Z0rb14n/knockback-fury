namespace Enemies.Cat
{
    public class CatEntityHealth : EntityHealth
    {
        public int[] triggers;

        public bool Invuln { get; set; } = false;
        private int _triggerVal;
        private CatBossManager _bossManager;
        protected override void Awake()
        {
            base.Awake();
            _triggerVal = 0;
            _bossManager = FindObjectOfType<CatBossManager>();
        }

        public override void TakeDamage(int dmg)
        {
            if (Invuln) return;
            if (health <= 0) return;
            DoTakeDamage(dmg);
            if (triggers != null && _triggerVal < triggers.Length && health < triggers[_triggerVal])
            {
                health = triggers[_triggerVal];
                _bossManager.CatHealthTriggerReach(_triggerVal);
                _triggerVal++;
            }

            if (health <= 0) Die();
        }
        
        protected override void Die()
        {
            _bossManager.OnCatDeath();
        }
    }
}