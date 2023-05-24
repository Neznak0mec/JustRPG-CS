using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Models.SubClasses;
using JustRPG.Services;
using Serilog;

namespace JustRPG.Modules.Responce;

public class SelectLocation
{
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;

    public SelectLocation(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;
    }

    public async Task Distributor(string[] buttonInfo)
    {
        switch (buttonInfo[2])
        {
            case "adventure":
                await GenerateAdventure(buttonInfo[1]);
                break;
        }
    }

    async Task GenerateAdventure(string userId)
    {
        Location location = _dataBase.LocationsDb.Get(_component.Data.Values.ToArray()[0]);
        Warrior mainPlayer = GenerateWarriorByUser((User)_dataBase.UserDb.Get(userId), _component.User.Username);
        Battle newBattle = new Battle
        {
            id = Guid.NewGuid().ToString(),
            type = "adventure",
            drop = location.drops,
            players = new[] { mainPlayer },
            enemies = new[] { GenerateMob(location, mainPlayer.stats) },
            log = "-"
        };

        _dataBase.BattlesDb.CreateObject(newBattle);

        await ResponceMessage(EmbedCreater.BattlEmbed(newBattle), component: ButtonSets.BattleButtonSet(newBattle.id,userId));
    }


    Warrior GenerateWarriorByUser(User user,string username) => new()
    {
        name = username,
        stats = new BattleStats(user.stats),
        inventory = InventoryToBattleInventory(user),
        lvl = user.lvl
    };

    List<Tuple<string, BattleStats>> InventoryToBattleInventory(User user)
    {
        // todo update to new system on add skills

        List<Tuple<string, BattleStats>> res = new List<Tuple<string, BattleStats>>();
        foreach (var i in user.inventory)
        {
            if (i == "fb75ff73-1116-4e95-ae46-8075c4e9a782")
                res.Add(new Tuple<string, BattleStats>("heal", new BattleStats()
                {
                    hp = user.stats.hp/4,
                    damage = 0,
                    defence = 0,
                    luck = 0,
                    MaxDef = 0,
                    MaxHP = 0,
                    speed = 0
                }));
        }

        return res;
    }

    Warrior GenerateMob(Location location, BattleStats player)
    {
        Warrior mob = new Warrior();
        mob.lvl = Random.Shared.Next(location.lvl,location.lvl+4);
        var names = location.monsters.Keys;
        mob.name = names.ToArray()[Random.Shared.Next(0, names.Count)];
        mob.stats = GenerateRandomStats(player, mob.lvl);
        mob.url = location.monsters[mob.name];
        return mob;
    }


    BattleStats GenerateRandomStats(BattleStats player, int lvl)
    {

        BattleStats mobStats = new BattleStats()
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
    
    
    private async Task ResponceMessage(Embed embed, MessageComponent component = null)
    {
        await _component.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });
    }
}