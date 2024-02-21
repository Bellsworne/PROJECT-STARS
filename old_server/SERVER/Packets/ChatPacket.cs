public class ChatPacket : Packet
{
    public ChatPacket(string username, string message) : base(Action.Chat, username, message)
    {
    }
}