using System.Collections;
using UnityEngine;

namespace NewFloorGen
{
    public class FolderSpawnPoint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite hasEnemiesSprite;
        [SerializeField] private Sprite spawningSprite;
        [SerializeField] private Sprite emptySprite;

        public GameObject[] enemiesToSpawn;
        [Min(0)]
        public float timeBetweenSpawns = 0.5f;

        private void Awake()
        {
            spriteRenderer.sprite = enemiesToSpawn.Length > 0 ? hasEnemiesSprite : emptySprite;
        }

        private IEnumerator SpawnCoroutine()
        {
            Vector3 pos = transform.position;
            spriteRenderer.sprite = spawningSprite;
            foreach (GameObject enemy in enemiesToSpawn)
            {
                yield return new WaitForSeconds(timeBetweenSpawns);
                Instantiate(enemy, pos, Quaternion.identity, transform);
            }

            spriteRenderer.sprite = emptySprite;
        }
        
        public void StartSpawn()
        {
            StartCoroutine(SpawnCoroutine());
        }
    }
}