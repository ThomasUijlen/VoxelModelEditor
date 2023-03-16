extends Node3D
class_name VoxelTool

var api : VoxelAPI

var toolData : Node
var blockMenu : Node

var blockType : String = "Default"

var toolDelay = 0.0

func _ready():
	api = get_tree().current_scene
	toolData = load("res://Tools/tool_data.tscn").instantiate()
	blockMenu = toolData.get_node("VoxelList")
#	blockMenu.get_node("VBoxContainer/VoxelTypeList").api = api
	blockMenu.get_node("VoxelTypeList").connect("typeSelected", typeSelected)
	add_child(toolData)

var tabHoldTime = 0.0
func _process(delta):
	if !is_visible_in_tree(): return
	toolDelay -= delta
	toolData.get_node("Data").text = getData()
	
	var tabPressed = Input.is_action_pressed("Tab")
	
	if Input.is_action_just_released("Tab"):
		if tabHoldTime < 0.5:
			if blockMenu.visible == true:
				blockMenu.visible = false
				Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
				return
			if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
				Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
			else:
				Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	
	if tabHoldTime > 0.2:
		blockMenu.visible = true
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	
	if tabPressed:
		tabHoldTime += delta
	else:
		tabHoldTime = 0.0

func typeSelected(name):
	if tabHoldTime > 0: return
	toolDelay = 0.2
	blockType = name
	blockMenu.visible = false
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func getData() -> String:
	return ""


