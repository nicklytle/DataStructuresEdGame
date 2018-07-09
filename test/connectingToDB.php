<?php

	$servername = "localhost";
	$username = "root";
	$passwd = "";
    $dbName = "dsgamelinkedlists";
	
	//est the connection:
	$conn = new mysqli($servername, $username, $passwd, $dbName);
	
	//check it aint null
	if($conn->connect_error)
	{
		die('Connect Error (' . mysqli_connect_errno(). ') ' .mysqli_connect_error());
	}
	else{
		echo 'Connection success';
	}
?>