using Microsoft.AspNetCore.Mvc;
using Stateflows.Common;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Transport.AspNetCore.WebApi.Responses;

namespace Stateflows.Transport.AspNetCore.WebApi
{
    [ApiController]
    [Route("[controller]")]
    public class StateflowsController : ControllerBase
    {
        private readonly IBehaviorLocator _locator;

        private readonly IBehaviorClassesProvider _behaviorClassesProvider;

        public StateflowsController(IBehaviorLocator locator, IBehaviorClassesProvider behaviorClassesProvider)
        {
            _locator = locator;
            _behaviorClassesProvider = behaviorClassesProvider;
        }

        [HttpPost("{type}/{name}/{instance}/initialize")]
        public async Task<IActionResult> PostInitialize(string type, string name, string instance)
        {
            InitializationRequest? initializationRequest = null;
            if (Request.Body.CanRead)
            {
                try
                {
                    initializationRequest = await Request.DeserializeObject<InitializationRequest>();
                }
                catch (Exception e)
                {
                    return BadRequest(new StateflowsBadRequestResponse("Unable to parse request data: " + e.Message));
                }

                if (initializationRequest == null)
                {
                    return BadRequest(new StateflowsBadRequestResponse("Unable to parse request data"));
                }
            }

            return (_locator.TryLocateBehavior(new BehaviorId(type, name, instance), out var behavior))
                ? Ok(new StateflowsInitializeResponse(await behavior.InitializeAsync(initializationRequest)))
                : BadRequest(new StateflowsBadRequestResponse("Behavior not found"));
        }

        [HttpGet("{type}/{name}/{instance}/status")]
        public async Task<IActionResult> GetStatus(string type, string name, string instance)
        {
            return (_locator.TryLocateBehavior(new BehaviorId(type, name, instance), out var behavior))
                ? Ok(new StateflowsBehaviorStatusResponse(await behavior.GetStatusAsync()))
                : BadRequest(new StateflowsBadRequestResponse("Behavior not found"));
        }

        [HttpPost("{type}/{name}/{instance}/send")]
        public async Task<IActionResult> PostSend(string type, string name, string instance)
        {
            Event? @event;
            try
            {
                @event = await Request.DeserializeEvent<Event>();
            }
            catch (Exception e)
            {
                return BadRequest(new StateflowsBadRequestResponse("Unable to parse event data: " + e.Message));
            }

            if (@event == null)
            {
                return BadRequest(new StateflowsBadRequestResponse("Unable to parse event data"));
            }

            return (@event != null && _locator.TryLocateBehavior(new BehaviorId(type, name, instance), out var behavior))
                ? Ok(await behavior.SendAsync(@event))
                : BadRequest(new StateflowsBadRequestResponse("Behavior not found"));
        }

        [HttpPost("{type}/{name}/{instance}/request")]
        public async Task<IActionResult> PostRequest(string type, string name, string instance)
        {
            try
            {
                Event? @event;
                try
                {
                    @event = await Request.DeserializeEvent<Event>();
                }
                catch (Exception e)
                {
                    return BadRequest(new StateflowsBadRequestResponse("Unable to parse request data: " + e.Message));
                }

                if (@event == null)
                {
                    return BadRequest(new StateflowsBadRequestResponse("Unable to parse request data"));
                }

                if (!@event.IsRequest())
                {
                    return BadRequest(new StateflowsBadRequestResponse("Request data is invalid"));
                }

                if (_locator.TryLocateBehavior(new BehaviorId(type, name, instance), out var behavior))
                {
                    var result = await behavior.SendAsync(@event);
                    return Ok(new StateflowsRequestResponse(result, @event.GetResponse()));
                }
                else
                {
                    return BadRequest(new StateflowsBadRequestResponse("Behavior not found"));
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("behaviorClasses")]
        public Task<IActionResult> GetBehaviorClasses(bool localOnly = false)
        {
            var classes = localOnly
                ? _behaviorClassesProvider.LocalBehaviorClasses
                : _behaviorClassesProvider.AllBehaviorClasses;

            return Task.FromResult(Ok(classes) as IActionResult);
        }

        [HttpGet("xxx")]
        public Task<IActionResult> GetXXX()
        {
            return Task.FromResult(Ok("yay!") as IActionResult);
        }
    }
}