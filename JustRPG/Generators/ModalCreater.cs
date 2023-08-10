using Discord;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Generators;

public class ModalCreater
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
}