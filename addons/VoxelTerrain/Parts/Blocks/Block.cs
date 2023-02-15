using Godot;
using System;

namespace VoxelPlugin {
	public enum SIDE {DEFAULT, TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK};

public partial class Block : Resource {
	[Signal]
	public delegate void BlockChangedEventHandler();

	public Vector3I coord;
	public Vector3 position;
	public BlockType blockType;
	public Chunk chunk;

	public Block(Chunk chunk) {
		this.chunk = chunk;
	}

	public void SetBlockType(BlockType blockType) {
		this.blockType = blockType;
		chunk.Update();
	}

	public static Vector3 SideToVector(SIDE side) {
		switch(side) {
			case SIDE.TOP: return Vector3.Up;
			case SIDE.BOTTOM: return Vector3.Down;
			case SIDE.LEFT: return Vector3.Left;
			case SIDE.RIGHT: return Vector3.Right;
			case SIDE.FRONT: return Vector3.Forward;
			case SIDE.BACK: return Vector3.Back;
		}

		return Vector3.Zero;
	}
}
}