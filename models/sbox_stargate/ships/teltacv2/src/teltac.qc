// Created by Crowbar 0.71

$modelname "James\teltac\teltac.mdl"

$bodygroup "body"
{
	studio "teltac.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -34.993 -60.195 194.075

$cdmaterials "\James\teltac\"

$cbox 0 0 0 0 0 0

$bbox -532.432 -449.994 -103.571 652.235 380.007 492.25

$definebone "BoneRight3" "" -279.99353 -299.994019 0 0 -89.999982 0 0 0 0 0 0 0
$definebone "BoneRight1" "" -369.909943 -69.994049 -5.370636 0 -89.999982 0 0 0 0 0 0 0
$definebone "BoneLeft1" "" 380.00647 -69.993927 0 0 -89.999982 0 0 0 0 0 0 0
$definebone "BoneLeft2" "" 380.00647 150.006042 0 0 -89.999982 0 0 0 0 0 0 0
$definebone "BoneRight2" "" -379.99353 150.00592 0 0 -89.999982 0 0 0 0 0 0 0
$definebone "BoneMain" "" 0.006479 0.006003 0 0 -89.999982 0 0 0 0 0 0 0
$definebone "BoneRight4" "" -349.99353 -249.994049 0 0 -89.999982 0 0 0 0 0 0 0
$definebone "BoneDoor1" "" -449.99353 -199.99408 0 0 -179.999978 0 0 0 0 0 0 0
$definebone "BoneDoor2" "" -449.99353 -179.99408 0 0 -179.999978 0 0 0 0 0 0 0
$definebone "BoneSlope" "" -118.993469 -342.75116 42.5 0 -179.999978 0 0 0 0 0 0 0


$sequence "idle" {
	"teltac_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$sequence "hide" {
	"teltac_anims\hide.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "unhide" {
	"teltac_anims\unhide.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "teltac_physics.smd"
{
	$mass 10000
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 144

}
