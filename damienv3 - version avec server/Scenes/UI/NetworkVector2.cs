using Godot;
using System;
using MessagePack;

[MessagePackObject]
public struct NetworkVector2
{
	[Key(0)]
	public int X { get; set; }
	
	[Key(1)]
	public int Y { get; set; }

	public NetworkVector2(int x, int y)
	{
		X = x;
		Y = y;
	}

	// Convertir depuis/vers le Vector2 de Godot
	public static NetworkVector2 FromGodotVector(Vector2 v)
	{
		return new NetworkVector2((int)v.X, (int)v.Y);
	}

	public Vector2 ToGodotVector()
	{
		return new Vector2(X, Y);
	}
}
