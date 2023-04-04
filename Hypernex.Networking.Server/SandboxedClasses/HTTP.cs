using System.Text;

namespace Hypernex.Networking.Server.SandboxedClasses;

public class HTTP
{
    private readonly HttpClient _client = new HttpClient();

    private string MediaTypeToString(HttpMediaType mediaType)
    {
        switch (mediaType)
        {
            case HttpMediaType.ApplicationJSON:
                return "application/json";
            case HttpMediaType.ApplicationXML:
                return "application/xml";
            case HttpMediaType.ApplicationURLEncoded:
                return "application/x-www-form-urlencoded";
            case HttpMediaType.TextPlain:
                return "text/plain";
            case HttpMediaType.TextXML:
                return "text/xml";
        }
        return MediaTypeToString(HttpMediaType.TextPlain);
    }

    public async Task<string> Get(string url) => await _client.GetStringAsync(url);

    public async Task<string> Post(string url, string data, HttpMediaType mediaType)
    {
        StringContent stringContent = new StringContent(data, Encoding.UTF8, MediaTypeToString(mediaType));
        HttpResponseMessage response = await _client.PostAsync(url, stringContent);
        return await response.Content.ReadAsStringAsync();
    }
}

public enum HttpMediaType
{
    ApplicationJSON = 1,
    ApplicationXML = 2,
    ApplicationURLEncoded = 3,
    TextPlain = 4,
    TextXML = 5
}