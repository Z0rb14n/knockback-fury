using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerMovementScript), typeof(Rigidbody2D))]
    public class PlayerWeaponControl : MonoBehaviour
    {
        private PlayerMovementScript _playerMovement;
        private Weapons.Weapon _weapon;
        private Camera _cam;
        private Rigidbody2D _body;

        private void Awake()
        {
            _weapon = GetComponentInChildren<Weapons.Weapon>();
            _cam = Camera.main;
            _body = GetComponent<Rigidbody2D>();
            _playerMovement = GetComponent<PlayerMovementScript>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) _weapon.Reload();

            if (Input.GetMouseButton(0))
            {
                Vector3 worldMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
                bool fireResult = _weapon.Fire(Input.GetMouseButtonDown(0));
                if (fireResult)
                    _playerMovement.RequestKnockback(((Vector2)(transform.position - worldMousePos)).normalized,
                        _weapon.weaponData.knockbackStrength);
            }

            if (Input.GetMouseButtonDown(1)) _weapon.UseMelee(_body.velocity);
        }
    }
}