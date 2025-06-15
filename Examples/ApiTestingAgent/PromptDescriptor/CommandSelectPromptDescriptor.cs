using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class CommandSelectPromptDescriptor : BasePromptDescriptor
{
    public override string DescriptorType => nameof(CommandSelectPromptDescriptor);

    public CommandSelectPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts[PromptsConstants.CommandSelect.Keys.RestSelectPromptKey] = @"
        ### 🧠 Command Selection Logic

        #### 🎯 Goal:
        Determine the most appropriate REST API command to execute, based strictly on the user's explicit instructions.

        #### 📄 Context for - Detected Commands With Content:
        Only use commands detected during the **REST Discovery** step. These are available in the context labeled **Detected Commands With Content** and include full method, URI, and request content structure. Never invent or guess commands.

        #### 🧾 Behavior Guidelines:

        - The selected command must match a known command from the above context.
        - You must use the **selected service domain** (from the domain selection step) in the request URI.
        - A command is **valid** only if:
          - All placeholders in the URI are filled with actual values.
          - The URI and method match a known discovered command.
          - The request content (if required) is either empty `{}` or valid JSON as expected by the known command.

        #### ✅ Use Prior Selection If Applicable:
        If a command was previously selected, and the user is only refining it (e.g., filling placeholders, modifying/adding request content), treat this as an update — not a new command. Use the existing command as a base.

        #### 🔁 Use Existing Command Context If Not Selected:
        If the user instruction implies modifying or adding to a known command (e.g., says ""add"" or ""change""), apply the instruction to the most relevant previously known command, even if it was not the last selected one. Do not replace the request content — **merge** or **append** the new values into the existing structure.

        #### 🔄 Detecting Changes:
        Set `commandDiscoveryDetectedInCurrentIteration = true` **only if** the user input results in any change to:
        - HTTP method  
        - URI pattern (route structure)  
        - Placeholder values in the URI  
        - Request content (even a small addition like a new property)

        Otherwise, set `commandDiscoveryDetectedInCurrentIteration = false`.

        #### 🧭 Next Step (instructionsToUser)
        Present a concise Markdown block showing the selected command:

        🛠️ Selected Command
        ```http
        PUT https://{domain}/resource/path
        ```
        ```json
        {
          ""property"": ""value""
        }
        ```

        Never include stray HTML tags such as `</body>` (or any `<…>` tags) in the output—automatically strip them out. End any Markdown code block with exactly three backticks (```), on their own line with no trailing characters or HTML — never output stray tags like </body>.
        If the selected command does not require request content, set the content field to null and do not display a JSON block in the output.

        If the command is invalid:
        Explain what’s missing (e.g., placeholder values, content)
        Suggest exactly what the user needs to provide next

        If no command has been selected yet, respond with:
        🛠️ Selected Command
        No command selected yet.

        🧭 Next Step:
        Please provide the HTTP method, full URI with placeholder values filled (e.g., subscriptionId, workspaceName), and a JSON body if required.

        Example:
        ```http
        PUT /subscriptions/.../tables/{tableName}
        ```
        ```json
        {}
        ```

        #### ⚠️ Do Not Proceed If:
        - No known command matches the user input
        - URI has unresolved placeholders
        → In these cases, return `commandIsValid = false` and guide the user clearly.
        ";

        // Initialize structured responses
        var commandDiscoveryReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                commandIsValid = new
                {
                    type = "boolean",
                    description = "Indicates whether the command is valid. A command is valid only if all placeholders in the URI are filled, and the combination of HTTP method and URI matches a known command from the Detected Commands With Content context."
                },
                instructionsToUser = new
                {
                    type = "string",
                    description = "A clear, user-facing message starting with '### 🛠️ Selected Command' and showing the full HTTP method, resolved request URI, and JSON content. If the command is invalid, the message must explain what is missing or incorrect and guide the user on how to fix it."
                },
                httpMethod = new
                {
                    type = "string",
                    description = "The HTTP method of the selected command (e.g., GET, POST, PUT, DELETE)."
                },
                requestUri = new
                {
                    type = "string",
                    description = "The fully resolved request URI, including the service domain selected during the domain selection step and all required placeholder values."
                },
                content = new
                {
                    type = "string",
                    description = "The JSON body of the selected command. If no content is needed, this should be an empty JSON object '{}'."
                },
                commandDiscoveryDetectedInCurrentIteration = new
                {
                    type = "boolean",
                    description = "Indicates whether the user made any change in this iteration that affected the selected command, including changes to HTTP method, URI pattern, placeholder values, or request content."
                }

            },
            required = new[] { "commandIsValid", "commandDiscoveryDetectedInCurrentIteration", "instructionsToUser", "httpMethod", "requestUri", "content" }
        };

        StructuredResponses.Add<CommandSelectOutput>(PromptsConstants.CommandSelect.Keys.RestSelectReturnedOutputKey, JsonSerializer.Serialize(commandDiscoveryReturnedOutputSchema));
    }
}