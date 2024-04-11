using TMPro;
using UnityEngine;

namespace NewFloorGen
{
    public class BreakableFile : EntityHealth
    {
        public BreakableFileData fileData;
        public bool randomize = true;
        
        [SerializeField] private GameObject fileDebris;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMeshPro text;

        protected override void Awake()
        {
            maxHealth = fileData.health;
            base.Awake();
            UpdateHealth();
            spriteRenderer.sprite = fileData.image;
        }

        public void SetData(BreakableFileData newData)
        {
            fileData = newData;
            maxHealth = fileData.health;
            health = maxHealth;
            spriteRenderer.sprite = fileData.image;
            UpdateHealth();
        }

        public override void TakeDamage(int dmg)
        {
            if (fileData.hasInfiniteHealth) return;
            base.TakeDamage(dmg);
        }

        protected override void DoTakeDamage(int dmg)
        {
            base.DoTakeDamage(dmg);
            UpdateHealth();
        }

        protected override void Die()
        {
            base.Die();
            if (fileDebris) Instantiate(fileDebris, transform.position, Quaternion.identity).GetComponent<FileDebris>().Initialize(fileData.image);
        }

        private void UpdateHealth()
        {
            int maxLog = Mathf.FloorToInt(Mathf.Log10(maxHealth))/3;
            int healthLog = Mathf.FloorToInt(Mathf.Log10(health)) / 3;
            string maxPart = $"/{maxHealth/Mathf.Pow(10,maxLog*3):F1} {GetLabel(maxLog)}";
            if (maxLog == healthLog)
            {
                text.text = $"{fileData.title}\n{health / Mathf.Pow(10,maxLog * 3):F1}{maxPart}";
            }
            else
            {
                text.text = $"{fileData.title}\n{health / Mathf.Pow(10,healthLog * 3):f1} {GetLabel(healthLog)}{maxPart}";
            }
        }

        private static string GetLabel(int log)
        {
            return log switch
            {
                0 => "KB",
                1 => "MB",
                2 => "GB",
                _ => "TB"
            };
        }
    }
}