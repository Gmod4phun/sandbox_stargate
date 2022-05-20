// Created by Crowbar 0.71

$modelname "James\teltac\gib_engine.mdl"

$bodygroup "body"
{
	studio "gib_engine.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -0.8 98.327 10.672

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -229.635 -32.294 -147.885 33.23 30.695 169.478

$definebone "Engine" "" -0.798029 229.634735 -147.884979 0 -179.999964 0 0 0 0 0 0 0


$sequence "idle" {
	"gib_engine_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "gib_engine_physics.smd"
{
	$mass 300
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
