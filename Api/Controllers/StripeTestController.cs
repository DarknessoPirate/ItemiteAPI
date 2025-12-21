// Add this file to your Controllers folder
// DELETE THIS FILE BEFORE PRODUCTION DEPLOYMENT!

#if DEBUG
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// TEST CONTROLLER - DELETE BEFORE PRODUCTION!
/// Provides helper endpoints for testing Stripe payments in Swagger
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Remove if you want to test with auth
public class StripeTestController : ControllerBase
{
    private readonly IStripeConnectService _stripeService;
    private readonly ILogger<StripeTestController> _logger;

    public StripeTestController(
        IStripeConnectService stripeService,
        ILogger<StripeTestController> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }



    /// <summary>
    /// Get test card numbers for different scenarios
    /// </summary>
    [HttpGet("test-cards")]
    public IActionResult GetTestCards()
    {
        return Ok(new
        {
            Message = "Use these card numbers to test different scenarios",
            Cards = new[]
            {
                new TestCard
                {
                    Number = "4242424242424242",
                    Description = "Success - No authentication",
                    Brand = "Visa",
                    ExpectedResult = "Payment succeeds immediately"
                },
                new TestCard
                {
                    Number = "4000002500003155",
                    Description = "Success - 3D Secure required",
                    Brand = "Visa",
                    ExpectedResult = "Requires 3D Secure (frontend only)"
                },
                new TestCard
                {
                    Number = "4000000000000002",
                    Description = "Decline - Generic",
                    Brand = "Visa",
                    ExpectedResult = "Card declined"
                },
                new TestCard
                {
                    Number = "4000000000009995",
                    Description = "Decline - Insufficient funds",
                    Brand = "Visa",
                    ExpectedResult = "Insufficient funds"
                },
                new TestCard
                {
                    Number = "4000000000009987",
                    Description = "Decline - Lost card",
                    Brand = "Visa",
                    ExpectedResult = "Card reported lost"
                }
            },
            Note = "All test cards use: Exp: 12/25, CVC: 123, ZIP: Any"
        });
    }

    /// <summary>
    /// Test PESEL numbers for Polish seller onboarding
    /// </summary>
    [HttpGet("test-pesel")]
    public IActionResult GetTestPesel()
    {
        return Ok(new
        {
            Message = "Use these PESEL numbers for testing Polish seller onboarding",
            Pesel = new[]
            {
                new { Number = "000000000", Result = "Success - ID verification passes" },
                new { Number = "111111111", Result = "Fail - Identity mismatch" },
                new { Number = "222222222", Result = "Success - Immediate verification" }
            }
        });
    }

}

public class TestCard
{
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
}
#endif