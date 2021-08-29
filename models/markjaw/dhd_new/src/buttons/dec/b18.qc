// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B18.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B18.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -9.815 6.761 25.354

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

$bbox -13.522 -19.63 0 0 0 50.707

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b18_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
