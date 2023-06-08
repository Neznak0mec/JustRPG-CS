using JustRPG_CS.Models;
using MongoDB.Driver;

namespace JustRPG_CS.Services.Collections;

public class ArenaDB
{
    private readonly IMongoCollection<PVP> _pvpCollection;

    private List<FindPVP> _findPvps;
    private List<PVP> _pvps;

    public ArenaDB(IMongoDatabase mongoDatabase)
    {
        _findPvps = new List<FindPVP>();
        _pvps = new List<PVP>();
    }
    
    public void AppFindPVP(FindPVP findPvp) => _findPvps.Add(findPvp);

    public void DeletFindPVP(long userId) =>  _findPvps = _findPvps.Where(x => x.userId != userId).ToList();

    public int CountOfFinfPVP() => _findPvps.Count;

    public List<FindPVP> GetAllFindPVP() => _findPvps;

    
    
    public PVP GetPVP(string val)
    {
        return _pvps.Where(x => x.battleId == val).ToList()[0];
    }

    public void AddPVP(PVP obj)
    {
        _pvps.Add(obj);
    }

    public void RemovePVP(PVP obj)
    {
        _pvps.Remove(obj);
    }
    
    public void UpdatePVP(PVP obj)
    {
        var temp = GetPVP(obj.battleId);
        _pvps.Remove(temp);
        temp.lastInteraction = DateTimeOffset.Now.ToUnixTimeSeconds();
        AddPVP(temp);
    }
    
}