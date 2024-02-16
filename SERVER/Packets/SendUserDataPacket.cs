public class SendUserDataPacket : Packet
{
    public SendUserDataPacket(string id) : base(Action.SendUserData, id)
    {
    }
}