using UnityEngine;

namespace Survivor.Environment
{
    /// <summary>
    /// Interface for underwater visual effects
    /// </summary>
    public interface IUnderwaterEffect
    {
        Color underwaterTint { get; set; }
        float underwaterBlur { get; set; }
        float underwaterDistortion { get; set; }
        float transitionSpeed { get; set; }
        float effectIntensity { get; set; }
        
        void SetSubmersionLevel(float submersionLevel);
    }
} 