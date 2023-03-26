extends Node3D
class_name VoxelTool

var api : VoxelAPI

var toolData : Node

func _ready():
	api = get_tree().current_scene
	toolData = load("res://Tools/tool_data.tscn").instantiate()
	add_child(toolData)
