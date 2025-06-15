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

        private NameTag nameTag;

        private void Awake()
        {
            // Initialize relationships dictionary if needed
            if (Relationships == null)
            {
                Relationships = new Dictionary<string, float>();
            }

            // Create name tag
            CreateNameTag();
        }

        private void CreateNameTag()
        {
            // Create name tag object
            GameObject nameTagObj = new GameObject("NameTag");
            nameTagObj.transform.SetParent(transform);
            nameTagObj.transform.localPosition = Vector3.zero;
            nameTagObj.transform.localRotation = Quaternion.identity;
            nameTag = nameTagObj.AddComponent<NameTag>();

            // If we already have a name, set it
            if (!string.IsNullOrEmpty(memberName))
            {
                nameTag.SetName(memberName);
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

            // Set the name tag
            if (nameTag != null)
            {
                nameTag.SetName(memberName);
            }
            else
            {
                CreateNameTag();
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