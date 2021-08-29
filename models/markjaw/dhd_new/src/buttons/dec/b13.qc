// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B13.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B13.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 3.351 0.985 24.54

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

$bbox -4.623 0 0 2.652 6.703 49.079

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b13_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
