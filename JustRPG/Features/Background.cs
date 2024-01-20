using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Models;
using JustRPG.Generators;
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
            try
            {
                await CheckFindingBattles();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            
            try
            {
                await CancelFindPVP();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            try
            {
                await CheckBattleToEnd();
            }
            catch(Exception e)
            {
                Log.Error(e.ToString());
            }

            _dataBase.ActionDb.RemoveOldActions();
        }
    }

    private async Task CheckBattleToEnd()
    {
        DateTimeOffset currentTime = DateTimeOffset.Now;
        List<Battle> battles = (List<Battle>)(await _dataBase.BattlesDb.GetAll())!;

        List<Battle> endedBattles = battles.Where(x => (x.type is BattleType.adventure or BattleType.dungeon) &&
                                                       x.lastActivity < currentTime.AddSeconds(-60).ToUnixTimeSeconds())
            .ToList();

        endedBattles.AddRange(battles.Where(x => (
                                                     x.type == BattleType.arena) &&
                                                 x.lastActivity < currentTime.AddSeconds(-30).ToUnixTimeSeconds())
            .ToList());

        foreach (var i in endedBattles.ToArray())
        {
            await EndBattle(i);
            try
            {
                await _dataBase.BattlesDb.Delete(i);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }

    async Task EndBattle(Battle battle)
    {
        battle.players[battle.currentUser].stats.hp = -1;

        if (battle.type == BattleType.arena)
        {
            battle.log =
                $":skull_crossbones:`{battle.players[battle.currentUser].name}` не успел сделать ход за отведённое время. Бой окончен.\n";
            battle.players[battle.currentUser].stats.hp = -1;

            await BattleMaster.Reward(battle, _dataBase);

            var emb = EmbedCreater.BattleEmbed(battle, true);
            var component = ButtonSets.BattleButtonSet(battle, 0, true, true);
            foreach (SocketInteraction temp in battle.originalInteraction)
            {
                await temp.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = emb;
                    x.Components = component;
                });
            }
        }
        else
        {
            battle.log = ":skull_crossbones:Вы проиграли\n:skull_crossbones:Вы бездействовали слишком долго, и противник отаковал вас в спину\n";
            battle.status = BattleStatus.playerDead;
            await BattleMaster.Reward(battle, _dataBase);

            var emb = EmbedCreater.BattleEmbed(battle, true);
            var component = ButtonSets.BattleButtonSet(battle, 0, true, true);
            foreach (SocketInteraction i in battle.originalInteraction)
            {
                await i.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = emb;
                    x.Components = component;
                });
            }
        }
    }

    async Task CheckFindingBattles()
    {
        var pvpPairs = new List<(FindPVP, FindPVP)>();
        int mmrIncrementPerSecond = 1;
        int mmrDifferenceThreshold = 100;

        var findPvps = _dataBase.ArenaDb.GetAllFindPVP();
        findPvps.Sort((pvp1, pvp2) => pvp1.mmr.CompareTo(pvp2.mmr));

        for (int i = 0; i < findPvps.Count - 1; i++)
        {
            try
            {
                var pvp1 = findPvps[i];
                var pvp2 = findPvps[i + 1];

                int mmrDifference = Math.Abs(pvp1.mmr - pvp2.mmr);
                int timeDifferenceSeconds = (int)Math.Abs(pvp1.stratTime - pvp2.stratTime);
                int mmrDifferenceAllowed = mmrIncrementPerSecond * timeDifferenceSeconds;

                if (mmrDifference <= mmrDifferenceThreshold + mmrDifferenceAllowed)
                {
                    pvpPairs.Add((pvp1, pvp2));
                    i++;
                }
            }
            catch (Exception e)
            {
                Log.Error("{error}", e.ToString());
                break;
            }
            
        }

        if (pvpPairs.Any())
            await StartBattles(pvpPairs);
    }

    async Task StartBattles(List<(FindPVP, FindPVP)> pairsPvp)
    {
        var tasks = pairsPvp.ToArray().Select(async pair =>
        {
            var m1 = await _client.GetUserAsync((ulong)pair.Item1.userId);
            var m2 = await _client.GetUserAsync((ulong)pair.Item2.userId);
            
            if ((m1 == null || m2 == null) || m1.Id == m2.Id)
            {
                _dataBase.ArenaDb.DeletFindPVP(pair.Item1.userId);
                _dataBase.ArenaDb.DeletFindPVP(pair.Item2.userId);

                await pair.Item1.msgLocation.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = EmbedCreater.ErrorEmbed("Произошла ошибка при поиске противника, попробуйте ещё раз");
                    x.Components = null;
                });
                await pair.Item2.msgLocation.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = EmbedCreater.ErrorEmbed("Произошла ошибка при поиске противника, попробуйте ещё раз");
                    x.Components = null;
                });
            }

            var u1Task = _dataBase.UserDb.Get(m1!.Id);
            var u2Task = _dataBase.UserDb.Get(m2!.Id);

            await Task.WhenAll(u1Task, u2Task);

            User u1 = (User)u1Task.Result!;
            User u2 = (User)u2Task.Result!;

            var w1Task = BattleGenerators.GenerateWarriorByUser(u1, m1.Username, _dataBase, m1.GetAvatarUrl());
            var w2Task = BattleGenerators.GenerateWarriorByUser(u2, m2.Username, _dataBase, m2.GetAvatarUrl());

            await Task.WhenAll(w1Task, w2Task);

            Warrior w1 = w1Task.Result;
            Warrior w2 = w2Task.Result;

            Battle battle = new Battle
            {
                id = Guid.NewGuid().ToString(),
                type = BattleType.arena,
                drop = new Dictionary<string, string>(),
                players = new[] { w1, w2 },
                enemies = new Warrior[] { },
                originalInteraction = new List<SocketInteraction>
                {
                    pair.Item1.msgLocation,
                    pair.Item2.msgLocation
                },
                log = "-"
            };

            await _dataBase.BattlesDb.CreateObject(battle);

            var modifyTasks = battle.originalInteraction.Select(async msg =>
            {
                var emb = EmbedCreater.BattleEmbed(battle);
                var component = ButtonSets.BattleButtonSet(battle, u1.id);
                SocketInteraction temp = (SocketInteraction)msg;
                await temp.ModifyOriginalResponseAsync(x =>
                {
                    x.Embed = emb;
                    x.Components = component;
                });
            });

            await Task.WhenAll(modifyTasks);

            _dataBase.ArenaDb.DeletFindPVP(pair.Item1.userId);
            _dataBase.ArenaDb.DeletFindPVP(pair.Item2.userId);
        });

        await Task.WhenAll(tasks);
    }

    async Task CancelFindPVP()
    {
        var findPvps = _dataBase.ArenaDb.GetAllFindPVP().ToArray();
        //cancel find pvp if time of start more than 5 minutes
        
        foreach (var i in findPvps.Where(i => i.stratTime < DateTimeOffset.Now.AddMinutes(-5).ToUnixTimeSeconds()))
        {
            _dataBase.ArenaDb.DeletFindPVP(i.userId);
            await i.msgLocation.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = EmbedCreater.WarningEmbed("Время поиска противника истекло, попробуйте ещё раз");
                x.Components = null;
            });
        }
    }
}