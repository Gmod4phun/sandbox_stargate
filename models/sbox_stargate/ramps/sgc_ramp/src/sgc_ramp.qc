// Created by Crowbar 0.71

$modelname "Zup\ramps\sgc_ramp.mdl"

$staticprop

$bodygroup "body"
{
	studio "sgc_ramp_ref.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0 -139.992 -2.036

$cdmaterials "Zup\ramps\"

$cbox 0 0 0 0 0 0

$bbox -128.316 -208.257 -146.416 408.184 208.243 142.084

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"sgc_ramp_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
	loop
}

$collisionmodel "sgc_ramp_physics.smd"
{
	$mass 5000
	$inertia 1
	$damping 0.5
	$rotdamping 0.5
	$rootbone " "
	$concave
	$maxconvexpieces 261

}
