using System.Collections.Generic;
using FileSave;
using UnityEngine;
using Upgrades;
using Util;
using Weapons;

using Random = System.Random;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class RoomData : MonoBehaviour
    {
        [HideInInspector]
        public UpgradePickup pickup;
        [HideInInspector]
        public CheesePickup cheesePickup;
        [HideInInspector]
        public WeaponPickup weaponPickup;
        
        public Layout[] layouts;
        
        public int ToPreview { get; set; } = -1;

        private readonly HashSet<EntityHealth> _enemies = new();

        public List<EntityHealth> Enemies => new(_enemies);
        
        public List<(SocketBehaviour, EnemySpawnType)> GenerateSockets(Random random, Dictionary<Vector2, List<GameObject>> socketPrefabSizes)
        {
            List<(SocketBehaviour, EnemySpawnType)> socketBehaviours = new();
            if (layouts == null || layouts.Length == 0)
            {
                Debug.LogWarning("[RoomData::GenerateSockets] No layouts for this object. Skipping.");
                return socketBehaviours;
            }
            Layout layout = layouts.GetRandom(random);
            foreach (SocketShape shape in layout.sockets)
            {
                if (socketPrefabSizes.TryGetValue(shape.size, out List<GameObject> possibleSocketPrefabs) && possibleSocketPrefabs.Count > 0)
                {
                    GameObject createdSocket = Instantiate(possibleSocketPrefabs.GetRandom(random), transform);
                    createdSocket.transform.localPosition = shape.position;
                    SocketBehaviour behaviour = createdSocket.GetComponent<SocketBehaviour>();
                    EnemySpawnType type = behaviour.AllowedSpawnTypes;
                    if (behaviour && type != 0) socketBehaviours.Add((behaviour, behaviour.AllowedSpawnTypes));
                }
                else
                {
                    Debug.LogWarning($"[RoomData::GenerateSockets] No game object entry for socket of size {shape.size}; skipping");
                }
            }

            return socketBehaviours;
        }

        public void AddEnemy(EntityHealth health)
        {
            _enemies.Add(health);
            health.OnDeath += OnEnemyDeath;
        }

        public void OnEnemyDeath(EntityHealth health)
        {
            health.OnDeath -= OnEnemyDeath;
            _enemies.Remove(health);
            if (_enemies.Count != 0) return;
            if (pickup) pickup.gameObject.SetActive(true);
            if (cheesePickup) cheesePickup.gameObject.SetActive(true);
            if (weaponPickup) weaponPickup.gameObject.SetActive(true);
        }
    }
}