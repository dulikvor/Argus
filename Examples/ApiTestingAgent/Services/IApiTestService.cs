﻿using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.Services
{
    public interface IApiTestService
    {
        public Task InvokeNext(HttpContext httpContext, CoPilotChatRequestMessage coPilotChatRequestMessage);
    }
}
