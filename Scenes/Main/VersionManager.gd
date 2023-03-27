extends Node

var versions : Array[Dictionary] = []
var versionI : int = 0

var currentVersion : Dictionary = {}
var changed = true

var t : float = 0.0
func _process(delta):
	if !changed: return
	t += delta
	if t < 0.2: return
	t -= 0.2
	
	changed = false
	while versions.size()-1 > versionI:
		versions.pop_back()
	
	versions.append(currentVersion.duplicate())
	if versions.size() > 30: versions.pop_front()
	versionI = versions.size()-1

func addChange(position : Vector3, block : String):
	currentVersion[position] = block
	changed = true

func applyVersion(version : Dictionary):
	var api : VoxelAPI = get_tree().current_scene
	
	for position in currentVersion:
		api.setBlock(position, "Air", false)
	
	for position in version:
		api.setBlock(position, version[position], false)

func _input(event):
	if event.is_action_pressed("Undo"):
		if versionI == 0: return
		versionI -= 1
		changed = false
		applyVersion(versions[versionI])
		currentVersion = versions[versionI]
	
	if event.is_action_pressed("Redo"):
		if versionI == versions.size()-1: return
		versionI += 1
		changed = false
		applyVersion(versions[versionI])
		currentVersion = versions[versionI]
