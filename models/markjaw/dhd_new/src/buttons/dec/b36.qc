// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B36.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B36.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -6.025 5.9 26.23

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

$bbox -11.801 -12.05 0 0 0 52.46

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b36_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
