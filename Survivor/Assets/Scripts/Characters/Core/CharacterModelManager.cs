using UnityEngine;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Survivor.Characters
{
    public class CharacterModelManager : MonoBehaviour
    {
        [Header("Character Model Settings")]
        [SerializeField] private string maleModelsPath = "Assets/Models/Characters/Male";
        [SerializeField] private string femaleModelsPath = "Assets/Models/Characters/Female";
        
        private GameObject[] maleModels;
        private GameObject[] femaleModels;

        private void Awake()
        {
            LoadCharacterModels();
        }

        private void LoadCharacterModels()
        {
#if UNITY_EDITOR
            // Load male models
            string[] maleGuids = AssetDatabase.FindAssets("t:GameObject", new[] { maleModelsPath });
            maleModels = maleGuids.Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
            if (maleModels.Length == 0)
            {
                Debug.LogWarning($"No male character models found in {maleModelsPath}");
            }

            // Load female models
            string[] femaleGuids = AssetDatabase.FindAssets("t:GameObject", new[] { femaleModelsPath });
            femaleModels = femaleGuids.Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
            if (femaleModels.Length == 0)
            {
                Debug.LogWarning($"No female character models found in {femaleModelsPath}");
            }
#else
            // In build, we need to use Resources.LoadAll
            maleModels = Resources.LoadAll<GameObject>(maleModelsPath);
            femaleModels = Resources.LoadAll<GameObject>(femaleModelsPath);
#endif
        }

        public void AssignCharacterModel(Character character)
        {
            if (character == null) return;

            // Get the appropriate model array based on gender
            GameObject[] models = character.Gender == Gender.Male ? maleModels : femaleModels;
            
            if (models == null || models.Length == 0)
            {
                Debug.LogError($"No character models available for gender: {character.Gender}");
                return;
            }

            // Randomly select a model
            GameObject selectedModel = models[Random.Range(0, models.Length)];
            
            // Instantiate the model as a child of the character
            GameObject modelInstance = Instantiate(selectedModel, character.transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;

            // Get the animator from the model and character
            Animator modelAnimator = modelInstance.GetComponent<Animator>();
            Animator characterAnimator = character.GetComponent<Animator>();
            
            if (modelAnimator != null && characterAnimator != null)
            {
                // Only update the avatar, keep the original animator controller
                characterAnimator.avatar = modelAnimator.avatar;
                
                // Destroy the model's animator since we're using the character's
                Destroy(modelAnimator);
            }
            else
            {
                Debug.LogError($"Missing Animator component on either the character or the model for {character.CharacterName}");
            }
        }
    }
} 