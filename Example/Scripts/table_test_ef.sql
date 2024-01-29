CREATE DATABASE IF NOT EXISTS `wanadi`;

USE `wanadi`;

CREATE TABLE `table_test_ef` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Description` varchar(255) NOT NULL,
  `Complement` varchar(255) DEFAULT NULL,
  `EnumStatus` int NOT NULL,
  `UUID` varchar(36) NOT NULL,
  `DateEnd` date DEFAULT NULL,
  `Price` decimal(18,2) NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `UpdatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `Active` bit(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
