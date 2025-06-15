namespace Argus.Common.Http
{
    public interface ITypedHttpServiceClientFactory
    {
        TIClient Create<TIClient, TClientImplementation>() where TIClient : class;
    }
}
