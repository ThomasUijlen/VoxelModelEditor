using Godot;
using System;
using System.Collections.Generic;

namespace VoxelPlugin {
public partial class VoxelRenderer : Node3D
{

	Mesh voxelFace;

	List<Face> faces = new List<Face>();

	Block[,,] recentGrid;
	Block[,,] activeGrid;
	bool threadActive = false;

	Queue<Rid> multiMeshes = new Queue<Rid>();

	List<Rid> instances = new List<Rid>();
	
	public override void _Ready() {
		voxelFace = GD.Load<Mesh>("res://addons/VoxelTerrain/Parts/Blocks/Mesh/VoxelFace.tres");

		for(int i = 0; i < 2; i++) {
			Rid instance = RenderingServer.InstanceCreate();
			instances.Add(instance);
        	RenderingServer.InstanceSetScenario(instance, GetWorld3D().Scenario);

			Rid multiMesh = RenderingServer.MultimeshCreate();
			RenderingServer.InstanceSetBase(instance, multiMesh);
			RenderingServer.MultimeshSetMesh(multiMesh, voxelFace.GetRid());
			multiMeshes.Enqueue(multiMesh);
		}
		SetProcess(false);
	}

	public override void _ExitTree() {
		foreach(Rid instance in instances) RenderingServer.FreeRid(instance);
	}

	float cooldown = 0.0f;
	public override void _Process(double delta) {
		cooldown -= Convert.ToSingle(delta);

		if(cooldown <= 0.0f) {
			if(threadActive) return;
			SetProcess(false);
			threadActive = true;
			activeGrid = (Block[,,]) recentGrid.Clone();
			VoxelPluginMain.GetThreadPool(this).RequestFunctionCall(this, "UpdateMesh");
		}
	}

	public void RequestUpdate(Block[,,] grid) {
		recentGrid = grid;
		SetProcess(true);
		if(cooldown < 0.0f) cooldown = 0.05f;
	}

	public void UpdateMesh() {
		CollectFaces(activeGrid);

		voxelFace.SurfaceGetMaterial(0).Set("shader_parameter/textureAtlas", BlockLibrary.texture);

		Rid newMultiMesh = multiMeshes.Dequeue();

        RenderingServer.MultimeshAllocateData(
            newMultiMesh,
            faces.Count,
            RenderingServer.MultimeshTransformFormat.Transform3D,
            false,
			true);

        for (int i = 0; i < faces.Count; i++)
        {
			Face face = faces[i];
            RenderingServer.MultimeshInstanceSetTransform(newMultiMesh, i, face.transform);
			RenderingServer.MultimeshInstanceSetCustomData(newMultiMesh, i,
				new Color(
					face.texture.UVPosition.X,
					face.texture.UVPosition.Y,
					face.texture.UVSize.X));
        }

		Rid oldMultiMesh = multiMeshes.Dequeue();
		RenderingServer.MultimeshAllocateData(oldMultiMesh, 0, RenderingServer.MultimeshTransformFormat.Transform3D);

		multiMeshes.Enqueue(oldMultiMesh);
		multiMeshes.Enqueue(newMultiMesh);

		threadActive = false;
	}

	private void CollectFaces(Block[,,] grid) {
		faces.Clear();
		int size = grid.GetLength(0);

		for(int x = 0; x < size; x++) {
            for(int y = 0; y < size; y++) {
                for(int z = 0; z < size; z++) {
					Block block = grid[x,y,z];
					CollectFace(block, SIDE.TOP);
					CollectFace(block, SIDE.BOTTOM);
					CollectFace(block, SIDE.LEFT);
					CollectFace(block, SIDE.RIGHT);
					CollectFace(block, SIDE.FRONT);
					CollectFace(block, SIDE.BACK);
				}
			}
		}
	}

	private void CollectFace(Block block, SIDE side) {
		BlockType blockType = block.blockType;
		if(blockType == null) return;
		Vector3 direction = Block.SideToVector(side);

		BlockType neighbour = Chunk.GetBlockType(block.position + direction);
		if(neighbour != null && !neighbour.transparent) return;

		faces.Add(new Face(
			new Transform3D(GetBasis(side), block.position),
			blockType.GetTexture(side)
		));
	}

	private Basis GetBasis(SIDE side) {
		switch(side) {
			case SIDE.TOP: return new Basis(new Vector3(1,0,0), 0);
			case SIDE.BOTTOM: return new Basis(new Vector3(1,0,0), Mathf.Pi);
			case SIDE.LEFT: return new Basis(new Vector3(0,0,1), Mathf.Pi/2f).Rotated(new Vector3(1,0,0), -Mathf.Pi/2f);
			case SIDE.RIGHT: return new Basis(new Vector3(0,0,1), -Mathf.Pi/2f).Rotated(new Vector3(1,0,0), Mathf.Pi/2f);
			case SIDE.FRONT: return new Basis(new Vector3(1,0,0), -Mathf.Pi/2f);
			case SIDE.BACK: return new Basis(new Vector3(1,0,0), Mathf.Pi/2f);
		}

		return new Basis(new Vector3(1,0,0), 0); 
	}

	private class Face {
		public Transform3D transform;
		public BlockTexture texture;

		public Face(Transform3D transform, BlockTexture texture) {
			this.transform = transform;
			this.texture = texture;
		}
	}
}
}