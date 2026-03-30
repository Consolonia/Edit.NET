using System.Text.Json;

namespace EditNET.Helpers
{
    public static class ObjectExtensions
    {
        public static T SerializedCopy<T>(this T original)
        {
            string serialized = JsonSerializer.Serialize(original);
            return JsonSerializer.Deserialize<T>(serialized)!;
        }
    }
}