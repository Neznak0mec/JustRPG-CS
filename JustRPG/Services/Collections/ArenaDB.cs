using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class ArenaDB
{
    private List<FindPVP> _findPvps;

    public ArenaDB(IMongoDatabase mongoDatabase)
    {
        _findPvps = new List<FindPVP>();
    }

    public void AppFindPVP(FindPVP findPvp) => _findPvps.Add(findPvp);

    public void DeletFindPVP(long userId)
    {
        _findPvps.RemoveAll(x => x.userId == userId);
    }

    public int CountOfFinfPVP() => _findPvps.Count;

    public List<FindPVP> GetAllFindPVP() => _findPvps;
    
    public bool IsFindPVP(long userId) => _findPvps.Any(x => x.userId == userId);
   
    public FindPVP? Get(long userId) => _findPvps.FirstOrDefault(x => x.userId == userId);
    
    public void Update(FindPVP findPvp)
    {
        var index = _findPvps.FindIndex(x => x.userId == findPvp.userId);
        _findPvps[index] = findPvp;
    }
}