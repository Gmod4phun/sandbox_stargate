// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B19.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B19.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -9.85 9.496 25.702

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

$bbox -18.992 -19.701 0 0 0 51.404

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b19_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
