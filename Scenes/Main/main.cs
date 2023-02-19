using Godot;
using System;
using VoxelPlugin;
using System.Collections.Generic;

public partial class main : Sprite2D
{
	RandomNumberGenerator rng = new RandomNumberGenerator();
	BlockType stone;
	BlockType log;
	BlockType leaves;
	public override void _Ready()
	{
		rng.Randomize();
		stone = new BlockType(GD.Load<CompressedTexture2D>("res://Textures/Stone.tres").GetImage(), new Color(1,1,1));
		log = new BlockType(
			new Dictionary<SIDE, Image>() {
				{SIDE.DEFAULT, GD.Load<CompressedTexture2D>("res://Textures/Logs.tres").GetImage()},
				{SIDE.TOP, GD.Load<CompressedTexture2D>("res://Textures/LogsTop.tres").GetImage()},
				{SIDE.BOTTOM, GD.Load<CompressedTexture2D>("res://Textures/LogsTop.tres").GetImage()}
				}, new Color(1,1,1));
		leaves = new BlockType(GD.Load<CompressedTexture2D>("res://Textures/Leaves.tres").GetImage(), new Color(1,1,1), true);

		BlockLibrary.AddBlockType("Default", new BlockType(new Color(1,1,1), 16));
		BlockLibrary.AddBlockType("Stone", stone);
		BlockLibrary.AddBlockType("Dirt", new BlockType(GD.Load<CompressedTexture2D>("res://Textures/Dirt.tres").GetImage(), new Color(1,1,1)));
		BlockLibrary.AddBlockType("Grass", new BlockType(GD.Load<CompressedTexture2D>("res://Textures/Grass.tres").GetImage(), new Color(1,1,1)));
		BlockLibrary.AddBlockType("Leaves", leaves);
		BlockLibrary.AddBlockType("Log", log);

		Texture = ImageTexture.CreateFromImage(BlockLibrary.textureAtlas);

		int size = 32;

		for(int x = 0; x < size; x++) {
            for(int y = 0; y < size; y++) {
                for(int z = 0; z < size; z++) {
					Chunk.SetBlock(new Vector3(x,y,z), stone);
				}
			}
		}
	}

	float time = 0.0f;
    public override void _Process(double delta)
    {
		Vector3 cameraPos = GetViewport().GetCamera3D().GlobalPosition;

		if(Input.IsActionPressed("MouseLeft")) {
			Vector3 startCoord = cameraPos - GetViewport().GetCamera3D().GlobalTransform.Basis.Z*40f;
			int radius = 8;

			for(int x = -radius; x < radius; x++) {
				for(int y = -radius; y < radius; y++) {
					for(int z = -radius; z < radius; z++) {
						Vector3 pos = new Vector3(x,y,z);
						float d = pos.DistanceTo(Vector3.Zero);
						if(d > radius) continue;
						Chunk.SetBlock(startCoord+pos, stone);
					}
				}
			}
		}

		if(Input.IsActionPressed("MouseRight")) {
			Vector3 startCoord = cameraPos - GetViewport().GetCamera3D().GlobalTransform.Basis.Z*40f;
			int radius = 8;

			for(int x = -radius; x < radius; x++) {
				for(int y = -radius; y < radius; y++) {
					for(int z = -radius; z < radius; z++) {
						Vector3 pos = new Vector3(x,y,z);
						float d = pos.DistanceTo(Vector3.Zero);
						if(d > radius) continue;
						Chunk.SetBlock(startCoord+pos, log);
					}
				}
			}
		}

		if(Input.IsActionPressed("MouseMiddle")) {
			Vector3 startCoord = cameraPos - GetViewport().GetCamera3D().GlobalTransform.Basis.Z*50f;
			int radius = 12;

			for(int x = -radius; x < radius; x++) {
				for(int y = -radius; y < radius; y++) {
					for(int z = -radius; z < radius; z++) {
						Vector3 pos = new Vector3(x,y,z);
						float d = pos.DistanceTo(Vector3.Zero);
						if(d > radius) continue;
						Chunk.SetBlock(startCoord+pos, BlockLibrary.GetBlockType("Air"));
					}
				}
			}
		}

		// if(Input.IsActionPressed("MouseMiddle")) {
		// 	Vector3 startCoord = cameraPos - GetViewport().GetCamera3D().GlobalTransform.Basis.Z*50f;
		// 	int radius = 12;

		// 	List<Vector3> positions = new List<Vector3>();

		// 	for(int x = -radius; x < radius; x++) {
		// 		for(int y = -radius; y < radius; y++) {
		// 			for(int z = -radius; z < radius; z++) {
		// 				Vector3 pos = new Vector3(x,y,z);
		// 				float d = pos.DistanceTo(Vector3.Zero);
		// 				if(d > radius) continue;
		// 				positions.Add(startCoord+pos);
		// 			}
		// 		}
		// 	}

		// 	Chunk.BulkSet(positions, BlockLibrary.GetBlockType("Air"));
		// }

		time += Convert.ToSingle(delta);
		if(time < 5f) return;
		time = 0.0f;

		// Chunk.SetBlock(new Vector3(rng.RandfRange(0,16),rng.RandfRange(0,16),rng.RandfRange(0,32)), null);
		int size = 100;

		

        // for(int i = 0; i < 3000; i++) {
		// 	Chunk.SetBlock(cameraPos + new Vector3(rng.RandfRange(-size, size),rng.RandfRange(-size, size),rng.RandfRange(-size, size)), stone);
		// 	Chunk.SetBlock(cameraPos + new Vector3(rng.RandfRange(-size, size),rng.RandfRange(-size, size),rng.RandfRange(-size, size)), log);
		// 	Chunk.SetBlock(cameraPos + new Vector3(rng.RandfRange(-size, size),rng.RandfRange(-size, size),rng.RandfRange(-size, size)), leaves);
		// }
    }
}
