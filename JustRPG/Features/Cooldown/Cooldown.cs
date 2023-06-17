using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;

namespace JustRPG.Features.Cooldown;

public class Cooldown : PreconditionAttribute
{
    private TimeSpan CooldownLength { get; set; }
    private readonly ConcurrentDictionary<CooldownInfo, DateTime> _cooldowns = new();

    public Cooldown(int seconds)
    {
        CooldownLength = TimeSpan.FromSeconds(seconds);
    }

    public struct CooldownInfo
    {
        public ulong UserId { get; }
        public string CommandHashCode { get; }

        public CooldownInfo(ulong userId, string commandName)
        {
            UserId = userId;
            CommandHashCode = commandName;
        }
    }

    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
        ICommandInfo commandInfo, IServiceProvider services)
    {
        var key = new CooldownInfo(context.User.Id, commandInfo.Name);
        if (_cooldowns.TryGetValue(key, out DateTime endsAt))
        {
            var difference = endsAt.Subtract(DateTime.UtcNow);
            if (difference.Ticks > 0)
            {
                return Task.FromResult(PreconditionResult.FromError(
                    $"Не так быстро. Вы сможете повторно использовать эту команду через `{(int)difference.TotalSeconds}` секунд"));
            }

            var time = DateTime.UtcNow.Add(CooldownLength);
            _cooldowns.TryUpdate(key, time, endsAt);
        }
        else
        {
            _cooldowns.TryAdd(key, DateTime.UtcNow.Add(CooldownLength));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}