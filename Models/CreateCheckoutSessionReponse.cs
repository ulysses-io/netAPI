using Newtonsoft.Json;


namespace netAPI.Models
{
  public class CreateCheckoutSessionResponse
  {
      [JsonProperty("sessionId")]
      public string SessionId { get; set; }
  }
}
