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

	public Godot.Collections.Dictionary GetTexturesAsDictionary() {
		Godot.Collections.Dictionary settings = new Godot.Collections.Dictionary();

		Godot.Collections.Dictionary textures = new Godot.Collections.Dictionary();
		foreach(KeyValuePair<SIDE, BlockTexture> keyValuePair in this.textures) {
			textures[(int) keyValuePair.Key] = keyValuePair.Value.texture.GetData();
		}

		CreateTextureTable();

		settings["Textures"] = textures;
		settings["Modulate"] = modulate;
		settings["Rendered"] = rendered;
		settings["Transparent"] = transparent;

		return settings;
	}

	public void SetTexturesFromDictionary(Godot.Collections.Dictionary settings) {
		Godot.Collections.Dictionary<int, byte[]> bytes = (Godot.Collections.Dictionary<int, byte[]>) settings["Textures"];
		
		foreach(KeyValuePair<int, byte[]> textureData in bytes) {
			textures[(SIDE) textureData.Key] = new BlockTexture(this, Image.CreateFromData(BlockLibrary.textureWidth, BlockLibrary.textureWidth, false, Image.Format.Rgba8, textureData.Value));
		}
		CreateTextureTable();

		modulate = (Color) settings["Modulate"];
		rendered = (bool) settings["Rendered"];
		transparent = (bool) settings["Transparent"];
	}
}

public class BlockTexture {
	public Vector2 UVPosition = Vector2.Zero;
	public Vector2 UVSize = Vector2.One;
	public Image texture;
	public BlockType owner;

	public BlockTexture(BlockType owner, int size) {
		this.owner = owner;

		texture = Image.Create(size, size, false, Image.Format.Rgba8);

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