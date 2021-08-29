// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B27.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B27.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 4.593 10.203 26.756

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

$bbox -20.406 0 0 0 9.185 53.512

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b27_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
