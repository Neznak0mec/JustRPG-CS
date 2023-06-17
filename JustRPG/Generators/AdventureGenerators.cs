using JustRPG.Features;
using JustRPG.Models;
using JustRPG.Models.Enums;
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
        
        if (battle.type == "arena")
        {
            int loserIndex = users.IndexOf(users.First(x=> x!.id == battle.players.First(x =>x.stats.hp <=0).id));
            int winerIndex = users.IndexOf(users.First(x=> x!.id == battle.players.First(x =>x.stats.hp > 0).id));

            int mmrDifference = Math.Abs(users[loserIndex]!.mmr - users[winerIndex]!.mmr);
            int transferPoints = Math.Max(mmrDifference / 2, 5);

            if (users[loserIndex]!.mmr < 5)
            {
                battle.log += $"${battle.players[loserIndex].name} не потерял mmr\n";
            }
            else
            {
                users[loserIndex]!.mmr -= transferPoints;
                battle.log += $"${battle.players[loserIndex].name} потерял {transferPoints} mmr\n";
            }

            users[winerIndex]!.mmr += transferPoints;
            battle.log += $"${battle.players[winerIndex].name} получил {transferPoints} mmr\n";
        }
        else
        {

            //todo:  reward players who alive
             for (int i = 0; i < battle.players.Length; i++)
            {
                if (battle.players[i].stats.hp <= 0 )
                    continue;

                List<String> inventory = users[i]!.inventory.ToList();


                int countOfDropedItem = (battle.type == "dungeon" ? 1 : 0);
                countOfDropedItem += GetRandomNumberForDrop();
                for (int j = 0; j < countOfDropedItem; j++)
                {
                    Tuple<string,string>? itemname = SecondaryFunctions.GetRandomKeyValuePair(battle.drop);
                    if (itemname == null)
                        break;
                    Item item = GenerateEquipmentItem(itemname.Item1, Random.Shared.Next(battle.enemies.First().lvl,battle.enemies.First().lvl+2),itemname.Item2);
                    inventory.Add(item.id);

                    await dataBase.ItemDb.CreateObject(item);
                    battle.log+= $"{battle.players[i]} получил {item.name} | {item.lvl}\n";
                }



                users[i]!.inventory = inventory.ToArray();
            }
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



    public static int GetRandomNumberForDrop()
    {
        Random random = new Random();
        double probability = random.NextDouble();

        if (probability <= 0.5)
            return 0;
        else if (probability <= 0.7)
            return 1;
        else if (probability <= 85)
            return 2;
        else if (probability <= 99)
            return 3;
        else
            return 4;
    }

    public static int GenEquipmentItemRarity()
    {
        int[] weights = { 50, 25, 15, 7, 3 };
        int totalWheight = 100;

        int randomNumber = new Random().Next(0, totalWheight);
        int currentwheight = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            currentwheight += weights[i];
            if (randomNumber <= currentwheight)
                return i;
        }

        return 0;
    }

    public static Item GenerateEquipmentItem(string type, int lvl, string name)
    {
        Stats stats = new Stats();

        int rarity = GenEquipmentItemRarity();

        if (true)
        {
            stats = new Stats()
            {
                damage = 1,
                defence = 1,
                hp = 100,
                speed = 1,
                luck = 1
            };
        }
        else
        {
            stats = type switch
            {
                "armor" => new Stats()
                {
                    damage = 1,
                    defence = 1,
                    hp = 100,
                    speed = 1,
                    luck = 1
                },
                "weapon" => new Stats()
                {
                    damage = 1,
                    defence = 1,
                    hp = 100,
                    speed = 1,
                    luck = 1
                },
                "gloves" => new Stats()
                {
                    damage = 1,
                    defence = 1,
                    hp = 100,
                    speed = 1,
                    luck = 1
                },
                "pants" => new Stats()
                {
                    damage = 1,
                    defence = 1,
                    hp = 100,
                    speed = 1,
                    luck = 1
                },
                "shoes" => new Stats()
                {
                    damage = 1,
                    defence = 1,
                    hp = 100,
                    speed = 1,
                    luck = 1
                },
                "helmet" => new Stats()
                {
                    damage = 1,
                    defence = 1,
                    hp = 100,
                    speed = 1,
                    luck = 1
                },
                _ => stats
            };
        }

        Item item = new Item()
        {
            id = Guid.NewGuid().ToString(),
            name = name,
            lvl = lvl,
            generated = true,
            giveStats = stats,
            rarity = Enum.GetName(typeof(Rarity), rarity)!,
            type = type
        };
        return item;
    }
}