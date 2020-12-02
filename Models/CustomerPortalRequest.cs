using Newtonsoft.Json;

namespace netAPI.Models
{
  public class CustomerPortalRequest
  {
      [JsonProperty("sessionId")]
      public string SessionId { get; set; }
  }
}
