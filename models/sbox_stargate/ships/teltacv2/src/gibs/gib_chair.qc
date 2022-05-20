// Created by Crowbar 0.71

$modelname "James\teltac\gib_chair.mdl"

$bodygroup "body"
{
	studio "gib_chair.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -10.99 -286.878 -60.661

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -21.108 -33.409 -171.267 594.624 11.677 50.195

$definebone "Chair" "" -33.408192 -594.623718 -171.267181 0 -179.999964 0 0 0 0 0 0 0


$sequence "idle" {
	"gib_chair_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "gib_chair_physics.smd"
{
	$mass 300
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
