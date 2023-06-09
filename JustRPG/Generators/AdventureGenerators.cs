using JustRPG.Models;
using JustRPG.Models.SubClasses;
using JustRPG.Services;

namespace JustRPG.Generators;

public static class AdventureGenerators
{
    public static async Task Reward(Battle? battle, DataBase dataBase)
    {
        List<User?> users = new List<User?>();
        foreach (var player in battle!.players)
        {
            User? user = (User) (await dataBase.UserDb.Get(player.id!))!;
            users.Add(user);
        }
        
        //todo:  reward players who alive
        foreach (var player in battle.players.Where(x =>x.stats.hp >0))
        {
            
        }
            
                
        // restore Heal poition
        for (int i = 0; i < battle.players.Length; i++)
        {
            List<String> inventory = users[i]!.inventory.ToList();
            foreach (var item in battle.players[i].inventory)
            {
                if (inventory.Count >=30)
                    break;
                inventory.Add(item.Item1);
            }
            users[i]!.inventory = inventory.ToArray();
        }
                
        foreach (var user in users)
        {
            await dataBase.UserDb.Update(user);
        }
    }
    
    public static async Task<Warrior> GenerateWarriorByUser(User user,string username, DataBase dataBase, string? avatarUrl = "") => new()
    {
        id = user.id,
        name = username,
        stats = await new BattleStats().BattleStatsAsync(user,dataBase),
        inventory = await InventoryToBattleInventory(user, dataBase),
        lvl = user.lvl,
        url = avatarUrl ?? ""
    };
    
    public static async Task<List<Tuple<string, BattleStats>>> InventoryToBattleInventory(User user, DataBase dataBase)
    {
        // todo: update to new system on add skills

        List<Tuple<string, BattleStats>> res = new List<Tuple<string, BattleStats>>();
        List<string> inventory = user.inventory.ToList();
        for (int i = 0; i < inventory.Count; i++)
        {

            if (inventory[i] == "fb75ff73-1116-4e95-ae46-8075c4e9a782")
            {
                res.Add(new Tuple<string, BattleStats>(inventory[i], new BattleStats()
                {
                    hp = user.stats.hp / 4,
                    damage = 0,
                    defence = 0,
                    luck = 0,
                    MaxDef = 0,
                    MaxHP = 0,
                    speed = 0
                }));
                inventory.RemoveAt(i);
                i--;
            }
        }

        user.inventory = inventory.ToArray();
        await dataBase.UserDb.Update(user);
        return res;
    }
    
    
    public static Warrior GenerateMob(Location location, BattleStats player)
    {
        Warrior mob = new Warrior();
        mob.lvl = Random.Shared.Next(location.lvl,location.lvl+4);
        var names = location.monsters.Keys;
        mob.name = names.ToArray()[Random.Shared.Next(0, names.Count)];
        mob.stats = GenerateRandomStats(player, mob.lvl);
        mob.url = location.monsters[mob.name];
        return mob;
    }
    
    public static BattleStats GenerateRandomStats(BattleStats player, int lvl)
    {

        BattleStats mobStats = new BattleStats
        {
            damage = 1,
            defence = 1,
            hp = 100,
            speed = 1,
            luck = 1,
            MaxHP = 100,
            MaxDef = 1
        };

        return mobStats;
    }
}