// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B32.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B32.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 1.434 4.511 25.979

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

$bbox -9.023 -0.699 0 0 3.567 51.957

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b32_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
