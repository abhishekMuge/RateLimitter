using System.Text.Json.Serialization;

public class ValidatePacket
{
    [JsonPropertyName("USerID")]
    public string UserId { get; set; }

    [JsonPropertyName("APIKey")]
    public string ApiKey { get; set; }
}

public class ForwardPacket
{
    public string Payload { get; set; }
    public string EndPoint { get; set; }
    public DateTime TimeStamp { get; set; }
}

public class RequestPacket
{
    [JsonPropertyName("ValidatePacket")]
    public ValidatePacket ValidatePacket { get; set; }

    [JsonPropertyName("ForwordPacket")]
    public ForwardPacket ForwardPacket { get; set; }
}