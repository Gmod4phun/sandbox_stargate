// Created by Crowbar 0.71

$modelname "James\teltac\gib_wing2.mdl"

$bodygroup "body"
{
	studio "gib_wing2.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -79.497 -22.386 1.258

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -129.852 -228.553 -156.159 174.537 70.554 158.924

$definebone "DoorWing1" "" -228.552612 -96.489334 -156.158508 0 -179.999909 0 0 0 0 0 0 0


$sequence "idle" {
	"gib_wing2_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "gib_wing2_physics.smd"
{
	$mass 300
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
