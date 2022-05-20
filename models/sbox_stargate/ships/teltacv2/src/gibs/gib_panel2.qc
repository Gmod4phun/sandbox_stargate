// Created by Crowbar 0.71

$modelname "James\teltac\gib_panel2.mdl"

$bodygroup "body"
{
	studio "gib_panel2.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0.262 -269.048 -60.578

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -25.286 -17.674 -163.208 563.131 18.199 42.303

$definebone "Object002" "" 0.262375 -563.131165 -163.208313 0 -179.999964 0 0 0 0 0 0 0
$definebone "Object007" "" 0.788017 -563.131165 -163.208313 0 -179.999964 0 0 0 0 0 0 0


$sequence "idle" {
	"gib_panel2_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "gib_panel2_physics.smd"
{
	$mass 300
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
