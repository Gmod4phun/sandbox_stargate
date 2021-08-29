// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B33.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B33.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -1.741 4.207 25.929

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

$bbox -8.414 -3.526 0 0 0.044 51.857

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b33_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
