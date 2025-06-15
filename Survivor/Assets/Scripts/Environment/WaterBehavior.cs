using UnityEngine;
using Survivor.Shared;

namespace Survivor.Environment
{
    public class WaterBehavior : MonoBehaviour
    {
        public float waterHeight = 0f;
        public float buoyancyForce = 2f;
        public float dragForce = 0.5f;

        private void OnTriggerEnter(Collider other)
        {
            IWaterInteractable interactable = other.GetComponent<IWaterInteractable>();
            if (interactable != null)
            {
                interactable.OnEnterWater(waterHeight);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IWaterInteractable interactable = other.GetComponent<IWaterInteractable>();
            if (interactable != null)
            {
                interactable.OnExitWater();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            IWaterInteractable interactable = other.GetComponent<IWaterInteractable>();
            if (interactable != null)
            {
                interactable.OnStayInWater(waterHeight, buoyancyForce, dragForce);
            }
        }
    }
} 