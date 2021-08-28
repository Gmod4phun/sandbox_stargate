// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B29.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B29.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 4.752 6.834 26.404

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

$bbox -13.669 0 0 0 9.504 52.808

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b29_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
