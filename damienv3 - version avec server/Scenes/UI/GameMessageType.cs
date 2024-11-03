using Godot;
using System;
using MessagePack;

public enum GameMessageType
{
	JoinRoom,
	Ready,
	GameStart,
	CellSelected,
	PlayerList,
	GameResults
}
