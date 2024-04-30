/*!40101 SET @OLD_CHARACTER_SET_CLIENT = @@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS = @@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION = @@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE = @@TIME_ZONE */;
/*!40103 SET TIME_ZONE = '+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS = @@UNIQUE_CHECKS, UNIQUE_CHECKS = 0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS = @@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS = 0 */;
/*!40101 SET @OLD_SQL_MODE = @@SQL_MODE, SQL_MODE = 'NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES = @@SQL_NOTES, SQL_NOTES = 0 */;

--
-- Table structure for table `DiscordLinks`
--

DROP TABLE IF EXISTS `DiscordLinks`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DiscordLinks`
(
    `DiscordId`    bigint(20) UNSIGNED NOT NULL,
    `PlayerId`     bigint(20)          NOT NULL,
    `Status`       tinyint(3) UNSIGNED NOT NULL,
    `AccessToken`  text                NOT NULL,
    `RefreshToken` text                NOT NULL,
    `StateHash`    text                NOT NULL,
    PRIMARY KEY (`PlayerId`, `DiscordId`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlayersPreferences`
--

DROP TABLE IF EXISTS `PlayersPreferences`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlayersPreferences`
(
    `PlayerId`         bigint(20) NOT NULL,
    `RememberMe`       tinyint(1) NOT NULL,
    `Subscribed`       tinyint(1) NOT NULL,
    `DiscordConfirmed` tinyint(1) NOT NULL,
    `CountryCode`      text       NOT NULL,
    PRIMARY KEY (`PlayerId`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Avatars`
--

DROP TABLE IF EXISTS `Avatars`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Avatars`
(
    `Id`       bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Containers`
--

DROP TABLE IF EXISTS `Containers`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Containers`
(
    `Id`       bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    `Count`    bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Covers`
--

DROP TABLE IF EXISTS `Covers`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Covers`
(
    `Id`       bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Graffities`
--

DROP TABLE IF EXISTS `Graffities`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Graffities`
(
    `Id`       bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Hulls`
--

DROP TABLE IF EXISTS `Hulls`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Hulls`
(
    `Xp`            bigint(20) NOT NULL,
    `SkinId`        bigint(20) NOT NULL,
    `Kills`         bigint(20) NOT NULL,
    `BattlesPlayed` bigint(20) NOT NULL,
    `Id`            bigint(20) NOT NULL,
    `PlayerId`      bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `HullSkins`
--

DROP TABLE IF EXISTS `HullSkins`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `HullSkins`
(
    `Id`       bigint(20) NOT NULL,
    `HullId`   bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `HullId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `HullId`) REFERENCES `Hulls` (`PlayerId`, `Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Invites`
--

DROP TABLE IF EXISTS `Invites`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Invites`
(
    `Id`            bigint(20)           NOT NULL AUTO_INCREMENT,
    `Code`          varchar(64)          NOT NULL,
    `RemainingUses` smallint(5) unsigned NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE (`Code`)
) ENGINE = InnoDB
  AUTO_INCREMENT = 3
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Modules`
--

DROP TABLE IF EXISTS `Modules`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Modules`
(
    `Id`       bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    `Level`    int(11)    NOT NULL,
    `Cards`    int(11)    NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Paints`
--

DROP TABLE IF EXISTS `Paints`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Paints`
(
    `Id`       bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Players`
--

DROP TABLE IF EXISTS `Players`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Players`
(
    `Id`                   bigint(20)              NOT NULL,
    `Username`             text                    NOT NULL COLLATE 'utf8mb4_bin',
    `DiscordId`            bigint(20)     UNSIGNED NOT NULL,
    `AutoLoginToken`       varbinary(255) DEFAULT  NULL,
    `PasswordHash`         varbinary(255)          NOT NULL,
    `HardwareFingerprint`  text                    NOT NULL,
    `Groups`               int(11)                 NOT NULL,
    `RewardedLeagues`      int(11)                 NOT NULL,
    `Subscribed`           tinyint(1)              NOT NULL,
    `CurrentAvatarId`      bigint(20)              NOT NULL,
    `CurrentPresetIndex`   int(11)                 NOT NULL,
    `Crystals`             bigint(20)              NOT NULL,
    `XCrystals`            bigint(20)              NOT NULL,
    `GoldBoxItems`         int(11)                 NOT NULL,
    `Experience`           bigint(20)              NOT NULL,
    `FractionName`         text                    NOT NULL,
    `FractionScore`        bigint(20)              NOT NULL,
    `RegistrationTime`     timestamp               NOT NULL,
    `LastLoginTime`        timestamp               NOT NULL,
    `Reputation`           int(11)        UNSIGNED NOT NULL,
    `GameplayChestScore`   bigint(20)              NOT NULL,
    `DesertedBattlesCount` bigint(20)              NOT NULL,
    `NeedGoodBattlesCount` int(11)                 NOT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE (`Username`, `DiscordId`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PresetModules`
--

DROP TABLE IF EXISTS `PresetModules`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PresetModules`
(
    `PlayerId`    bigint(20)          NOT NULL,
    `PresetIndex` int(11)             NOT NULL,
    `Slot`        tinyint(3) unsigned NOT NULL,
    `Entity`      bigint(20)          NOT NULL,
    PRIMARY KEY (`PlayerId`, `PresetIndex`, `Slot`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `PresetIndex`) REFERENCES `Presets` (`PlayerId`, `Index`) ON DELETE CASCADE
) ENGINE = InnoDB
  AUTO_INCREMENT = 24
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Presets`
--

DROP TABLE IF EXISTS `Presets`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Presets`
(
    `PlayerId`   bigint(20)    NOT NULL,
    `Index`      int(11)       NOT NULL,
    `Name`       varchar(4000) NOT NULL,
    `Weapon`     bigint(20)    NOT NULL,
    `Hull`       bigint(20)    NOT NULL,
    `WeaponSkin` bigint(20)    NOT NULL,
    `HullSkin`   bigint(20)    NOT NULL,
    `Cover`      bigint(20)    NOT NULL,
    `Paint`      bigint(20)    NOT NULL,
    `Shell`      bigint(20)    NOT NULL,
    `Graffiti`   bigint(20)    NOT NULL,
    PRIMARY KEY (`PlayerId`, `Index`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Weapon`) REFERENCES `Weapons` (`PlayerId`, `Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Hull`) REFERENCES `Hulls` (`PlayerId`, `Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Weapon`, `WeaponSkin`) REFERENCES `WeaponSkins` (`PlayerId`, `WeaponId`, `Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Hull`, `HullSkin`) REFERENCES `HullSkins` (`PlayerId`, `HullId`, `Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Cover`) REFERENCES `Covers` (`PlayerId`, `Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Paint`) REFERENCES `Paints` (`PlayerId`, `Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Weapon`, `Shell`) REFERENCES `Shells` (`PlayerId`, `WeaponId`, `Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `Graffiti`) REFERENCES `Graffities` (`PlayerId`, `Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Punishments`
--

DROP TABLE IF EXISTS `Punishments`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Punishments`
(
    `PlayerId`   bigint(20) NOT NULL,
    `Id`         bigint(20) NOT NULL AUTO_INCREMENT,
    `Type`       int(11)    NOT NULL,
    `PunishTime` timestamp  NOT NULL,
    `Duration`   bigint(20) DEFAULT NULL,
    `Reason`     text       DEFAULT NULL,
    `Active`     tinyint(1) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    UNIQUE KEY `Id` (`Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  AUTO_INCREMENT = 24
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Relations`
--

DROP TABLE IF EXISTS `Relations`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Relations`
(
    `SourcePlayerId` bigint(20) NOT NULL,
    `TargetPlayerId` bigint(20) NOT NULL,
    `Types`          int(11)    NOT NULL,
    PRIMARY KEY (`SourcePlayerId`, `TargetPlayerId`),
    FOREIGN KEY (`SourcePlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`TargetPlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ReputationStatistics`
--

DROP TABLE IF EXISTS `ReputationStatistics`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ReputationStatistics`
(
    `PlayerId`     bigint(20)       NOT NULL,
    `Date`         date             NOT NULL,
    `SeasonNumber` int(11) UNSIGNED NOT NULL,
    `Reputation`   int(11) UNSIGNED NOT NULL,
    PRIMARY KEY (`PlayerId`, `Date`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SeasonStatistics`
--

DROP TABLE IF EXISTS `SeasonStatistics`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SeasonStatistics`
(
    `PlayerId`         bigint(20)       NOT NULL,
    `SeasonNumber`     int(11) UNSIGNED NOT NULL,
    `Reputation`       int(11) UNSIGNED NOT NULL,
    `BattlesPlayed`    int(11) UNSIGNED NOT NULL,
    `Kills`            int(11) UNSIGNED NOT NULL,
    `ExperienceEarned` int(11) UNSIGNED NOT NULL,
    `CrystalsEarned`   int(11) UNSIGNED NOT NULL,
    `XCrystalsEarned`  int(11) UNSIGNED NOT NULL,
    `ContainersOpened` int(11) UNSIGNED NOT NULL,
    PRIMARY KEY (`PlayerId`, `SeasonNumber`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Shells`
--

DROP TABLE IF EXISTS `Shells`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Shells`
(
    `Id`       bigint(20) NOT NULL,
    `WeaponId` bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `WeaponId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `WeaponId`) REFERENCES `Weapons` (`PlayerId`, `Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Statistics`
--

DROP TABLE IF EXISTS `Statistics`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Statistics`
(
    `PlayerId`                     bigint(20)          NOT NULL,
    `BattlesParticipated`          bigint(20)          NOT NULL,
    `AllBattlesParticipated`       bigint(20)          NOT NULL,
    `AllCustomBattlesParticipated` bigint(20)          NOT NULL,
    `Victories`                    bigint(20)          NOT NULL,
    `Defeats`                      bigint(20)          NOT NULL,
    `CrystalsEarned`               bigint(20) UNSIGNED NOT NULL,
    `XCrystalsEarned`              bigint(20) UNSIGNED NOT NULL,
    `Kills`                        int(11) UNSIGNED    NOT NULL,
    `Deaths`                       int(11) UNSIGNED    NOT NULL,
    `FlagsDelivered`               int(11) UNSIGNED    NOT NULL,
    `FlagsReturned`                int(11) UNSIGNED    NOT NULL,
    `GoldBoxesCaught`              int(11) UNSIGNED    NOT NULL,
    `Shots`                        int(11) UNSIGNED    NOT NULL,
    `Hits`                         int(11) UNSIGNED    NOT NULL,
    PRIMARY KEY (`PlayerId`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Weapons`
--

DROP TABLE IF EXISTS `Weapons`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Weapons`
(
    `Id`            bigint(20) NOT NULL,
    `Xp`            bigint(20) NOT NULL,
    `SkinId`        bigint(20) NOT NULL,
    `ShellId`       bigint(20) NOT NULL,
    `Kills`         bigint(20) NOT NULL,
    `BattlesPlayed` bigint(20) NOT NULL,
    `PlayerId`      bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `WeaponSkins`
--

DROP TABLE IF EXISTS `WeaponSkins`;
/*!40101 SET @saved_cs_client = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `WeaponSkins`
(
    `Id`       bigint(20) NOT NULL,
    `WeaponId` bigint(20) NOT NULL,
    `PlayerId` bigint(20) NOT NULL,
    PRIMARY KEY (`PlayerId`, `WeaponId`, `Id`),
    FOREIGN KEY (`PlayerId`) REFERENCES `Players` (`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`PlayerId`, `WeaponId`) REFERENCES `Weapons` (`PlayerId`, `Id`) ON DELETE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE = @OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE = @OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS = @OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT = @OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS = @OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION = @OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES = @OLD_SQL_NOTES */;
