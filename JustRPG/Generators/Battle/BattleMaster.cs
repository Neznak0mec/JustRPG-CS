using JustRPG.Features;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Models.SubClasses;
using JustRPG.Services;
using Serilog;

namespace JustRPG.Generators;

public static class BattleMaster
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
                if (users[i]!.inventory.Count(x => x == "fb75ff73-1116-4e95-ae46-8075c4e9a782") >= 5 || users[i]!.inventory.Count >= 30)
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
        int winnerIndex = users.IndexOf(users.First(x => x!.id == battle.players.First(warrior => warrior.stats.hp > 0).id));

        int mmrDifference = Math.Abs(users[loserIndex]!.mmr - users[winnerIndex]!.mmr);
        int transferPoints = Math.Max(mmrDifference / 2, 5);

        if (users[loserIndex]!.mmr < 5)
        {
            battle.log += $":heavy_equals_sign:`{battle.players[loserIndex].name}` не потерял mmr\n";
        }
        else
        {
            users[loserIndex]!.mmr -= transferPoints;
            battle.log += $":heavy_minus_sign:`{battle.players[loserIndex].name}` потерял `{transferPoints}` mmr\n";
        }

        users[winnerIndex]!.mmr += transferPoints;
        battle.log += $":heavy_plus_sign:`{battle.players[winnerIndex].name}` получил `{transferPoints}` mmr\n";
        return users;
    }

    private static async Task<List<User?>> BattleEnd(Battle? battle, List<User?> users, DataBase dataBase)
    {
        switch (battle!.status)
        {
            case BattleStatus.playerWin:
                return await HandlePlayerWin(battle, users, dataBase);
            case BattleStatus.playerDead:
                return await HandlePlayerDead(battle, users, dataBase);
            case BattleStatus.playerRun:
                return await HandlePlayerRun(battle, users, dataBase);
            default:
                return users;
        }
    }

    private static async Task<List<User?>> HandlePlayerWin(Battle battle, List<User?> users, DataBase dataBase)
    {
        List<BattleResultDrop> results = new List<BattleResultDrop>();
        
        for (int i = 0; i < battle.players.Length; i++)
        {
            if (battle.players[i].stats.hp <= 0)
                continue;
            
            List<(int,int)> drop =new List<(int, int)>() ;
            foreach (var enemy in battle.enemies)
            {
                var loot = BattleGenerators.GenEquipmentItemsRarity(battle.players[i].lvl, enemy.lvl);
                drop.AddRange(loot.Select(j => (j, enemy.lvl)));
            }

            if (battle.type == BattleType.dungeon)
            {
                drop = drop.Take(battle.enemies.Length*2 + 1).ToList();
                if (drop.Count < battle.enemies.Length + 1)
                {
                    int countOfDrop = battle.enemies.Length + 1 - drop.Count;
                    for (int j = 0; j < countOfDrop; j++)
                    {
                        drop.Add((1, battle.enemies[0].lvl));
                    }
                }
            }

            List<Item> items = new List<Item>();
            
            foreach (var rarity in drop)
            {
                Tuple<string, string>? itemName = SecondaryFunctions.GetRandomKeyValuePair(battle.drop);
                if (itemName == null)
                    break;
                Item item = BattleGenerators.GenerateEquipmentItem(itemName.Item1,
                    rarity.Item2, itemName.Item2, rarity.Item1);
            
                // if (users[i]!.inventory.Count >= 30)
                // {
                //     battle.log += $":school_satchel:У `{battle.players[i].name}` не хватило места для `{item.name} {SecondaryFunctions.GetRarityColoredEmoji(item.rarity)} | {item.lvl}`\n";
                //     break;
                // }
                // users[i]!.inventory.Add(item.id);

                await dataBase.ItemDb.CreateObject(item);
                items.Add(item);
                
                battle.log += $":school_satchel:`{battle.players[i].name}` нашёл `{item.name} {SecondaryFunctions.GetRarityColoredEmoji(item.rarity)} | {item.lvl}`\n";
            }
            
            results.Add(new BattleResultDrop(users[i]!, items));
            
            
            int countOfDroppedItem = BattleGenerators.GetRandomNumberForDrop();
            for (int j = 0; j < countOfDroppedItem; j++)
            {
                if (users[i]!.inventory.Count(x => x == "fb75ff73-1116-4e95-ae46-8075c4e9a782") >= 5 || users[i]!.inventory.Count >= 30)
                    break;
                users[i]!.inventory.Add("fb75ff73-1116-4e95-ae46-8075c4e9a782");
            }

            int exp = 0;
            int coins = 0;
            
            foreach (var e in battle.enemies)
            {
                exp += Random.Shared.Next(e.lvl * 3, e.lvl * 5) + Random.Shared.Next(3, 7 * users[i]!.stats.luck);
                coins += e.lvl * 3 + Random.Shared.Next(0, 7 * users[i]!.stats.luck);
            }
            
            users[i]!.exp += exp;
            users[i]!.cash += coins;

            battle.log += $":gift:`{battle.players[i].name}` получил `{exp}` опыта и `{coins}` монет\n";
        }

        foreach (var result in results)
            dataBase.BattlesDb.AddDrop(result);
    

        return users;
    }
    
    static async Task<List<User?>> HandlePlayerDead(Battle battle, List<User?> users, DataBase dataBase)
    {
        for (int i = 0; i < battle.players.Length; i++)
        {
            if (battle.players[i].stats.hp > 0)
                continue;
            int looseCash = Random.Shared.Next(0, users[i]!.cash / 5);
            int looseExp = Random.Shared.Next(0, (int)users[i]!.exp / 5);
            users[i]!.cash -= looseCash;
            users[i]!.exp -= looseExp;
            battle.log += $":warning:`{battle.players[i].name}` потерял `{looseExp}` опыта и `{looseCash}` монет\n";

            if (users[i]!.cash < 0)
                users[i]!.cash = 0;

            if (users[i]!.exp < 0)
                users[i]!.exp = 0;

            string? droppedItem = null;
            if (Random.Shared.Next(0, 100) < 20)
            {
                switch (Random.Shared.Next(0, 6))
                {
                    case 1:
                        droppedItem = users[i]!.equipment.helmet;
                        users[i]!.equipment.helmet = null;
                        break;
                    case 2:
                        droppedItem = users[i]!.equipment.armor;
                        users[i]!.equipment.armor = null;
                        break;
                    case 3:
                        droppedItem = users[i]!.equipment.gloves;
                        users[i]!.equipment.gloves = null;
                        break;
                    case 4:
                        droppedItem = users[i]!.equipment.weapon;
                        users[i]!.equipment.weapon = null;
                        break;
                    case 5:
                        droppedItem = users[i]!.equipment.pants;
                        users[i]!.equipment.pants = null;
                        break;
                    case 6:
                        droppedItem = users[i]!.equipment.shoes;
                        users[i]!.equipment.shoes = null;
                        break;
                }
            }

            if (droppedItem == null) continue;
            Item item = (Item)(await dataBase.ItemDb.Get(droppedItem))!;
            battle.log += $":warning:`{battle.players[i].name}` потерял `{item.name}`\n";
        } 
        return users;
    }
    
    static async Task<List<User?>> HandlePlayerRun(Battle battle, List<User?> users, DataBase dataBase)
    {
        for (int i = 0; i < battle.players.Length; i++)
        {
            if (battle.players[i].stats.hp < 0)
            {
                users[i]!.exp -= Random.Shared.Next(0, (int)users[i]!.exp / 10);
                if (users[i]!.exp < 0)
                    users[i]!.exp = 0;
            }
            else
            {
                string? droppedItem = null;
                if (Random.Shared.Next(0, 100) < 5)
                {
                    switch (Random.Shared.Next(0, 6))
                    {
                        case 1:
                            droppedItem = users[i]!.equipment.helmet;
                            users[i]!.equipment.helmet = null;
                            break;
                        case 2:
                            droppedItem = users[i]!.equipment.armor;
                            users[i]!.equipment.armor = null;
                            break;
                        case 3:
                            droppedItem = users[i]!.equipment.gloves;
                            users[i]!.equipment.gloves = null;
                            break;
                        case 4:
                            droppedItem = users[i]!.equipment.weapon;
                            users[i]!.equipment.weapon = null;
                            break;
                        case 5:
                            droppedItem = users[i]!.equipment.pants;
                            users[i]!.equipment.pants = null;
                            break;
                        case 6:
                            droppedItem = users[i]!.equipment.shoes;
                            users[i]!.equipment.shoes = null;
                            break;
                    }
                }

                if (droppedItem == null) continue;
                Item item = (Item)(await dataBase.ItemDb.Get(droppedItem))!;
                battle.log += $":warning:`{battle.players[i].name}` потерял `{item.name}`\n";
            }
        }
        return users;
    }
    
   
}