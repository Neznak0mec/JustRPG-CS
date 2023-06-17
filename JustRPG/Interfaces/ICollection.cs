namespace JustRPG.Interfaces;

public interface ICollection
{
    Task<object?> Get(object val, string key = "id");

    Task<object?> CreateObject(object? id);

    Task Update(object? obj);
}