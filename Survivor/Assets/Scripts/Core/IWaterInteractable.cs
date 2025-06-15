using UnityEngine;

namespace Survivor.Core
{
    public interface IWaterInteractable
    {
        void OnEnterWater(float waterHeight);
        void OnExitWater();
        void OnStayInWater(float waterHeight, float buoyancyForce, float dragForce);
    }
} 