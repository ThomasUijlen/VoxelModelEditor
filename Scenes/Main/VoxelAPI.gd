class_name VoxelAPI
extends Node3D

signal worldChanged

var camera : Camera3D
var previousCamPos : Vector3

func _ready():
	camera = get_viewport().get_camera_3d()

var changed : bool = false
func _process(delta):
	var currentCamPos = camera.global_position.floor()
	if previousCamPos != currentCamPos:
		changed = true
		previousCamPos = currentCamPos
	
	if changed:
		changed = false
		emit_signal("worldChanged")

func setBlock(coord : Vector3, name : String):
	changed = true
	$VoxelWorld.call("SetBlock", coord, name)

func getBlockType(coord : Vector3) -> String:
	return $VoxelWorld.call("GetBlockType", coord)
