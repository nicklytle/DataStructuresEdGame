<?php

header('Content-Type: text/plain');
$host = 'localhost';
$user = 'root';
$pass = '';
$db = 'myDB';
$con = mysqli_connect($host, $user, $pass, $db);
if(mysqli_connect_errno()){
	echo 'Failed to connect to MySQL: ' .mysqli_connect_error();
}
else{
	echo "connected successfully to mydb database \n";
}
//Querying..
//$playerID = $_POST["playerID"];
$playerID = 13234;
//$actionMessage = $_POST["actionMsg"];
$actionMessage = "hello world";
//$actionType = $_POST["actionType"];
$actionType = "traversal";
//$timestamp = $_POST["timestamp"];
$timestamp = "12-23-2019";


//$sql = "INSERT INTO actiontable (playerID, actionMessage, actionType, timestamp) VALUES (22222, 'connected linkB to platB', 'connection', '8.15.2018')";
// this adds the ' --> .'\''
$msg = $playerID .', ' .'\''.$actionMessage .'\'' .', ' .'\'' .$actionType .'\'' .', ' .'\'' .$timestamp .'\'';
echo $msg;


$sql = "INSERT INTO actiontable (playerID, actionMessage, actionType, timestamp) VALUES (" .$msg . ")";
echo $sql;

$query = mysqli_query($con, $sql);
if(!$query){
	echo 'data insertion failed';
}
if($query){
	echo 'data inserted successfully';
}
mysqli_close($con);

?>