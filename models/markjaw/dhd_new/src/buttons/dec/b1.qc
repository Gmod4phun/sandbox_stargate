// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B1.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B1.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -9.561 12.168 26.028

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

$bbox -24.336 -19.122 0 0 0 52.057

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b1_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
