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

	Queue<MultiMeshInstance> multiMeshes = new Queue<MultiMeshInstance>();
	
	public override void _Ready() {
		voxelFace = GD.Load<Mesh>("res://addons/VoxelTerrain/Parts/Blocks/Mesh/VoxelFace.tres");

		for(int i = 0; i < 2; i++) {
			MultiMeshInstance multiMeshInstance = new MultiMeshInstance();
			multiMeshInstance.instance = RenderingServer.InstanceCreate();
        	RenderingServer.InstanceSetScenario(multiMeshInstance.instance, GetWorld3D().Scenario);

			multiMeshInstance.multiMesh = RenderingServer.MultimeshCreate();
			RenderingServer.InstanceSetBase(multiMeshInstance.instance, multiMeshInstance.multiMesh);
			RenderingServer.MultimeshSetMesh(multiMeshInstance.multiMesh, voxelFace.GetRid());
			multiMeshes.Enqueue(multiMeshInstance);
		}
	}

	float cooldown = 0.0f;
	bool processing = false;
	public override void _Process(double delta) {
		if(!processing || !active) return;

		cooldown -= Convert.ToSingle(delta);

		if(cooldown <= 0.0f) {
			if(threadActive) return;
			processing = false;
			threadActive = true;
			cancelThread = false;
			activeGrid = (Block[,,]) recentGrid.Clone();
			VoxelPluginMain.GetThreadPool(VoxelPluginMain.POOL_TYPE.RENDERING, this).RequestFunctionCall(this, "UpdateMesh");
		}
	}

	bool active = false;
	bool cancelThread = false;

	public void Activate() {
		active = true;
	}

	public void Deactivate() {
		active = false;
		cancelThread = true;

		for(int i = 0; i < 2; i++) {
			MultiMeshInstance multiMeshInstance = multiMeshes.Dequeue();
			RenderingServer.InstanceSetVisible(multiMeshInstance.instance, false);
			RenderingServer.MultimeshAllocateData(multiMeshInstance.multiMesh, 0, RenderingServer.MultimeshTransformFormat.Transform3D);
			multiMeshes.Enqueue(multiMeshInstance);
		}
	}

	public void RequestUpdate(Block[,,] grid) {
		recentGrid = grid;
		processing = true;
		if(cooldown < 0.0f) cooldown = 0.05f;
	}

	public void UpdateMesh() {
		CollectFaces(activeGrid);

		voxelFace.SurfaceGetMaterial(0).Set("shader_parameter/textureAtlas", BlockLibrary.texture);

		MultiMeshInstance newMultiMeshInstance = multiMeshes.Dequeue();

        RenderingServer.MultimeshAllocateData(
            newMultiMeshInstance.multiMesh,
            faces.Count,
            RenderingServer.MultimeshTransformFormat.Transform3D,
            false,
			true);

        for (int i = 0; i < faces.Count; i++)
        {
			if(cancelThread) break;
			Face face = faces[i];
            RenderingServer.MultimeshInstanceSetTransform(newMultiMeshInstance.multiMesh, i, face.transform);
			RenderingServer.MultimeshInstanceSetCustomData(newMultiMeshInstance.multiMesh, i,
				new Color(
					face.texture.UVPosition.X,
					face.texture.UVPosition.Y,
					face.texture.UVSize.X));
        }

		RenderingServer.InstanceSetVisible(newMultiMeshInstance.instance, true);

		MultiMeshInstance oldMultiMeshInstance = multiMeshes.Dequeue();
		//RenderingServer.MultimeshAllocateData(oldMultiMeshInstance.multiMesh, 0, RenderingServer.MultimeshTransformFormat.Transform3D);
		RenderingServer.InstanceSetVisible(oldMultiMeshInstance.instance, false);

		multiMeshes.Enqueue(oldMultiMeshInstance);
		multiMeshes.Enqueue(newMultiMeshInstance);

		threadActive = false;
	}

	private void CollectFaces(Block[,,] grid) {
		faces.Clear();
		Vector3I size = new Vector3I(grid.GetLength(0), grid.GetLength(1), grid.GetLength(2));

		for(int x = 0; x < size.X; x++) {
            for(int y = 0; y < size.Y; y++) {
                for(int z = 0; z < size.Z; z++) {
					Block block = grid[x,y,z];
					if(block.blockType == null) continue;
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

	private class MultiMeshInstance {
		public Rid instance;
		public Rid multiMesh;
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