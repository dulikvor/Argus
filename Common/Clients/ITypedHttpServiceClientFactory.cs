namespace Argus.Common.Clients
{
    public interface ITypedHttpServiceClientFactory
    {
        TIClient Create<TIClient, TClientImplementation>() where TIClient : class;
    }
}
