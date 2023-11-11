namespace Enemies.Cat
{
    public class CatEntityHealth : EntityHealth
    {
        private CatBossManager _bossManager;
        protected override void Awake()
        {
            base.Awake();
            _bossManager = FindObjectOfType<CatBossManager>();
        }

        public override void TakeDamage(int dmg)
        {
            if (health <= 0) return;
            base.TakeDamage(dmg);
        }
        
        protected override void Die()
        {
            _bossManager.EndPhaseOne();
        }
    }
}