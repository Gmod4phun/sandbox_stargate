// Created by Crowbar 0.71

$modelname "Madman07\ring_panel\goauld_panel.mdl"

$bodygroup "body"
{
	studio "goauld.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0 -1.7 12.5

$cdmaterials "Madman07\ring_panel\"

$texturegroup "skinfamilies"
{
	{ "goauld_buttons"   }
	{ "goauld_buttons_1" }
	{ "goauld_buttons_2" }
	{ "goauld_buttons_3" }
	{ "goauld_buttons_4" }
	{ "goauld_buttons_5" }
	{ "goauld_buttons_6" }
}

$cbox 0 0 0 0 0 0

$bbox -0.25 -8.75 -0.75 3.4 8.75 25.75

$definebone "Box09" "" 0 0 12.5 0 0 89.999982 0 0 0 0 0 0
$definebone "Object01" "" 0 0 12.5 0 0 89.999982 0 0 0 0 0 0


$sequence "idle" {
	"goauld_panel_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "goauld_panel_physics.smd"
{
	$mass 10
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 1

}

$keyvalues
{
	prop_data 
	{
		"base"		"metal.Small"
	}
}
