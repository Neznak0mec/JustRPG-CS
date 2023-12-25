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
            User user = (User)(await dataBase.UserDb.Get(player.id!))!;
            users.Add(user);
        }

        if (battle.type == BattleType.arena)
            users = BattleArenaEnd(battle, users);
        else
            users = await BattleEnd(battle, users, dataBase);


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

    static List<User?> BattleArenaEnd(Battle battle,List<User?> users)
    {
        int loserIndex = users.IndexOf(users.First(x => x!.id == battle.players.First(warrior => warrior.stats.hp <= 0).id));
        int winnerIndex = users.IndexOf(users.First(x => x!.id == battle.players.First(x => x.stats.hp > 0).id));

        int mmrDifference = Math.Abs(users[loserIndex]!.mmr - users[winnerIndex]!.mmr);
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

        users[winnerIndex]!.mmr += transferPoints;
        battle.log += $"{battle.players[winnerIndex].name} получил {transferPoints} mmr\n";
        return users;
    }

    static async Task<List<User?>>  BattleEnd(Battle? battle,List<User?> users, DataBase dataBase)
    {
        switch (battle!.status)
        {
            case BattleStatus.playerWin:
            {
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

                    int exp = Random.Shared.Next(battle.enemies.First().lvl * 3, battle.enemies.First().lvl * 5) + Random.Shared.Next(0, 7 *  users[i]!.stats.luck);
                    int coins = battle.enemies.First().lvl * 3 + Random.Shared.Next(0, 7 * users[i]!.stats.luck);
                    users[i]!.exp += exp;
                    users[i]!.cash += coins;

                    battle.log += $"{battle.players[i].name} получил {exp} опыта и {coins} монет\n";
                }

                break;
            }
            case BattleStatus.playerDead:
            {
                for (int i = 0; i < battle.players.Length; i++)
                {
                    if (battle.players[i].stats.hp > 0)
                        continue;
                    int looseCash = Random.Shared.Next(0, users[i]!.cash / 5);
                    int looseExp = Random.Shared.Next(0, (int)users[i]!.exp / 5);
                    users[i]!.cash -= looseCash;
                    users[i]!.exp -= looseExp;
                    battle.log += $"{battle.players[i].name} потерял {looseExp} опыта и {looseCash} монет\n";

                    if (users[i]!.cash < 0)
                        users[i]!.cash = 0;
                    
                    if (users[i]!.exp < 0)
                        users[i]!.exp = 0;

                    string? dropedItem = null;
                    if (Random.Shared.Next(0, 100) < 20)
                    {
                        switch (Random.Shared.Next(0,6))
                        {
                            case 1:
                                dropedItem = users[i]!.equipment.helmet;
                                users[i]!.equipment.helmet = null;
                                break;
                            case 2:
                                dropedItem = users[i]!.equipment.armor;
                                users[i]!.equipment.armor = null;
                                break;
                            case 3:
                                dropedItem = users[i]!.equipment.gloves;
                                users[i]!.equipment.gloves = null;
                                break;
                            case 4:
                                dropedItem = users[i]!.equipment.weapon;
                                users[i]!.equipment.weapon = null;
                                break;
                            case 5:
                                dropedItem = users[i]!.equipment.pants;
                                users[i]!.equipment.pants = null;
                                break;
                            case 6:
                                dropedItem = users[i]!.equipment.shoes;
                                users[i]!.equipment.shoes = null;
                                break;
                        }
                    }

                    if (dropedItem == null) continue;
                    Item item = (Item)(await dataBase.ItemDb.Get(dropedItem))!;
                    battle.log += $"{battle.players[i].name} потерял {item.name}\n";
                }

                break;
            }
            case BattleStatus.playerRun:
                for (int i = 0; i < battle.players.Length; i++)
                {
                    if (battle.players[i].stats.hp < 0)
                    {
                        users[i]!.exp -= Random.Shared.Next(0, (int)users[i]!.exp / 10);;
                        if (users[i]!.exp < 0)
                            users[i]!.exp = 0;
                    }
                    else
                    {
                        string? dropedItem = null;
                        if (Random.Shared.Next(0, 100) < 20)
                        {
                            switch (Random.Shared.Next(0,6))
                            {
                                case 1:
                                    dropedItem = users[i]!.equipment.helmet;
                                    users[i]!.equipment.helmet = null;
                                    break;
                                case 2:
                                    dropedItem = users[i]!.equipment.armor;
                                    users[i]!.equipment.armor = null;
                                    break;
                                case 3:
                                    dropedItem = users[i]!.equipment.gloves;
                                    users[i]!.equipment.gloves = null;
                                    break;
                                case 4:
                                    dropedItem = users[i]!.equipment.weapon;
                                    users[i]!.equipment.weapon = null;
                                    break;
                                case 5:
                                    dropedItem = users[i]!.equipment.pants;
                                    users[i]!.equipment.pants = null;
                                    break;
                                case 6:
                                    dropedItem = users[i]!.equipment.shoes;
                                    users[i]!.equipment.shoes = null;
                                    break;
                            }
                        }

                        if (dropedItem == null) continue;
                        Item item = (Item)(await dataBase.ItemDb.Get(dropedItem))!;
                        battle.log += $"{battle.players[i].name} потерял {item.name}\n";
                    }
                }
                break;
        }

        return users;
    }

    public static async Task<Warrior> GenerateWarriorByUser(User user, string username, DataBase dataBase,
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

    private static async Task<List<Tuple<string, BattleStats>>> InventoryToBattleInventory(User user, DataBase dataBase)
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
        Tuple<string, string> monster = SecondaryFunctions.GetRandomKeyValuePair(location.monsters)!;
        Warrior mob = new Warrior
        {
            name = monster.Item1,
            fullName = monster.Item1,
            lvl = Random.Shared.Next(location.lvl, location.lvl + 4),
            url = monster.Item2
        };
        mob.stats = GenerateRandomStatsForMob(mob.lvl);


        return mob;
    }

    private static BattleStats GenerateRandomStatsForMob(int mobLvl)
    {
        Stats mobStats;
        if (mobLvl is >= 1 and <= 5)
        {
            mobStats = new Stats
            {
                damage = new Random().Next(5, 8) + new Random().Next(4,6) * mobLvl,
                defence =  new Random().Next(10, 15) + new Random().Next(7,10) * mobLvl,
                hp = new Random().Next(20, 35) + new Random().Next(7,10) * mobLvl,
                speed =  new Random().Next(2, 5) +3 * mobLvl,
                luck = new Random().Next(2, 5) +3 * mobLvl
            };
        }
        else
        {
            mobStats = new Stats
            {
                damage = (6 + new Random().Next(10, 14)) * mobLvl,
                defence = (7 + new Random().Next(7, 9)) * mobLvl,
                hp = (18 + new Random().Next(20, 30)) * mobLvl,
                speed = (2 + new Random().Next(2, 5)) * mobLvl,
                luck = (3 + new Random().Next(2, 6)) * mobLvl
            };
        }

        return new BattleStats(mobStats);
    }


    private static int GetRandomNumberForDrop()
    {
        int probability = Random.Shared.Next(0, 100);
        return probability switch
        {
            <= 70 => 0,
            <= 95 => 1,
            _ => 2
        };
    }

    private static int GenEquipmentItemRarity()
    {
        int[] weights = { 50, 25, 15, 7, 3 };
        int totalWheight = 100;

        int randomNumber = Random.Shared.Next(0, totalWheight);
        int currentwheight = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            currentwheight += weights[i];
            if (randomNumber <= currentwheight)
                return i + 1;
        }

        return 0;
    }

    private static Item GenerateEquipmentItem(string type, int lvl, string name)
    {
        Stats stats = new Stats();

        int rarity = GenEquipmentItemRarity();
        stats = type switch
        {
            "armor" => new Stats()
            {
                damage = (new Random().Next(1, 5)) * lvl * rarity,
                defence = (5 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (25 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (1 + new Random().Next(0, 3)) * lvl * rarity,
                luck = (new Random().Next(0, 2)) * lvl * rarity
            },
            "weapon" => new Stats()
            {
                damage = (4 + new Random().Next(1, 5)) * lvl * rarity,
                defence = (1 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (2 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (3 + new Random().Next(0, 3)) * lvl * rarity,
                luck = (2 + new Random().Next(0, 2)) * lvl * rarity
            },
            "gloves" => new Stats()
            {
                damage = (2 + new Random().Next(1, 5)) * lvl * rarity,
                defence = (3 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (5 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (new Random().Next(0, 3)) * lvl * rarity,
                luck = (5 + new Random().Next(0, 2)) * lvl * rarity
            },
            "pants" => new Stats()
            {
                damage = (new Random().Next(1, 5)) * lvl * rarity,
                defence = (7 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (10 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (1 + new Random().Next(0, 3)) * lvl * rarity,
                luck = (new Random().Next(0, 2)) * lvl * rarity
            },
            "shoes" => new Stats()
            {
                damage = (1 + new Random().Next(1, 5)) * lvl * rarity,
                defence = (10 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (5 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (4 + new Random().Next(0, 3)) * lvl * rarity,
                luck = (new Random().Next(0, 2)) * lvl * rarity
            },
            "helmet" => new Stats()
            {
                damage = (new Random().Next(1, 5)) * lvl * rarity,
                defence = (20 + new Random().Next(3, 7)) * lvl * rarity,
                hp = (10 + new Random().Next(6, 10)) * lvl * rarity,
                speed = (new Random().Next(0, 3)) * lvl * rarity,
                luck = (new Random().Next(0, 2)) * lvl * rarity
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