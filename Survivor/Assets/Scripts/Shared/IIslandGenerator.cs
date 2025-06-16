using UnityEngine;

namespace Survivor.Shared
{
    public interface IIslandGenerator
    {
        void GenerateIsland();
        bool IsGenerationComplete();
        Vector3 GetIslandCenter();
        float GetIslandSize();
        float IslandRadius { get; }
        Vector3? GetCampPosition();
    }
} 