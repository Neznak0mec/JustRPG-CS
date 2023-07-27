using JustRPG.Features;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Models.SubClasses;
using JustRPG.Services;
using Serilog;

namespace JustRPG.Generators;

public static class AdventureGenerators
{
    public static async Task Reward(Battle? battle, DataBase dataBase)
    {
        List<User?> users = new List<User?>();
        foreach (var player in battle!.players)
        {
            User? user = (User)(await dataBase.UserDb.Get(player.id!))!;
            users.Add(user);
        }

        if (battle.type == BattleType.arena)
        {
            int loserIndex = users.IndexOf(users.First(x => x!.id == battle.players.First(x => x.stats.hp <= 0).id));
            int winerIndex = users.IndexOf(users.First(x => x!.id == battle.players.First(x => x.stats.hp > 0).id));

            int mmrDifference = Math.Abs(users[loserIndex]!.mmr - users[winerIndex]!.mmr);
            int transferPoints = Math.Max(mmrDifference / 2, 5);

            if (users[loserIndex]!.mmr < 5)
            {
                battle.log += $"{battle.players[loserIndex].name} не потерял mmr\n";
            }
            else
            {
                users[loserIndex]!.mmr -= transferPoints;
                battle.log += $"{battle.players[loserIndex].name} потерял {transferPoints} mmr\n";
            }

            users[winerIndex]!.mmr += transferPoints;
            battle.log += $"{battle.players[winerIndex].name} получил {transferPoints} mmr\n";
        }
        else
        {
            //todo:  reward players who alive
            for (int i = 0; i < battle.players.Length; i++)
            {
                if (battle.players[i].stats.hp <= 0)
                    continue;

                int countOfDroppedItem = (battle.type == BattleType.dungeon ? 1 : 0);
                countOfDroppedItem += GetRandomNumberForDrop();
                for (int j = 0; j < countOfDroppedItem; j++)
                {
                    Tuple<string, string>? itemName = SecondaryFunctions.GetRandomKeyValuePair(battle.drop);
                    if (itemName == null)
                        break;
                    Item item = GenerateEquipmentItem(itemName.Item1,
                        battle.enemies.First().lvl, itemName.Item2);
                    users[i]!.inventory.Add(item.id);

                    await dataBase.ItemDb.CreateObject(item);
                    battle.log += $"{battle.players[i].name} получил {item.name} | {item.lvl}\n";
                }

            }
        }


        // restore Heal poition
        for (int i = 0; i < battle.players.Length; i++)
        {
            foreach (var item in battle.players[i].inventory)
            {
                if (users[i]!.inventory.Count >= 30)
                    break;
                users[i]!.inventory.Add(item.Item1);
            }
        }

        foreach (var user in users)
        {
            await dataBase.UserDb.Update(user);
        }
        await dataBase.BattlesDb.Delete(battle);
    }

    public static async Task<Warrior> GenerateWarriorByUser(User user, string username, DataBase dataBase,
        string? avatarUrl = "") => new()
    {
        id = user.id,
        name = username,
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

        user.inventory = inventory;
        await dataBase.UserDb.Update(user);
        return res;
    }


    public static Warrior GenerateMob(Location location)
    {
        Tuple<string,string> monster = SecondaryFunctions.GetRandomKeyValuePair(location.monsters)!;
        Warrior mob = new Warrior
        {
            name = monster.Item1,
            lvl = Random.Shared.Next(location.lvl, location.lvl + 4),
            url = monster.Item2
        };
        mob.stats = GenerateRandomStatsForMob(mob.lvl);


        return mob;
    }

    public static BattleStats GenerateRandomStatsForMob(int mobLvl)
    {
         Stats mobStats = new Stats()
        {
            damage = 15 + 5 * mobLvl,
            defence = 20 + 3 * mobLvl,
            hp = 50 + 10 * mobLvl,
            speed = 2 + 1 * mobLvl,
            luck = 3 + 1 * mobLvl
        };

         return new BattleStats(mobStats);
    }


    public static int GetRandomNumberForDrop()
    {
        int probability = Random.Shared.Next(0,100);
        if (probability <= 50)
            return 0;
        else if (probability <= 70)
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

        int randomNumber = Random.Shared.Next(0, totalWheight);
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
        if (false)
        {
//            stats = new Stats()
//            {
//                damage = 7 + dmgMltp * lvl,
//                defence = 10 + defMltp * lvl,
//                hp = 25 + hpMltp * lvl,
//                speed = 3 + spdMltp * lvl,
//                luck = 2 + luckMltp * lvl
//            };
        }
        else
        {
            stats = type switch
            {
                "armor" => new Stats()
                {
                    damage = 1 + new Random().Next(3, 7) * lvl,
                    defence = 10 + new Random().Next(5, 9) * lvl,
                    hp = 25 + new Random().Next(8, 12) * lvl,
                    speed = 3 + new Random().Next(1, 5) * lvl,
                    luck = 2 + new Random().Next(0, 4) * lvl
                },
                "weapon" => new Stats()
                {
                    damage = 1 + new Random().Next(3, 7) * lvl,
                    defence = 10 + new Random().Next(5, 9) * lvl,
                    hp = 25 + new Random().Next(8, 12) * lvl,
                    speed = 3 + new Random().Next(1, 5) * lvl,
                    luck = 2 + new Random().Next(0, 4) * lvl
                },
                "gloves" => new Stats()
                {
                    damage = 1 + new Random().Next(3, 7) * lvl,
                    defence = 10 + new Random().Next(5, 9) * lvl,
                    hp = 25 + new Random().Next(8, 12) * lvl,
                    speed = 3 + new Random().Next(1, 5) * lvl,
                    luck = 2 + new Random().Next(0, 4) * lvl
                },
                "pants" => new Stats()
                {
                    damage = 1 + new Random().Next(3, 7) * lvl,
                    defence = 10 + new Random().Next(5, 9) * lvl,
                    hp = 25 + new Random().Next(8, 12) * lvl,
                    speed = 3 + new Random().Next(1, 5) * lvl,
                    luck = 2 + new Random().Next(0, 4) * lvl
                },
                "shoes" => new Stats()
                {
                    damage = 1 + new Random().Next(3, 7) * lvl,
                    defence = 10 + new Random().Next(5, 9) * lvl,
                    hp = 25 + new Random().Next(8, 12) * lvl,
                    speed = 3 + new Random().Next(1, 5) * lvl,
                    luck = 2 + new Random().Next(0, 4) * lvl
                },
                "helmet" => new Stats()
                {
                    damage = 1 + new Random().Next(3, 7) * lvl,
                    defence = 10 + new Random().Next(5, 9) * lvl,
                    hp = 25 + new Random().Next(8, 12) * lvl,
                    speed = 3 + new Random().Next(1, 5) * lvl,
                    luck = 2 + new Random().Next(0, 4) * lvl
                },
                _ => stats
            };
        }

        Item item = new Item()
        {
            id = Guid.NewGuid().ToString(),
            lvl = lvl,name = name,
            type = type,
            giveStats = stats,
            rarity = (Rarity)rarity,
            generated = true
        };
        return item;
    }
}