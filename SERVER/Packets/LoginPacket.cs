public class LoginPacket : Packet
{
    public LoginPacket(string username) : base(Action.Login, username)
    {
    }
}