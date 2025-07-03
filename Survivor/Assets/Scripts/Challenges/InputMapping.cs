using UnityEngine;
using UnityEngine.InputSystem;

namespace Survivor.Challenges
{
    /// <summary>
    /// Data structure for defining input mappings that can be applied when a checkpoint is triggered.
    /// 
    /// How it works:
    /// 1. Key codes define which keys are used for each action
    /// 2. Permission booleans control whether each action is allowed
    /// 3. When a checkpoint is triggered, only allowed inputs will work
    /// 4. Use the preset methods (CreateDefault, CreateForwardOnly, etc.) for common scenarios
    /// </summary>
    [System.Serializable]
    public class InputMapping
    {
        [Header("Checkpoint Info")]
        public string checkpointName = "Checkpoint";
        public int checkpointIndex = 1;
        
        [Header("Movement Controls")]
        [Tooltip("Key code for forward movement (W)")]
        public Key forwardKey = Key.W;
        [Tooltip("Key code for backward movement (S)")]
        public Key backwardKey = Key.S;
        [Tooltip("Key code for left movement (A)")]
        public Key leftKey = Key.A;
        [Tooltip("Key code for right movement (D)")]
        public Key rightKey = Key.D;
        
        [Header("Action Controls")]
        [Tooltip("Key code for jumping (Space)")]
        public Key jumpKey = Key.Space;
        [Tooltip("Key code for sprinting (Left Shift)")]
        public Key sprintKey = Key.LeftShift;
        [Tooltip("Key code for crouching (C)")]
        public Key crouchKey = Key.C;
        
        [Header("Special Movement Controls")]
        [Tooltip("Key code for crawl movement mashing (Space by default)")]
        public Key crawlMovementKey = Key.Space;
        
        [Header("Input Permissions - Basic Movement")]
        [Tooltip("Allow forward movement (W key)")]
        public bool allowForward = true;
        [Tooltip("Allow backward movement (S key)")]
        public bool allowBackward = true;
        [Tooltip("Allow left movement (A key)")]
        public bool allowLeft = true;
        [Tooltip("Allow right movement (D key)")]
        public bool allowRight = true;
        
        [Header("Input Permissions - Actions")]
        [Tooltip("Allow jumping (Space key)")]
        public bool allowJump = true;
        [Tooltip("Allow sprinting (Left Shift key)")]
        public bool allowSprint = true;
        [Tooltip("Allow crouching (C key) - entering crawl mode")]
        public bool allowCrouch = true;
        
        [Header("Input Permissions - Special Movement")]
        [Tooltip("Allow crawl movement (space bar mashing while crawling)")]
        public bool allowCrawlMovement = true;
        [Tooltip("Allow swimming movement (movement while in water)")]
        public bool allowSwimming = true;
        
        [Header("Description")]
        [TextArea(2, 4)]
        public string description = "Checkpoint description";
        
        /// <summary>
        /// Creates a default input mapping that allows all inputs
        /// </summary>
        public static InputMapping CreateDefault()
        {
            return new InputMapping
            {
                checkpointName = "Default",
                checkpointIndex = 0,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = true,
                allowBackward = true,
                allowLeft = true,
                allowRight = true,
                allowJump = true,
                allowSprint = true,
                allowCrouch = true,
                allowCrawlMovement = true,
                allowSwimming = true,
                description = "Default input mapping - all controls enabled (WASD + Space + Shift + C + Crawling + Swimming)"
            };
        }
        
        /// <summary>
        /// Creates an input mapping that only allows forward movement
        /// </summary>
        public static InputMapping CreateForwardOnly()
        {
            return new InputMapping
            {
                checkpointName = "Forward Only",
                checkpointIndex = 1,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = true,
                allowBackward = false,
                allowLeft = false,
                allowRight = false,
                allowJump = false,
                allowSprint = false,
                allowCrouch = false,
                allowCrawlMovement = false,
                allowSwimming = false,
                description = "Only forward movement allowed"
            };
        }
        
        /// <summary>
        /// Creates an input mapping that disables all inputs
        /// </summary>
        public static InputMapping CreateDisabled()
        {
            return new InputMapping
            {
                checkpointName = "Disabled",
                checkpointIndex = 2,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = false,
                allowBackward = false,
                allowLeft = false,
                allowRight = false,
                allowJump = false,
                allowSprint = false,
                allowCrouch = false,
                allowCrawlMovement = false,
                allowSwimming = false,
                description = "All inputs disabled"
            };
        }
        
        /// <summary>
        /// Creates an input mapping that only allows jumping
        /// </summary>
        public static InputMapping CreateJumpOnly()
        {
            return new InputMapping
            {
                checkpointName = "Jump Only",
                checkpointIndex = 3,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = false,
                allowBackward = false,
                allowLeft = false,
                allowRight = false,
                allowJump = true,
                allowSprint = false,
                allowCrouch = false,
                allowCrawlMovement = false,
                allowSwimming = false,
                description = "Only jumping allowed"
            };
        }
        
        /// <summary>
        /// Creates an input mapping that allows crawling but disables crawl movement
        /// </summary>
        public static InputMapping CreateCrawlModeOnly()
        {
            return new InputMapping
            {
                checkpointName = "Crawl Mode Only",
                checkpointIndex = 4,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = true,
                allowBackward = true,
                allowLeft = true,
                allowRight = true,
                allowJump = false,
                allowSprint = false,
                allowCrouch = true,
                allowCrawlMovement = false, // Can enter crawl mode but can't move while crawling
                allowSwimming = false,
                description = "Can enter crawl mode but cannot move while crawling (no space bar mashing)"
            };
        }
        
        /// <summary>
        /// Creates an input mapping that allows full crawling movement
        /// </summary>
        public static InputMapping CreateCrawlMovementOnly()
        {
            return new InputMapping
            {
                checkpointName = "Crawl Movement Only",
                checkpointIndex = 5,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = true,
                allowBackward = true,
                allowLeft = true,
                allowRight = true,
                allowJump = false,
                allowSprint = false,
                allowCrouch = true,
                allowCrawlMovement = true, // Can crawl and move while crawling
                allowSwimming = false,
                description = "Crawling movement only - direction keys + crouch + space bar mashing"
            };
        }
        
        /// <summary>
        /// Creates an input mapping that allows swimming but no other movement
        /// </summary>
        public static InputMapping CreateSwimmingOnly()
        {
            return new InputMapping
            {
                checkpointName = "Swimming Only",
                checkpointIndex = 6,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = true,
                allowBackward = true,
                allowLeft = true,
                allowRight = true,
                allowJump = false,
                allowSprint = false,
                allowCrouch = false,
                allowCrawlMovement = false,
                allowSwimming = true, // Only swimming movement allowed
                description = "Swimming movement only - direction keys work only when in water"
            };
        }
        
        /// <summary>
        /// Creates an input mapping that disables swimming movement
        /// </summary>
        public static InputMapping CreateNoSwimming()
        {
            return new InputMapping
            {
                checkpointName = "No Swimming",
                checkpointIndex = 7,
                forwardKey = Key.W,
                backwardKey = Key.S,
                leftKey = Key.A,
                rightKey = Key.D,
                jumpKey = Key.Space,
                sprintKey = Key.LeftShift,
                crouchKey = Key.C,
                crawlMovementKey = Key.Space,
                allowForward = true,
                allowBackward = true,
                allowLeft = true,
                allowRight = true,
                allowJump = true,
                allowSprint = true,
                allowCrouch = true,
                allowCrawlMovement = true,
                allowSwimming = false, // Swimming movement disabled
                description = "All movement except swimming - player cannot move while in water"
            };
        }
    }
} 