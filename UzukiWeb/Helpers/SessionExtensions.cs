using System.Text.Json;

namespace UzukiWeb.Helpers
{
    public static class SessionExtensions
    {
        // Hàm nhét đồ vào Giỏ (Mã hóa thành JSON)
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Hàm lấy đồ từ Giỏ ra (Giải mã JSON)
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}