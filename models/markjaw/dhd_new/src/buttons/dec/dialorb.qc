// Created by Crowbar 0.71

$modelname "MarkJaw/dhd_new/buttons/dialorb.mdl"

$staticprop

$bodygroup "Body"
{
	studio "dialorb.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -1.742 9.468 27.857

$cdmaterials "MarkJaw\dhd_new\"

$texturegroup "skinfamilies"
{
	{ "dialorb_sg1_off" }
	{ "dialorb_sg1_on"  }
	{ "dialorb_sga_off" }
	{ "dialorb_sga_on"  }
	{ "dialorb_sgu_off" }
	{ "dialorb_sgu_on"  }
	{ "dialorb_sgi_off" }
	{ "dialorb_sgi_on"  }
}

$cbox 0 0 0 0 0 0

$bbox -18.936 -6.581 0 0 3.098 55.714

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"dialorb_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}
