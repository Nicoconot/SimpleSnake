using Godot;
using System;
using System.Collections.Generic;

public partial class SnakeBody : Sprite2D
{
	private Vector2 nextPosition;
	private float nextRotation;
	private SnakeBody nextBody;


	public void Init(Vector2 initialPos, float initialRotation, bool moveDirectly = true)
	{
		if (moveDirectly)
		{
			Position = initialPos;
			Rotation = initialRotation;	
		}
		QueueMovement(initialPos, initialRotation);
	}

	public void AddBody(SnakeBody next)
	{
		nextBody = next;
	}

	public void QueueMovement(Vector2 nextPos, float newRotation)
	{
		nextPosition = nextPos;
		nextRotation = newRotation;
	}

	public void Move()
	{
		var oldPos = Position;
		Position = nextPosition;
		Rotation = nextRotation;
		
		if (nextBody != null)
		{
			nextBody.QueueMovement(oldPos, Rotation);
			nextBody.Move();
		}
	}

	public void AddNext(SnakeBody newBody)
	{
		nextBody = newBody;
	}
}
