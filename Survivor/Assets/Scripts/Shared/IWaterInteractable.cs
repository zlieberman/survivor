using UnityEngine;

namespace Survivor.Shared
{
    public interface IWaterInteractable
    {
        void OnEnterWater(float waterHeight);
        void OnExitWater();
        void OnStayInWater(float waterHeight, float buoyancyForce, float dragForce);
    }
} 