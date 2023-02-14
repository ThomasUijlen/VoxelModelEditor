using Godot;
using System;

namespace VoxelTerrainPlugin {
public class BlockType {
	public Image texture;
	public Color modulate;
	public Vector2 UVPosition = Vector2.Zero;
	public Vector2 UVSize = Vector2.One;

	public BlockType(Image texture, Color modulate) {
		this.texture = texture;
		this.modulate = modulate;
	}
}
}