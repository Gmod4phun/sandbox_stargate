// Created by Crowbar 0.71

$modelname "Zup\Stargate\iris.mdl"

$bodygroup "body"
{
	studio "iris_ref.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0.115 -0.928 0.011

$cdmaterials "Zup\Stargate\"

$cbox 0 0 0 0 0 0

$bbox 0 -160.784 -160.938 1.855 161.014 160.96

$definebone "origin" "" 0 0 -0.001218 0 0 89.999982 0 0 0 0 0 0
$definebone "joint2" "origin" 10.274936 103.125015 0 0 0 0 0 0 0 0 0 0
$definebone "joint3" "origin" -22.025021 101.150009 0 0 0 0 0 0 0 0 0 0
$definebone "joint4" "origin" -52.425014 89.525009 0 0 0 0 0 0 0 0 0 0
$definebone "joint5" "origin" -77.350021 68.925003 0 0 0 0 0 0 0 0 0 0
$definebone "joint6" "origin" -94.650024 41.649994 0 0 0 0 0 0 0 0 0 0
$definebone "joint7" "origin" -102.975029 10.349992 0 0 0 0 0 0 0 0 0 0
$definebone "joint8" "origin" -101.175026 -22.050011 0 0 0 0 0 0 0 0 0 0
$definebone "joint9" "origin" -89.600044 -52.60001 0 0 0 0 0 0 0 0 0 0
$definebone "joint10" "origin" -68.95005 -77.500008 0 0 0 0 0 0 0 0 0 0
$definebone "joint11" "origin" -41.60004 -94.975006 0 0 0 0 0 0 0 0 0 0
$definebone "joint12" "origin" -10.225044 -103.175011 0 0 0 0 0 0 0 0 0 0
$definebone "joint13" "origin" 22.224955 -101.400024 0 0 0 0 0 0 0 0 0 0
$definebone "joint14" "origin" 52.52496 -89.550018 0 0 0 0 0 0 0 0 0 0
$definebone "joint15" "origin" 77.574951 -68.95002 0 0 0 0 0 0 0 0 0 0
$definebone "joint16" "origin" 94.974953 -41.600025 0 0 0 0 0 0 0 0 0 0
$definebone "joint17" "origin" 102.999962 -10.350026 0 0 0 0 0 0 0 0 0 0
$definebone "joint18" "origin" 101.349953 22.199974 0 0 0 0 0 0 0 0 0 0
$definebone "joint19" "origin" 89.449951 52.474976 0 0 0 0 0 0 0 0 0 0
$definebone "joint20" "origin" 68.949951 77.524979 0 0 0 0 0 0 0 0 0 0
$definebone "joint21" "origin" 41.649952 94.849976 0 0 0 0 0 0 0 0 0 0


$sequence "iris_01_idle" {
	"iris_anims\iris_01_idle.smd"
	activity "ACT_VM_01_IDLE" 1
	fadein 0.2
	fadeout 0.2
	fps 30
	loop
}

$sequence "iris_open_idle" {
	"iris_anims\iris_open_idle.smd"
	activity "ACT_VM_OPEN_IDLE" 1
	fadein 0.2
	fadeout 0.2
	fps 30
	loop
}

$sequence "iris_open" {
	"iris_anims\iris_open.smd"
	activity "ACT_VM_OPEN" 1
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "iris_close" {
	"iris_anims\iris_close.smd"
	activity "ACT_VM_CLOSE" 1
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "iris_closed_idle" {
	"iris_anims\iris_closed_idle.smd"
	activity "ACT_VM_CLOSED_IDLE" 1
	fadein 0.2
	fadeout 0.2
	fps 30
	loop
}

$collisionmodel "iris_physics.smd"
{
	$mass 1000
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 1

}
