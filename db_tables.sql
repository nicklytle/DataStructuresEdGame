-- Table structure used for logging in and logging data

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `playerId` int(11) NOT NULL,
  `password` varchar(255) NOT NULL,
  `levelOn` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=41 DEFAULT CHARSET=latin1;

--
-- Table structure for table `actiontable`
--

DROP TABLE IF EXISTS `actiontable`;
CREATE TABLE IF NOT EXISTS `actiontable` (
  `logID` int(11) NOT NULL AUTO_INCREMENT,
  `playerID` int(11) NOT NULL,
  `levelFile` varchar(100) NOT NULL,
  `actionMessage` text NOT NULL,
  `timestamp` varchar(255) NOT NULL,
  PRIMARY KEY (`logID`)
) ENGINE=MyISAM AUTO_INCREMENT=2027 DEFAULT CHARSET=latin1;
COMMIT;
