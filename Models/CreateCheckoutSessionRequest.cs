using Newtonsoft.Json;

namespace netAPI.Models 
{
  public class CreateCheckoutSessionRequest
  {
    [JsonProperty("priceId")]
    public string PriceId { get; set; }
  }
}