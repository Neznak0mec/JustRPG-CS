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

    async Task GenerateAdventure(string userID)
    {
        Location location = _dataBase.LocationsDb.Get(_component.Data.Values.ToArray()[0]);
        Warrior mainPlayer = GenerateWarriorByUser((User)_dataBase.UserDb.Get(userID), _component.User.Username);
        Battle newBattle = new Battle();
        newBattle.id = Guid.NewGuid().ToString();
        newBattle.type = "adventure";
        newBattle.drop = location.drops;
        newBattle.players = new[] { mainPlayer };
        newBattle.enemies = new[] { GenerateMob(location, mainPlayer.stats) };
        newBattle.log = "-";


        await ResponceMessage(EmbedCreater.BattlEmbed(newBattle));
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

        int damage = (int)(player.damage * (1 + Random.Shared.NextDouble() * 0.2 - 0.1));
        int defense = (int)(player.defence * (1 + Random.Shared.NextDouble() * 0.2 - 0.1));
        int luck = (int)(player.luck * (1 + Random.Shared.NextDouble() * 0.2 - 0.1));
        int speed = (int)(player.speed * (1 + Random.Shared.NextDouble() * 0.2 - 0.1));
        BattleStats mobStats = new BattleStats()
        {
            damage = damage,
            defence = defense,
            hp = 100 + lvl*10,
            speed = speed,
            luck = luck,
            MaxHP = 100 + lvl*10,
            MaxDef = defense
        };

        return mobStats;
    }

    public Tuple<int,int,int> GenerateMobStats(int locationLevel, int playerLevel)
    {
        int maxLocationLevel = 10;
        int maxPlayerLevel = 20;
        // Generate HP
        int hp = 0;
        if (locationLevel <= maxLocationLevel && playerLevel <= maxPlayerLevel)
        {
            double factor = (double)locationLevel / (double)maxLocationLevel;
            int baseHP = playerLevel * 100;
            hp = (int)(factor * baseHP);
        }
        else
        {
            hp = maxLocationLevel * 100;
        }
        int damage = 0;
        if (locationLevel <= maxLocationLevel && playerLevel <= maxPlayerLevel)
        {
            double factor = (double)locationLevel / (double)maxLocationLevel;
            int baseDamage = playerLevel * 25;
            damage = (int)(factor * baseDamage);
        }
        else
        {
            damage = maxLocationLevel * 25;
        }
        int def = 0;
        if (locationLevel <= maxLocationLevel && playerLevel <= maxPlayerLevel)
        {
            double factor = (double)locationLevel / (double)maxLocationLevel;
            int baseDex = playerLevel * 2;
            def = (int)(factor * baseDex);
        }
        else
        {
            def = maxLocationLevel * 2;
        }
        return new Tuple<int, int, int>(hp,damage,def);
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