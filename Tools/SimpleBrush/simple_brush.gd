extends VoxelTool

var hoverDistance : int = 15
var castDistance : int = 15
var radius : int = 1

var ray : RayCast3D
var cube : Node3D

var cubePool : Array = []
var activeCubes : Array = []

var collide = true
var hollow = false
var replaceMode = false

func _ready():
	super._ready()
	ray = $RayCast3D
	cube = $Cube
	remove_child(cube)
	setRadius(radius)

func setRadius(radius : int):
	if radius < 1: radius = 1
	if radius > 15:
		radius = 15
		return
	self.radius = radius
	
	var half = floor(radius/2.0)
	
	for cube in activeCubes:
		$Cubes.remove_child(cube)
		cubePool.append(cube)
	activeCubes.clear()
	
	for x in range(radius):
		for y in range(radius):
			for z in range(radius):
				var coord = Vector3(x,y,z)
				if radius % 2 == 0: coord += Vector3.ONE*0.5
				if (Vector3.ZERO+Vector3.ONE*half).distance_to(coord) <= radius/2.0 and (!hollow or (Vector3.ZERO+Vector3.ONE*half).distance_to(coord) > radius/2.0-1):
					var newCube = null
					if cubePool.size() == 0:
						newCube = cube.duplicate()
					else:
						newCube = cubePool.pop_back()
					newCube.position = coord-Vector3.ONE*half
					newCube.tool = self
					newCube.replaceMode = replaceMode
					if radius % 2 == 0: newCube.position -= Vector3.ONE*0.5
					$Cubes.add_child(newCube)
					activeCubes.append(newCube)

var cubePosition : Vector3
func _physics_process(delta):
	var camera : Camera3D = get_viewport().get_camera_3d()
	ray.global_transform.basis = camera.global_transform.basis
	ray.global_transform.origin = camera.global_position
	
	ray.target_position.z = -castDistance-radius/2
	
	var colPos : Vector3
	if ray.is_colliding() and collide:
		colPos = ray.get_collision_point()
		if !replaceMode:
			colPos += ray.get_collision_normal()*0.55
		else:
			colPos -= ray.get_collision_normal()*0.45
	else:
		colPos = ray.to_global(Vector3(0,0,-hoverDistance-radius/2))
	
	if cubePosition != colPos.floor():
		cubePosition = colPos.floor()
		var tween : Tween = get_tree().create_tween()
		tween.tween_property($Cubes, "global_position", colPos.floor(), 0.1)
	
	if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		if toolDelay > 0: return
		if Input.is_action_just_pressed("MouseLeft"):
			for cube in activeCubes:
				if cube.visible: api.setBlock(cube.global_position+Vector3.ONE*0.2, blockType)
		if Input.is_action_just_pressed("MouseRight"):
			for cube in activeCubes:
				if cube.visible: api.setBlock(cube.global_position+Vector3.ONE*0.2, "Air")

func _input(event):
	if Input.mouse_mode != Input.MOUSE_MODE_CAPTURED: return
	var radiusIncrease = 0
	
	if event.is_action_pressed("H"):
		hollow = !hollow
		setRadius(radius)
	
	if event.is_action_pressed("R"):
		replaceMode = !replaceMode
		setRadius(radius)
	
	if event.is_action_pressed("C"):
		collide = !collide
	
	if Input.is_action_pressed("Ctrl") || Input.is_action_pressed("Shift"): return
	if event.is_action_pressed("ScrollUp"):
		radiusIncrease += 1
	if event.is_action_pressed("ScrollDown"):
		radiusIncrease -= 1
	if radiusIncrease == 0: return
	
	setRadius(radius + radiusIncrease)

func getData() -> String:
	var text : String = ""
	
	text += "[b]Tool Information:[/b]"
	text += "\n"
	text += "Brush Position: "+str($Cubes.global_position.floor())
	text += "\n"
	if Input.mouse_mode != Input.MOUSE_MODE_CAPTURED:
		text += "Active: [color=PALE_VIOLET_RED]false[/color]  [i][color=#abd7cf](Press TAB)[/color][/i]"
	else:
		text += "Active: [color=PALE_GREEN]true[/color]  [i][color=#abd7cf](Press TAB)[/color][/i]"
	text += "\n"
	text += "\n"
	text += "[b]Tool Settings:[/b]"
	text += "\n"
	text += "Selected Block: "+blockType+ "  [i][color=#abd7cf](Hold TAB)[/color][/i]"
	text += "\n"
	text += "\n"
	var color = Color.WHITE.lerp(Color.DARK_ORANGE, radius/15.0)
	text += "Radius: [color=#"+str(color.to_html())+"]"+str(radius) + "[/color]  [i][color=#abd7cf](Scroll)[/color][/i]"
	text += "\n"
	
	if !collide:
		text += "Collision: [color=PALE_VIOLET_RED]disabled[/color]  [i][color=#abd7cf](Press C)[/color][/i]"
	else:
		text += "Collision: [color=PALE_GREEN]enabled[/color]  [i][color=#abd7cf](Press C)[/color][/i]"
	
	text += "\n"
	if !hollow:
		text += "Hollow: [color=PALE_VIOLET_RED]"+str(hollow)+"[/color]  [i][color=#abd7cf](Press H)[/color][/i]"
	else:
		text += "Hollow: [color=PALE_GREEN]"+str(hollow)+"[/color]  [i][color=#abd7cf](Press H)[/color][/i]"
	
	text += "\n"
	if !replaceMode:
		text += "Replace Mode: [color=PALE_VIOLET_RED]disabled[/color]  [i][color=#abd7cf](Press R)[/color][/i]"
	else:
		text += "Replace Mode: [color=PALE_GREEN]enabled[/color]  [i][color=#abd7cf](Press R)[/color][/i]"
	
	return text
