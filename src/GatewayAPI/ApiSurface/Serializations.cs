using Newtonsoft.Json;
using System.Text;

namespace GatewayAPI.ApiSurface;

public static class Serializations
{
    public static string AsBase64Json<T>(T input)
    {
        var json = JsonConvert.SerializeObject(input);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public static T? FromBase64JsonOrDefault<T>(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return default;
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(Convert.FromBase64String(input)));
        }
        catch (Exception)
        {
            return default;
        }
    }
}
