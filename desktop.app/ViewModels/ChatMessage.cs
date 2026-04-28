using CommunityToolkit.Mvvm.ComponentModel;

namespace desktop.app.ViewModels;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string role = string.Empty;   // "user" | "assistant" | "system"

    [ObservableProperty]
    private string content = string.Empty;

    public bool IsUser => Role == "user";
    public bool IsAssistant => Role == "assistant";
    public bool IsSystem => Role == "system";
}
