using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

[MessagePackObject]
public class GameMessage
{
	[Key(0)]
	public GameMessageType Type { get; set; }

	[Key(1)]
	public string Gamemaster { get; set; }

	[Key(2)]
	public NetworkVector2 CellPosition { get; set; }

	[Key(3)]
	public List<string> PlayersList { get; set; }

	[Key(4)]
	public List<PlayerRanking> Rankings { get; set; }

	public GameMessage()
	{
		PlayersList = new List<string>();
		Rankings = new List<PlayerRanking>();
		CellPosition = new NetworkVector2();
	}
}
