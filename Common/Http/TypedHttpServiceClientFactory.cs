namespace Argus.Common.Http
{
    public class TypedHttpServiceClientFactory : ITypedHttpServiceClientFactory
    {
        IHttpClientFactory _httpClientFactory;

        public TypedHttpServiceClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public TIClient Create<TIClient, TClientImplementation>() where TIClient : class
        {
            var client = _httpClientFactory.CreateClient(typeof(TIClient).Name);
            var instance = Activator.CreateInstance(typeof(TClientImplementation), client) as TIClient;
            if (instance == null)
            {
                throw new InvalidOperationException($"Unable to create an instance of {typeof(TIClient).FullName}.");
            }
            return instance;
        }
    }
}
