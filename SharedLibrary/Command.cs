using MessagePack;
using System.Collections.Generic;

[MessagePackObject]
public class Command
{
    [Key(0)]
    public string Type { get; set; }

    [Key(1)]
    public Dictionary<string, string> Data { get; set; }  // Changed to <string, string>

    public Command()  // Add parameterless constructor for MessagePack
    {
        Data = new Dictionary<string, string>();
    }

    public Command(string type) : this()  // Constructor that calls the parameterless one
    {
        Type = type;
    }
}