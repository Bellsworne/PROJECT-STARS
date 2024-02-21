public class AddUserPacket : Packet
{
    public AddUserPacket(string id, string username) : base(Action.AddUser, id, username) { }
}