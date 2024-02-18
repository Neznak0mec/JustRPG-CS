using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Modules.Buttons;

public class SelectRewardsInteractions: InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>> {
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;

    public SelectRewardsInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }
    
    [ComponentInteraction("Battle|StartSelectReward_*_*", true)]
    async Task StartSelectReward(string userId,string someletter)
    {
        BattleResultDrop? drop = _dataBase.BattlesDb.GetLastUserDrop(Context.User.Id);
        
        if (drop == null)
            throw new UserInteractionException("Что то пошло не так, вероятно время для выбора награды истекло");
        
        
        await Context.Interaction.RespondAsync(embed: await EmbedCreater.SelectRewardsEmbed(drop, _dataBase),
            components: ButtonSets.SelectRewardsComponents(Context.User.Id, drop));
    }

    [ComponentInteraction("SelectRewards|prewItem_*_*", true)]
    private async Task PrewItem(string userId, string resultId)
    {
        BattleResultDrop? drop = _dataBase.BattlesDb.GetDrop(resultId);
        if (drop == null)
        {
            throw new UserInteractionException("Что то пошо не так, вероятно время для выбора награды истекло");
        }
        
        drop.DecrementItemIndex();
        await UpdateMessage(embed: await EmbedCreater.SelectRewardsEmbed(drop, _dataBase),components:ButtonSets.SelectRewardsComponents(Context.User.Id,drop));
        
    }
    
    
    [ComponentInteraction("SelectRewards|nextItem_*_*", true)]
    private async Task NextItem(string userId, string resultId)
    {
        BattleResultDrop? drop = _dataBase.BattlesDb.GetDrop(resultId);
        if (drop == null)
        {
            throw new UserInteractionException("Что то пошо не так, вероятно время для выбора награды истекло");
        }
        
        drop.IncrementItemIndex();
        await UpdateMessage(embed: await EmbedCreater.SelectRewardsEmbed(drop,_dataBase),components:ButtonSets.SelectRewardsComponents(Context.User.Id,drop));
    }
    
    [ComponentInteraction("SelectRewards|Select_*_*", true)]
    private async Task SelectItem(string userId, string resultId)
    {
        BattleResultDrop? drop = _dataBase.BattlesDb.GetDrop(resultId);
        if (drop == null)
        {
            throw new UserInteractionException("Что то пошо не так, вероятно время для выбора награды истекло");
        }
        
        drop.SelectItem(drop.CurrentItemIndex);
        await UpdateMessage(embed: await EmbedCreater.SelectRewardsEmbed(drop,_dataBase),components:ButtonSets.SelectRewardsComponents(Context.User.Id,drop));
    }
    
    
    [ComponentInteraction("SelectRewards|Complete_*_*", true)]
    private async Task Complete(string userId, string resultId)
    {
        BattleResultDrop? drop = _dataBase.BattlesDb.GetDrop(resultId);
        if (drop == null)
        {
            throw new UserInteractionException("Что то пошо не так, вероятно время для выбора награды истекло");
        }
        
        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;
        if (drop.selectedItems.Count + user.inventory.Count > 30)
        {
            throw new UserInteractionException("У вас слишком много предметов, сначала освободите место в инвентаре или выберите меньше предметов");
        }
        
        await drop.GiveRewards(_dataBase);
        _dataBase.BattlesDb.DeleteDrop(drop.id);
        await UpdateMessage(embed: await EmbedCreater.SelectRewardsEmbed(drop,_dataBase, true),components:ButtonSets.SelectRewardsComponents(Context.User.Id,drop,true));
    }
    
    public async Task UpdateMessage(Embed? embed = null, MessageComponent? components = null)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = components;
        });
    }
}