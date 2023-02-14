using Godot;
using System;
using VoxelTerrainPlugin;

public partial class main : Sprite2D
{
	public override void _Ready()
	{
		BlockLibrary blockLibrary = new BlockLibrary();

		blockLibrary.AddBlockType(Image.LoadFromFile("res://Stone.png"), new Color(1,1,1));
		blockLibrary.AddBlockType(Image.LoadFromFile("res://Stone.png"), new Color(1,1,0));
		blockLibrary.AddBlockType(Image.LoadFromFile("res://Stone.png"), new Color(1,0,1));
		blockLibrary.AddBlockType(Image.LoadFromFile("res://Stone.png"), new Color(0,1,1));

		Texture = ImageTexture.CreateFromImage(blockLibrary.textureAtlas);
	}
}
