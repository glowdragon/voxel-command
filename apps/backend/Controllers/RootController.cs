using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace VoxelCommand.Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController(AnthropicClient anthropicClient) : ControllerBase
    {
        private readonly AnthropicClient _anthropicClient = anthropicClient;

        [HttpGet]
        public async Task<ActionResult<object>> Get()
        {
            var messages = new List<Message>() { new(RoleType.User, "What is 2 + 2?") };

            var parameters = new MessageParameters()
            {
                Messages = messages,
                MaxTokens = 1024,
                Model = AnthropicModels.Claude3Haiku,
                Stream = false,
                Temperature = 1.0m,
            };
            var result = await _anthropicClient.Messages.GetClaudeMessageAsync(parameters);

            return Ok(new { Response = result.Message.ToString() });
        }
    }
}
