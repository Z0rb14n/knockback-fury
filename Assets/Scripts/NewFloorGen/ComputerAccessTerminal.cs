using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace NewFloorGen
{
    public class ComputerAccessTerminal : TriggerTextScript
    {
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockingSprite;
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite disabledSprite;
        [SerializeField] private SpriteRenderer computerSprite;
        public State stateOnAwake = State.Locked;
        [Min(0)]
        public float hackDuration = 1;
        public UnityEvent<ComputerAccessTerminal> onUnlock;

        private State _state;
        private State CurrentState
        {
            get => _state;
            set
            {
                _state = value;
                switch (_state)
                {
                    case State.Disabled:
                        computerSprite.sprite = disabledSprite;
                        break;
                    case State.Locked:
                        computerSprite.sprite = lockedSprite;
                        break;
                    case State.Unlocking:
                        computerSprite.sprite = unlockingSprite;
                        break;
                    case State.Unlocked:
                        computerSprite.sprite = unlockedSprite;
                        break;
                    default:
                        computerSprite.sprite = disabledSprite;
                        break;
                }
            }
        }

        protected override bool CanInteract
        {
            get => CurrentState == State.Locked;
            set
            {
                CurrentState = value ? stateOnAwake : State.Disabled;
                UpdatePlayerGrapple();
            }
        }

        private void Awake()
        {
            CurrentState = stateOnAwake;
        }

        private void OnEnable()
        {
            CurrentState = stateOnAwake;
        }

        private void OnDisable()
        {
            CurrentState = State.Disabled;
        }

        private IEnumerator HackCoroutine()
        {
            yield return new WaitForSeconds(hackDuration);
            CurrentState = State.Unlocked;
            stateOnAwake = State.Unlocked;
            onUnlock?.Invoke(this);
        }

        protected override void OnPlayerInteraction()
        {
            StartCoroutine(HackCoroutine());
            CurrentState = State.Unlocking;
            Destroy(notification.gameObject);
            notification = null;
        }

        public enum State
        {
            Disabled, Locked, Unlocking, Unlocked
        }
    }
}