using System.Net;
using Alpha.App.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Alpha.App.Controllers
{
    public class GetPaymentInfoRequest
    {
        public string? PolicyId { get; set; }

        public string? AgentId { get; set; }
    }

    public class PaymentInfoResponse
    {
        public decimal Amount { get; set; }
    }

    // Mirrors the customer's reported false positive: an ASP.NET Core controller that
    // logs a [FromQuery]-bound request object via `log.LogDebug(..., request.ToJson())`.
    // Snyk Code reports csharp/LogForging with `log.LogDebug` as the sink.
    [ApiController]
    [Route("api/[controller]")]
    public class AgentPaymentController : ControllerBase
    {
        private readonly ILogger<AgentPaymentController> log;

        public AgentPaymentController(ILogger<AgentPaymentController> logger)
        {
            log = logger;
        }

        [HttpGet("PaymentInfoByPolicyId", Name = "GetAgentPaymentInfoByPolicyId")]
        [ProducesResponseType(typeof(PaymentInfoResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetPaymentInfoByPolicyId([FromQuery] GetPaymentInfoRequest request)
        {
            log.LogDebug("GET GetPaymentInfoByPolicyId called: {Request}", request.ToJson());

            await Task.CompletedTask;
            return Ok(new PaymentInfoResponse { Amount = 0m });
        }

        // Second sink at a different log level, to check whether the FQN and any extension
        // registered against it are per-method or shared across ILogger log levels.
        [HttpGet("PaymentInfoByAgentId", Name = "GetAgentPaymentInfoByAgentId")]
        public async Task<IActionResult> GetPaymentInfoByAgentId([FromQuery] GetPaymentInfoRequest request)
        {
            log.LogError("GET GetPaymentInfoByAgentId failed: {Request}", request.ToJson());

            await Task.CompletedTask;
            return Ok(new PaymentInfoResponse { Amount = 0m });
        }
    }
}
