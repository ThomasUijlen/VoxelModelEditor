using Godot;
using System;

namespace VoxelPlugin {
public partial class VoxelWorld : Node
{
	[Export]
	public Vector3I chunks = Vector3I.One;

	public override void _Ready()
	{
		for(int x = 0; x < chunks.X; x++) {
            for(int y = 0; y < chunks.Y; y++) {
                for(int z = 0; z < chunks.Z; z++) {
					Chunk chunk = new Chunk();
					chunk.Position = new Vector3(x,y,z)*Chunk.SIZE;
					AddChild(chunk);
				}
			}
		}
	}
}
}