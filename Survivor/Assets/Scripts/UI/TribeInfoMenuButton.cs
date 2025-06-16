using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Survivor.UI
{
    [RequireComponent(typeof(Button))]
    public class TribeInfoMenuButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Button button;
        private Image buttonImage;
        private TribeInfoMenuController menu;

        private void Awake()
        {
            Debug.Log("[TribeInfoMenuButton] Awake called");
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
            menu = GetComponentInParent<TribeInfoMenuController>();

            if (menu == null)
            {
                Debug.LogError("[TribeInfoMenuButton] TribeInfoMenuController component not found in parent hierarchy!");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("[TribeInfoMenuButton] Clicked!");
            if (menu != null)
            {
                menu.ToggleMenu();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("[TribeInfoMenuButton] Mouse entered");
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("[TribeInfoMenuButton] Mouse exited");
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }
        }
    }
} 