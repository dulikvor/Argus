using System.ComponentModel.DataAnnotations;

namespace Argus.Common.Clients
{
    public class ServiceHttpClientOptions
    {
        public Uri Endpoint { get; set; }
        public string Audience { get; set; }
    }
}
