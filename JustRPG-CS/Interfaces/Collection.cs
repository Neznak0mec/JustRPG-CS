using System.Net.Sockets;
using MongoDB.Driver;

namespace JustRPG.Interfaces;

public interface Collection
{
    object? Get(object val,string key = "id");

    object CreateObject(object id);

    void Add(object where,string fieldKey, int value);

    void Update(object obj);
}