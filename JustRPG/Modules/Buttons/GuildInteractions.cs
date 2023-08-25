using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Generators;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;
using Action = JustRPG.Models.Action;

namespace JustRPG.Modules.Buttons;

public class GuildInteractions : InteractionModuleBase<SocketInteractionContext<SocketMessageComponent>>
{
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public GuildInteractions(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }
    
    [ComponentInteraction("Guild_*_*_main", true)]
    private async Task GuildMain(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        
        await UpdateMessage(embed: EmbedCreater.GuildEmbed(guild),components:ButtonSets.GuildComponents(guild,Context.User.Id));
    }

    [ComponentInteraction("Guild_*_*_leave", true)]
    private async Task LeaveGuild(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        
        if (member == null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не являетесь участником этой гильдии"),
                ephemeral: true);
            return;
        }
        
        string actionUid = Guid.NewGuid().ToString();
        
        Action action = new Action
        {
            id = "Action_" + actionUid,
            date = DateTimeOffset.Now.ToUnixTimeSeconds(),
            type = "GuildLeave",
            userId = (long)Context.User.Id,
            args = new[]
            {
                guildTag
            }
        };

        await _dataBase.ActionDb.CreateObject(action);
        Embed embed =
            EmbedCreater.WarningEmbed($"Вы уверены что хотите покинуть гильдию?");
        await RespondAsync(embed: embed, components: ButtonSets.AcceptActions(actionUid, (long)Context.User.Id),
            ephemeral: true);
    }

    [ComponentInteraction("Guild_*_*_join", true)]
    private async Task JoinGuild(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? guildMember = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;
        
        if (user.guildTag == guildTag)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы уже участник гильдии другой гильд"),
                ephemeral: true);
        }
        else if (guildMember != null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы уже участник гильдии"),
                ephemeral: true);
        }
        else if (guild.members.Count >= 30 || guild.join_type == JoinType.close)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("В данную гильдию нельзя вступить. Закрыт набор или она достигла максимального числа участников"),
                ephemeral: true);
        }
        else if (guild.wantJoin.Contains((long)Context.User.Id))
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы уже подали заявку на вступление в эту гильдию"),
                ephemeral: true);
        }
        else if (guild.join_type == JoinType.open)
        {

            user.guildTag = guild.tag;
            user.guildEmblem = guild.symbol;

            guild.members.Add(new GuildMember{user = user.id, rank = GuildRank.warrior});

            await _dataBase.GuildDb.Update(guild);
            await _dataBase.UserDb.Update(user);

            await RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы ступили в гильдию"), ephemeral: true);
        }
        else
        {
            guild.wantJoin.Add((long)Context.User.Id);
            await RespondAsync(embed: EmbedCreater.SuccessEmbed("Ваша заявка на вступление оставлена"), ephemeral: true);
        }
    }

    [ComponentInteraction("Guild_*_*_members", true)]
    private async Task GuildMembers(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        
        await UpdateMessage(embed: EmbedCreater.GuildMembers(guild),components: ButtonSets.GuildMembers(guild,Context.User.Id));
    }
    
    [ComponentInteraction("Guild_*_*_applications", true)]
    private async Task GuildApplications(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        
        await UpdateMessage(embed: EmbedCreater.GuildApplications(guild),components: ButtonSets.GuildApplications(guild,Context.User.Id) );
    }
    
    [ComponentInteraction("Guild_*_*_kick", true)]
    private async Task GuildKick(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        
        await RespondWithModalAsync(ModalCreator.GuildKickModal(guild.tag));
    }
    
    [ComponentInteraction("Guild_*_*_accept", true)]
    private async Task GuildAccept(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember ? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        object? user = await _dataBase.UserDb.Get(guild.wantJoin[0]);

        if (user == null)
        {
            guild.wantJoin.RemoveAt(0);
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Пользователь не найден"),
                ephemeral: true);
        }
        else if (((User)user).guildTag != null)
        {
            guild.wantJoin.RemoveAt(0);
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Пользователь уже участник гильдии"),
                ephemeral: true);
        }
        else if (member == null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не участником этой гильдии"),
                ephemeral: true);
        }
        else if (member.rank < GuildRank.officer)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("У вас нет права для этого действия"),
                ephemeral: true);
        }
        else if (guild.join_type is JoinType.close or JoinType.open || guild.members.Count >= 30)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("На данный момент нельзя никого принять в гильдию"),
                ephemeral: true);
        }
        else
        {
            long userToAddId = guild.wantJoin[0];
            guild.wantJoin.RemoveAt(0);

            GuildMember guildMember = new GuildMember
            {
                user = userToAddId,
                rank = GuildRank.warrior
            };
            
            guild.members.Add(guildMember);
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.SuccessEmbed("Пользователь принят в гильдию"), ephemeral: true);
        }
    }

    [ComponentInteraction("Guild_*_*_denied", true)]
    private async Task GuildDenied(string userId, string guildTag)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember ? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        if (member == null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не участником этой гильдии"),
                ephemeral: true);
        }
        else if (member.rank < GuildRank.officer)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("У вас нет права для этого действия"),
                ephemeral: true);
        }
        else
        {
            guild.wantJoin.RemoveAt(0);
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.SuccessEmbed("Пользователю отклонена заявка на вступлению в гильдию"), ephemeral: true);
        }
    }

    [ComponentInteraction("Guild_*_create", true)]
    private async Task GuildCreate(string userId)
    {
        await RespondWithModalAsync(ModalCreator.CreateGuild());
    }
    
    public async Task UpdateMessage(Embed embed, MessageComponent components)
    {
        await Context.Interaction.UpdateAsync(x =>
        {
            x.Embed = embed;
            x.Components = components;
        });
    }
}