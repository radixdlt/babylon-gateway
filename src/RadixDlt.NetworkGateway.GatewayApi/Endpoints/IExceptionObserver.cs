using Microsoft.AspNetCore.Mvc;
using RadixDlt.NetworkGateway.GatewayApi.Exceptions;
using System;

namespace RadixDlt.NetworkGateway.GatewayApi.Endpoints;

public interface IExceptionObserver
{
    void OnExceptionXxx(ActionContext actionContext, Exception exception, KnownGatewayErrorException gatewayErrorException);
}
