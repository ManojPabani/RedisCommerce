namespace RedisCommerce.Application.Interfaces;

public enum BitwiseOperation
{
    And,
    Or,
    Xor,
}

public interface IBitmapService
{
    Task<bool> SetBitAsync(string key, long position, bool value = true);
    Task<bool> GetBitAsync(string key, long position);
    Task<long> CountAsync(string key);
    Task<long> BitPositionAsync(string key, bool bit);
    Task<long> BitOpAsync(BitwiseOperation operation, string destinationKey, params string[] sourceKeys);
    Task<bool> DeleteAsync(string key);
}
