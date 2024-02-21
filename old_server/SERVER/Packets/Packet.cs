using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;


public enum Action
{
    /// <summary>
    /// Used when a client is sending the server their username
    /// </summary>
    Login,
    /// <summary>
    /// Used after a Login, sends the client back their unique ID
    /// </summary>
    SendUserData,
    /// <summary>
    /// Used when a client disconnects
    /// </summary>
    UserDisonnected,
    /// <summary>
    /// Used when a client connects after login
    /// </summary>
    AddUser,
    /// <summary>
    /// Used to send a Chat message
    /// </summary>
    Chat,
}

public class Packet
{
    public Action Action { get; set; }
    public string[] Payloads { get; set; }

    public Packet(Action action, params string[] payloads)
    {
        Action = action;
        Payloads = payloads;
    }

    public override string ToString()
    {
        var serializedDict = new Dictionary<string, object> { { "a", Action.ToString() } };
        for (int i = 0; i < Payloads.Length; i++)
        {
            serializedDict.Add($"p{i}", Payloads[i]);
        }
        string data = JsonConvert.SerializeObject(serializedDict);
        return data;
    }

    public byte[] ToBytes()
    {
        return System.Text.Encoding.UTF8.GetBytes(this.ToString());
    }
}

public static class PacketFactory
{
    public static Packet FromJson(string jsonStr)
    {
        var objDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);

        if (objDict == null)
        {
            GD.Print($"JSON object is null");
            return null;
        }
        
        Action action = 0;
        List<string> payloads = new List<string>();
        foreach (var item in objDict)
        {
            if (item.Key == "a")
            {
                action = (Action)Enum.Parse(typeof(Action), item.Value.ToString());
            }
            else if (item.Key.StartsWith("p"))
            {
                int index = int.Parse(item.Key.Substring(1));
                while (payloads.Count <= index)
                {
                    payloads.Add(null);
                }
                payloads[index] = item.Value.ToString();
            }
        }

        // Construct the packet
        string className = action + "Packet";
        try
        {
            Type constructor = Type.GetType(className);
            Packet packet = (Packet)Activator.CreateInstance(constructor, payloads.ToArray());
            GD.Print($"Constructed packet: {packet}");
            return packet;
        }
        catch (Exception e)
        {
            GD.PrintErr($"{className} is not a valid packet name or can't handle arguments {string.Join(", ", payloads)}.\nStacktrace: {e}");
            return null;
        }
    }
}