<?php
# return either success or fail based on if the level marker was properly updated
###

$host = 'localhost';
$user = 'USER';
$pass = '';
$db = 'DB_NAME';
$con = mysqli_connect($host, $user, $pass, $db);
if(mysqli_connect_errno()){
	echo 'Failed to connect to MySQL: ' .mysqli_connect_error();
	exit(1);
}

// from: https://docs.unity3d.com/Manual/webgl-networking.html
header("Access-Control-Allow-Credentials: true");
header("Access-Control-Allow-Headers: Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
header("Access-Control-Allow-Methods: POST, GET, OPTIONS");
header("Access-Control-Allow-Origin: *");


$playerId = mysqli_real_escape_string($_POST['playerID']);
$levelOn = mysqli_real_escape_string($_POST['levelOn']);
$qry = "UPDATE users SET levelOn=" . ($levelOn) . " WHERE playerId = " . ($playerId);

$res = mysqli_query($con, $qry);
if(!$res){
	echo 'database error';
	mysqli_close($con);
	exit(1);
}

echo 'success';

mysqli_close($con);

?>