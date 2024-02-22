using Godot;
using System;

public class TEST_CLIENT_UI : Control
{
    private RichTextLabel _chatBox;
    private LineEdit _messageTextInput;
    private LineEdit _addressTextInput;
    private LineEdit _portTextInput;
    private LineEdit _usernameTextInput;
    private Button _sendButton;
    private Button _joinButton;
    private RichTextLabel _connectedUsers;

    /// <summary>
    /// Emmited when the "send" button is pressed. (string: Message)
    /// </summary>
    public event Action<string> SendMessage;

    /// <summary>
    /// Emmited when the "join" button is pressed. (string: Address, int: Port, string: Username)
    /// </summary>
    public event Action<string, int, string> JoinServer;

    /// <summary>
    /// Gets the address from the address input field.
    /// </summary>
    public string Address => _addressTextInput.Text;
    public int Port => int.Parse(_portTextInput.Text);
    public string Username => _usernameTextInput.Text;

    public override void _Ready()
    {
        _chatBox = GetNode<RichTextLabel>("Panel/ChatBox");
        _messageTextInput = GetNode<LineEdit>("Panel/MessageTextInput");
        _addressTextInput = GetNode<LineEdit>("Panel/VBox/AddressTextInput");
        _portTextInput = GetNode<LineEdit>("Panel/VBox/PortNumberInput");
        _usernameTextInput = GetNode<LineEdit>("Panel/VBox/UsernameTextInput");
        _sendButton = GetNode<Button>("Panel/SendButton");
        _joinButton = GetNode<Button>("Panel/VBox/JoinButton");
        _connectedUsers = GetNode<RichTextLabel>("Panel/ConnectedUsers");

        _sendButton.Connect("pressed", this, nameof(OnSendButtonPressed));
        _joinButton.Connect("pressed", this, nameof(OnJoinButtonPressed));

    }

    private void OnSendButtonPressed()
    {
        string message = _messageTextInput.Text;
        if (message.Length > 0)
        {
            SendMessage.Invoke(message);
            _messageTextInput.Text = "";
        }
    }

    private void OnJoinButtonPressed()
    {
        if (Address.Length == 0 || Port == 0 || Username.Length == 0) return;
        
        JoinServer.Invoke(Address, Port, Username);
    }

    public void AddMessage(string message)
    {
        _chatBox.BbcodeText += $"{message}\n";
    }

    public void AddConnectedUser(string username)
    {
        _connectedUsers.BbcodeText += $"{username}\n";
    }

    public void RemoveConnectedUser(string username)
    {
        _connectedUsers.BbcodeText = _connectedUsers.Text.Replace($"{username}\n", "");
    }

    public void LogMessageToChat(string message)
    {
        _chatBox.BbcodeText += $"[color=green]{message}[/color]\n";
    }

    public void LogErrorToChat(string message)
    {
        _chatBox.BbcodeText += $"[color=red]Error: {message}[/color]\n";
    }
}
