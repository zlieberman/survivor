using System.Collections.Generic;
using UnityEngine;
using Survivor.Tribes;

namespace Survivor.Tribes
{
    public interface ITribeManager
    {
        // Core tribe management
        List<TribeMember> GetTribeMembers(string tribeName);
        TribeMember GetPlayer();
        void EliminateMember(TribeMember member);
        IEnumerable<string> GetActiveNPCs();
        TribeMember GetNPCData(string npcName);
        void UpdateRelationship(string npcName, string targetName, float delta);

        // Challenge system integration
        bool CanStartChallenge(Vector3 position);
        void StartChallenge(List<TribeMember> participants);
        void EliminateParticipant(TribeMember member);
        bool IsParticipantInChallenge(TribeMember member);
        List<TribeMember> GetActiveParticipants();
    }
} 