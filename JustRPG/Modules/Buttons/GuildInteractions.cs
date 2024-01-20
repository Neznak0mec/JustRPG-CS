using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class GuildInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private readonly DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public GuildInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ComponentInteraction("Guild_*_*", true)]
    private async Task GuildMain(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;

        await UpdateMessage(embed: EmbedCreater.GuildEmbed(guild),components:ButtonSets.GuildComponents(guild,Context.User.Id));
    }

    [ComponentInteraction("Guild|Leave_*_*", true)]
    private async Task LeaveGuild(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);

        if (member == null)
        {
            throw new UserInteractionException("Вы не являетесь участником этой гильдии");
        }

        string actionUid = Guid.NewGuid().ToString();

        Action action = new Action
        {
            id = "Action_" + actionUid,
            date = DateTime.Now,
            type = "GuildLeave",
            userId = (long)Context.User.Id,
            args = new[]
            {
                guildTag
            }
        };

        _dataBase.ActionDb.CreateObject(action);
        Embed embed =
            EmbedCreater.WarningEmbed($"Вы уверены что хотите покинуть гильдию?");
        await RespondAsync(embed: embed, components: ButtonSets.AcceptActions(actionUid, (long)Context.User.Id),
            ephemeral: true);
    }

    [ComponentInteraction("Guild|Join_*_*", true)]
    private async Task JoinGuild(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? guildMember = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;

        if (user.guildTag == guildTag)
        {
            throw new UserInteractionException("Вы уже участник гильдии другой гильд");
        }
        else if (guildMember != null)
        {
            throw new UserInteractionException("Вы уже участник гильдии");
        }
        else if (guild.members.Count >= 30 || guild.join_type == JoinType.close)
        {
            throw new UserInteractionException("В данную гильдию нельзя вступить. Закрыт набор или она достигла максимального числа участников");
        }
        else if (guild.wantJoin.Contains((long)Context.User.Id))
        {
            throw new UserInteractionException("Вы уже подали заявку на вступление в эту гильдию");
        }
        else if (guild.join_type == JoinType.open)
        {

            user.guildTag = guild.tag;


            var members = guild.members;
            members.Add(new GuildMember{user = user.id, rank = GuildRank.warrior});
            guild.members = members;

            await _dataBase.GuildDb.Update(guild);
            await _dataBase.UserDb.Update(user);

            await RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы ступили в гильдию"), ephemeral: true);
        }
        else
        {
            guild.wantJoin.Add((long)Context.User.Id);
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.SuccessEmbed("Ваша заявка на вступление оставлена"), ephemeral: true);
        }
    }

    [ComponentInteraction("Guild|Members_*_*", true)]
    private async Task GuildMembers(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;

        await UpdateMessage(embed: await EmbedCreater.GuildMembers(guild,_client),components: ButtonSets.GuildMembers(guild,Context.User.Id,_client));
    }

    [ComponentInteraction("Guild|Applications_*_*", true)]
    private async Task GuildApplications(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;

        await UpdateMessage(embed: await EmbedCreater.GuildApplications(guild,_client),components: ButtonSets.GuildApplications(guild,Context.User.Id) );
    }
 
    [ComponentInteraction($"Guild|Kick_*_*", true)]
    private async Task GuildKick(string userId, string guildTag)
    {
        await RespondWithModalAsync<GuildKickModal>($"Guild|Kick_{guildTag}");
    }

    [ComponentInteraction("Guild|Accept_*_*", true)]
    private async Task GuildAccept(string userId, string guildTag)
    {
        await RespondWithModalAsync<GuildApplicationAcceptModal>($"Guild|Accept_" + guildTag);
    }

    [ComponentInteraction("Guild|Denied_*_*", true)]
    private async Task GuildDenied(string userId, string guildTag)
    {
       await RespondWithModalAsync<GuildApplicationDeniedModal>("Guild|Denied_" + guildTag);
    }
    
        
    [ComponentInteraction("Guild|Officer_*_*", true)]
    private async Task GuildOfficers(string userId, string guildTag)
    {
        await RespondWithModalAsync<GuildOfficersModal>("Guild|Officers_" + guildTag);
    }
    
    [ComponentInteraction("Guild|Settings_*_*", true)]
    private async Task GuildSettings(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;

        await UpdateMessage(embed: EmbedCreater.GuildSettings(guild),components: ButtonSets.GuildSettings(guild,Context.User.Id));
    }

    [ComponentInteraction("Guild|Create_*", true)]
    private async Task GuildCreate(string userId)
    {
        await RespondWithModalAsync<GuildCreateModal>("Guild|Create");
    }
    
    [ComponentInteraction($"Guild|EditSymbol_*_*", true)]
    private async Task GuildEdit(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);

        if (member == null)
        {
            throw new UserInteractionException("Вы не являетесь участником этой гильдии");
        }

        if (!guild.premium)
        {
            throw new UserInteractionException("У вашей гильдии нет премиум статуса. Он выдаётся тем кто поддержал разработку бота звонкой монетой)" +
                "\nЕсли хотите поддержать автора обратитесь на официальном сервере бота или в личные сообщения @neznakomec");
        }

        if (member.rank < GuildRank.owner)
        {
            throw new UserInteractionException("У вас недостаточно прав для этого действия");
        }

        await RespondWithModalAsync<GuildEditSymbolModal>($"Guild|EditSymbol_{guildTag}");
    }
    
    private async Task UpdateMessage(Embed embed, MessageComponent components)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = components;
        });
    }
}