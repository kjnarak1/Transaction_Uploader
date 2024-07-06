using System.Collections.Concurrent;

public class CacheKeyManager
{
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new ConcurrentDictionary<string, bool>();

    public void AddKey(string key)
    {
        _cacheKeys.TryAdd(key, true);
    }

    public IEnumerable<string> GetAllKeys()
    {
        return _cacheKeys.Keys.ToArray();
    }

    public void RemoveKey(string key)
    {
        _cacheKeys.TryRemove(key, out _);
    }

    public void ClearAllKeys()
    {
        _cacheKeys.Clear();
    }
}
