using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Newtonsoft.Json;
using netAPI.Models;

// This example sets up an endpoint using the ASP.NET MVC framework.
// Watch this video to get started: https://youtu.be/2-mMOB8MhmE.
namespace netAPI.Controllers
{
  
  [ApiController]
  public class PaymentsController : Controller
  {
    public readonly IOptions<StripeOptions> options;
    private readonly IStripeClient client;

    public PaymentsController(IOptions<StripeOptions> options)
    {
        this.options = options;
        this.client = new StripeClient(this.options.Value.SecretKey);
    }

    [HttpGet("setup")]
    public SetupResponse Setup()
    {
        return new SetupResponse
        {
            ProPrice = this.options.Value.ProPrice,
            BasicPrice = this.options.Value.BasicPrice,
            PublishableKey = this.options.Value.PublishableKey,
        };
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest req)
    {
        var options = new SessionCreateOptions
        {
            SuccessUrl = $"{this.options.Value.Domain}/success.html?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{this.options.Value.Domain}/cancel.html",
            PaymentMethodTypes = new List<string>
            {
                "card",
            },
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = req.PriceId,
                    Quantity = 1,
                },
            },
        };
        var service = new SessionService(this.client);
        try
        {
            var session = await service.CreateAsync(options);
            return Ok(new CreateCheckoutSessionResponse
            {
                SessionId = session.Id,
            });
        }
        catch (StripeException e)
        {
            Console.WriteLine(e.StripeError.Message);
            return BadRequest(new ErrorResponse
            {
                ErrorMessage = new ErrorMessage
                {
                    Message = e.StripeError.Message,
                }
            });
        }
    }

    [HttpGet("checkout-session")]
    public async Task<IActionResult> CheckoutSession(string sessionId)
    {
        var service = new SessionService(this.client);
        var session = await service.GetAsync(sessionId);
        return Ok(session);
    }

    [HttpPost("customer-portal")]
    public async Task<IActionResult> CustomerPortal([FromBody] CustomerPortalRequest req)
    {
        // For demonstration purposes, we're using the Checkout session to retrieve the customer ID. 
        // Typically this is stored alongside the authenticated user in your database.
        var checkoutSessionId = req.SessionId;
        var checkoutService = new SessionService(this.client);
        var checkoutSession = await checkoutService.GetAsync(checkoutSessionId);

        // This is the URL to which your customer will return after
        // they are done managing billing in the Customer Portal.
        var returnUrl = this.options.Value.Domain;

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = checkoutSession.CustomerId,
            ReturnUrl = returnUrl,
        };
        var service = new Stripe.BillingPortal.SessionService(this.client);
        var session = await service.CreateAsync(options);

        return Ok(session);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                this.options.Value.WebhookSecret
            );
            Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something failed {e}");
            return BadRequest();
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
            Console.WriteLine($"Session ID: {session.Id}");
            // Take some action based on session.
        }

        return Ok();
    }
  }
  
}
/*
namespace netAPI.Controllers
{
  
  [Route("create-payment-intent")]
  [ApiController]
  public class PaymentsController : Controller
  {
    public PaymentsController()
    {
      // Set your secret key. Remember to switch to your live secret key in production!
    // See your keys here: https://dashboard.stripe.com/account/apikeys
    StripeConfiguration.ApiKey = "sk_test_51HrRklGYoaaAuwGvYAXpUkWZ5hC7bvz0Irehbsprg3CQ2zYpAiZzouDG1DtG3m3ZyD5eN3Y7P9wlhCugQ1ippvDg009wi247g1";
    }

    [HttpPost]
    public ActionResult Create(PaymentIntentCreateRequest request)
    {
      var paymentIntents = new PaymentIntentService();
      var paymentIntent = paymentIntents.Create(new PaymentIntentCreateOptions
      {
        Amount = CalculateOrderAmount(request.Shirts),
        Currency = "usd",
      });
      return Json(new { clientSecret = paymentIntent.ClientSecret });
    }

    private int CalculateOrderAmount(Shirt[] items)
    {
      // Replace this constant with a calculation of the order's amount
      // Calculate the order total on the server to prevent
      // people from directly manipulating the amount on the client
      return 1400;
    }

    public class Shirt
    {
      [JsonProperty("id")]
      public string Id { get; set; }
    }
    public class PaymentIntentCreateRequest
    {
      [JsonProperty("items")]
      public Shirt[] Shirts { get; set; }
    }
  }


}*/

