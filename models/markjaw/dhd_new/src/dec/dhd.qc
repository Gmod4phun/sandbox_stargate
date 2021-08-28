// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/dhd.mdl"

$staticprop

$bodygroup "Body"
{
	studio "detail.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -1.704 18.913 27.857

$cdmaterials "MarkJaw\dhd_new\"

$texturegroup "skinfamilies"
{
	{ "dialorb_sg1_off" "buttons_sg1_off" "mid_sg1" "bottom_sg1" "top_sg1" }
	{ "dialorb_sg1_on"  "buttons_sg1_on"  "mid_sg1" "bottom_sg1" "top_sg1" }
	{ "dialorb_sga_off" "buttons_sga_off" "mid_sga" "bottom_sga" "top_sga" }
	{ "dialorb_sga_on"  "buttons_sga_on"  "mid_sga" "bottom_sga" "top_sga" }
	{ "dialorb_sgu_off" "buttons_sgu_off" "mid_sgu" "bottom_sgu" "top_sgu" }
	{ "dialorb_sgu_on"  "buttons_sgu_on"  "mid_sgu" "bottom_sgu" "top_sgu" }
	{ "dialorb_sgi_off" "buttons_sgi_off" "mid_sgi" "bottom_sgi" "top_sgi" }
	{ "dialorb_sgi_on"  "buttons_sgi_on"  "mid_sgi" "bottom_sgi" "top_sgi" }
}

$cbox 0 0 0 0 0 0

$bbox -48.695 -33.17 0 10.847 30.229 55.714

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"dhd_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "dhd_physics.smd"
{
	$mass 250
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 1

}
