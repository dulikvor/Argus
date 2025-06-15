using Argus.Common.Data;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using Microsoft.AspNetCore.Http;
using OpenAI.Chat;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Argus.Common.Web
{
    public class StreamReporter
    {
        private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _streamWriter;

        public StreamReporter(
            IResponseStreamWriter<ServerSentEventsStreamWriter> streamWriter)
        {
            _streamWriter = streamWriter;
        }

        public async Task ReportAsync(string message, ChatCompletion chatCompletion, HttpContext context = null)
        {
            if (_streamWriter == null) return;
            var sb = new StringBuilder(message);
            sb.AppendLine();
            sb.AppendLine();
            var msg = new CoPilotChatResponseMessage(sb.ToString(), chatCompletion, true);
            var ctx = context ?? CallContext.GetData("HttpContext") as HttpContext;
            await _streamWriter.WriteToStreamAsync(ctx, new List<object> { msg });
        }

        public async Task ReportAsync(IEnumerable<CoPilotChatResponseMessage> messages, HttpContext context = null)
        {
            if (_streamWriter == null) return;
            var ctx = context ?? CallContext.GetData("HttpContext") as HttpContext;
            await _streamWriter.WriteToStreamAsync(ctx, messages.Cast<object>().ToList());
        }

        public async Task ReportAsync(CopilotConfirmationRequestMessage message, HttpContext context = null)
        {
            if (_streamWriter == null) return;
            var ctx = context ?? CallContext.GetData("HttpContext") as HttpContext;
            await _streamWriter.WriteToStreamAsync(ctx, new List<object> { message }, EventType.CopilotConfirmation);
        }
    }
}
