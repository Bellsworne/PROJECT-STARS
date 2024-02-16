public class ChatPacket : Packet
{
    public ChatPacket(string message) : base(Action.Chat, message)
    {
    }
}