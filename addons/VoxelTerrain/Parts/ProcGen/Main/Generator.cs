using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public class Generator
{
	public List<Generator> children = new List<Generator>();
	public virtual void Generate(Chunk chunk) {
		foreach(Generator generator in children) {
			generator.Generate(chunk);
		}
	}

	public virtual void ApplySettings(Godot.Collections.Dictionary<String, Variant> data) {
		// Create children
		Godot.Collections.Array children = (Godot.Collections.Array) data["Children"];
		foreach(Godot.Collections.Dictionary<String, Variant> childData in children) {
			Generator child = GetGenerator((String) childData["Name"]);
			if(child == null) continue;

			this.children.Add(child);
			child.ApplySettings(childData);
		}
	}

	private Generator GetGenerator(String name) {
		Generator generator = null;

		switch(name) {
			case "NoiseTerrain":
			generator = new NoiseTerrain();
			break;
			case "NoiseCaves":
			generator = new NoiseCaves();
			break;
		}

		return generator;
	}
}
}
