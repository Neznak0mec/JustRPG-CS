using System.Xml.Serialization;
using Discord;
using Discord.Interactions;
using JustRPG.Models;
using JustRPG.Services;

namespace JustRPG.Generators;

public class SellItemModal : IModal
{
    // Inventory_SetSellItemPrice_{itemName}
    public string Title { get; set; } = $"Продажа предмета ?";

    [InputLabel("Цена продажи")]
    [ModalTextInput("price",minLength: 1,placeholder:"Введите цену за которую хотете продать предмет")]
    [RequiredInput()]
    public string Price { get; set; }
}

public class GuildCreateModal : IModal
{
    //custom id - Guild_Create
    public string Title { get; } = "Создание Гильдии";

    [InputLabel("Имя Гильдии")]
    [ModalTextInput("name",minLength:3 ,maxLength: 20,placeholder:"Придумайте имя гильдии")]
    [RequiredInput()]
    public string Name { get; set; }

    [InputLabel("Тег гильдии")]
    [ModalTextInput("tag", minLength: 3, maxLength: 4,
        placeholder: "Введите тег гильдии (Только английские буквы и цифры)")]
    [RequiredInput()]
    public string Tag { get; set; }

}

public class GuildKickModal : IModal
{
    public string Title { get; } = "Кик участника гильдии";

    [InputLabel("Id участника для кика")]
    [ModalTextInput("id", maxLength: 20,placeholder:"ведите id участника которого хотите кикнуть")]
    [RequiredInput()]
    public string Id { get; set; }
    
}