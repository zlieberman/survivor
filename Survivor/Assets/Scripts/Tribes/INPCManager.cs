using System.Collections.Generic;
using Survivor.Characters;

namespace Survivor.Tribes
{
    public interface INPCManager
    {
        Character GetTribeMember(string memberName);
        List<Character> GetAllTribeMembers();
        void AddTribeMember(Character member);
        void RemoveTribeMember(string memberName);
        List<string> GetActiveNPCs();
    }
} 