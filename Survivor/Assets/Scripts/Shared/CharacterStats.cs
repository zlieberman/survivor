using UnityEngine;

namespace Survivor.Shared
{
    [System.Serializable]
    public class CharacterStats
    {
        [Range(0, 100)]
        public float perception;
        [Range(0, 100)]
        public float deception;
        [Range(0, 100)]
        public float persuasion;
        [Range(0, 100)]
        public float puzzleSolving;
        [Range(0, 100)]
        public float swimming;
        [Range(0, 100)]
        public float speed;
        [Range(0, 100)]
        public float strength;
        [Range(0, 100)]
        public float agility;
        [Range(0, 100)]
        public float intelligence;
        [Range(0, 100)]
        public float stamina;
        [Range(0, 100)]
        public float charisma;
        [Range(0, 100)]
        public float honesty;
        [Range(0, 100)]
        public float trust;
        [Range(0, 100)]
        public float honor;
    }
} 