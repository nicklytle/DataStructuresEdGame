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
//Querying..
$logID = $_POST["logID"];

$playerID = $_POST["playerID"];
//$playerID = 13234;
$levelFile = $_POST["levelFile"];
//this is from the POST request sent from the Unity file
$actionMessage = $_POST["actionMsg"];
$timestamp = $_POST["timestamp"];
//$timestamp = "12-23-2019";


//$sql = "INSERT INTO actionTable (playerID, actionMessage, timestamp) VALUES (22222, 'connected linkB to platB', 'connection', '8.15.2018')";
// this adds the ' --> .'\''
//this is from the DB col name
$msg = $playerID .', ' .'\''.$levelFile .'\'' .', ' .'\'' .$actionMessage .'\'' .', ' .'\'' .$timestamp .'\'';
echo $msg . "\n\n\n";


$sql = "INSERT INTO actiontable (playerID, levelFile, actionMessage, timestamp) VALUES (" .$msg . ")";
echo $sql . "\n\n\n";

$query = mysqli_query($con, $sql);
if(!$query){
	echo 'data insertion failed';
}
if($query){
	echo 'data inserted successfully';
}
mysqli_close($con);

?>