using JustRPG.Interfaces;
using JustRPG.Models;

namespace JustRPG.Services.Collections;

public class InventoryDB
{
    private readonly LocalCache<Inventory> _cache;
    private readonly DataBase _dataBase;

    public InventoryDB(DataBase dataBase)
    {
        _cache = new LocalCache<Inventory>();
        _dataBase = dataBase;
    }

    public async Task<Inventory?> Get(string id)
    {
        Inventory temp;
        if (_cache.Exists(id))
        {
            temp = _cache.Get(id)!;
            if (temp.itemLvl != null)
            {
                temp.Items = temp.Items
                    .Where(item => item.lvl >= temp.itemLvl.Item1 && item.lvl <= temp.itemLvl.Item2)
                    .ToList();
            }

            if (temp.itemRarity != null)
            {
                temp.Items = temp.Items
                    .Where(item => item.rarity == temp.itemRarity.Value)
                    .ToList();
            }

            if (temp.itemType != null)
            {
                temp.Items = temp.Items
                    .Where(item => item.type == temp.itemType.Value)
                    .ToList();
            }

            return temp;
        }


        User user = (User)(await _dataBase.UserDb.Get(id.Split('_')[2]))!;
        temp = new Inventory
        {
            id = id,
            userId = user.id
        };
        await temp.Reload(_dataBase);

        _cache.Add(temp.id, temp);

        TimeSpan aTimeSpan = new TimeSpan(0, 0, 5, 0);
        if (((DateTimeOffset)temp!.lastUsage).ToUnixTimeSeconds() <
            DateTimeOffset.Now.Subtract(aTimeSpan).ToUnixTimeSeconds())
        {
            temp = await CreateObject(temp);
        }

        return temp;
    }

    public async Task<Inventory> CreateObject(Inventory inventory)
    {
        if (_cache.Exists(inventory.id!))
        {
            _cache.Remove(inventory.id!);
        }

        _cache.Add(inventory.id!, inventory);

        string userid = inventory.id!.Split('_')[2];
        User user = (User)(await _dataBase.UserDb.Get(userid))!;
        await inventory.Reload(_dataBase);

        return inventory;
    }

    public void Update(Inventory inventory)
    {
        _cache.Add(inventory.id!, inventory);
    }

    public void ClearCache()
    {
        List<Inventory?> temp = _dataBase.InventoryDb.GetList();
        foreach (var inventory in temp)
        {
            if (inventory == null)
                continue;
            TimeSpan aTimeSpan = new TimeSpan(0, 0, 5, 0);
            if (((DateTimeOffset)inventory.lastUsage).ToUnixTimeSeconds() <
                DateTimeOffset.Now.Subtract(aTimeSpan).ToUnixTimeSeconds())
            {
                _cache.Remove(inventory.id);
            }
        }
    }

    private List<Inventory?> GetList()
    {
        return _cache.GetAll();
    }
}