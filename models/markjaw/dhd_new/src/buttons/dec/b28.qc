// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B28.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B28.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 4.774 8.523 26.586

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

$bbox -17.046 0 0 0 9.549 53.171

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b28_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
