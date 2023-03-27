class_name VoxelAPI
extends Node3D

signal worldChanged

var camera : Camera3D
var previousCamPos : Vector3

enum SIDE {
	DEFAULT = 1 << 0,
	TOP = 1 << 1,
	BOTTOM = 1 << 2,
	LEFT = 1 << 3,
	RIGHT = 1 << 4,
	FRONT = 1 << 5,
	BACK = 1 << 6
}

func _ready():
	camera = get_viewport().get_camera_3d()

var changed : bool = false
var changedTimer : float = 0.0
func _process(delta):
	var currentCamPos = camera.global_position.floor()
	if previousCamPos != currentCamPos:
		changed = true
		previousCamPos = currentCamPos
	
	changedTimer -= delta
	if changed and changedTimer <= 0.0:
		changedTimer = 0.2
		changed = false
		emit_signal("worldChanged")

func sideToStr(side : SIDE) -> String:
	match(side):
		SIDE.DEFAULT:
			return "Default"
		SIDE.TOP:
			return "Top"
		SIDE.BOTTOM:
			return "Bottom"
		SIDE.LEFT:
			return "Left"
		SIDE.RIGHT:
			return "Right"
		SIDE.BACK:
			return "Back"
		SIDE.FRONT:
			return "Front"
	return ""

func setBlock(coord : Vector3, name : String, save : bool = true):
	changed = true
	$VoxelEditor/VoxelWorld.call("SetBlock", coord, name)
	if save: $VersionManager.addChange(coord, name)

func getBlockType(coord : Vector3) -> String:
	return $VoxelEditor/VoxelWorld.call("GetBlockType", coord)

func hasBlockType(name : String) -> bool:
	return $VoxelEditor/VoxelWorld.call("HasBlockType", name)

func getBlockTypes() -> Array:
	return $VoxelEditor/VoxelWorld.call("GetBlockTypes")

func createBlockType(name : String, image, modulate : Color, rendered : bool = true):
	$VoxelEditor/VoxelWorld.call("CreateBlockType", name, image, modulate, rendered)

func getTexture(name : String, side : SIDE) -> Image:
	return $VoxelEditor/VoxelWorld.call("GetTexture", name, side)

func getModulate(name : String) -> Color:
	return $VoxelEditor/VoxelWorld.call("GetModulate", name)

func setModulate(name : String, modulate : Color):
	$VoxelEditor/VoxelWorld.call("SetModulate", name, modulate)

func setTexture(name : String, side : SIDE, texture : Image):
	$VoxelEditor/VoxelWorld.call("SetTexture", name, side, texture)

func getTextureSize() -> int:
	return $VoxelEditor/VoxelWorld.call("GetTextureSize") 

func getAllQuads() -> Array:
	return $VoxelEditor/VoxelWorld.call("GetAllQuads")

func getTextureSettings() -> Dictionary:
	return $VoxelEditor/VoxelWorld.call("GetTextureSettings")

func setTextureSettings(settings : Dictionary):
	return $VoxelEditor/VoxelWorld.call("SetTextureSettings", settings)

func setVersion(version : Dictionary):
	$VersionManager.applyVersion(version)
	$VersionManager.changed = true

func getVersion() -> Dictionary:
	return $VersionManager.currentVersion
