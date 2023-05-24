using Discord;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Models.SubClasses;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;


// todo: Refactor, and split enemies attacks by battle types 
public class BattleInteractions : IInteractionMaster {
    private DiscordSocketClient _client;
    private SocketMessageComponent _component;
    private DataBase _dataBase;

    public BattleInteractions(DiscordSocketClient client, SocketMessageComponent component, object? service)
    {
        _client = client;
        _component = component;
        _dataBase = (DataBase)service!;
    }
    
    public async Task Distributor(string[] buttonInfo)
    {
        object? temp = _dataBase.BattlesDb.Get(buttonInfo[3]);
        if (temp == null){
            await WrongInteraction("Боя не существует или он завершился");
            return;
        }
        Battle? battle = (Battle)temp;
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
        if (battle.type is "adventure" or "dungeon")
        {
            Warrior user = battle.players[0];
            Warrior enemy = battle.enemies[battle.selectedEnemy];
            
            user.Attack(battle,enemy);
            
            if (battle.enemies.All(x => x.stats.hp <= 0))
            {
                battle.log += "Вы победили\n";
                await UpdateBattle(battle, true);
                return;
            }
            
            if (battle.type == "adventure")
                enemy.Attack(battle,user);
            else
                foreach (var enem in battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(battle,user);}
            
            if (user.stats.hp <= 0)
            {
                battle.log += "Вы проиграли\n";
                await UpdateBattle(battle, true);
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

        if (battle.type == "adventure" || battle.type == "dungeon")
        {
            Warrior user = battle.players[0];
            Warrior enemy = battle.enemies[0];
            
            if (battle.type == "adventure")
                enemy.Attack(battle,user);
            else
                foreach (var enem in battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(battle,user);}
            
            if (user.stats.hp <= 0)
            {
                battle.log += "Вы проиграли\n";
                await UpdateBattle(battle, true);
            }
        }
        
        await UpdateBattle(battle);
    }
    
    async Task Run(Battle battle)
    {
        
        if (battle.type == "adventure" || battle.type == "dungeon")
        {
            
            Warrior user = battle.players[0];
            Warrior enemy = battle.enemies[0];

            if (Random.Shared.Next(1, 100) < 1 + user.stats.luck)
            {
                battle.log += "Вы успешно сбежали\n";
                await UpdateBattle(battle,true);
                return;
            }
            else
                battle.log += "Вам не удалось сбежать\n";

            if (battle.type == "adventure")
                enemy.Attack(battle,user);
            else
                foreach (var enem in battle.enemies){
                    if (enem.stats.hp>0) enem.Attack(battle,user);}
            
            if (user.stats.hp <= 0)
            {
                battle.log += "Вы проиграли\n";
                await UpdateBattle(battle, true);
            }
        }
        
        await UpdateBattle(battle);
    }
    
    async Task SelectEnemy(Battle battle)
    {
        battle.selectedEnemy = Convert.ToInt16(_component.Data.Values.ToArray()[0]);
        battle.log += $"{battle.players[battle.currentUser].name} изменил цель на {battle.enemies[battle.selectedEnemy]}";
        await UpdateBattle(battle, disableSelectEnemy:true);
    }
    
    async Task UpdateBattle(Battle battle, bool gameEnded = false, bool disableSelectEnemy = false)
    {
        if (gameEnded){
            AdventureGenerators.Reward(battle,_dataBase);
            _dataBase.BattlesDb.Delete(battle);
        }
        else
            _dataBase.BattlesDb.Update(battle);
        
        
        
        await _component.UpdateAsync(x =>
        {
            x.Embed = EmbedCreater.BattlEmbed(battle, gameEnded);
            x.Components = ButtonSets.BattleButtonSet(battle, _component.User.Id.ToString(), gameEnded, disableSelectEnemy);
        });
    }
    
    private async Task WrongInteraction(string text)
    {
        await _component.RespondAsync(embed: EmbedCreater.ErrorEmbed(text), ephemeral:true);
    }
}