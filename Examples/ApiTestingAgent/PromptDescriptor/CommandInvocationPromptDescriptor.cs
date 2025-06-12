using ApiTestingAgent.StructuredResponses;
using Argus.Common.Builtin.PromptDescriptor;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class CommandInvocationPromptDescriptor : StringPromptDescriptor
{
    public override string DescriptorType => nameof(CommandInvocationPromptDescriptor);

    public CommandInvocationPromptDescriptor()
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
        // Initialize prompts
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationAnalysisPromptKey] =
            "📊 Analyze the result of a recently invoked command using the information provided in the context.\n" +
            "\n" +
            "### ✅ MANDATORY PRECONDITIONS\n" +
            "- Both a selected command and a command result **must** be present in the context.\n" +
            "- The command result includes:\n" +
            "  - `HttpStatus`\n" +
            "  - `Content` (optional)\n" +
            "\n" +
            "### 🎯 GOAL\n" +
            "- If an expected outcome is defined in the context, analyze the actual result against the expected outcome.\n" +
            "- If the actual result does **not** match the expected:\n" +
            "  - Report what was expected.\n" +
            "  - Report what was the actual outcome.\n" +
            "  - Explain clearly the difference between the two.\n" +
            "- If there is no expected outcome, summarize the result briefly and clearly.\n" +
            "\n" +
            "### 🧾 OUTPUT STRUCTURE\n" +
            "Return a **valid JSON object** with the following shape:\n" +
            "{\n" +
            "  \"analysis\": \"<Full user-facing message, including all required sections>\"\n" +
            "}\n" +
            "\n" +
            "### 📋 CONTENT OF `analysis` FIELD\n" +
            "The `analysis` value must be a single string containing these **four labeled sections**, each with a relevant emoji icon:\n" +
            "1. 🧭 **Selected Command** – Summarize the invoked command (method, URI, and body).\n" +
            "2. 🎯 **Expected Outcome** – If it exists, show the expected status and content.\n" +
            "3. 📬 **Actual Result** – Present the actual HTTP status and returned content.\n" +
            "4. 🧠 **Analysis** – Provide a short, informative summary or expected vs. actual comparison.\n" +
            "\n" +
            "Use Markdown-style formatting to improve clarity:\n" +
            "- Bold headers with emojis.\n" +
            "- Inline code formatting for URI, method, and status using backticks.\n" +
            "- JSON payloads in triple backtick blocks.\n" +
            "\n" +
            "### ❌ IMPORTANT RULES\n" +
            "- Do **not** guess or fabricate missing data.\n" +
            "- Do **not** perform the analysis unless both a selected command and a result exist.\n" +
            "- Do **not** return anything other than the required JSON object.\n" +
            "\n" +
            "### 📌 EXAMPLE OUTPUT FORMAT\n" +
            "{\n" +
            "  \"analysis\": \"🧭 **Selected Command:**\\nHTTP PUT to `https://localhost:5001/subscriptions/bbf99725-4174-4a55-a11c-94cf2eea98a6/resourceGroups/DudiTest/providers/Microsoft.OperationalInsights/workspaces/dudi-kuku3/tables/Perf`\\nBody:\\n```json\\n{\\n  \\\"properties\\\": {\\n    \\\"totalRetentionInDays\\\": 100\\n  },\\n  \\\"systemData\\\": {}\\n}\\n```\\n\\n🎯 **Expected Outcome:**\\n_No specific expected outcome was defined._\\n\\n📬 **Actual Result:**\\nHTTP Status: `0`\\nContent:\\n```\nAn error occurred while invoking the command: No connection could be made because the target machine actively refused it. (localhost:5001).\\n```\\n\\n🧠 **Analysis:**\\nThe command failed due to a connection issue with the target service at `localhost:5001`. This typically means the service is not running or is actively rejecting connections. Since no expected outcome was defined, this is reported as a connectivity failure that blocked execution.\"\n" +
            "}\n";

        // Add a prompt to explain the result of a REST API invocation to the user
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationHttpResultExplanationPromptKey] =
            "You are to explain the result of a REST API invocation to the user.\n" +
            "Given the HTTP status code and the content returned by the API, provide a clear, concise explanation of what the result means.\n" +
            "If the status indicates success, summarize the returned content.\n" +
            "If the status indicates an error, explain the error and what it means for the user.\n" +
            "Always use user-friendly language and avoid technical jargon where possible.";

        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationPromptKey] = @"
        You are a REST API assistant.
        A command has been selected. It includes:
        - `httpMethod`: the HTTP method (e.g., GET, POST)
        - `url`: the full endpoint URL
        - `content`: an optional object representing the body payload

        Your task is to generate a valid JSON input for the `RestTool` with the following structure:
        {
          ""method"": ""GET | POST | ..."", 
          ""url"": ""https://..."", 
          ""headers"": { ... },           // optional 
          ""body"": ""...""               // optional, must be a string
        }

        Instructions:
        1. Copy `httpMethod` to `method`, and `url` to `url`.
        2. If `content` exists, set:
           - `headers` to include ""Content-Type"": ""application/json""
           - `body` to a stringified JSON version of `content`
        3. If `content` is missing, omit both `headers` and `body`.
        4. Output ONLY the final JSON object. Do NOT include any explanation or surrounding text.
        ";

        // Add a prompt to detect user intent for state transitions
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStatePromptKey] =
            "🧭 Analyze the user's input and determine the next workflow state based on these rules:\n" +
            "\n" +
            "### 🔄 TRANSITION LOGIC\n" +
            "1. If the user explicitly asks to change or select the command (URI, method, or body), return `CommandSelect`.\n" +
            "2. If the user explicitly asks to define or change the expected outcome (status code, response content, or error), return `ExpectedOutcomeSelect`.\n" +
            "3. If no command has been selected yet (not just available), return `CommandSelect`. Use the \"Currently Selected Command\" line in the context to determine whether a command was already selected.\n" +
            "4. Do **not** require an expected outcome unless the user explicitly provides one.\n" +
            "5. If the user explicitly says to proceed (e.g., \"run\", \"execute\", \"go ahead\", \"I accept\"), and a command has already been selected, return `CommandInvocation`.\n" +
            "   If no command has been selected, treat this as implicit confirmation and return CommandSelect to initiate selection.\n" +
            "6. If a command result already exists in the context, return an empty string `\"\"` and do not transition.\n" +
            "\n" +
            "### 🚫 IMPORTANT RULES\n" +
            "- Do **not** infer intent from prior context or memory.\n" +
            "- Only transition state if the user **explicitly** indicates intent.\n" +
            "- Do **not** assume the user wants to define an expected outcome unless clearly stated.\n" +
            "\n" +
            "### 🧾 OUTPUT FORMAT\n" +
            "Respond strictly with a JSON object containing **only** this field:\n" +
            "- `nextState`: One of `CommandSelect`, `ExpectedOutcomeSelect`, `CommandInvocation`, or `\"\"` (empty string).\n" +
            "\n" +
            "### ❌ Do not return any other fields or text.\n";

        var commandInvocationAnalysisReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                analysis = new { type = "string", description = "Full user-facing message, including all required sections." }
            },
            required = new[] { "analysis" }
        };

        StructuredResponses.Add<CommandInvocationAnalysisOutput>(PromptsConstants.CommandInvocation.Keys.CommandInvocationAnalysisReturnedOutputKey,
            JsonSerializer.Serialize(commandInvocationAnalysisReturnedOutputSchema));

        // Output schema for detecting next state and returning current status
        var commandInvocationDetectNextStateOutputSchema = new
        {
            type = "object",
            properties = new
            {
                nextState = new { type = "string", description = "The next state to transition to. One of: 'CommandSelect', 'ExpectedOutcome', or 'None'." },
                reasoning = new { type = "string", description = "A brief explanation of the logic and evidence leading to the nextState decision." }
            },
            required = new[] { "nextState", "reasoning" }
        };

        StructuredResponses.Add<CommandInvocationDetectNextStateOutput>(PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStateOutputKey,
            JsonSerializer.Serialize(commandInvocationDetectNextStateOutputSchema));
    }
}