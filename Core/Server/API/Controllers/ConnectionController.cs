using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Status;

namespace Vint.Core.Server.API.Controllers;

public class ConnectionController : IApiController {
    [MessageId(36)]
    public IClientDTO SetSubscriptions(ApiConnection connection, Subscriptions subscriptions) {
        connection.Subscriptions = subscriptions;
        return SuccessDTO.NoContent();
    }
}
