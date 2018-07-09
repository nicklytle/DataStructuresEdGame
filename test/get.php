<?php

	$servername = "localhost";
	$username = "root";
	$passwd = "";
    $dbName = "dsgamelinkedlists";
	
	//est the connection:
	$conn = new mysqli($servername, $username, $password, $dbName);
	
	//check it aint null);
	if(!$conn)
	{
		die("Connection failed");
	else{
		echo "Connection success");
	
	



?>