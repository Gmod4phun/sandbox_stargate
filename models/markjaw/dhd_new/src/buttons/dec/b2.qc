// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B2.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B2.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -8.356 14.322 26.297

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

$bbox -28.643 -16.712 0 0 0 52.593

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b2_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
