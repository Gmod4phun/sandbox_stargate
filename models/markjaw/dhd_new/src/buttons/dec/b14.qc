// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B14.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B14.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -1.741 0.282 24.444

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

$bbox -3.457 -4.581 0 2.893 1.098 48.887

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b14_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
