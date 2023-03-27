extends Node2D

var api : VoxelAPI

func _ready():
	api = get_tree().current_scene
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE

func _on_export_pressed():
	$ExportDialog.popup_centered()

func _on_export_dialog_file_selected(path):
	var multiMesh : MultiMesh = $Mesh.multimesh
	var quads : Array = api.getAllQuads()
	multiMesh.instance_count = quads.size()
	for i in range(quads.size()):
		multiMesh.set_instance_transform(i, quads[i][0])
		multiMesh.set_instance_color(i, quads[i][1])
		multiMesh.set_instance_custom_data(i, quads[i][2])
	
	ResourceSaver.save($Mesh.multimesh, path, ResourceSaver.FLAG_BUNDLE_RESOURCES)
	$ExportDialog.visible = false
	
	setSucces("Mesh exported")

func _on_save_pressed():
	$SaveDialog.popup_centered()

func _on_save_dialog_file_selected(path):
	var data : Dictionary = {}
	data["World"] = api.getVersion()
	data["Textures"] = api.getTextureSettings()
	
	var file := FileAccess.open(path, FileAccess.WRITE)
	file.store_var(data)
	file.close()
	
	setSucces("Project saved")

func _on_load_pressed():
	$LoadDialog.popup_centered()

func _on_load_dialog_file_selected(path):
	if !FileAccess.file_exists(path):
		setError("File not found")
		return
	var file := FileAccess.open(path, FileAccess.READ)
	
	var data : Dictionary = file.get_var()
	api.setTextureSettings(data["Textures"])
	api.setVersion(data["World"])
	file.close()
	
	setSucces("Project loaded")


func _on_exit_pressed():
	get_tree().quit()

func setError(text : String):
	%Error.text = text
	%Error.visible = true
	%Succes.visible = false

func setSucces(text : String):
	%Succes.text = text
	%Succes.visible = true
	%Error.visible = false


func _on_visibility_changed():
	if is_visible_in_tree():
		%Error.visible = false
		%Succes.visible = false
