using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelPlugin {

public partial class BlockType : Resource {
	public Dictionary<SIDE, BlockTexture> textures = new Dictionary<SIDE, BlockTexture>();
	public Dictionary<SIDE, BlockTexture> textureTable = new Dictionary<SIDE, BlockTexture>() {
		{SIDE.TOP, null},
		{SIDE.BOTTOM, null},
		{SIDE.FRONT, null},
		{SIDE.BACK, null},
		{SIDE.LEFT, null},
		{SIDE.RIGHT, null}
	};
	public Color modulate;
	public bool rendered = true;
	public bool transparent = false;
	public string name;

	public BlockType(Color modulate, int size, bool transparent = false) {
		textures.Add(SIDE.DEFAULT, new BlockTexture(this, size));
		CreateTextureTable();
		
		this.modulate = modulate;
		this.transparent = transparent;
	}

	public BlockType(Image defaultTexture, Color modulate, bool transparent = false) {
		textures.Add(SIDE.DEFAULT, new BlockTexture(this, defaultTexture));
		CreateTextureTable();

		this.modulate = modulate;
		this.transparent = transparent;
	}

	public BlockType(Dictionary<SIDE, Image> textures, Color modulate, bool transparent = false) {
		foreach(KeyValuePair<SIDE, Image> keyValuePair in textures) {
			this.textures.Add(keyValuePair.Key, new BlockTexture(this, keyValuePair.Value));
		}
		
		CreateTextureTable();
		this.modulate = modulate;
		this.transparent = transparent;
	}

	private void CreateTextureTable() {
		foreach(SIDE side in textureTable.Keys) {
			if(textures.Keys.Contains(side)) {
				textureTable[side] = textures[side];
			} else {
				textureTable[side] = textures[SIDE.DEFAULT];
			}
		}
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

	public BlockTexture(BlockType owner, int size) {
		this.owner = owner;

		texture = Image.Create(size, size, true, Image.Format.Rgba8);

		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				texture.SetPixel(x, y, new Color(1,1,1));
			}
		}
	}

	public string GetData() {
		Image duplicate = (Image) texture.Duplicate();
		int size = duplicate.GetSize().X;
		for(int x = 0; x < size; x++) {
			for(int y = 0; y < size; y++) {
				duplicate.SetPixel(x, y, duplicate.GetPixel(x,y)*owner.modulate);
			}
		}

		return BitConverter.ToString(duplicate.GetData());
	}

	public BlockTexture(BlockType owner, Image texture) {
		this.owner = owner;
		this.texture = texture;
	}
}
}