using Argus.Common.Builtin.PromptDescriptor;

namespace ApiTestingAgent.PromptDescriptor;
public class RestDiscoveryPromptDescriptor : StringPromptDescriptor
{
    public override string DescriptorType => nameof(RestDiscoveryPromptDescriptor);

    public RestDiscoveryPromptDescriptor()
    {
        // Do NOT call Initialize() here. The base constructor already calls it.
    }

    protected override void Initialize()
    {
        base.Initialize();
        // Initialize prompts
        Prompts[PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey] =@"
        ### 🔍 Currently Discovering Azure Resources

        You are assisting in discovering Azure REST API operations using Swagger (OpenAPI) specifications.

        You are given optional user input that may include a Swagger file URL or other instructions.

        ---

        ### 🔧 TOOL CALL RULE

        Trigger the `fetchSwagger` tool only if the user provides a GitHub link to a Swagger/OpenAPI `.json` file using the GitHub UI format:

        https://github.com/{user}/{repo}/blob/{branch}/{pathToFile}.json

        From this URL, extract:
        - `user` → GitHub username or organization name  
        - `repo` → GitHub repository name  
        - `branch` → Git branch name (e.g., `main`)  
        - `pathToFile` → File path inside the repo (e.g., `swagger/api.json`)

        Use these values to call the `fetchSwagger` tool.

        ⚠️ Do **not** require the user to provide a raw CDN URL. The tool will convert the GitHub link to raw content internally.

        ---

        ### 📊 LOGIC BRANCHES

        1. **If a valid Swagger GitHub URL is detected**:
        - Extract `user`, `repo`, `branch`, and `pathToFile`.
        - Trigger a tool call to `fetchSwagger`.
        - Do **not** produce user-facing output — let the tool result guide the next step.

        2. **If no Swagger URL is given AND no known resources exist**:
        - Instruct the user to provide a Swagger URL in GitHub format.
        - Keep the message short, with an example:
            Please provide a Swagger file URL (e.g., https://github.com/org/repo/blob/main/swagger.json) so we can discover Azure operations.

        3. **If known resources already exist AND no new Swagger is provided**:
        - Summarize the known operations as briefly as possible:
            - Show only the HTTP method(s) and REST route.
            - If multiple operations share a path, group their methods together:
            - GET/PUT/PATCH /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Example/...

        - Then prompt the user with:
            - ✅ Approve these operations
            - ➕ Provide another Swagger URL to discover more

        ---

        ### 🖇️ OUTPUT FORMAT

        Always format your output in clean, labeled **Markdown**.

        #### Examples:

        **When known resources exist:**
        ### 🔍 Currently Discovering Azure Resources

        #### ✅ Known Operations:
        - GET /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Example/resourceA
        - GET/PUT /subscriptions/{subscriptionId}/providers/Microsoft.Example/resourceB

        #### 🧭 Next Step:
        You can **approve these operations** or **provide another Swagger file URL** (e.g., from a GitHub repo) to discover more.

        **When no Swagger is available and nothing is known:**
        ### 🔍 Currently Discovering Azure Resources

        No operations are currently known.

        #### 🧭 Next Step:
        Please provide a Swagger file URL (e.g., from a GitHub repository) that describes Azure REST API operations in JSON format.

        Example: https://github.com/org/repo/blob/main/swagger.json

        ---

        💡 Keep output brief and focused. Do **not** include explanations outside of the structured markdown response.
        ";

        Prompts[PromptsConstants.RestDiscovery.Keys.PostRunSwaggerSummaryPromptKey] = 
        "### 📥 Swagger Fetch Result Summary\n\n" +
        "You are summarizing the result of a recent Swagger file retrieval and parsing operation.\n\n" +
        "You are given:\n" +
        "- The tool method that was called (e.g., `fetchSwagger`)\n" +
        "- The HTTP status and body returned (either successful JSON or error text)\n" +
        "- A list of any detected operations in the form of `{ httpMethod, url }`\n\n" +
        "---\n\n" +
        "### 📊 SUMMARY INSTRUCTIONS\n\n" +
        "1. **If the fetchSwagger call succeeded and operations were parsed**:\\n" +
        "   - Group operations **by route**, and consolidate HTTP methods when multiple operations share the same path.\\n" +
        "   - Each unique path should appear only once, with all its HTTP methods combined.\\n" +
        "   - Do **not** include the request content or response schema — only summarize method(s) and route.\\n\\n" +
        "   Example:\\n" +
        "   ```markdown\\n" +
        "   ### ✅ Successfully Discovered Operations:\\n" +
        "   - GET/POST /subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.App/resourceA\\n" +
        "   - DELETE /subscriptions/{subscriptionId}/providers/Microsoft.App/resourceB\\n" +
        "   ```\\n\\n" +
        "2. **If the fetchSwagger call returned an error**:\n" +
        "   - Summarize the HTTP status code (e.g., 404, 500).\n" +
        "   - Briefly state the error reason as returned from the response body or headers.\n" +
        "   - Provide a helpful, concise message to guide user action.\n\n" +
        "   Example:\n" +
        "   ```markdown\n" +
        "   ### ❌ Swagger Fetch Failed\n" +
        "   **Status:** 404 Not Found  \n" +
        "   **Reason:** The requested file could not be retrieved from GitHub.  \n" +
        "   Please verify that the URL points to a valid `.json` file in a public repository.\n" +
        "   ```\n\n" +
        "3. **If the tool call returned invalid JSON or could not be parsed**:\n" +
        "   - Indicate parsing failed and what might be wrong.\n" +
        "   - Recommend checking the content format.\n\n" +
        "   Example:\n" +
        "   ```markdown\n" +
        "   ### ⚠️ Swagger Parsing Error\n" +
        "   The file was retrieved, but its contents could not be parsed as a valid OpenAPI JSON file.  \n" +
        "   Please ensure the file follows Swagger/OpenAPI 2.0 or 3.0 JSON format.\n" +
        "   ```\n\n" +
        "---\n\n" +
        "💡 Always return clean, labeled **Markdown**. Keep it concise and actionable. Do not repeat the tool result verbatim unless it adds clarity.";
    }
}
