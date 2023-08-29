using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;
using JustRPG.Services.Collections;

namespace JustRPG.Modules.Modals;

// public class GuildModals : IModalMaster
// {
//     DiscordSocketClient _client;
//     SocketModal _modal;
//     DataBase _dataBase;
//
//
//     public GuildModals(DiscordSocketClient client, SocketModal modal, IServiceProvider service)
//     {
//         _client = client;
//         _modal = modal;
//         _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
//     }
//     
//     public async Task Distributor(string[] modalInfo)
//     {
//         switch (modalInfo[1])
//         {
//             case "Kick":
//                 await GuildKick(modalInfo);
//                 break;
//             default:
//                 await GuildCreate(modalInfo);
//                 break;
//         }
//     }
//
//     private async Task GuildCreate(string[] modalInfo)
//     {
//         List<SocketMessageComponentData> components = _modal.Data.Components.ToList();
//         
//         string guildName = components.First(x => x.CustomId == "name").Value;
//         string guildTag = components.First(x => x.CustomId == "tag").Value;
//         
//         object? objGuild = await _dataBase.GuildDb.Get(guildTag);
//         if (objGuild != null)
//         {
//             await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Гильдия с таким тегом уже существует"),
//                     ephemeral: true);
//             return;
//         }
//         objGuild = await _dataBase.GuildDb.Get(guildName,"name");
//         if (objGuild != null)
//         {
//             await  _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Гильдия с таким именем уже существует"),
//                 ephemeral: true);
//             return;
//         }
//         
//         User user = (User)(await _dataBase.UserDb.Get(_modal.User.Id))!;
//         if (!string.IsNullOrEmpty(user.guildTag))
//         {
//             await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы уже участник гильдии"),
//                 ephemeral: true);
//             return;
//         }
//         
//
//         if (user.cash < 10000)
//         {
//             await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("У вас недостаточно средств для создания гильдии"),
//                 ephemeral: true);
//             return;
//         }
//         
//         Guild guild = new Guild
//         {
//             join_type = JoinType.close,
//             logo = "",
//             members = new List<GuildMember>(){new GuildMember(){rank = GuildRank.owner, user = (long)_modal.User.Id}},
//             name = guildName,
//             symbol = "",
//             tag = guildTag,
//             wantJoin = new List<long>()
//         };
//         
//         user.guildTag = guildTag;
//         user.cash -= 10000;
//
//         await _dataBase.GuildDb.CreateObject(guild);
//         await _dataBase.UserDb.Update(user);
//         
//         await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы успешно создали гильдию"),
//             ephemeral: true);
//         
//     }
//
//     private async Task GuildKick(string[] modalInfo)
//     {
//         List<SocketMessageComponentData> components = _modal.Data.Components.ToList();
//
//         long userId;
//         try
//         {
//             userId = Convert.ToInt64(components.First(x => x.CustomId == "id").Value);
//         }
//         catch (Exception e)
//         {
//             await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Id пользователя должно быть числом"),
//                 ephemeral: true);
//             return;
//         }
//         
//         Guild guild = (Guild)(await _dataBase.GuildDb.Get(modalInfo[2]))!;
//         object? objUser = guild.members.FirstOrDefault(x => x.user == userId);
//         if (objUser == null)
//         {
//             await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Такого пользователя в гильдии нет"),
//                 ephemeral: true);
//             return; 
//         }
//         
//         GuildMember  guildMember = (GuildMember)objUser;
//         GuildMember? moderator = guild.members.FirstOrDefault(x => x.user == (long)_modal.User.Id);
//         if (moderator == null)
//         {
//             await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не являетесь участником этой гильдии"),
//                 ephemeral: true);
//             return; 
//         }
//         
//         if (moderator.rank <= guildMember.rank)
//         {
//             await _modal.RespondAsync(embed: EmbedCreater.ErrorEmbed("Нельзя кикнуть пользователя с таким же рангом как у вас или выше"),
//                 ephemeral: true);
//             return; 
//         }
//         
//         guild.members.Remove(guildMember);
//         await _dataBase.GuildDb.Update(guild);
//         
//         User user = (User)(await _dataBase.UserDb.Get(guildMember.user))!;
//         user.guildTag = null;
//         user.guildEmblem = null;
//         
//         await _dataBase.UserDb.Update(user);
//         await _modal.RespondAsync(embed: EmbedCreater.SuccessEmbed("Пользователь успешно кикнут из гильдии"));
//     }
// }

public class GuildModals : InteractionModuleBase<SocketInteractionContext<SocketModal>>
{
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public GuildModals(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }
    
    [ModalInteraction("Guild|Create")]
    private async Task GuildCreate(GuildCreateModal modal)
     {
         
         string guildName = modal.Name;
         string guildTag = modal.Tag;
         
         object? objGuild = await _dataBase.GuildDb.Get(guildTag);
         if (objGuild != null)
         {
             await RespondAsync(embed: EmbedCreater.ErrorEmbed("Гильдия с таким тегом уже существует"),
                     ephemeral: true);
             return;
         }
         objGuild = await _dataBase.GuildDb.Get(guildName,"name");
         if (objGuild != null)
         {
             await  RespondAsync(embed: EmbedCreater.ErrorEmbed("Гильдия с таким именем уже существует"),
                 ephemeral: true);
             return;
         }
         
         User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;
         if (!string.IsNullOrEmpty(user.guildTag))
         {
             await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы уже участник гильдии"),
                 ephemeral: true);
             return;
         }
         

         if (user.cash < 10000)
         {
             await RespondAsync(embed: EmbedCreater.ErrorEmbed("У вас недостаточно средств для создания гильдии"),
                 ephemeral: true);
             return;
         }
         
         Guild guild = new Guild
         {
             join_type = JoinType.close,
             logo = "",
             members = new List<GuildMember>(){new GuildMember(){rank = GuildRank.owner, user = (long)Context.User.Id}},
             name = guildName,
             symbol = "",
             tag = guildTag,
             wantJoin = new List<long>()
         };
         
         user.guildTag = guildTag;
         user.cash -= 10000;

         await _dataBase.GuildDb.CreateObject(guild);
         await _dataBase.UserDb.Update(user);
         
         await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы успешно создали гильдию"),
             ephemeral: true);
         
     }

    [ModalInteraction($"Guild|Kick_*",true)]
    private async Task GuildKick(string guildTag,GuildKickModal modal)
    {
        long userId;
        try
        {
            userId = Convert.ToInt64(modal.Id);
        }
        catch (Exception e)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Id пользователя должно быть числом"),
                ephemeral: true);
            return;
        }
        
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        object? objUser = guild.members.FirstOrDefault(x => x.user == userId);
        if (objUser == null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Такого пользователя в гильдии нет"),
                ephemeral: true);
            return; 
        }
        
        GuildMember  guildMember = (GuildMember)objUser;
        GuildMember? moderator = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        if (moderator == null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не являетесь участником этой гильдии"),
                ephemeral: true);
            return; 
        }
        
        if (moderator.rank <= guildMember.rank)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Нельзя кикнуть пользователя с таким же рангом как у вас или выше"),
                ephemeral: true);
            return; 
        }
        
        guild.members.Remove(guildMember);
        await _dataBase.GuildDb.Update(guild);
        
        User user = (User)(await _dataBase.UserDb.Get(guildMember.user))!;
        user.guildTag = null;
        user.guildEmblem = null;
        
        await _dataBase.UserDb.Update(user);
        await RespondAsync(embed: EmbedCreater.SuccessEmbed("Пользователь успешно кикнут из гильдии"),
            ephemeral: true);
    }
}