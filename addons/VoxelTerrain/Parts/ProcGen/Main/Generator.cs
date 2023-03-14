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

	public virtual void ApplySettings(Dictionary<String, Variant> settings) {
		
	}
}
}
