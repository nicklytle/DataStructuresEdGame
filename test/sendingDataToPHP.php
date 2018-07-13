<?php

//header('Content-Type: text/plain');
$host = 'localhost';
$user = 'root';
$pass = '';
$db = 'edgamedb';
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
$logID = $_POST["logID"];
$playerID = $_POST["playerID"];
$levelFile = $_POST["levelFile"];
$actionMessage = $_POST["actionMsg"];
$timestamp = $_POST["timestamp"];


// values to insert
$msg = $playerID .', ' .'\''.$levelFile .'\'' .', ' .'\'' .$actionMessage .'\'' .', ' .'\'' .$timestamp .'\'';
//echo $msg . "\n\n\n";


$sql = "INSERT INTO actiontable (playerID, levelFile, actionMessage, timestamp) VALUES (" .$msg . ")";
//echo $sql . "\n\n\n";

$query = mysqli_query($con, $sql);
if(!$query){
	echo 'data insertion failed';
}
if($query){
	echo 'data inserted successfully';
}
mysqli_close($con);

?>