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
        placeholder: "Введите тег гильдии (Только английские буквы)")]
    [RequiredInput()]
    public string Tag { get; set; }

}

public class GuildKickModal : IModal
{
    public string Title { get; } = "Кик участника гильдии";

    [InputLabel("Id участника")]
    [ModalTextInput("id", maxLength: 30, placeholder: "ведите id или имя участника которого хотите кикнуть")]
    [RequiredInput()]
    public string Id { get; set; }
}

public class GuildApplicationAcceptModal : IModal
{
    public string Title { get; } = "Принять заявку на вступление в гильдию";

    [InputLabel("Id участника")]
    [ModalTextInput("id", maxLength: 30, placeholder: "ведите id участника которого хотите принять в гильдию")]
    [RequiredInput()]
    public string Id { get; set; }
}

public class GuildApplicationDeniedModal : IModal
{
    public string Title { get; } = "Отклонить заявку на вступление в гильдию";

    [InputLabel("Id участника")]
    [ModalTextInput("id", maxLength: 30, placeholder: "ведите id участника заявку которого хотите отклонить")]
    [RequiredInput()]
    public string Id { get; set; }
}

public class GuildOfficersModal : IModal
{
    public string Title { get; } = "Назначить\\снять офицера";

    [InputLabel("Id участника")]
    [ModalTextInput("id", maxLength: 30, placeholder: "ведите id участника которому хотите назначить\\снять офицерское звание")]
    [RequiredInput()]
    public string Id { get; set; }
}

public class GuildEditSymbolModal : IModal
{
    public string Title { get; } = "Изменить значок гильдии";

    [InputLabel("Значок гильдии")]
    [ModalTextInput("symbol", placeholder: "Введите эмодзи значка гильдии")]
    [RequiredInput()]
    public string Symbol { get; set; }
}