// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B23.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B23.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -2.668 12.751 27.045

$cdmaterials "MarkJaw\dhd_new\"

$texturegroup "skinfamilies"
{
	{ "buttons_sg1_off" }
	{ "buttons_sg1_on"  }
	{ "buttons_sga_off" }
	{ "buttons_sga_on"  }
	{ "buttons_sgu_off" }
	{ "buttons_sgu_on"  }
	{ "buttons_sgi_off" }
	{ "buttons_sgi_on"  }
}

$cbox 0 0 0 0 0 0

$bbox -25.502 -5.337 0 0 0 54.09

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b23_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
