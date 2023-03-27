extends ScrollContainer

signal typeSelected(name)

@export var normalStyle : StyleBox
@export var selectedStyle : StyleBox
@export var autoSelect : bool = true

var api : VoxelAPI

var emptyLabel : Control

var selected : String = ""

func _ready():
	if api == null: api = get_tree().current_scene
	if emptyLabel == null:
		emptyLabel = $TypeList/Air
		%TypeList.remove_child(emptyLabel)

func refreshTypeList(select : bool = true):
	var blockTypes : Array = api.getBlockTypes()
	
	for child in %TypeList.get_children():
		child.free()
	
	for type in blockTypes:
		if type == "Air": continue
		var label : Control = emptyLabel.duplicate()
		label.connect("blockTypeSelected", blockTypeSelected)
		%TypeList.add_child(label)
		label.name = type
		label.prepare()
	
	if select:
		if autoSelect:
			if blockTypes.has(selected):
				blockTypeSelected(selected)
			else:
				blockTypeSelected(blockTypes[1])

func blockTypeSelected(name):
	selected = name
	emit_signal("typeSelected", name)
	
	for child in %TypeList.get_children():
		if child.name == name:
			child.set("theme_override_styles/panel", selectedStyle)
		else:
			child.set("theme_override_styles/panel", normalStyle)

func _on_visibility_changed():
	if is_visible_in_tree() and api != null:
		refreshTypeList()
