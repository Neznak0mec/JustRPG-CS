using Discord;
using Discord.WebSocket;
using JustRPG.Features;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Models.SubClasses;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;


// todo: Refactor, and split enemies attacks by battle types 
public class BattleInteractions : IInteractionMaster {
    private DiscordSocketClient _client;
    private readonly SocketMessageComponent _component;
    private readonly DataBase _dataBase;

    public BattleInteractions(DiscordSocketClient client, SocketMessageComponent component, IServiceProvider service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }
    
    public async Task Distributor(string[] buttonInfo)
    {
        object? temp = await _dataBase.BattlesDb.Get(buttonInfo[3]);
        if (temp == null){
            await WrongInteraction("Боя не существует или он завершился");
            return;
        }
        Battle battle = (Battle)temp;
        switch (buttonInfo[2])
        {
            case "Attack":
                await Attack(battle);
                break;
            case "Heal":
                await Heal(battle);
                break;
            case "Run":
                await Run(battle);
                break;
            case "SelectEnemy":
                await SelectEnemy(battle);
                break;
        }
    }
    
    async Task Attack(Battle battle)
    {
        if (battle.type is BattleType.adventure or BattleType.dungeon)
        {
            Warrior user = battle.players[0];
            Warrior enemy = battle.enemies[battle.selectedEnemy];
            
            user.Attack(battle,enemy);
            
            if (battle.enemies.All(x => x.stats.hp <= 0))
            {
                battle.log += "Вы победили\n";
                battle.status = BattleStatus.playerWin;
                await UpdateBattle(battle, true);
                return;
            }
            
            if (battle.type == BattleType.adventure)
                enemy.Attack(battle,user);
            else
                foreach (var enem in battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(battle,user);}
            
            if (user.stats.hp <= 0)
            {
                battle.log += "Вы проиграли\n";
                battle.status = BattleStatus.playerDead;
                await UpdateBattle(battle, true);
                return;
            }
        }

        if (battle.type is BattleType.arena)
        {
            Warrior user = battle.players[battle.currentUser];
            Warrior enemy = battle.players[battle.currentUser == 1 ? 0 : 1];

            user.Attack(battle,enemy);

            if (battle.players.Any(x => x.stats.hp <= 0))
            {
                battle.log += $"{battle.players[battle.currentUser].name} побеждает\n";
                await UpdateBattle(battle, true);
                return;
            }

        }
        
        await UpdateBattle(battle);
    }
    
    async Task Heal(Battle battle)
    {
        if (battle.players[battle.currentUser].inventory.Any(x => x.Item1 == "fb75ff73-1116-4e95-ae46-8075c4e9a782")){
            battle.log +=
                $"{battle.players[battle.currentUser].name} восстановил себе {battle.players[battle.currentUser].Heal()}";
            int index = battle.players[battle.currentUser].inventory.FindIndex(x=>x.Item1 == "");
            battle.players[battle.currentUser].inventory.RemoveAt(index);
        }
        else
        {
            battle.log +=
                $"Покапашившись в сумке {battle.players[battle.currentUser].name} не нашёл у себя зелье для восстановления ";
        }

        if (battle.type is BattleType.adventure or BattleType.dungeon)
        {
            Warrior user = battle.players[0];
            Warrior enemy = battle.enemies[0];
            
            if (battle.type == BattleType.adventure)
                enemy.Attack(battle,user);
            else
                foreach (var enem in battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(battle,user);}
            
            if (user.stats.hp <= 0)
            {
                battle.log += "Вы проиграли\n";
                battle.status = BattleStatus.playerDead;
                await UpdateBattle(battle, true);
                return;
            }
        }
        
        await UpdateBattle(battle);
    }
    
    async Task Run(Battle battle)
    {
        
        if (battle.type is BattleType.adventure or BattleType.dungeon)
        {
            
            Warrior user = battle.players[0];
            Warrior enemy = battle.enemies[0];

            if (Random.Shared.Next(1, 100) < 1 + user.stats.luck)
            {
                battle.log += "Вы успешно сбежали\n";
                battle.status = BattleStatus.playerRun;
                await UpdateBattle(battle,true);
                return;
            }
            else
                battle.log += "Вам не удалось сбежать\n";

            if (battle.type == BattleType.adventure)
                enemy.Attack(battle,user);
            else
                foreach (var enem in battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(battle,user);}
            
            if (user.stats.hp <= 0)
            {
                battle.log += "Вы проиграли\n";
                battle.status = BattleStatus.playerDead;
                await UpdateBattle(battle, true);
                return;
            }
        }

        if (battle.type is BattleType.arena)
        {

            Warrior user = battle.players[battle.currentUser];

            if (Random.Shared.Next(1, 100) < 1 + user.stats.luck)
            {
                battle.log += $"{battle.players[battle.currentUser].name} успешно сбежал\n";
                battle.status = BattleStatus.playerRun;
                await UpdateBattle(battle, true);
                return;
            }
            else
                battle.log += $"{battle.players[battle.currentUser].name} не удалось сбежать\n";
        }

        await UpdateBattle(battle);
    }
    
    async Task SelectEnemy(Battle battle)
    {
        battle.selectedEnemy = Convert.ToInt16(_component.Data.Values.ToArray()[0]);
        battle.log += $"{battle.players[battle.currentUser].name} изменил цель на {battle.enemies[battle.selectedEnemy].name}";
        await UpdateBattle(battle, disableSelectEnemy:true);
    }
    
    async Task UpdateBattle(Battle battle, bool gameEnded = false, bool disableSelectEnemy = false)
    {
        if (battle.type == BattleType.arena)
            battle.currentUser = (short)(battle.currentUser == 1 ? 0 : 1);

        if (gameEnded){
            await AdventureGenerators.Reward(battle,_dataBase);
        }
        else
            await _dataBase.BattlesDb.Update(battle);
        

        Embed embed = EmbedCreater.BattleEmbed(battle, gameEnded);
        MessageComponent component = ButtonSets.BattleButtonSet(battle,(long)battle.players[battle.currentUser].id!, gameEnded, disableSelectEnemy);
        

        await _component.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = component;
        });

        if (battle.type == BattleType.arena)
        {
            foreach (SocketInteraction i in battle.originalInteraction)
            {
                if (await i.GetOriginalResponseAsync() != await _component.GetOriginalResponseAsync())
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
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral:true);
    }
}