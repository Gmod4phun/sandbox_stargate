// Created by Crowbar 0.71

$modelname "James\teltac\gib_panel1.mdl"

$bodygroup "body"
{
	studio "gib_panel1.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0.788 -214.128 -44.676

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -39.254 -50.203 -152.258 467.26 51.779 63.156

$definebone "Control1" "" 0.788017 -467.260376 -152.258362 0 -179.999964 0 0 0 0 0 0 0
$definebone "Object004" "" 0.788017 -467.260376 -152.258362 0 -179.999964 0 0 0 0 0 0 0
$definebone "Object006" "" 0.788017 -467.260376 -152.258362 0 -179.999964 0 0 0 0 0 0 0


$sequence "idle" {
	"gib_panel1_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "gib_panel1_physics.smd"
{
	$mass 300
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
