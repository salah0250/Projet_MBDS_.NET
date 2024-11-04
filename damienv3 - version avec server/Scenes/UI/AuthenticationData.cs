using Godot;
using System;
using MessagePack;

[MessagePackObject]
public class AuthenticationData
{
	[Key(0)]
	public string PlayerName { get; set; }

	[Key(1)]
	public string Token { get; set; }
}
