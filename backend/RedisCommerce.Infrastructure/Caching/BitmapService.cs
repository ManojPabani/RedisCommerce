using RedisCommerce.Application.Interfaces;
using StackExchange.Redis;

namespace RedisCommerce.Infrastructure.Caching;

public class BitmapService : RedisServiceBase, IBitmapService
{
    public BitmapService(IConnectionMultiplexer connectionMultiplexer)
        : base(connectionMultiplexer)
    {
    }

    public async Task<bool> SetBitAsync(string key, long position, bool value = true) =>
        await Database.StringSetBitAsync(key, position, value);

    public async Task<bool> GetBitAsync(string key, long position) =>
        await Database.StringGetBitAsync(key, position);

    public async Task<long> CountAsync(string key) =>
        await Database.StringBitCountAsync(key);

    public async Task<long> BitPositionAsync(string key, bool bit) =>
        await Database.StringBitPositionAsync(key, bit);

    public async Task<long> BitOpAsync(BitwiseOperation operation, string destinationKey, params string[] sourceKeys)
    {
        var bitwise = operation switch
        {
            BitwiseOperation.And => Bitwise.And,
            BitwiseOperation.Or => Bitwise.Or,
            BitwiseOperation.Xor => Bitwise.Xor,
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };

        var keys = sourceKeys.Select(k => (RedisKey)k).ToArray();
        return await Database.StringBitOperationAsync(bitwise, destinationKey, keys);
    }

    public async Task<bool> DeleteAsync(string key) =>
        await DeleteKeyAsync(key);
}
