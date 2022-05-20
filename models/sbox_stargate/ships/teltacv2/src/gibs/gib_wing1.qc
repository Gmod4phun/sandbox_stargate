// Created by Crowbar 0.71

$modelname "James\teltac\gib_wing1.mdl"

$bodygroup "body"
{
	studio "gib_wing1.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -128.264 -58.053 -43.117

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -175.327 -311.8 -107.745 291.434 55.522 21.761

$definebone "RightWing1" "" -311.80011 15.263107 -107.744507 0 -179.999964 0 0 0 0 0 0 0


$sequence "idle" {
	"gib_wing1_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "gib_wing1_physics.smd"
{
	$mass 300
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
