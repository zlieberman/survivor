using UnityEngine;

namespace Survivor.Core
{
    [System.Serializable]
    public class NPCStats
    {
        [Header("Physical Attributes")]
        public float strength = 50f;      // Physical power and endurance
        public float speed = 50f;         // Movement and reaction speed
        public float stamina = 50f;       // Energy and recovery rate

        [Header("Mental Attributes")]
        public float intelligence = 50f;  // Problem-solving and learning
        public float charisma = 50f;      // Social influence and persuasion
        public float leadership = 50f;    // Team management and decision making

        [Header("Survival Skills")]
        public float survival = 50f;      // General survival knowledge
        public float hunting = 50f;       // Hunting and gathering ability
        public float crafting = 50f;      // Building and crafting skills

        public void InitializeRandomStats()
        {
            strength = Random.Range(30f, 80f);
            speed = Random.Range(30f, 80f);
            stamina = Random.Range(30f, 80f);
            intelligence = Random.Range(30f, 80f);
            charisma = Random.Range(30f, 80f);
            leadership = Random.Range(30f, 80f);
            survival = Random.Range(30f, 80f);
            hunting = Random.Range(30f, 80f);
            crafting = Random.Range(30f, 80f);
        }

        public void ModifyStat(string statName, float amount)
        {
            switch (statName.ToLower())
            {
                case "strength":
                    strength = Mathf.Clamp(strength + amount, 0f, 100f);
                    break;
                case "speed":
                    speed = Mathf.Clamp(speed + amount, 0f, 100f);
                    break;
                case "stamina":
                    stamina = Mathf.Clamp(stamina + amount, 0f, 100f);
                    break;
                case "intelligence":
                    intelligence = Mathf.Clamp(intelligence + amount, 0f, 100f);
                    break;
                case "charisma":
                    charisma = Mathf.Clamp(charisma + amount, 0f, 100f);
                    break;
                case "leadership":
                    leadership = Mathf.Clamp(leadership + amount, 0f, 100f);
                    break;
                case "survival":
                    survival = Mathf.Clamp(survival + amount, 0f, 100f);
                    break;
                case "hunting":
                    hunting = Mathf.Clamp(hunting + amount, 0f, 100f);
                    break;
                case "crafting":
                    crafting = Mathf.Clamp(crafting + amount, 0f, 100f);
                    break;
            }
        }
    }
} 