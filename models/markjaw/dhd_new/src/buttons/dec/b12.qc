// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B12.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B12.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 5.696 3.07 24.739

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

$bbox -6.866 0 0 0.727 11.392 49.478

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b12_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
