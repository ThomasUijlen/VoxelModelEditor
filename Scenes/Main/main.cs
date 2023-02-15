using Godot;
using System;
using VoxelTerrainPlugin;
using System.Collections.Generic;

public partial class main : Sprite2D
{
	BlockLibrary blockLibrary = new BlockLibrary();
	RandomNumberGenerator rng = new RandomNumberGenerator();
	public override void _Ready()
	{
		blockLibrary.AddBlockType(new BlockType(Image.LoadFromFile("res://Textures/Stone.png"), new Color(1,1,1)));
		blockLibrary.AddBlockType(new BlockType(Image.LoadFromFile("res://Textures/Dirt.png"), new Color(1,1,1)));
		blockLibrary.AddBlockType(new BlockType(Image.LoadFromFile("res://Textures/Leaves.png"), new Color(1,1,1)));
		blockLibrary.AddBlockType(new BlockType(
			new Dictionary<BlockType.SIDE, Image>() {
				{BlockType.SIDE.DEFAULT, Image.LoadFromFile("res://Textures/Logs.png")},
				{BlockType.SIDE.TOP, Image.LoadFromFile("res://Textures/Wood.png")},
				{BlockType.SIDE.BOTTOM, Image.LoadFromFile("res://Textures/Wood.png")}
				}, new Color(1,1,1)));

		Texture = ImageTexture.CreateFromImage(blockLibrary.textureAtlas);
	}
}
