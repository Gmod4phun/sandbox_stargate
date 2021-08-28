// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B9.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B9.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 8.109 9.496 25.702

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

$bbox -18.992 0 0 0 16.218 51.404

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b9_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
