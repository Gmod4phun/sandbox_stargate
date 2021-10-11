// Created by Crowbar 0.71

$modelname "James\teltac\inner_door.mdl"

$bodygroup "body"
{
	studio "inner_door.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0.004 172.744 97.612

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -206.726 -27.666 41.751 -138.728 45.2 153.812

$definebone "InnerDoorRightBone" "" 15.095341 206.72554 94.705627 0 89.999982 0 0 0 0 0 0 0
$definebone "InnerDoorLeftBone" "" -15.833878 206.72554 94.705627 0 89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"inner_door_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$sequence "open" {
	"inner_door_anims\open.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "close" {
	"inner_door_anims\close.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "inner_door_physics.smd"
{
	$mass 50
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
