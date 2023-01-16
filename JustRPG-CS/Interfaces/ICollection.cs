
namespace JustRPG.Interfaces;

public interface ICollection
{
    object? Get(object val,string key = "id");

    object CreateObject(object id);

    void Add(object where,string fieldKey, int value);

    void Update(object obj);
}