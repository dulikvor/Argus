using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Reflection;

public class SchemaValidator
{
    public static bool ValidateSchema(string schemaJson, Type actualType)
    {
        // Parse the JSON schema
        var schema = JsonNode.Parse(schemaJson);

        // Extract properties from the schema
        var schemaProperties = schema?["properties"]?.AsObject();
        if (schemaProperties == null) return false;

        // Get properties of the actual type
        var actualProperties = actualType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .ToDictionary(p => p.Name.ToLower(), p => p.PropertyType);

        // Check if all schema properties exist in the actual type
        foreach (var schemaProperty in schemaProperties)
        {
            if (!actualProperties.ContainsKey(schemaProperty.Key.ToLower()))
            {
                Console.WriteLine($"Missing property: {schemaProperty.Key}");
                return false;
            }

            // Verify the type of the property
            var expectedTypeName = schemaProperty.Value?["type"]?.ToString().ToLower() ?? string.Empty;
            if (expectedTypeName != null)
            {
                var actualPropertyType = actualProperties[schemaProperty.Key.ToLower()];

                // Check for array type compatibility
                if (expectedTypeName == "array")
                {
                    if (!actualPropertyType.IsArray &&
                        !(actualPropertyType.IsGenericType && actualPropertyType.GetGenericTypeDefinition() == typeof(List<>)) &&
                        !(typeof(System.Collections.IEnumerable).IsAssignableFrom(actualPropertyType) && actualPropertyType != typeof(string)))
                    {
                        Console.WriteLine($"Type mismatch for property: {schemaProperty.Key}. Expected: array, Actual: {actualType}");
                        return false;
                    }
                }
                else if (!actualPropertyType.Name.ToLower().Equals(expectedTypeName))
                {
                    Console.WriteLine($"Type mismatch for property: {schemaProperty.Key}. Expected: {expectedTypeName}, Actual: {actualType}");
                    return false;
                }
            }
        }

        // Check required fields
        var requiredFields = schema?["required"]?.AsArray()?.Select(r => r.ToString());
        if (requiredFields != null)
        {
            foreach (var requiredField in requiredFields)
            {
                if (!actualProperties.ContainsKey(requiredField.ToLower()))
                {
                    Console.WriteLine($"Missing required field: {requiredField}");
                    return false;
                }
            }
        }

        return true;
    }
}
