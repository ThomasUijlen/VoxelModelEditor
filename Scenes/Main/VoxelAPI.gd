class_name VoxelAPI
extends Node3D


func setBlock(coord : Vector3, name : String):
	$VoxelWorld.call("SetBlock", coord, name)

func getBlockType(coord : Vector3) -> String:
	return $VoxelWorld.call("GetBlockType", coord)

func _process(delta):
	var cameraPos = get_viewport().get_camera_3d().global_position;
	
	if Input.is_action_pressed("MouseLeft"):
		var startCoord = cameraPos - get_viewport().get_camera_3d().global_transform.basis.z*5;
		setBlock(startCoord, "Stone")
