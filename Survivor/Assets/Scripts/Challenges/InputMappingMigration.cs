using UnityEngine;
using UnityEditor;
using Survivor.Challenges;
using UnityEngine.InputSystem;

namespace Survivor.Challenges
{
    /// <summary>
    /// Migration script to convert old KeyCode values to new Key enum values
    /// Run this once to update existing checkpoints in the scene
    /// </summary>
    public class InputMappingMigration : MonoBehaviour
    {
        [MenuItem("Tools/Input Mapping/Migrate KeyCode to Key")]
        public static void MigrateKeyCodeToKey()
        {
            // Find all InputMappingCheckpoint components in the scene
            InputMappingCheckpoint[] checkpoints = FindObjectsOfType<InputMappingCheckpoint>();
            
            if (checkpoints.Length == 0)
            {
                Debug.Log("[InputMappingMigration] No checkpoints found to migrate.");
                return;
            }
            
            int migratedCount = 0;
            
            foreach (var checkpoint in checkpoints)
            {
                if (MigrateCheckpoint(checkpoint))
                {
                    migratedCount++;
                }
            }
            
            Debug.Log($"[InputMappingMigration] Successfully migrated {migratedCount} checkpoints from KeyCode to Key.");
            
            // Mark the scene as dirty so Unity knows to save the changes
            EditorUtility.SetDirty(FindObjectOfType<InputMappingCheckpoint>());
        }
        
        private static bool MigrateCheckpoint(InputMappingCheckpoint checkpoint)
        {
            bool wasModified = false;
            InputMapping mapping = checkpoint.GetInputMappingForMigration();
            
            // Convert KeyCode values to Key values
            if (IsOldKeyCodeValue(mapping.forwardKey, 119)) // KeyCode.W
            {
                mapping.forwardKey = Key.W;
                wasModified = true;
            }
            if (IsOldKeyCodeValue(mapping.backwardKey, 115)) // KeyCode.S
            {
                mapping.backwardKey = Key.S;
                wasModified = true;
            }
            if (IsOldKeyCodeValue(mapping.leftKey, 97)) // KeyCode.A
            {
                mapping.leftKey = Key.A;
                wasModified = true;
            }
            if (IsOldKeyCodeValue(mapping.rightKey, 100)) // KeyCode.D
            {
                mapping.rightKey = Key.D;
                wasModified = true;
            }
            if (IsOldKeyCodeValue(mapping.jumpKey, 32)) // KeyCode.Space
            {
                mapping.jumpKey = Key.Space;
                wasModified = true;
            }
            if (IsOldKeyCodeValue(mapping.sprintKey, 304)) // KeyCode.LeftShift
            {
                mapping.sprintKey = Key.LeftShift;
                wasModified = true;
            }
            if (IsOldKeyCodeValue(mapping.crouchKey, 99)) // KeyCode.C
            {
                mapping.crouchKey = Key.C;
                wasModified = true;
            }
            
            if (wasModified)
            {
                checkpoint.SetInputMappingForMigration(mapping);
                Debug.Log($"[InputMappingMigration] Migrated checkpoint: {checkpoint.name}");
                EditorUtility.SetDirty(checkpoint);
            }
            
            return wasModified;
        }
        
        /// <summary>
        /// Checks if a Key enum value corresponds to an old KeyCode integer value
        /// </summary>
        private static bool IsOldKeyCodeValue(Key key, int oldKeyCodeValue)
        {
            // Convert the Key enum to its underlying integer value and compare
            return (int)key == oldKeyCodeValue;
        }
        
        [MenuItem("Tools/Input Mapping/Reset All Checkpoints to Default")]
        public static void ResetAllCheckpointsToDefault()
        {
            InputMappingCheckpoint[] checkpoints = FindObjectsOfType<InputMappingCheckpoint>();
            
            if (checkpoints.Length == 0)
            {
                Debug.Log("[InputMappingMigration] No checkpoints found to reset.");
                return;
            }
            
            foreach (var checkpoint in checkpoints)
            {
                checkpoint.SetInputMappingForMigration(InputMapping.CreateDefault());
                EditorUtility.SetDirty(checkpoint);
            }
            
            Debug.Log($"[InputMappingMigration] Reset {checkpoints.Length} checkpoints to default values.");
        }
    }
} 