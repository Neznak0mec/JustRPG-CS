using System.Text.RegularExpressions;
using Discord.Interactions;
using Discord.WebSocket;
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

        objGuild = await _dataBase.GuildDb.Get(guildName, "name");
        if (objGuild != null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Гильдия с таким именем уже существует"),
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

        if (!Regex.IsMatch(guildTag, @"^[a-zA-Z]+$"))
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Тег гильдии может состоять только из английских букв"),
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
            members = new List<GuildMember>()
                { new GuildMember() { rank = GuildRank.owner, user = (long)Context.User.Id } },
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

        GuildMember guildMember = (GuildMember)objUser;
        GuildMember? moderator = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        if (moderator == null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не являетесь участником этой гильдии"),
                ephemeral: true);
            return;
        }

        if (moderator.rank <= guildMember.rank)
        {
            await RespondAsync(
                embed: EmbedCreater.ErrorEmbed("Нельзя кикнуть пользователя с таким же рангом как у вас или выше"),
                ephemeral: true);
            return;
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
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Id пользователя должно быть числом"),
                ephemeral: true);
            return;
        }

        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        object? user = await _dataBase.UserDb.Get(userId);

        if (user == null)
        {
            guild.wantJoin.Remove(userId);
            await _dataBase.GuildDb.Update(guild);
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Пользователь не найден"),
                ephemeral: true);
        }
        else if (((User)user).guildTag != null)
        {
            guild.wantJoin.Remove(userId);
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
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Id пользователя должно быть числом"),
                ephemeral: true);
            return;
        }

        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);


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
        else if (!guild.wantJoin.Contains(userId))
        {
            await RespondAsync(
                embed: EmbedCreater.ErrorEmbed("Пользователь не подавал заявку на вступление в гильдию"),
                ephemeral: true);
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
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Id пользователя должно быть числом"),
                ephemeral: true);
            return;
        }

        Guild guild = (Guild)(await _dataBase.GuildDb.Get(guildTag))!;
        GuildMember? member = guild.members.FirstOrDefault(x => x.user == (long)Context.User.Id);
        GuildMember? officer = guild.members.FirstOrDefault(x => x.user == userId);


        if (member == null)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не участником этой гильдии"),
                ephemeral: true);
        }
        else if (member.rank < GuildRank.owner)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("У вас нет права для этого действия"),
                ephemeral: true);
        }
        else if (officer == null)
        {
            await RespondAsync(
                embed: EmbedCreater.ErrorEmbed("Пользователь не состоит в гильдии"),
                ephemeral: true);
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
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("Вы не участником этой гильдии"),
                ephemeral: true);
        }
        else if (member.rank < GuildRank.owner)
        {
            await RespondAsync(embed: EmbedCreater.ErrorEmbed("У вас нет права для этого действия"),
                ephemeral: true);
        }
        else if (!guild.premium)
        {
            await RespondAsync(
                embed: EmbedCreater.ErrorEmbed(
                    "У вашей гильдии нет премиум статуса, для его получения обратитесь к @neznakomec"),
                ephemeral: true);
        }
        else
        {
            string symbol = modal.Symbol;

            if (!IsDiscordEmoji(symbol))
            {
                await RespondAsync(embed: EmbedCreater.ErrorEmbed(
                        "Значок гильдии должен быть дискорд эмоджи. Пример: <:emoji_name:emoji_id>" +
                        "\nДля помощи обратитесь за помощью на официальном или на прямую к @neznakomec"),
                    ephemeral: true);
                return;
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