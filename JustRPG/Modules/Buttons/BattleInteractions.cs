using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;


// todo: Refactor, and split enemies attacks by _battle types
public class BattleInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>> {
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    private Battle _battle;
    private User User;

    public BattleInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }
    
    public override async Task<Task> BeforeExecuteAsync(ICommandInfo command)
    {
        // User = (await _dataBase.UserDb.Get(Context.User.Id))!;
        // LanguageExtensions.UseLanguage(User.language);
        
        var buttonInfo = Context.Interaction.Data.CustomId.Split('_');
        object? temp = _dataBase.BattlesDb.Get(buttonInfo[2]);
        if (temp == null){
            await WrongInteraction("Боя не существует или он завершился");
            return Task.CompletedTask;
        }
        else{
            _battle = (Battle)temp;
            return base.BeforeExecuteAsync(command);
        }
    }

    [ComponentInteraction("Battle|Attack_*_*", true)]
    async Task Attack(string userId, string battleId)
    {
        if (_battle.type is BattleType.adventure or BattleType.dungeon)
        {
            Warrior user = _battle.players[0];
            Warrior enemy = _battle.enemies[_battle.selectedEnemy];

            user.Attack(_battle,enemy);

            if (_battle.enemies.All(x => x.stats.hp <= 0))
            {
                _battle.log += ":crown:Вы победили\n";
                _battle.status = BattleStatus.playerWin;
                await UpdateBattle(true);
                return;
            }
            
            if (_battle.type == BattleType.adventure)
                enemy.Attack(_battle,user);
            else
                foreach (var enem in _battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(_battle,user);}
            
            if (user.stats.hp <= 0)
            {
                _battle.log += ":skull_crossbones:Вы проиграли\n";
                _battle.status = BattleStatus.playerDead;
                await UpdateBattle(true);
                return;
            }
        }

        if (_battle.type is BattleType.arena)
        {
            Warrior user = _battle.players[_battle.currentUser];
            Warrior enemy = _battle.players[_battle.currentUser == 1 ? 0 : 1];

            user.Attack(_battle,enemy);

            if (_battle.players.Any(x => x.stats.hp <= 0))
            {
                _battle.log += $":crown:`{_battle.players[_battle.currentUser].name}` побеждает\n";
                await UpdateBattle(true);
                return;
            }

        }
        
        await UpdateBattle();
    }
    

    [ComponentInteraction("Battle|Heal_*_*", true)]
    async Task Heal(string userId, string battleId)
    {
        if (_battle.players[_battle.currentUser].inventory.Any(x => x.Item1 == "fb75ff73-1116-4e95-ae46-8075c4e9a782"))
        {
            _battle.log +=
                $":heart:`{_battle.players[_battle.currentUser].name}` восстановил себе `{_battle.players[_battle.currentUser].Heal()}`\n";
            int index = _battle.players[_battle.currentUser].inventory.FindIndex(x => x.Item1 == "fb75ff73-1116-4e95-ae46-8075c4e9a782");
            _battle.players[_battle.currentUser].inventory.RemoveAt(index);
        }
        else
        {
            _battle.log +=
                $":school_satchel:Покапавшись в сумке `{_battle.players[_battle.currentUser].name}` не нашёл у себя зелье для восстановления \n";
        }

        if (_battle.type is BattleType.adventure or BattleType.dungeon)
        {
            Warrior user = _battle.players[0];
            Warrior enemy = _battle.enemies[0];
            
            if (_battle.type == BattleType.adventure)
                enemy.Attack(_battle,user);
            else
                foreach (var enem in _battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(_battle,user);}
            
            if (user.stats.hp <= 0)
            {
                _battle.log += ":person_running:Вы проиграли\n";
                _battle.status = BattleStatus.playerDead;
                await UpdateBattle(true);
                return;
            }
        }
        
        await UpdateBattle();
    }
    

    [ComponentInteraction("Battle|Run_*_*", true)]
    async Task Run(string userId, string battleId)
    {
        
        if (_battle.type is BattleType.adventure or BattleType.dungeon)
        {
            
            Warrior user = _battle.players[0];
            Warrior enemy = _battle.enemies[0];

            if (Random.Shared.Next(1, 100) < 1 + user.stats.luck)
            {
                _battle.log += ":person_running:Вы успешно сбежали\n";
                _battle.status = BattleStatus.playerRun;
                await UpdateBattle(true);
                return;
            }
            else
                _battle.log += ":person_running:Вам не удалось сбежать\n";

            if (_battle.type == BattleType.adventure)
                enemy.Attack(_battle,user);
            else
                foreach (var enem in _battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(_battle,user);}
            
            if (user.stats.hp <= 0)
            {
                _battle.log += ":skull_crossbones:Вы проиграли\n";
                _battle.status = BattleStatus.playerDead;
                await UpdateBattle(true);
                return;
            }
        }

        if (_battle.type is BattleType.arena)
        {

            Warrior user = _battle.players[_battle.currentUser];

            if (Random.Shared.Next(1, 100) < 1 + user.stats.luck)
            {
                _battle.log += $":person_running:`{_battle.players[_battle.currentUser].name}` успешно сбежал\n";
                _battle.status = BattleStatus.playerRun;
                await UpdateBattle(true);
                return;
            }
            else
                _battle.log += $":person_running:`{_battle.players[_battle.currentUser].name}` не удалось сбежать\n";
        }

        await UpdateBattle();
    }
    
    [ComponentInteraction("Battle|SelectEnemy_*_*", true)]
    async Task SelectEnemy(string userId, string battleId,string[] selected)
    {
        _battle.selectedEnemy = Convert.ToInt16(selected[0]);
        _battle.log += $":dart:`{_battle.players[_battle.currentUser].name}` изменил цель на `{_battle.enemies[_battle.selectedEnemy].name}`";
        await UpdateBattle(disableSelectEnemy:true);
    }
    
    async Task UpdateBattle(bool gameEnded = false, bool disableSelectEnemy = false)
    {
        if (_battle.type == BattleType.arena)
            _battle.currentUser = (short)(_battle.currentUser == 1 ? 0 : 1);

        if (gameEnded){
            await BattleMaster.Reward(_battle,_dataBase);
        }
        else
            await _dataBase.BattlesDb.Update(_battle);
        

        Embed embed = EmbedCreater.BattleEmbed(_battle, gameEnded);
        MessageComponent component = ButtonSets.BattleButtonSet(_battle,(long)_battle.players[_battle.currentUser].id!, gameEnded, disableSelectEnemy);
        

        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });

        if (_battle.type == BattleType.arena)
        {
            foreach (SocketInteraction i in _battle.originalInteraction)
            {
                if (await i.GetOriginalResponseAsync() != await GetOriginalResponseAsync())
                    await i.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed;
                        x.Components = component;
                    });
            }
        }
    }
    
    private async Task WrongInteraction(string text)
    {
        await Context.Interaction.RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral:true);
    }
}