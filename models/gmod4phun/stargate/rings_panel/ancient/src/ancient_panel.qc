// Created by Crowbar 0.71

$modelname "Madman07\ring_panel\ancient_panel.mdl"

$staticprop

$bodygroup "body"
{
	studio "ancient.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0 -0.25 11

$cdmaterials "Madman07\ring_panel\"

$texturegroup "skinfamilies"
{
	{ "ancient_buttons"   }
	{ "ancient_buttons_1" }
	{ "ancient_buttons_2" }
	{ "ancient_buttons_3" }
	{ "ancient_buttons_4" }
	{ "ancient_buttons_5" }
	{ "ancient_buttons_6" }
	{ "ancient_buttons_7" }
	{ "ancient_buttons_8" }
	{ "ancient_buttons_9" }
}

$cbox 0 0 0 0 0 0

$bbox -0.75 -4.75 -0.25 1.25 4.75 22.25

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"ancient_panel_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "ancient_panel_physics.smd"
{
	$mass 2.163584
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
