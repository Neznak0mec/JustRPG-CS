using Discord;
using Discord.WebSocket;
using JustRPG.Models;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;
using Serilog;

namespace JustRPG.Features;

public class Background
{
    private readonly DataBase _dataBase;
    private readonly DiscordSocketClient _client;


    public Background(DataBase dataBase, DiscordSocketClient client)
    {
        _dataBase = dataBase;
        _client = client;
    }

    public async Task BackgroundMaster()
    {
        while (true)
        {
            await Task.Delay(5000);
            await CheckBattles();
        }
    }

    async Task CheckBattles()
    {
        List<Tuple<FindPVP, FindPVP>> pvpPairs = new List<Tuple<FindPVP, FindPVP>>();

        int mmrIncrementPerSecond = 1;
        int mmrDifferenceThreshold = 100;

        var findPvps = _dataBase.ArenaDb.GetAllFindPVP();

        for (int i = 0; i < findPvps.Count; i++)
        {
            FindPVP pvp1 = findPvps[i];

            for (int j = i + 1; j < findPvps.Count; j++)
            {
                FindPVP pvp2 = findPvps[j];

                int mmrDifference = Math.Abs(pvp1.mmr - pvp2.mmr);
                int timeDifferenceSeconds = (int)Math.Abs(pvp1.stratTime - pvp2.stratTime);

                int mmrDifferenceAllowed = mmrIncrementPerSecond * timeDifferenceSeconds;

                if (mmrDifference <= mmrDifferenceThreshold + mmrDifferenceAllowed)
                {
                    Tuple<FindPVP, FindPVP> pair = new Tuple<FindPVP, FindPVP>(pvp1, pvp2);
                    pvpPairs.Add(pair);
                }
            }
        }

        if (pvpPairs.Count != 0)
            await StartBattles(pvpPairs);
    }

    async Task StartBattles(List<Tuple<FindPVP, FindPVP>> pairsPvp)
    {
        foreach (var pair in pairsPvp)
        {
            var m1 = await _client.GetUserAsync((ulong)pair.Item1.userId);
            var m2 = await _client.GetUserAsync((ulong)pair.Item2.userId);

            User u1 = (User)(await _dataBase.UserDb.Get(m1.Id)!)!;
            User u2 = (User)(await _dataBase.UserDb.Get(m2.Id))!;

            Warrior w1 = await AdventureGenerators.GenerateWarriorByUser(u1, m1.Username, _dataBase, m1.GetAvatarUrl());
            Warrior w2 = await AdventureGenerators.GenerateWarriorByUser(u2, m2.Username, _dataBase, m2.GetAvatarUrl());

            Battle? battle = new Battle()
            {
                id = Guid.NewGuid().ToString(),
                type = "arena",
                drop = { },
                players = new[] { w1, w2 },
                enemies = { },
                log = "-"
            };

            PVP pvp = new PVP()
            {
                lastInteraction = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds(),
                battleId = battle.id,
                msgLocations = new List<SocketInteraction> { pair.Item1.msgLocation, pair.Item2.msgLocation }
            };

            await _dataBase.BattlesDb.CreateObject(battle);

            foreach (var msg in pvp.msgLocations)
            {
                var emb = EmbedCreater.BattleEmbed(battle);
                var component = ButtonSets.BattleButtonSet(battle, u1.id);
                await msg.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = emb;
                    x.Components = component;
                });
            }

            _dataBase.ArenaDb.DeletFindPVP(pair.Item1.userId);
            _dataBase.ArenaDb.DeletFindPVP(pair.Item2.userId);

            _dataBase.ArenaDb.AddPVP(pvp);
        }
    }
}