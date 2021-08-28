// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B4.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B4.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -3.73 16.221 26.575

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

$bbox -32.442 -7.461 0 0 0 53.151

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b4_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
