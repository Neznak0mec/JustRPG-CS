using JustRPG.Models;
using MongoDB.Driver;

namespace JustRPG.Services.Collections;

public class ArenaDB
{
    private LocalCache<FindPVP> _findPvps;

    public ArenaDB(IMongoDatabase mongoDatabase)
    {
        _findPvps = new LocalCache<FindPVP>();
    }

    public void AppFindPVP(FindPVP findPvp) => _findPvps.Add(findPvp.userId.ToString(),findPvp);

    public void DeletFindPVP(long userId)
    {
        _findPvps.Remove(userId.ToString());
    }

    public int CountOfFinfPVP() => _findPvps.GetAll().Count;

    public List<FindPVP> GetAllFindPVP() => _findPvps.GetAll();
    
    public bool IsFindPVP(long userId) => _findPvps.Any(x => x.userId == userId);
   
    public FindPVP? Get(long userId) => _findPvps.Get(userId.ToString());
    
    public void Update(FindPVP findPvp)
    {
        _findPvps.Add(findPvp.userId.ToString(),findPvp);
    }
}