using Godot;
using System;
using System.Threading.Tasks;
using MessagePack;

[MessagePackObject]
public class ReadyCheckResponse
{
	[Key(0)]
	public bool IsReady { get; set; }
	
	[Key(1)]
	public string Message { get; set; } = string.Empty;
}
