
namespace JustRPG.Interfaces;

public interface ICollection
{
    Task<object?> Get(object val,string key = "id");

    Task<object?> CreateObject(object? id);

    Task Add(object where,string fieldKey, int value);

    Task Update(object? obj);
}