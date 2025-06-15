using System.Collections.Generic;

namespace Survivor.Tribes
{
    public interface INPCManager
    {
        TribeMember GetTribeMember(string memberName);
        List<TribeMember> GetAllTribeMembers();
        void AddTribeMember(TribeMember member);
        void RemoveTribeMember(string memberName);
        List<string> GetActiveNPCs();
    }
} 