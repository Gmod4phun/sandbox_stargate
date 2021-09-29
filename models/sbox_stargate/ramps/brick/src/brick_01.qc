// Created by Crowbar 0.71

$modelname "Zup\ramps\brick_01.mdl"

$staticprop

$bodygroup "body"
{
	studio "brick_01_ref.smd"
}


$surfaceprop "concrete"

$contents "solid"

$illumposition 0.031 -64.016 -80.003

$cdmaterials "Zup\ramps\"

$cbox 0 0 0 0 0 0

$bbox -72.109 -192.218 -160.141 200.391 192.532 0

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"brick_01_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
	loop
}

$collisionmodel "brick_01_physics.smd"
{
	$mass 1000
	$inertia 1
	$damping 0.5
	$rotdamping 0.5
	$rootbone " "
	$concave
	$maxconvexpieces 13

}
