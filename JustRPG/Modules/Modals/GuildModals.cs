using System.Text.RegularExpressions;
using Discord.Interactions;
using Discord.WebSocket;
using JustRPG.Exceptions;
using JustRPG.Generators;
using JustRPG.Interfaces;
using JustRPG.Models;
using JustRPG.Models.Enums;
using JustRPG.Services;
using JustRPG.Services.Collections;

namespace JustRPG.Modules.Modals;

public class GuildModals : InteractionModuleBase<SocketInteractionContext<SocketModal>>
{
    private DiscordSocketClient _client;
    private readonly DataBase _dataBase;


    public GuildModals(IServiceProvider service)
    {
        _client = (DiscordSocketClient)service.GetService(typeof(DiscordSocketClient))!;
        _dataBase = (DataBase)service.GetService(typeof(DataBase))!;
    }

    [ModalInteraction("Guild|CreateModal")]
    private async Task GuildCreate(GuildCreateModal modal)
    {
        string guildName = modal.GuildName;
        string guildTag = modal.Tag;

        object? objGuild = await _dataBase.GuildDb.Get(guildTag);
        if (objGuild != null)
        {
            throw new UserInteractionException("Гильдия с таким тегом уже существует");
        }

        objGuild = await _dataBase.GuildDb.Get(guildName, "name");
        if (objGuild != null)
        {
            throw new UserInteractionException("Гильдия с таким именем уже существует");
        }

        User user = (User)(await _dataBase.UserDb.Get(Context.User.Id))!;
        if (!string.IsNullOrEmpty(user.guildTag))
        {
            throw new UserInteractionException("Вы уже участник гильдии");

        }

        if (!Regex.IsMatch(guildTag, @"^[a-zA-Z]+$"))
        {
            throw new UserInteractionException("Тег гильдии может состоять только из английских букв");
        }

        if (user.cash < 10000)
        {
            throw new UserInteractionException("У вас недостаточно средств для создания гильдии");
        }

        Guild guild = new Guild
        {
            join_type = JoinType.close,
            logo = "",
            members = new List<GuildMember>()
                { new GuildMember() { rank = GuildRank.owner, user = (long)Context.User.Id } },
            name = guildName,
            symbol = "",
            tag = guildTag.ToUpper(),
            wantJoin = new List<long>()
        };

        user.guildTag = guildTag;
        user.cash -= 10000;

        await _dataBase.GuildDb.CreateObject(guild);
        await _dataBase.UserDb.Update(user);

        await RespondAsync(embed: EmbedCreater.SuccessEmbed("Вы успешно создали гильдию"),
            ephemeral: true);
    }

    [ModalInteraction($"Guild|Kick_*", true)]
    private async Task GuildKick(string guildTag, GuildKickModal modal)
    {
        long userId;
        try
        {
            userId = Convert.ToInt64(modal.Id);
        }
        catch (Exception e)
        {
            throw new UserInteractionException("Id пользователя должно быть числом");
        }

        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        object? objUser = guild.members.FirstOrDefault(x => x.user == userId);
        if (objUser == null)
        {
            throw new UserInteractionException("Такого пользователя в гильдии нет");
        }

        GuildMember guildMember = (GuildMember)objUser;
        GuildMember? moderator = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        if (moderator == null)
        {
            throw new UserInteractionException("Вы не являетесь участником этой гильдии");
        }

        if (moderator.rank <= guildMember.rank)
        {
            throw new UserInteractionException("Нельзя кикнуть пользователя с таким же рангом как у вас или выше");
        }

        guild.members.Remove(guildMember);
        await _dataBase.GuildDb.Update(guild);

        User user = (User)(await _dataBase.UserDb.Get(guildMember.user))!;
        user.guildTag = null;

        await _dataBase.UserDb.Update(user);
        await RespondAsync(embed: EmbedCreater.SuccessEmbed("Пользователь успешно кикнут из гильдии"),
            ephemeral: true);
    }

    [ModalInteraction($"Guild|Accept_*", true)]
    private async Task GuildAcceptApplication(string guildTag, GuildApplicationAcceptModal modal)
    {
        long userId;
        try
        {
            userId = Convert.ToInt64(modal.Id);
        }
        catch (Exception e)
        {
            throw new UserInteractionException("Id пользователя должно быть числом");
        }

        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        object? user = await _dataBase.UserDb.Get(userId);

        if (user == null)
        {
            guild.wantJoin.Remove(userId);
            await _dataBase.GuildDb.Update(guild);
            throw new UserInteractionException("Пользователь не найден");
        }
        else if (((User)user).guildTag != null)
        {
            guild.wantJoin.Remove(userId);
            await _dataBase.GuildDb.Update(guild);
            throw new UserInteractionException("Пользователь уже участник гильдии");
        }
        else if (member == null)
        {
            throw new UserInteractionException("Вы не участником этой гильдии");
        }
        else if (member.rank < GuildRank.officer)
        {
            throw new UserInteractionException("У вас нет права для этого действия");
        }
        else if (guild.join_type is JoinType.close or JoinType.open || guild.members.Count >= 30)
        {
            throw new UserInteractionException("На данный момент нельзя никого принять в гильдию");
        }
        else
        {
            guild.wantJoin.Remove(userId);

            GuildMember guildMember = new GuildMember
            {
                user = userId,
                rank = GuildRank.warrior
            };

            guild.members.Add(guildMember);
            await _dataBase.GuildDb.Update(guild);

            User userDb = (User)(await _dataBase.UserDb.Get(userId))!;
            userDb.guildTag = guildTag;
            await _dataBase.UserDb.Update(userDb);

            await RespondAsync(embed: EmbedCreater.SuccessEmbed($"Пользователь <@{userId}> принят в гильдию"),
                ephemeral: true);
        }
    }

    [ModalInteraction($"Guild|Denied_*", true)]
    private async Task GuildDeniedApplication(string guildTag, GuildApplicationDeniedModal modal)
    {
        long userId;
        try
        {
            userId = Convert.ToInt64(modal.Id);
        }
        catch (Exception e)
        {
            throw new UserInteractionException("Id пользователя должно быть числом");
        }

        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);


        if (member == null)
        {
            throw new UserInteractionException("Вы не участником этой гильдии");
        }
        else if (member.rank < GuildRank.officer)
        {
            throw new UserInteractionException("У вас нет права для этого действия");
        }
        else if (!guild.wantJoin.Contains(userId))
        {
            throw new UserInteractionException("Пользователь не подавал заявку на вступление в гильдию");
        }
        else
        {
            guild.wantJoin.Remove(userId);
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(
                embed: EmbedCreater.SuccessEmbed($"Пользователю <@{userId}> отклонена заявка на вступлению в гильдию"),
                ephemeral: true);
        }
    }

    [ModalInteraction($"Guild|Officers_*", true)]
    private async Task GuildOfficers(string guildTag, GuildOfficersModal modal)
    {
        long userId;
        try
        {
            userId = Convert.ToInt64(modal.Id);
        }
        catch (Exception e)
        {
            throw new UserInteractionException("Id пользователя должно быть числом");
        }

        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        GuildMember? officer = guild.members.FirstOrDefault(x => x.user == userId);


        if (member == null)
        {
            throw new UserInteractionException("Вы не участником этой гильдии");
        }
        else if (member.rank < GuildRank.owner)
        {
            throw new UserInteractionException("У вас нет права для этого действия");
        }
        else if (officer == null)
        {
            throw new UserInteractionException("Пользователь не состоит в гильдии");
        }
        else if (officer.rank == GuildRank.officer)
        {
            officer.rank = GuildRank.warrior;
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(
                embed: EmbedCreater.SuccessEmbed($"Пользователь <@{officer.user}> снят с должности офицера"),
                ephemeral: true);
        }
        else
        {
            officer.rank = GuildRank.officer;
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.SuccessEmbed($"Пользователь <@{officer.user}> назначен офицером"),
                ephemeral: true);
        }
    }

    [ModalInteraction($"Guild|EditSymbol_*", true)]
    private async Task GuildEditSymbol(string guildTag, GuildEditSymbolModal modal)
    {
        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);


        if (member == null)
        {
            throw new UserInteractionException("Вы не участником этой гильдии");
        }
        else if (member.rank < GuildRank.owner)
        {
            throw new UserInteractionException("У вас нет права для этого действия");
        }
        else if (!guild.premium)
        {
            throw new UserInteractionException("У вашей гильдии нет премиум статуса, для его получения обратитесь к @neznakomec");
        }
        else
        {
            string symbol = modal.Symbol;

            if (!IsDiscordEmoji(symbol))
            {
                throw new UserInteractionException("Значок гильдии должен быть дискорд эмоджи. Пример: <:emoji_name:emoji_id>" +
                    "\nДля помощи обратитесь за помощью на официальном или на прямую к @neznakomec");
            }

            guild.symbol = modal.Symbol;
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.SuccessEmbed($"Значок гильдии успешно изменён"), ephemeral: true);
        }
    }

    private bool IsDiscordEmoji(string symbol)
    {
        Regex regex = new Regex(@"^<a?:[a-zA-Z0-9]+:[0-9]+>$");
        return regex.IsMatch(symbol);
    }
}