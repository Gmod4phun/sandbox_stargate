// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B31.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B31.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 3.257 5.097 26.083

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

$bbox -10.194 0 0 0 6.515 52.166

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b31_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
