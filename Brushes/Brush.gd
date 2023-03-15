extends Node3D
class_name VoxelTool

var api : VoxelAPI

func _ready():
	api = get_tree().current_scene
