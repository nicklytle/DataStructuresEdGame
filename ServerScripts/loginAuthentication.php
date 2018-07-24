<?php
# return either success or fail based on if they succeeded in logging in or not.
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
$pw = mysqli_real_escape_string($_POST['pw']);
$qry = "SELECT password, levelOn FROM users WHERE playerId = " . ($playerId);

$res = mysqli_query($con, $qry);
if(!$res){
	echo 'database error';
	mysqli_close($con);
	exit(1);
}
if ($res->num_rows < 1) {
	echo 'invalid playerId';
	mysqli_close($con);
	exit(1);
}
$res_arr = $res->fetch_assoc();

# verify that the password matches the playerId
if (isset($res_arr['password']) && $res_arr['password'] == $pw && isset($res_arr['levelOn'])) {
	echo 'success ' . $playerId . ' ' . $res_arr['levelOn'];
} else {
	echo 'fail';
}

mysqli_close($con);

?>