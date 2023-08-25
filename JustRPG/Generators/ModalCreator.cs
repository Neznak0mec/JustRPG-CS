using Discord;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Generators;

public class ModalCreator
{
    public static Modal SellItemModal(string id, string itemName)
    {
        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle($"Продажа предмета {id}")
            .WithCustomId($"Inventory_SetSellItemPrice_{itemName}")
            .AddTextInput(label: "Цена продажи", placeholder: "Введите цену за которую хотете продать предмет",
                customId: "price", required: true, minLength: 1);

        return modalBuilder.Build();
    }
    
    public static Modal GuildKickModal(string guildTag)
    {
        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle($"Кик участника гильдии")
            .WithCustomId($"Guild_Kick_{guildTag}")
            .AddTextInput(label: "Id участника для кика", placeholder: "Введите id участника которого хотите кикнуть",
                customId: "id", required: true);

        return modalBuilder.Build();
    }

    public static Modal CreateGuild()
    {
        ModalBuilder modalBuilder = new ModalBuilder()
            .WithTitle($"Создание Гильдии")
            .WithCustomId($"Guild_Create")
            .AddTextInput(label: "Имя Гильдии", placeholder: "Придумайте имя гильдии",
                customId: "name", required: true, minLength: 3, maxLength: 20)
            .AddTextInput(label: "Тег гильдии", placeholder: "Введите тег гильдии (Только английские буквы и цифры)",
                    customId: "tag", required: true, minLength: 3, maxLength: 4);

        return modalBuilder.Build();
    }
}