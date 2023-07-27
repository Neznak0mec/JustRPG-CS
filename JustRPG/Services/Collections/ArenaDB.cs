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

    public void DeletFindPVP(long userId) => _findPvps = _findPvps.Where(x => x.userId != userId).ToList();

    public int CountOfFinfPVP() => _findPvps.Count;

    public List<FindPVP> GetAllFindPVP() => _findPvps;
   
}