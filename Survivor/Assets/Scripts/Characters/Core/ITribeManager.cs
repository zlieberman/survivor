using System.Collections.Generic;
using UnityEngine;

namespace Survivor.Characters
{
    public interface ITribeManager
    {
        // Core tribe management
        List<Character> GetTribeMembers(string tribeName);
        Character GetPlayer();
        void EliminateMember(Character member);
        IEnumerable<string> GetActiveNPCs();
        Character GetNPCData(string npcName);
        void UpdateRelationship(string npcName, string targetName, float delta);

        // Challenge system integration
        bool CanStartChallenge(Vector3 position);
        void StartChallenge(List<Character> participants);
        void EliminateParticipant(Character member);
        bool IsParticipantInChallenge(Character member);
        List<Character> GetActiveParticipants();
    }
} 