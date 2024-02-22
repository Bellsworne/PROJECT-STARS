using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

public class Packet
{
    public string Action { get; set; }
    public List<object> Payloads { get; set; }

    public Packet(string action, List<object> payloads)
    {
        Action = action;
        Payloads = payloads;
    }

    public override string ToString()
    {
        var serializedDict = new Dictionary<string, object> { { "a", Action } };
        for (int i = 0; i < Payloads.Count; i++)
        {
            serializedDict.Add($"p{i}", Payloads[i]);
        }
        string data = JsonConvert.SerializeObject(serializedDict);
        return data;
    }

    public static byte[] CreatePacket(Packet packet)
    {
        string data = packet.ToString();
        return System.Text.Encoding.UTF8.GetBytes(data);
    }

    public static (string, List<object>) JsonToActionPayloads(string jsonString)
    {
        var objDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

        string action = null;
        List<object> payloads = new List<object>();
        foreach (var item in objDict)
        {
            if (item.Key == "a")
            {
                action = item.Value.ToString();
            }
            else if (item.Key.StartsWith("p"))
            {
                int index = int.Parse(item.Key.Substring(1));
                while (payloads.Count <= index)
                {
                    payloads.Add(null);
                }
                payloads[index] = item.Value;
            }
        }

        return (action, payloads);
    }
}