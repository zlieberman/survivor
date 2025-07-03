using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using System.Collections.Generic;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 5.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 15f;

        [Tooltip("Swim speed of the character in m/s")]
        public float SwimSpeed = 3.0f;

        [Tooltip("Crawl speed of the character in m/s")]
        public float CrawlSpeed = 1.0f;

        [Tooltip("Maximum crawl speed when mashing space key")]
        public float MaxCrawlSpeed = 8.0f;

        [Tooltip("How quickly crawl speed decays when not mashing")]
        public float CrawlSpeedDecayRate = 5.0f;

        [Tooltip("Time window for measuring space key mash frequency (seconds)")]
        public float MashTimeWindow = 1.0f;

        [Tooltip("Minimum space presses per second to start moving")]
        public float MinMashRate = 1.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Header("Crawling Collision")]
        [Tooltip("Height of the character controller when crawling (should be smaller than standing height)")]
        public float CrawlHeight = 0.8f;

        [Tooltip("Radius of the character controller when crawling (can be slightly smaller for better fit)")]
        public float CrawlRadius = 0.3f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Header("Swimming Audio")]
        public AudioClip SwimmingAudioClip;
        [Range(0, 1)] public float SwimmingAudioVolume = 0.5f;
        [Tooltip("Interval between swimming sounds while moving (seconds)")]
        [Range(0.5f, 3.0f)] public float SwimmingSoundInterval = 1.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // swimming sound timer
        private float _swimmingSoundTimer = 0f;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDIsCrawling;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        private bool _isCrawling = false;
        private bool _isSwimming = false;
        private bool _crawlInputPressed = false;

        // Space key mashing variables for crawling
        private List<float> _spacePressTimes = new List<float>();
        private float _currentCrawlSpeed = 0f;
        private bool _spacePressed = false;

        // Character controller original values for crawling
        private float _originalCharacterHeight;
        private float _originalCharacterRadius;
        private Vector3 _originalCharacterCenter;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // Store original character controller values
            _originalCharacterHeight = _controller.height;
            _originalCharacterRadius = _controller.radius;
            _originalCharacterCenter = _controller.center;
            
            // Movement system will be accessed through static accessor if available
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            HandleCrawlInput();
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDIsCrawling = Animator.StringToHash("IsCrawling");
        }

        private void HandleCrawlInput()
        {
            // Check if crawl input was just pressed (toggle crawling)
            if (_input.crawl && !_crawlInputPressed)
            {
                if (_isCrawling)
                {
                    // Trying to stand up - check if there's enough room
                    if (CanStandUp())
                    {
                        _isCrawling = false;
                        
                        // Stand up - restore original character controller values
                        _controller.height = _originalCharacterHeight;
                        _controller.radius = _originalCharacterRadius;
                        _controller.center = _originalCharacterCenter;
                        
                        _currentCrawlSpeed = 0f; // Reset when exiting crawl
                        
                        Debug.Log($"[ThirdPersonController] Character controller restored to standing - Height: {_originalCharacterHeight}, Radius: {_originalCharacterRadius}");
                    }
                    else
                    {
                        Debug.Log($"[ThirdPersonController] Cannot stand up - not enough room above player!");
                        // Don't toggle crawling state, player remains crawling
                    }
                }
                else
                {
                    // Start crawling
                    _isCrawling = true;
                    
                    // Go prone - adjust character controller for crawling
                    _controller.height = CrawlHeight;
                    _controller.radius = CrawlRadius;
                    _controller.center = new Vector3(0, CrawlHeight / 2f, 0);
                    
                    _currentCrawlSpeed = CrawlSpeed; // Start at base crawl speed
                    _spacePressTimes.Clear();
                    
                    Debug.Log($"[ThirdPersonController] Character controller adjusted for crawling - Height: {CrawlHeight}, Radius: {CrawlRadius}");
                }
            }
            
            // Update the pressed state - reset when key is released
            if (_input.crawl)
            {
                _crawlInputPressed = true;
            }
            else
            {
                _crawlInputPressed = false;
            }

            // Handle space key mashing for crawling movement
            if (_isCrawling)
            {
                HandleSpaceMashing();
            }

            // Update animator - pause animation when crawling but not moving
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDIsCrawling, _isCrawling);
                _animator.speed = (_isCrawling && _input.move == Vector2.zero) ? 0f : 1f;
            }
        }

        /// <summary>
        /// Checks if the player has enough room above them to stand up from crawling position
        /// </summary>
        /// <returns>True if there's enough room to stand, false if blocked by obstacles</returns>
        private bool CanStandUp()
        {
            // Only exclude the player's own layer - we want to detect ALL obstacles including those on ground layers
            int playerLayer = gameObject.layer;
            int playerLayerMask = 1 << playerLayer;
            
            // Check everything except the player's own layer
            int checkLayerMask = ~playerLayerMask;
            
            // Calculate positions for the collision check
            Vector3 currentPosition = transform.position;
            
            // Start the check from the current crawling center position
            Vector3 checkStart = currentPosition + new Vector3(0, CrawlHeight * 0.5f, 0);
            
            // End the check at the standing height position
            Vector3 checkEnd = currentPosition + new Vector3(0, _originalCharacterHeight - 0.1f, 0);
            
            // Use the standing radius for accurate collision detection
            float checkRadius = _originalCharacterRadius * 0.9f;
            
            // Perform the main collision check - this will detect the Net and other obstacles
            bool hasObstacle = Physics.CheckCapsule(
                checkStart,
                checkEnd,
                checkRadius,
                checkLayerMask,
                QueryTriggerInteraction.Ignore
            );
            
            // Additional raycast checks for extra precision - multiple rays for better coverage
            bool hasObstacleRaycast = false;
            float raycastDistance = _originalCharacterHeight - CrawlHeight;
            Vector3 rayStartPos = currentPosition + new Vector3(0, CrawlHeight * 0.5f, 0);
            
            // Center raycast
            hasObstacleRaycast |= Physics.Raycast(rayStartPos, Vector3.up, raycastDistance, checkLayerMask, QueryTriggerInteraction.Ignore);
            
            // Side raycasts for more thorough detection
            float sideOffset = _originalCharacterRadius * 0.5f;
            hasObstacleRaycast |= Physics.Raycast(rayStartPos + new Vector3(sideOffset, 0, 0), Vector3.up, raycastDistance, checkLayerMask, QueryTriggerInteraction.Ignore);
            hasObstacleRaycast |= Physics.Raycast(rayStartPos + new Vector3(-sideOffset, 0, 0), Vector3.up, raycastDistance, checkLayerMask, QueryTriggerInteraction.Ignore);
            hasObstacleRaycast |= Physics.Raycast(rayStartPos + new Vector3(0, 0, sideOffset), Vector3.up, raycastDistance, checkLayerMask, QueryTriggerInteraction.Ignore);
            hasObstacleRaycast |= Physics.Raycast(rayStartPos + new Vector3(0, 0, -sideOffset), Vector3.up, raycastDistance, checkLayerMask, QueryTriggerInteraction.Ignore);
            
            // Check if there's any obstacle detected by either method
            bool canStand = !hasObstacle && !hasObstacleRaycast;
            
            // Enhanced debug logging
            if (hasObstacle || hasObstacleRaycast)
            {
                Debug.Log($"[ThirdPersonController] CanStandUp BLOCKED: capsuleHit={hasObstacle}, raycastHit={hasObstacleRaycast} at position {currentPosition}");
                
                // Additional debug - find what we're hitting
                if (Physics.CheckCapsule(checkStart, checkEnd, checkRadius, checkLayerMask, QueryTriggerInteraction.Ignore))
                {
                    Collider[] hits = Physics.OverlapCapsule(checkStart, checkEnd, checkRadius, checkLayerMask, QueryTriggerInteraction.Ignore);
                    foreach (var hit in hits)
                    {
                        Debug.Log($"[ThirdPersonController] Obstacle detected: {hit.name} on layer {hit.gameObject.layer}");
                    }
                }
            }
            else
            {
                Debug.Log($"[ThirdPersonController] CanStandUp OK: No obstacles detected, can stand up");
            }
            
            return canStand;
        }

        private void HandleSpaceMashing()
        {
            // If not pressing a direction key, don't move at all
            if (_input.move == Vector2.zero)
            {
                _currentCrawlSpeed = 0f;
                return;
            }

            // Check if space key was just pressed
            if (_input.jump && !_spacePressed)
            {
                _spacePressed = true;
                _spacePressTimes.Add(Time.time);
                
                // Remove old press times outside the time window
                float cutoffTime = Time.time - MashTimeWindow;
                _spacePressTimes.RemoveAll(time => time < cutoffTime);
                
                // Calculate mash rate (presses per second)
                float mashRate = _spacePressTimes.Count / MashTimeWindow;
                float maxMashRate = 10f; // Mash rate needed for max speed
                
                // Calculate crawl speed based on mash rate
                if (mashRate >= MinMashRate)
                {
                    float boostMultiplier = Mathf.Clamp01((mashRate - MinMashRate) / (maxMashRate - MinMashRate));
                    float boostedSpeed = Mathf.Lerp(CrawlSpeed, MaxCrawlSpeed, boostMultiplier);
                    _currentCrawlSpeed = boostedSpeed;
                }
                else
                {
                    // If not mashing enough, stay at base crawl speed (but only if moving)
                    _currentCrawlSpeed = CrawlSpeed;
                }
                
                Debug.Log($"[ThirdPersonController] Space mashed! Rate: {mashRate:F1}/s, Speed: {_currentCrawlSpeed:F1}");
            }
            else if (!_input.jump)
            {
                _spacePressed = false;
            }
            
            // Decay crawl speed when not mashing, but don't go below base crawl speed
            if (_spacePressTimes.Count == 0 || Time.time - _spacePressTimes[_spacePressTimes.Count - 1] > MashTimeWindow)
            {
                _currentCrawlSpeed = Mathf.Lerp(_currentCrawlSpeed, CrawlSpeed, Time.deltaTime * CrawlSpeedDecayRate);
            }

            // Reset jump so each press is only counted once
            // Only reset jump input when actually crawling to avoid interfering with input mapping system
            if (_isCrawling)
            {
                _input.jump = false;
            }
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = 0.0f;
            
            if (_isSwimming)
            {
                targetSpeed = SwimSpeed;
            }
            else if (_isCrawling)
            {
                targetSpeed = _currentCrawlSpeed;
            }
            else if (_input.sprint)
            {
                targetSpeed = SprintSpeed;
            }
            else
            {
                targetSpeed = MoveSpeed;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0 (including when crawling with no direction input)
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            
            // When crawling, we need input direction for movement, but speed comes from space mashing
            // So we should use input magnitude for direction control, but not for speed
            if (_isCrawling && _input.move == Vector2.zero)
            {
                inputMagnitude = 0f; // No direction input = no movement
            }

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }

            // Handle swimming sounds while moving
            HandleSwimmingSounds(inputMagnitude);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump (disabled while crawling)
                bool canJump = _input.jump && _jumpTimeoutDelta <= 0.0f;
                
                // Check if crawling - disable jumping while crawling
                bool isCrawling = _isCrawling;
                
                canJump = canJump && !isCrawling;
                
                if (canJump)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips != null && FootstepAudioClips.Length > 0 && _controller != null)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudioClip != null && _controller != null)
                {
                    AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void PlaySwimmingSound()
        {
            if (SwimmingAudioClip != null && _controller != null)
            {
                AudioSource.PlayClipAtPoint(SwimmingAudioClip, transform.TransformPoint(_controller.center), SwimmingAudioVolume);
            }
        }

        // Public property to check if player is crawling
        public bool IsCrawling
        {
            get
            {
                return _isCrawling;
            }
        }

        // Public property to get current crawl speed
        public float CurrentCrawlSpeed => _currentCrawlSpeed;
        
        /// <summary>
        /// Forces the player to stand up from crawling if there's enough room above them.
        /// Used by systems like checkpoints that disable crawling.
        /// </summary>
        /// <returns>True if the player was successfully stood up, false if blocked</returns>
        public bool ForceStandUp()
        {
            if (!_isCrawling)
            {
                return true; // Already standing
            }
            
            if (CanStandUp())
            {
                _isCrawling = false;
                
                // Restore original character controller values
                _controller.height = _originalCharacterHeight;
                _controller.radius = _originalCharacterRadius;
                _controller.center = _originalCharacterCenter;
                
                _currentCrawlSpeed = 0f; // Reset when exiting crawl
                
                Debug.Log($"[ThirdPersonController] Forced to stand up - character controller restored to standing");
                return true;
            }
            else
            {
                Debug.LogWarning($"[ThirdPersonController] Cannot force stand up - not enough room above player!");
                return false;
            }
        }

        public bool IsSwimming
        {
            get
            {
                return _isSwimming;
            }
        }

        public void SetSwimming(bool swimming)
        {
            _isSwimming = swimming;
            
            // Reset swimming sound timer when swimming state changes
            _swimmingSoundTimer = 0f;
        }

        private void HandleSwimmingSounds(float inputMagnitude)
        {
            // Only play swimming sounds if we're swimming and moving
            if (_isSwimming && inputMagnitude > 0.1f)
            {
                _swimmingSoundTimer += Time.deltaTime;
                
                // Play swimming sound at intervals
                if (_swimmingSoundTimer >= SwimmingSoundInterval)
                {
                    PlaySwimmingSound();
                    _swimmingSoundTimer = 0f;
                }
            }
            else
            {
                // Reset timer when not swimming or not moving
                _swimmingSoundTimer = 0f;
            }
        }
    }
}