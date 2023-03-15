extends Resource
class_name GenerationSettings

signal settingsChanged

var settings : Dictionary = {
	"Name" : "Root",
	"Children" : [
		{
		"Name" : "NoiseTerrain",
		"Settings" : {
			"Seed" : 0,
			"StartHeight" : 30,
			"Layers" : [
				"Stone",
				"Dirt",
				"Dirt",
				"Dirt",
				"Grass"
			]
		},
		"Children" : [
			{
			"Name" : "NoiseCaves",
			"Settings" : {
				"Seed" : 0,
				"SpawnChance" : 10.0,
				"Radius" : 4,
				"MinLength" : 10,
				"MaxLength" : 20,
				"MinBranches" : 0,
				"MaxBranches" : 3
			},
			"Children" : []
			}
		]
		}
	],
	"Settings" : {}}

