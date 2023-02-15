using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelTerrainPlugin {
public partial class BlockType : Resource {
	public enum SIDE {DEFAULT, TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK}
	public Dictionary<SIDE, BlockTexture> textures = new Dictionary<SIDE, BlockTexture>();
	public Color modulate;

	

	public BlockType(Image defaultTexture, Color modulate) {
		textures.Add(SIDE.DEFAULT, new BlockTexture(this, defaultTexture));
		this.modulate = modulate;
	}

	public BlockType(Dictionary<SIDE, Image> textures, Color modulate) {
		foreach(KeyValuePair<SIDE, Image> keyValuePair in textures) {
			this.textures.Add(keyValuePair.Key, new BlockTexture(this, keyValuePair.Value));
		}
		
		this.modulate = modulate;
	}

	public List<BlockTexture> GetAllTextures() {
		return textures.Values.ToList<BlockTexture>();
	}
}

public class BlockTexture {
	public Vector2 UVPosition = Vector2.Zero;
	public Vector2 UVSize = Vector2.One;
	public Image texture;
	public BlockType owner;

	public BlockTexture(BlockType owner, Image texture) {
		this.owner = owner;
		this.texture = texture;
	}
}
}