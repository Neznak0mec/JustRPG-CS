using JustRPG.Features;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Models.SubClasses;
using JustRPG.Services;

namespace JustRPG.Generators;

public class BattleGenerators
{
     public static async Task<Warrior> GenerateWarriorFromUser(User user, string username, DataBase dataBase,
        string? avatarUrl = "") => new()
    {
        id = user.id,
        name = username,
        fullName = await user.GetFullName(username,dataBase),
        stats = await new BattleStats().BattleStatsAsync(user, dataBase),
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
                    hp = user.stats.hp *0.33,
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

        user.inventory = inventory;
        await dataBase.UserDb.Update(user);
        return res;
    }


    public static Warrior GenerateMob(Location location)
    {
        Tuple<string, string> monster = SecondaryFunctions.GetRandomKeyValuePair(location.monsters)!;

        int mobLvl;
        int probability = Random.Shared.Next(0, 100);
        mobLvl = probability switch
        {
            <= 50 => location.lvl,
            <= 70 => location.lvl + 1,
            <= 85 => location.lvl + 2,
            <= 95 => location.lvl + 3,
            _ => location.lvl + 4
        };
        
        Warrior mob = new Warrior
        {
            name = monster.Item1,
            fullName = monster.Item1,
            lvl = mobLvl,
            url = monster.Item2
        };
        mob.stats = GenerateRandomStatsForMob(mob.lvl);


        return mob;
    }

    public static BattleStats GenerateRandomStatsForMob(int mobLvl)
    {
        Stats mobStats = mobLvl switch
        {
            >= 1 and <= 4 => new Stats
            {
                damage = new Random().Next(5, 8) + new Random().Next(4, 6) * mobLvl,
                defence = new Random().Next(10, 15) + new Random().Next(7, 10) * mobLvl,
                hp = new Random().Next(20, 35) + new Random().Next(7, 10) * mobLvl,
                speed = new Random().Next(2, 5) + 3 * mobLvl,
                luck = new Random().Next(2, 5) + 3 * mobLvl
            },
            <= 9 => new Stats
            {
                damage = new Random().Next(6, 15) * mobLvl,
                defence = new Random().Next(10, 20) * mobLvl,
                hp = new Random().Next(20, 35) + new Random().Next(7, 10) * mobLvl,
                speed = new Random().Next(3, 10) * mobLvl,
                luck = new Random().Next(3, 10) * mobLvl
            },
            _ => new Stats
            {
                damage = (int)(Random.Shared.Next(25, 41) * mobLvl * 1.5 - 119),
                defence = Random.Shared.Next(100, 144) * mobLvl - 346,
                hp = Random.Shared.Next(100, 132) * mobLvl - 248,
                speed = Random.Shared.Next(20, 28) * mobLvl - 67,
                luck = Random.Shared.Next(18, 26) * mobLvl - 59
            }
        };

        return new BattleStats(mobStats);
    }


    public static int GetRandomNumberForDrop()
    {
        int probability = Random.Shared.Next(0, 100);
        return probability switch
        {
            <= 70 => 0,
            <= 95 => 1,
            _ => 2
        };
    }

    public static List<int> GenEquipmentItemsRarity(int playerLvl, int moblvl)
    {
        List<int> dropList = new List<int>();


        int levelDifferenceBonus = (moblvl - playerLvl)*2;
        
        int noItemChance = 30 + levelDifferenceBonus; 
        int[] rarityChances = { 40, 15, 10, 4, 1 };

        int numberOfItems = Random.Shared.Next(1, 5);
        for (int i = 0; i < numberOfItems; i++)
        {
            int totalChance = Random.Shared.Next(0, 100);

            if (totalChance < noItemChance)
            {
                dropList.Add(0);
            }
            else
            {
                int rarity = 0;
                totalChance -= noItemChance;

                for (int j = 0; j < rarityChances.Length; j++)
                {
                    if (totalChance < rarityChances[j])
                    {
                        rarity = j + 1;
                        break;
                    }

                    totalChance -= rarityChances[j];
                }

                dropList.Add(rarity);
            }

            noItemChance -= 25;
        }

        dropList.RemoveAll(x => x == 0);
        return dropList;
    }

    public static Item GenerateEquipmentItem(string type, int lvl, string name, int rarity)
    {
        Stats stats = new Stats();

        stats = type switch
        {
            "armor" => new Stats()
            {
                damage = (new Random().Next(1, 5)) * lvl * rarity,
                defence = (5 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (10 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (1 + new Random().Next(0, 3)) * lvl * rarity,
                luck = (1+new Random().Next(0, 2)) * lvl * rarity
            },
            "weapon" => new Stats()
            {
                damage = (4 + new Random().Next(1, 5)) * lvl * rarity,
                defence = (1 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (new Random().Next(2, 5)) * lvl * rarity,
                speed = (1+new Random().Next(0, 3)) * lvl * rarity,
                luck = (1+new Random().Next(0, 2)) * lvl * rarity
            },
            "gloves" => new Stats()
            {
                damage = (2 + new Random().Next(1, 5)) * lvl * rarity,
                defence = (3 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (new Random().Next(6, 10)) * lvl * rarity,
                speed = (1+new Random().Next(0, 3)) * lvl * rarity,
                luck = (5 + new Random().Next(0, 2)) * lvl * rarity
            },
            "pants" => new Stats()
            {
                damage = (new Random().Next(1, 5)) * lvl * rarity,
                defence = (7 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (new Random().Next(6, 10)) * lvl * rarity,
                speed = (1 + new Random().Next(0, 3)) * lvl * rarity,
                luck = (1+new Random().Next(0, 2)) * lvl * rarity
            },
            "shoes" => new Stats()
            {
                damage = ( new Random().Next(1, 5)) * lvl * rarity,
                defence = (10 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (new Random().Next(6, 10)) * lvl * rarity,
                speed = (4 + new Random().Next(0, 3)) * lvl * rarity,
                luck = (1+new Random().Next(0, 2)) * lvl * rarity
            },
            "helmet" => new Stats()
            {
                damage = (new Random().Next(1, 5)) * lvl * rarity,
                defence = (5 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (5 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (1+new Random().Next(0, 3)) * lvl * rarity,
                luck = (1+new Random().Next(0, 2)) * lvl * rarity
            },
            _ => stats
        };

        Item item = new Item
        {
            id = Guid.NewGuid().ToString(),
            lvl = lvl, name = name,
            type = (ItemType)Enum.Parse(typeof(ItemType), type),
            giveStats = stats,
            rarity = (Rarity)rarity,
            generated = true
        };
        return item;
    }
}