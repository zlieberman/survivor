using UnityEngine;

namespace Survivor.Characters
{
    /// <summary>
    /// Simple script to register camp position with the static CampPositionManager
    /// Can be attached to tent, campfire, or any camp object
    /// </summary>
    public class CampPositionRegistrar : MonoBehaviour
    {
        [Header("Registration Settings")]
        public bool registerOnStart = true;
        public bool registerOnAwake = false;
        public float registrationDelay = 0f;

        private void Awake()
        {
            if (registerOnAwake)
            {
                RegisterCampPosition();
            }
        }

        private void Start()
        {
            if (registerOnStart)
            {
                if (registrationDelay > 0f)
                {
                    Invoke(nameof(RegisterCampPosition), registrationDelay);
                }
                else
                {
                    RegisterCampPosition();
                }
            }
        }

        public void RegisterCampPosition()
        {
            CampPositionManager.SetCampPosition(transform.position);
            Debug.Log($"[CampPositionRegistrar] Registered camp position from {gameObject.name}: {transform.position}");
        }

        private void OnDestroy()
        {
            // Optionally clear the camp position when this object is destroyed
            // Uncomment the next line if you want to clear the position when camp objects are destroyed
            // CampPositionManager.ClearCampPosition();
        }
    }
} 