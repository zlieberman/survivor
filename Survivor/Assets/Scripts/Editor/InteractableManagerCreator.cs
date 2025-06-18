using UnityEngine;
using Survivor.Interactables;

namespace Survivor.Editor
{
    public class InteractableManagerCreator : MonoBehaviour
    {
        [ContextMenu("Create InteractableManager")]
        public void CreateInteractableManager()
        {
            if (InteractableManager.Instance == null)
            {
                GameObject interactableManagerObj = new GameObject("InteractableManager");
                interactableManagerObj.AddComponent<InteractableManager>();
                Debug.Log("[InteractableManagerCreator] Created InteractableManager in scene");
            }
            else
            {
                Debug.Log("[InteractableManagerCreator] InteractableManager already exists in scene");
            }
        }
        
        [ContextMenu("Check InteractableManager Status")]
        public void CheckInteractableManagerStatus()
        {
            if (InteractableManager.Instance == null)
            {
                Debug.LogWarning("[InteractableManagerCreator] InteractableManager.Instance is null!");
                Debug.LogWarning("[InteractableManagerCreator] This will prevent campfire wood from burning!");
            }
            else
            {
                Debug.Log($"[InteractableManagerCreator] InteractableManager found with {InteractableManager.Instance.GetListenerCount()} registered listeners");
            }
        }
    }
} 