# Brian's Claude for Visual Studio

A Visual Studio 2022/2026 extension that replaces GitHub Copilot with [Claude](https://claude.ai) (Anthropic) as your primary AI assistant.

Bring your own Anthropic API key — no subscription required beyond what you pay Anthropic directly.

## Features

- **Inline completions** — Ghost-text AI suggestions as you type (Tab to accept)
- **Chat window** — Side panel for conversational AI assistance (View → Brian's Claude Chat)
- **Slash commands** — `/explain`, `/fix`, `/tests`, `/doc`, `/refactor` with editor selection context
- **Code actions** — Lightbulb menu and right-click → Quick Actions integration

## Getting Started

1. Install the extension from the [Visual Studio Marketplace](https://marketplace.visualstudio.com)
2. Get an API key from [console.anthropic.com](https://console.anthropic.com)
3. Open **Tools → Options → Brian's Claude → General**
4. Paste your API key and click **Verify Key**
5. Open the chat window via **View → Brian's Claude Chat**

## Slash Commands

Type any of these in the chat window — they include your current editor selection as context automatically.

| Command | Description |
|---|---|
| `/explain` | Explain the selected code |
| `/fix` | Fix bugs or issues in the selection |
| `/tests` | Generate unit tests for the selection |
| `/doc` | Add XML documentation comments |
| `/refactor` | Suggest refactoring improvements |

## Configuration

All settings are in **Tools → Options → Brian's Claude → General**:

- **Anthropic API Key** — encrypted with DPAPI, never stored in plaintext
- **Chat Model** — Opus (best quality), Sonnet (balanced), or Haiku (fastest)
- **Inline Completion Model** — defaults to Haiku for speed
- **Enable Inline Completions** — toggle ghost-text suggestions on/off
- **Max Chat Context** — token budget for conversation history (2,000–16,000)

## Building from Source

Requires Visual Studio 2022 or later with the **Visual Studio extension development** workload.

```powershell
git clone https://github.com/BrianProgrammer/BriansClaudeForVS
cd BriansClaudeForVS
nuget restore BriansClaudeForVS.sln
msbuild BriansClaudeForVS.sln /p:Configuration=Debug
# Press F5 in VS to launch the Experimental Instance
```

To run tests:
```powershell
dotnet test src/BriansClaudeVS.Tests/
```

## Security

Your API key is encrypted using Windows DPAPI (current-user scope) before being stored in the Visual Studio settings store. The plaintext key exists only in memory during active API calls.

## License

MIT — see [LICENSE](LICENSE).

## Contributing

Pull requests welcome. Please open an issue first for significant changes.
