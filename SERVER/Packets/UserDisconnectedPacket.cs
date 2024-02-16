public class UserDisconnectedPacket : Packet
{
    public UserDisconnectedPacket(string id) : base(Action.UserDisonnected, id)
    {
    }
}