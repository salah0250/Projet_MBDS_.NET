using Godot;
using System;
using MessagePack;

[MessagePackObject]
public class PlayerRanking
{
	[Key(0)]
	public string PlayerName { get; set; } = string.Empty;
	
	[Key(1)]
	public int Position { get; set; }
	
	[Key(2)]
	public bool IsEligible { get; set; }
}
