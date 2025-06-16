using System.Collections.Generic;

namespace Survivor.Characters
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