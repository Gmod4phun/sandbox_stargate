// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B22.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B22.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -4.308 12.438 26.991

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

$bbox -24.876 -8.617 0 0 0 53.982

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b22_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
