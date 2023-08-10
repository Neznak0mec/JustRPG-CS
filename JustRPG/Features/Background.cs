using Discord;
using Discord.WebSocket;
using JustRPG.Models;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Models.Enums;
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
            try {
                await CheckFindingBattles();}
            catch
            {
            }
            try {
                await CheckBattleToEnd();
            }
            catch
            {
            }
        }
    }

    private async Task CheckBattleToEnd()
    {
        DateTimeOffset currentTime = DateTimeOffset.Now;
        List<Battle> battles = (List<Battle>)(await _dataBase.BattlesDb.GetAll())!;

        List<Battle> endedBattles = battles.Where(x =>(
            x.type == BattleType.adventure ||
            x.type == BattleType.dungeon) &&
            x.lastActivity < currentTime.AddSeconds(-60).ToUnixTimeSeconds()).ToList();

        endedBattles.AddRange(battles.Where(x =>(
            x.type == BattleType.arena) &&
            x.lastActivity < currentTime.AddSeconds(-30).ToUnixTimeSeconds()).ToList());

        foreach (var i in endedBattles)
        {
            await EndBattle(i);
            await _dataBase.BattlesDb.Delete(i);
        }
    }

    async Task EndBattle(Battle battle)
    {
        battle.players[battle.currentUser].stats.hp = -1;

        if (battle.type == BattleType.arena)
        {
            battle.log = $"{battle.players[battle.currentUser].name} не успел сделать ход за отведённое время. Бой окончен.\n";
            battle.players[battle.currentUser].stats.hp = -1;

            await AdventureGenerators.Reward(battle, _dataBase);

            var emb = EmbedCreater.BattleEmbed(battle, true);
            var component = ButtonSets.BattleButtonSet(battle, 0, true, true);
            foreach (var i in battle.originalInteraction)
            {
                SocketInteraction temp = (SocketInteraction)i;
                await temp.ModifyOriginalResponseAsync( x =>
                {
                    x.Embed = emb;
                    x.Components = component;
                });
            }


        }
        else
        {
            battle.log = "Вы проиграли\nВы бездействовали слишком долго, и противник отаковал вас в спину\n";
            battle.status = BattleStatus.playerDead;
            await AdventureGenerators.Reward(battle, _dataBase);

            var emb = EmbedCreater.BattleEmbed(battle, true);
            var component = ButtonSets.BattleButtonSet(battle, 0, true, true);
            SocketMessageComponent temp = (SocketMessageComponent)battle.originalInteraction[0];
            await temp.ModifyOriginalResponseAsync( x =>
            {
                x.Embed = emb;
                x.Components = component;
            });
        }

    }

    async Task CheckFindingBattles()
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

            Battle? battle = new Battle();
            battle.id = Guid.NewGuid().ToString();
            battle.type = BattleType.arena;
            battle.drop = new Dictionary<string,string>();
            battle.players = new[] { w1, w2 };
            battle.enemies = new Warrior[]{};
            battle.originalInteraction = new List<object>{
                pair.Item1.msgLocation,
                pair.Item2.msgLocation
            };
            battle.log = "-";

            await _dataBase.BattlesDb.CreateObject(battle);

            foreach (var msg in battle.originalInteraction)
            {
                var emb = EmbedCreater.BattleEmbed(battle);
                var component = ButtonSets.BattleButtonSet(battle, u1.id);
                SocketInteraction temp = (SocketInteraction)msg;
                await temp.ModifyOriginalResponseAsync( x =>
                {
                    x.Embed = emb;
                    x.Components = component;
                });
            }

            _dataBase.ArenaDb.DeletFindPVP(pair.Item1.userId);
            _dataBase.ArenaDb.DeletFindPVP(pair.Item2.userId);

        }
    }
}