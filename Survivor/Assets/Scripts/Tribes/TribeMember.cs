using UnityEngine;
using System.Collections.Generic;

namespace Survivor.Tribes
{
    [System.Serializable]
    public class TribeMemberStats
    {
        public float perception;
        public float deception;
        public float persuasion;
        public float puzzleSolving;
        public float swimming;
        public float speed;
        public float strength;
        public float agility;
        public float intelligence;
        public float stamina;
        public float charisma;
        public float honesty;
        public float trust;
        public float honor;
    }

    public class TribeMember : MonoBehaviour
    {
        [Header("Member Properties")]
        public string memberName;
        public string tribeName;
        public bool IsPlayer;
        public TribeMemberStats stats;
        public bool IsInChallenge = false;
        public Dictionary<string, float> Relationships = new Dictionary<string, float>();

        private void Awake()
        {
            // Initialize relationships dictionary if needed
            if (Relationships == null)
            {
                Relationships = new Dictionary<string, float>();
            }
        }

        public void Initialize(string name, string tribe, bool isPlayer, int id)
        {
            memberName = name;
            tribeName = tribe;
            IsPlayer = isPlayer;
            
            // Initialize stats if they don't exist
            if (stats == null)
            {
                stats = new TribeMemberStats();
            }
        }

        public void UpdateRelationship(string otherMemberName, float delta)
        {
            if (!Relationships.ContainsKey(otherMemberName))
            {
                Relationships[otherMemberName] = 0f;
            }
            Relationships[otherMemberName] = Mathf.Clamp(Relationships[otherMemberName] + delta, -1f, 1f);
        }

        public float GetRelationship(string otherMemberName)
        {
            return Relationships.ContainsKey(otherMemberName) ? Relationships[otherMemberName] : 0f;
        }
    }
} 