<?php
# return either success or fail based on if they succeeded in logging in or not.
###

$host = 'localhost';
$user = 'root';
$pass = '';
$db = 'EdGameDB';
$con = mysqli_connect($host, $user, $pass, $db);
if(mysqli_connect_errno()){
	echo 'Failed to connect to MySQL: ' .mysqli_connect_error();
	exit(1);
}

header('Content-Type: text/plain');

$playerId = $_POST['playerID'];
$pw = $_POST['pw'];
$qry = "SELECT password FROM users WHERE playerId = " . htmlspecialchars($playerId);

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
if (isset($res_arr['password']) && $res_arr['password'] == $pw) {
	echo 'success';
} else {
	echo 'fail';
}

mysqli_close($con);

?>