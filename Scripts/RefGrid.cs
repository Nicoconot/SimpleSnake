using Godot;
using System;

public partial class RefGrid : GridContainer
{
	//Simple script to duplicate a base grid block an X amount of times for debug purposes
	[Export] private ColorRect baseRect;
	[Export] private int size = 390;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (baseRect == null) return;
		for (int i = 0; i < size - 1; i++)
		{
			var newChild = baseRect.Duplicate();
			AddChild(newChild);
		}
	}
}
