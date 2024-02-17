namespace JustRPG.Models;

public class LocalCache<T>
{
    private Dictionary<string, T?> cache = new Dictionary<string, T?>();

    public T? Get(string key)
    {
        return cache.TryGetValue(key, out var value) ? value : default(T);
    }

    public void Add(string key, T? value)
    {
        cache[key] = value;
    }

    public void Remove(string key)
    {
        cache.Remove(key);
    }

    public bool Exists(string key)
    {
        return cache.ContainsKey(key);
    }

    public List<T?> GetAll()
    {
        return cache.Values.ToList();
    }
    
    public int Count()
    {
        return cache.Count;
    }   
    
    public bool Any(Func<T?, bool> predicate)
    {
        return cache.Values.Any(predicate);
    }
}