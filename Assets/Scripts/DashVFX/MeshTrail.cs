using System.Collections;
using UnityEngine;

namespace DashVFX
{
    public class MeshTrail : MonoBehaviour
    {
        public Sprite playerSprite;

        [Range(0,1),Tooltip("Affects time it takes to fade (higher = faster)")]
        public float fadeSpeed = 0.6f;
        [Tooltip("Maximum Time between generating VFX (seconds)")]
        public float maxTimeInterval = 1;

        private bool _shouldShowVFX;
        private bool _isFlipped;

        public void StartDash(bool flipped)
        {
            _shouldShowVFX = true;
            _isFlipped = flipped;
            StartCoroutine(DashCoroutine());
        }

        public void StopDash()
        {
            _shouldShowVFX = false;
        }

        private IEnumerator DashCoroutine()
        {
            while (_shouldShowVFX)
            {
                StartCoroutine(GenerateOneVFX());
                yield return new WaitForSeconds(maxTimeInterval);
            }
        }

        private IEnumerator GenerateOneVFX()
        {
            GameObject obj = new GameObject("VFXObject", typeof(SpriteRenderer))
            {
                transform =
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    localScale = transform.localScale
                }
            };
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = playerSprite;
            spriteRenderer.flipX = _isFlipped;


            while (spriteRenderer.color.a >= 0f)
            {
                Color spriteColor = spriteRenderer.color;
                spriteColor.a -= 0.01f;
                spriteRenderer.color = spriteColor;
                yield return new WaitForSeconds(Time.deltaTime / fadeSpeed);
            }

            Destroy(obj);
        }
    }
}