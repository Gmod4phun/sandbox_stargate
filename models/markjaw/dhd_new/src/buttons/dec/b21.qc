// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/B21.mdl"

$staticprop

$bodygroup "Body"
{
	studio "B21.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -5.576 11.557 26.896

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

$bbox -23.114 -11.152 0 0 0 53.792

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"b21_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
