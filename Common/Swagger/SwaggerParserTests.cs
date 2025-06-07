using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Argus.Common.Swagger
{
    [TestClass]
    public class SwaggerParserTests
    {
        [TestMethod]
        public void ParseOperations_OutputsTypeAndEnumOnlySchema()
        {
            // Arrange
            var swaggerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Common", "Swagger", "sample-swagger.json");
            var swaggerJson = File.ReadAllText(swaggerPath);

            // Act
            var operations = SwaggerParser.ParseOperations(swaggerJson);

            // Assert
            foreach (var op in operations)
            {
                Console.WriteLine($"HTTP: {op.HttpMethod}\nURL: {op.Url}");
                if (op.Content != null)
                {
                    Console.WriteLine("Content schema:");
                    Console.WriteLine(op.Content.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    Console.WriteLine("No content schema.");
                }
                Console.WriteLine("-------------------");
            }
        }
    }
}
