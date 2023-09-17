using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Snake : Sprite2D
{
	[Export] private Node2D bodyContainer = null;
	[Export] private Polygon2D background;
	[Export] private Node2D food;
	[Export] private Texture2D closedMouthTexture, openMouthTexture;
	
	//Constants
	//the size in pixels of one "tile". It is equal to the head/body/food image size * its size,
	//background dimensions have to be divisible by that number to work
	float tileSize = 40; 
	int currentDirection = 2; //0 - left, 1 - up, 2 - right, 3 - down
	private double moveTime = .8;
	private bool debugFaster = false;

	private List<(int score, float time)> levels = new()
	{
		(1, .7f),
		(3, .6f),
		(6, .5f),
		(10, .4f),
		(16, .3f),
		(30, .2f),
		(50, .1f)
	};
	
	Vector2[] directions = new Vector2[4]
	{
		new Vector2(-1,  0), // left
		new Vector2( 0, -1), // up
		new Vector2( 1,  0), // right
		new Vector2( 0,  1)  // down
	};
	float[] rotations = new float[4]
	{
		-3.141593f,   // left
		-1.570796f,   // up
		0f,           // right
		1.570796f     // down
	};
	
	//Runtime
	private Vector2 initialPos;
	private readonly List<SnakeBody> bodies = new List<SnakeBody>();
	private double timer = 0;
	private bool ready = false;
	private int wallMinX, wallMaxX, wallMinY, wallMaxY;
	private readonly List<Vector2> gridPositions = new();
	private bool moveDirectly = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GameManager.OnGameStarted += Init;
		GameManager.OnGameOver += OnGameOver;
		
		wallMinX = Mathf.RoundToInt(background.Polygon[0].X + tileSize / 2);
		wallMaxX = Mathf.RoundToInt(background.Polygon[1].X - tileSize / 2);
		wallMinY = Mathf.RoundToInt(background.Polygon[0].Y + tileSize / 2);
		wallMaxY = Mathf.RoundToInt(background.Polygon[2].Y - tileSize / 2);
		
		//Find possible positions in grid
		int xSize = wallMaxX - wallMinX;
		int ySize = wallMaxY - wallMinY;

		float xTiles = xSize / tileSize;
		float yTiles = ySize / tileSize;
		
		//GD.Print($"Wall: X min {wallMinX}, X max {wallMaxX}, Y min {wallMinY}, Y max {wallMaxY}, xSize {xSize}, ySize {ySize}, xTiles {xTiles}, yTiles {yTiles}");

		for (int x = 1; x < xTiles; x++)
		{
			for (int y = 1; y < yTiles; y++)
			{
				gridPositions.Add(new Vector2((x * tileSize), (y * tileSize)));
			}
		}

		initialPos = new Vector2(((int)(xTiles / 2) * tileSize), ((int)(yTiles / 2) * tileSize));
		food.Visible = false;
	}

	private void Init()
	{
		GameManager.OnGameStarted -= Init;

		Cleanup();

		if (debugFaster) moveTime /= 2;

		var firstBody = bodyContainer.GetChild<SnakeBody>(0);
		bodies.Add(firstBody);
		firstBody.Init(new Vector2(Position.X - tileSize, Position.Y), firstBody.Rotation);
		var sprite = this as Sprite2D;
		sprite.Texture = closedMouthTexture;
		GD.Print("Snake initialized");
		ready = true;
		moveDirectly = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!ready) return;
		
		timer += delta;

		if (timer >= moveTime)
		{
			MoveHead();
		}
	}
	
	public override void _Input(InputEvent @event)
	{	
		if(@event.IsActionPressed("spawn_body"))
		{
			AddBody();
		}
		
		if(@event.IsActionPressed("move_left"))
		{
			currentDirection = 0;
		}
		else if(@event.IsActionPressed("move_right"))
		{
			currentDirection = 2;	
		}
		else if(@event.IsActionPressed("move_up"))
		{
			currentDirection = 1;
		}
		else if(@event.IsActionPressed("move_down"))
		{
			currentDirection = 3;
		}
	}

	void MoveHead()
	{
		if(moveDirectly) bodies[0].QueueMovement(Position, Rotation);
		
		Position += directions[currentDirection] * tileSize;
		Rotation = rotations[currentDirection];
		timer = 0;

		if (moveDirectly)
		{
			bodies[0].Move();
			moveDirectly = false;
		}
			
		//GD.Print(Position);
			
		if (!CheckIfGameOver()) MoveChildren();
			
		CheckIfAteFood();
		CheckIfCloseToFood();
	}
	async Task MoveChildren()
	{
		await ToSignal(GetTree().CreateTimer(moveTime), SceneTreeTimer.SignalName.Timeout);
		bodies[0].QueueMovement(Position, Rotation);
		bodies[0].Move();
	}

	void AddBody()
	{
		var child = bodies[^1].Duplicate() as Node2D;
		var childscript = child as SnakeBody;

		if (childscript == null)
		{
			GD.PrintErr("Body duplication gone wrong!");
			return;
		}
		bodies[^1].AddNext(childscript);
		childscript.Init(bodies[^1].Position, bodies[^1].Rotation, false);
		
		bodies.Add(childscript);
		bodyContainer.AddChild(child);
	}
	
	void Cleanup()
	{
		Position = initialPos;

		food.Visible = true;
		MoveFood();

		for (int i = 2; i < bodies.Count; i++)
		{
			bodies[i].QueueFree();
		}
		
		AdjustSpeed();
	}
	
	#region Food

	void CheckIfCloseToFood()
	{
		var sprite = this as Sprite2D;
		float xDistance = Mathf.Abs(Position.X - food.Position.X);
		float yDistance = Mathf.Abs(Position.Y - food.Position.Y);

		if (xDistance + yDistance < tileSize * 3)
		{
			sprite.Texture = openMouthTexture;
		}
		else sprite.Texture = closedMouthTexture;
	}
	void CheckIfAteFood()
	{
		if (Position == food.Position)
		{
			Eat();
		}
	}
	void Eat()
	{
		AddBody();
		GameManager.instance.AddScore();
		
		MoveFood();
		
		AdjustSpeed();
	}
	void MoveFood()
	{
		//Find possible positions in grid
		
		List<Vector2> unusedPositions = new(gridPositions);

		unusedPositions.Remove(new Vector2(Position.X , Position.Y));
		foreach (var body in bodies)
		{
			unusedPositions.Remove(new Vector2(body.Position.X , body.Position.Y));
		}

		if (unusedPositions.Count == 0)
		{
			GameManager.instance.GameOver();
			return;
		}
		//Pick one
		int rando = GD.RandRange(0, unusedPositions.Count);

		Vector2 newFoodPos = new Vector2((unusedPositions[rando].X),
			(unusedPositions[rando].Y));
		
		//GD.Print($"Picked tile {rando}, meaning pos {newFoodPos}");

		food.Position = newFoodPos;
	}
	#endregion
	
	#region Progression
	void AdjustSpeed()
	{
		foreach (var level in levels)
		{
			if (GameManager.score == level.score)
			{
				moveTime = level.time / (debugFaster ? 2 : 1);
				moveDirectly = true;
				return;
			}
		}
	}
	bool CheckIfGameOver()
	{
		//Check walls
		if (Position.X < wallMinX || Position.X > wallMaxX || Position.Y < wallMinY || Position.Y > wallMaxY)
		{
			GameManager.instance.GameOver();
			return true;
		}
		//Check colision with body
		if (bodies.Any(body => body.Position == Position))
		{
			GameManager.instance.GameOver();
			return true;
		}

		return false;
	}

	void OnGameOver()
	{
		ready = false;

		GameManager.OnGameStarted += Init;
	}
	#endregion
}
