<?php
# return success of an error message based on if the log message was successfully submitted
###

$host = 'localhost';
$user = 'USER';
$pass = '';
$db = 'DB_NAME';
$con = mysqli_connect($host, $user, $pass, $db);
if(mysqli_connect_errno()){
	echo 'Failed to connect to MySQL: ' .mysqli_connect_error();
}
else{
	echo "connected successfully to mydb database \n";
}

// from: https://docs.unity3d.com/Manual/webgl-networking.html
header("Access-Control-Allow-Credentials: true");
header("Access-Control-Allow-Headers: Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
header("Access-Control-Allow-Methods: POST, GET, OPTIONS");
header("Access-Control-Allow-Origin: *");

// POST variables
$playerID = mysqli_real_escape_string($_POST["playerID"]);
$levelFile = mysqli_real_escape_string($_POST["levelFile"]);
$actionMessage = mysqli_real_escape_string($_POST["actionMsg"]);
$timestamp = mysqli_real_escape_string($_POST["timestamp"]);
$worldState = mysqli_real_escape_string($_POST["worldState"]);

// values to insert
$msg = $playerID .', ' .'\''.$levelFile .'\'' .', ' .'\'' .$actionMessage .'\'' .', ' .'\'' .$timestamp . '\', \'' . $worldState .'\'';

$sql = "INSERT INTO actiontable (playerID, levelFile, actionMessage, timestamp, worldState) VALUES (" .$msg . ")";

$query = mysqli_query($con, $sql);
if(!$query){
	echo 'data insertion failed';
}
if($query){
	echo 'data inserted successfully';
}
mysqli_close($con);

?>