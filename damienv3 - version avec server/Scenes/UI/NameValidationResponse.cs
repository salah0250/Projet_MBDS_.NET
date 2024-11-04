using Godot;
using System;
using MessagePack;

[MessagePackObject]
public class NameValidationResponse
{
	[Key(0)]
	public bool IsValid { get; set; }
}
