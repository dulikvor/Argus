namespace Argus.Common.StructuredResponses
{
    public class StructuredResponsesDictionary
    {
        private readonly Dictionary<string, string> _structuredResponses;
        public StructuredResponsesDictionary()
        {
            _structuredResponses = new Dictionary<string, string>();
        }
        public void Add<TStructureResponse>(string key, string value)
        {
            if(!SchemaValidator.ValidateSchema(value,typeof(TStructureResponse)))
            {
                throw new ArgumentException($"The value does not match the schema for {typeof(TStructureResponse).Name}");
            }

            _structuredResponses.Add(key, value);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_structuredResponses.TryGetValue(key, out value))
            {
                return true;
            }
            return false;
        }
    }
}
