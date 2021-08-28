// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B15.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B15.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -5.093 0.985 24.54

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

$bbox -4.623 -10.185 0 2.652 0 49.079

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b15_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
