-- phpMyAdmin SQL Dump
-- version 4.2.12deb2+deb8u2
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Jan 30, 2018 at 12:38 AM
-- Server version: 5.5.57-0+deb8u1
-- PHP Version: 5.6.30-0+deb8u1

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `nesvideos_site`
--

DELIMITER $$
--
-- Functions
--
DROP FUNCTION IF EXISTS `calc_baxter_formula`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `calc_baxter_formula`(userid INT,
   x DECIMAL(13,10),
   y DECIMAL(13,10)) RETURNS decimal(30,12)
    READS SQL DATA
    DETERMINISTIC
BEGIN
  DECLARE result DECIMAL(30,12) DEFAULT 0;
  DECLARE nmovies INT DEFAULT 0;
  
  SELECT SUM(POW(calc_movie_average_rating(m.id),x)
           / POW(calc_number_of_authors(m.id),y)  
           * GREATEST(0.000001, 1*(m.obsoleted_by=-1))),
         (SELECT COUNT(m2.id) FROM movie m2,user_player up2 
                              WHERE m2.playerid=up2.playerid
                              and up2.userid=userid   
                              AND m2.obsoleted_by<>-1)
         INTO result,nmovies    
    FROM movie m,user_player up 
    WHERE m.playerid=up.playerid
    AND up.userid=userid; 
  IF result IS NULL AND nmovies > 0 THEN
    SET result = 0.000000000001;
  END IF;
  RETURN result;
END$$

DROP FUNCTION IF EXISTS `calc_donation_worth`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `calc_donation_worth`(userid INT) RETURNS decimal(10,4)
    READS SQL DATA
    DETERMINISTIC
BEGIN
  DECLARE d DECIMAL(10,4) DEFAULT 0;
  SELECT donated INTO d FROM users WHERE id=userid;
  RETURN pow(d,0.7)/pow(20,0.7);
END$$

DROP FUNCTION IF EXISTS `calc_movie_average_rating`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `calc_movie_average_rating`(movieid INT) RETURNS decimal(13,10)
    READS SQL DATA
    DETERMINISTIC
BEGIN
  
  DECLARE result DECIMAL(13,10) DEFAULT 0;
  SELECT AVG(u.auth_sum * mr.value * r.priority)
        /AVG(r.priority)
        /AVG(u.auth_sum) INTO result
   FROM movie_rating mr
   JOIN rating r ON mr.ratingname=r.name
   JOIN users  u ON mr.userid=u.id   
   WHERE mr.movieid = movieid;
  RETURN result;
END$$

DROP FUNCTION IF EXISTS `calc_movie_pagename`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `calc_movie_pagename`(type CHAR(1),
   movieid INT,
   submissionid INT,
   pagename VARCHAR(255)
 ) RETURNS varchar(255) CHARSET latin1
    DETERMINISTIC
BEGIN
  CASE type
    WHEN 'M' THEN RETURN CONCAT(movieid, 'M');
    WHEN 'S' THEN RETURN CONCAT(submissionid, 'S');
    ELSE RETURN pagename;
  END CASE;
  RETURN pagename;
END$$

DROP FUNCTION IF EXISTS `calc_number_of_authors`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `calc_number_of_authors`(movieid INT) RETURNS int(11)
    READS SQL DATA
    DETERMINISTIC
BEGIN
  DECLARE result INT DEFAULT 0;
  SELECT COUNT(up.userid)INTO result
   FROM user_player up,movie m
   WHERE up.playerid=m.playerid
   AND m.id=movieid;
  RETURN result;   
END$$

DROP FUNCTION IF EXISTS `calc_submission_average_rating`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `calc_submission_average_rating`(submissionid INT) RETURNS decimal(13,10)
    READS SQL DATA
    DETERMINISTIC
BEGIN
  
  DECLARE result DECIMAL(13,10) DEFAULT 0;
  SELECT AVG(u.auth_sum * sr.value * r.priority)
        /AVG(r.priority)
        /AVG(u.auth_sum) INTO result
   FROM submission_rating sr
   JOIN rating r ON sr.ratingname=r.name
   JOIN users  u ON sr.userid=u.id
   WHERE sr.submissionid = submissionid;
  RETURN result;
END$$

DROP FUNCTION IF EXISTS `get_force_userid`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `get_force_userid`(uname VARCHAR(255), ip TEXT) RETURNS int(11)
    MODIFIES SQL DATA
    DETERMINISTIC
BEGIN
  DECLARE uid INT;
  
    SELECT id INTO uid FROM users WHERE UPPER(name)=UPPER(uname);
    IF uid IS NULL THEN
      INSERT INTO users SET name=
        (SELECT username FROM nesvideos_forum.users
                         WHERE username=uname);
      SET uid = LAST_INSERT_ID();
      INSERT INTO user_maintenancelog
              (userid,editorid,timestamp,type,content)
        VALUES(uid, 344,
               UNIX_TIMESTAMP(NOW()),
               'N',
               CONCAT('New registration from ', ip));
      INSERT INTO user_role (user,role) VALUES(uid, 0);
    END IF;
  
  RETURN uid;
END$$

DROP FUNCTION IF EXISTS `get_force_userid2`$$
CREATE DEFINER=`root`@`%` FUNCTION `get_force_userid2`(uname VARCHAR(255), ip TEXT) RETURNS int(11)
    MODIFIES SQL DATA
    DETERMINISTIC
BEGIN
  DECLARE uid INT;
  
    SELECT id INTO uid FROM users WHERE UPPER(name)=UPPER(uname);
    IF uid IS NULL THEN
      INSERT INTO users SET name=
        (SELECT username FROM nesvideos_forum.users
                         WHERE username=uname);
      SET uid = LAST_INSERT_ID();
      INSERT INTO user_maintenancelog
              (userid,editorid,timestamp,type,content)
        VALUES(uid, 344,
               UNIX_TIMESTAMP(NOW()),
               'N',
               CONCAT('New registration from ', ip));
      INSERT INTO user_role (user,role) VALUES(uid, 0);
    END IF;
  
  RETURN uid;
END$$

DROP FUNCTION IF EXISTS `get_user_role`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `get_user_role`(userid INT) RETURNS varchar(255) CHARSET latin1
    READS SQL DATA
    DETERMINISTIC
BEGIN
  DECLARE result VARCHAR(255);
  SELECT GROUP_CONCAT(roles.name ORDER BY roles.id DESC SEPARATOR ',') INTO result FROM user_role JOIN roles ON user_role.role=roles.id WHERE user_role.user=userid;
  RETURN result;
END$$

DROP FUNCTION IF EXISTS `user_evaluate`$$
CREATE DEFINER=`grunt`@`localhost` FUNCTION `user_evaluate`(role VARCHAR(255),
        name VARCHAR(255)) RETURNS decimal(10,4)
    READS SQL DATA
    DETERMINISTIC
BEGIN
 DECLARE posts, has_movies INT;
 DECLARE postmin INT DEFAULT 22;
 DECLARE scale DECIMAL(10,4) DEFAULT 1.0;
 CASE
   WHEN role LIKE '%adminassistant%' THEN RETURN 1.0;
   WHEN role LIKE '%admin%' THEN RETURN 1.0;
   WHEN role LIKE '%senior%' THEN RETURN 1.0;
   WHEN role LIKE '%judge%' THEN RETURN 1.0;
   WHEN role LIKE '%publisher%' THEN RETURN 1.0;
   WHEN role LIKE '%nobody%' THEN RETURN 0.00001;
   ELSE SET has_movies=0;
 END CASE;
 SELECT
   (SELECT user_posts FROM nesvideos_forum.users
    WHERE username=name),
   (SELECT COUNT(up.playerid)FROM user_player up,users u
    WHERE userid=u.id and u.name=name)
 INTO posts, has_movies;
 
 IF has_movies > 0 THEN
   SET postmin = 9;
   SET scale   = 1.0;
 END IF;
 
 IF name = 'neofix' or name = 'C0DE RED' or name = 'p0rtal_0f_rain' OR name = 'GGG' OR name = 'Mister Frank' THEN
   SET SCALE = 0.00001;
 END IF;
 
 IF posts >= postmin THEN RETURN scale; END IF;
 RETURN (posts+1) * scale / postmin;
END$$

DROP FUNCTION IF EXISTS `user_has_privilege`$$
CREATE DEFINER=`root`@`localhost` FUNCTION `user_has_privilege`(`username` varchar(255), `privilege` varchar(255)) RETURNS varchar(255) CHARSET latin1
    READS SQL DATA
    DETERMINISTIC
RETURN
(
  SELECT `users`.`name` FROM `users`
  INNER JOIN `user_role` ON `user_role`.`user`=`users`.`id`
  INNER JOIN `roles` ON `roles`.`id`=`user_role`.`role`
  INNER JOIN `role_privilege` ON `role_privilege`.`role`=`roles`.`id`
  INNER JOIN `privileges` ON `privileges`.`id`=`role_privilege`.`privilege`
  WHERE `users`.`name`=`username` AND `privileges`.`name`=`privilege`
  UNION
  SELECT `users`.`name` FROM `users`
  INNER JOIN `user_privilege` ON `user_privilege`.`user`=`users`.`id`
  INNER JOIN `privileges` ON `privileges`.`id`=`user_privilege`.`privilege`
  WHERE `users`.`name`=`username` AND `privileges`.`name`=`privilege`
)$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `access_analyzer`
--

DROP TABLE IF EXISTS `access_analyzer`;
CREATE TABLE IF NOT EXISTS `access_analyzer` (
  `accesstime` int(11) NOT NULL,
  `ipv6prefix` char(24) NOT NULL DEFAULT '00000000000000000000ffff',
  `ipaddr` int(11) unsigned NOT NULL,
  `refer` varchar(255) NOT NULL,
  `url` varchar(255) NOT NULL,
  `agent` varchar(255) NOT NULL
) ENGINE=MEMORY DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `access_blocks`
--

DROP TABLE IF EXISTS `access_blocks`;
CREATE TABLE IF NOT EXISTS `access_blocks` (
  `ipv6prefix` char(24) NOT NULL DEFAULT '00000000000000000000ffff',
  `ipaddr` int(11) unsigned NOT NULL,
  `blockbegin` int(11) NOT NULL,
  `blockend` int(11) NOT NULL,
  `blocktype` tinyint(4) NOT NULL DEFAULT '0'
) ENGINE=MEMORY DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `api_keys`
--

DROP TABLE IF EXISTS `api_keys`;
CREATE TABLE IF NOT EXISTS `api_keys` (
`id` int(11) NOT NULL,
  `user` int(11) NOT NULL,
  `data` varchar(255) CHARACTER SET latin1 COLLATE latin1_bin NOT NULL,
  `ktype` int(11) NOT NULL,
  `ktag` varchar(255) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `api_privileges`
--

DROP TABLE IF EXISTS `api_privileges`;
CREATE TABLE IF NOT EXISTS `api_privileges` (
  `key` int(11) NOT NULL,
  `privilege` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `api_sessions`
--

DROP TABLE IF EXISTS `api_sessions`;
CREATE TABLE IF NOT EXISTS `api_sessions` (
`id` bigint(20) unsigned NOT NULL,
  `pkey` int(11) NOT NULL,
  `expiry` bigint(20) NOT NULL,
  `nonces` int(10) unsigned NOT NULL,
  `nonces_hi` int(10) unsigned NOT NULL,
  `slock` int(11) NOT NULL,
  `secret` char(44) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `awards`
--

DROP TABLE IF EXISTS `awards`;
CREATE TABLE IF NOT EXISTS `awards` (
  `UserID` int(11) NOT NULL,
  `MovieID` int(11) NOT NULL,
  `Award` int(11) NOT NULL,
  `Year` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `awards_classes`
--

DROP TABLE IF EXISTS `awards_classes`;
CREATE TABLE IF NOT EXISTS `awards_classes` (
`award` int(11) NOT NULL,
  `awardclass` enum('movie','user') NOT NULL,
  `shortname` varchar(32) NOT NULL,
  `description` varchar(255) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `awards_temp`
--

DROP TABLE IF EXISTS `awards_temp`;
CREATE TABLE IF NOT EXISTS `awards_temp` (
  `UserID` int(11) NOT NULL,
  `MovieID` int(11) NOT NULL,
  `Award` enum('tas_year','tas_first','tas_nes','tas_snes','tas_n64','tas_sega','tas_gbx','tas_gba','tas_ds','tas_arcade','tas_pce','tas_psx','tas_new','taser_year','rookie_year','taser_nes','taser_snes','taser_n64','taser_sega','taser_gbx','taser_gba','taser_ds','taser_arcade','taser_pce','taser_psx','taser_new','tas_funny','tas_glitchy','tas_lucky','tas_speedy','tas_innovative','tas_computer','taser_computer','tas_n64wii','taser_n64wii') NOT NULL,
  `Year` int(11) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `classtype`
--

DROP TABLE IF EXISTS `classtype`;
CREATE TABLE IF NOT EXISTS `classtype` (
`id` int(11) NOT NULL,
  `abbr` varchar(255) NOT NULL DEFAULT '',
  `positivetext` varchar(255) NOT NULL DEFAULT '',
  `negativetext` varchar(255) NOT NULL DEFAULT '',
  `specific` enum('N','Y') NOT NULL DEFAULT 'N',
  `old_id` int(11) unsigned NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=9058 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `donate`
--

DROP TABLE IF EXISTS `donate`;
CREATE TABLE IF NOT EXISTS `donate` (
  `username` varchar(64) NOT NULL,
  `times` int(10) unsigned NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `filetype`
--

DROP TABLE IF EXISTS `filetype`;
CREATE TABLE IF NOT EXISTS `filetype` (
  `filetype` char(2) NOT NULL DEFAULT '',
  `filetypeclass` enum('M','T','I','S','U','O') NOT NULL,
  `fileext` varchar(4) NOT NULL DEFAULT '',
  `description` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `flagstype`
--

DROP TABLE IF EXISTS `flagstype`;
CREATE TABLE IF NOT EXISTS `flagstype` (
`id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `iconpath` varchar(255) DEFAULT NULL,
  `linkpage` varchar(255) DEFAULT NULL,
  `key` varchar(48) NOT NULL,
  `special` int(11) NOT NULL DEFAULT '0',
  `privilege` varchar(255) DEFAULT NULL,
  `privilegeid` int(11) DEFAULT NULL,
  `hidden` tinyint(1) NOT NULL DEFAULT '0',
  `priority` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `game`
--

DROP TABLE IF EXISTS `game`;
CREATE TABLE IF NOT EXISTS `game` (
`id` int(11) NOT NULL,
  `systemid` int(11) NOT NULL DEFAULT '0',
  `derived_from` int(11) NOT NULL DEFAULT '-1',
  `gamename` varchar(255) NOT NULL DEFAULT '',
  `description` text NOT NULL,
  `resources` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `gamename`
--

DROP TABLE IF EXISTS `gamename`;
CREATE TABLE IF NOT EXISTS `gamename` (
`gn_id` int(11) NOT NULL,
  `goodname` varchar(255) NOT NULL COMMENT 'Good Set or some other official naming convention',
  `displayname` varchar(64) NOT NULL COMMENT 'Name used for display purposes',
  `abbreviation` varchar(8) DEFAULT NULL COMMENT 'abbreviation',
  `sys_id` int(11) DEFAULT NULL COMMENT 'id from the system table',
  `resource_page` varchar(255) DEFAULT NULL,
  `searchkey` varchar(64) DEFAULT NULL,
  `youtube_tags` varchar(255) NOT NULL DEFAULT '',
  `screenshot` varchar(255) DEFAULT NULL
) ENGINE=InnoDB AUTO_INCREMENT=1993 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `gamename_group`
--

DROP TABLE IF EXISTS `gamename_group`;
CREATE TABLE IF NOT EXISTS `gamename_group` (
  `gn_id` int(11) NOT NULL,
  `group` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `gamename_groupname`
--

DROP TABLE IF EXISTS `gamename_groupname`;
CREATE TABLE IF NOT EXISTS `gamename_groupname` (
`id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `searchkey` varchar(255) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=207 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `gamename_link`
--

DROP TABLE IF EXISTS `gamename_link`;
CREATE TABLE IF NOT EXISTS `gamename_link` (
  `gn_id` int(11) NOT NULL,
  `link` int(11) NOT NULL,
  `linktype` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `gamename_linkclass`
--

DROP TABLE IF EXISTS `gamename_linkclass`;
CREATE TABLE IF NOT EXISTS `gamename_linkclass` (
  `type` int(11) NOT NULL,
  `description` varchar(255) NOT NULL,
  `dual` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `img_access_log`
--

DROP TABLE IF EXISTS `img_access_log`;
CREATE TABLE IF NOT EXISTS `img_access_log` (
  `id` char(32) NOT NULL,
  `url_md5` char(32) NOT NULL,
  `counter` int(10) unsigned NOT NULL DEFAULT '1',
  `timestamp` int(11) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `img_size_cache`
--

DROP TABLE IF EXISTS `img_size_cache`;
CREATE TABLE IF NOT EXISTS `img_size_cache` (
  `url_md5` char(32) NOT NULL,
  `url` varchar(8000) NOT NULL,
  `timestamp` int(11) NOT NULL,
  `width` mediumint(9) NOT NULL,
  `height` mediumint(9) NOT NULL,
  `is_animated` enum('N','Y') NOT NULL DEFAULT 'N',
  `blobfile` varchar(255) DEFAULT NULL,
  `blob_tilex` mediumint(9) DEFAULT NULL,
  `blob_tiley` mediumint(9) DEFAULT NULL,
  `blob_x` varchar(20000) DEFAULT NULL,
  `blob_y` varchar(20000) DEFAULT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie`
--

DROP TABLE IF EXISTS `movie`;
CREATE TABLE IF NOT EXISTS `movie` (
`id` int(11) NOT NULL,
  `playerid` int(11) NOT NULL,
  `gameid` int(11) NOT NULL DEFAULT '0',
  `systemid` int(11) NOT NULL,
  `gamename` varchar(255) NOT NULL DEFAULT '',
  `nickname` varchar(255) NOT NULL,
  `gameversion` varchar(255) NOT NULL DEFAULT 'USA',
  `romname` varchar(255) NOT NULL DEFAULT '',
  `lastchange` int(11) NOT NULL DEFAULT '0',
  `submissionid` int(11) NOT NULL DEFAULT '-1',
  `obsoleted_by` int(11) NOT NULL DEFAULT '-1',
  `recommended` enum('N','Y','T') NOT NULL DEFAULT 'N',
  `verified` enum('N','Y') NOT NULL DEFAULT 'N',
  `inpure` enum('N','Y') NOT NULL DEFAULT 'N',
  `pubdate` int(11) NOT NULL DEFAULT '0',
  `published_by` int(11) NOT NULL DEFAULT '-1',
  `tier` int(11) NOT NULL DEFAULT '2',
  `rating` decimal(13,10) NOT NULL DEFAULT '0.0000000000'
) ENGINE=InnoDB AUTO_INCREMENT=3619 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_class`
--

DROP TABLE IF EXISTS `movie_class`;
CREATE TABLE IF NOT EXISTS `movie_class` (
  `movieid` int(11) NOT NULL DEFAULT '0',
  `classid` int(11) NOT NULL DEFAULT '0',
  `value` tinyint(3) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Triggers `movie_class`
--
DROP TRIGGER IF EXISTS `movie_class_insert`;
DELIMITER //
CREATE TRIGGER `movie_class_insert` BEFORE INSERT ON `movie_class`
 FOR EACH ROW BEGIN 
    UPDATE movie SET lastchange=UNIX_TIMESTAMP(NOW()) where id=NEW.movieid;
  END
//
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `movie_file`
--

DROP TABLE IF EXISTS `movie_file`;
CREATE TABLE IF NOT EXISTS `movie_file` (
`id` int(11) NOT NULL,
  `movieid` int(11) NOT NULL DEFAULT '0',
  `filesize` bigint(18) NOT NULL DEFAULT '0',
  `filename` text NOT NULL,
  `filetime` int(11) NOT NULL DEFAULT '0',
  `filererecords` int(11) NOT NULL DEFAULT '0',
  `length` decimal(10,3) NOT NULL DEFAULT '0.000',
  `typech` char(2) NOT NULL,
  `description` text NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `uploader` int(11) DEFAULT NULL,
  `uploaded` int(11) DEFAULT NULL,
  `updated` int(11) DEFAULT NULL
) ENGINE=InnoDB AUTO_INCREMENT=31221 DEFAULT CHARSET=latin1;

--
-- Triggers `movie_file`
--
DROP TRIGGER IF EXISTS `movie_file_insert`;
DELIMITER //
CREATE TRIGGER `movie_file_insert` BEFORE INSERT ON `movie_file`
 FOR EACH ROW BEGIN
    UPDATE movie SET lastchange=UNIX_TIMESTAMP(NOW()) where id=NEW.movieid;
  END
//
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `movie_file_storage`
--

DROP TABLE IF EXISTS `movie_file_storage`;
CREATE TABLE IF NOT EXISTS `movie_file_storage` (
  `filename` varchar(255) NOT NULL DEFAULT '',
  `filedata` longblob NOT NULL,
  `filetime` varchar(64) NOT NULL DEFAULT '',
  `zipped` enum('N','Y') NOT NULL DEFAULT 'Y'
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_flag`
--

DROP TABLE IF EXISTS `movie_flag`;
CREATE TABLE IF NOT EXISTS `movie_flag` (
  `movie` int(11) NOT NULL,
  `flag` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_formats`
--

DROP TABLE IF EXISTS `movie_formats`;
CREATE TABLE IF NOT EXISTS `movie_formats` (
  `format` varchar(4) NOT NULL,
  `filetype` char(2) NOT NULL,
  `alias` varchar(4) DEFAULT NULL,
  `blocked` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_maintenancelog`
--

DROP TABLE IF EXISTS `movie_maintenancelog`;
CREATE TABLE IF NOT EXISTS `movie_maintenancelog` (
`id` int(11) NOT NULL,
  `movieid` int(11) NOT NULL,
  `userid` int(11) NOT NULL,
  `timestamp` int(11) NOT NULL,
  `type` enum('C','F','T','R','H','I','L') NOT NULL,
  `addremove` enum('A','R','C') NOT NULL,
  `content` text
) ENGINE=InnoDB AUTO_INCREMENT=56827 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_rating`
--

DROP TABLE IF EXISTS `movie_rating`;
CREATE TABLE IF NOT EXISTS `movie_rating` (
  `userid` int(11) NOT NULL,
  `movieid` int(11) NOT NULL DEFAULT '0',
  `ratingname` varchar(255) NOT NULL DEFAULT '',
  `value` decimal(5,2) NOT NULL DEFAULT '5.00'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_search`
--

DROP TABLE IF EXISTS `movie_search`;
CREATE TABLE IF NOT EXISTS `movie_search` (
  `movieid` int(11) NOT NULL DEFAULT '0',
  `gamename` text NOT NULL,
  `nickname` text NOT NULL,
  `gamenick` text NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_streaming`
--

DROP TABLE IF EXISTS `movie_streaming`;
CREATE TABLE IF NOT EXISTS `movie_streaming` (
`id` int(11) NOT NULL,
  `movieid` int(11) NOT NULL DEFAULT '0',
  `userid` int(11) NOT NULL,
  `user_enabled` enum('N','Y') NOT NULL DEFAULT 'N',
  `user_modtime` int(11) NOT NULL DEFAULT '0',
  `server_enabled` enum('N','Y') NOT NULL DEFAULT 'N',
  `server_modtime` int(11) NOT NULL DEFAULT '0',
  `server_comment` text,
  `class` enum('M','C','H','T','O') NOT NULL DEFAULT 'M',
  `is_streaming` enum('N','Y') NOT NULL DEFAULT 'Y',
  `streaming_url` varchar(255) NOT NULL DEFAULT '',
  `description` text NOT NULL,
  `length` decimal(10,3) NOT NULL DEFAULT '0.000',
  `filesize` int(11) NOT NULL DEFAULT '0',
  `width` smallint(6) NOT NULL DEFAULT '256',
  `height` smallint(6) NOT NULL DEFAULT '224'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `movie_tiers`
--

DROP TABLE IF EXISTS `movie_tiers`;
CREATE TABLE IF NOT EXISTS `movie_tiers` (
`id` int(11) NOT NULL,
  `name` varchar(64) NOT NULL,
  `weight` double NOT NULL,
  `privilege` varchar(255) DEFAULT NULL,
  `privilegeid` int(11) DEFAULT NULL,
  `iconpath` varchar(255) DEFAULT NULL,
  `linkpage` varchar(255) DEFAULT NULL,
  `starlike` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `page_alias`
--

DROP TABLE IF EXISTS `page_alias`;
CREATE TABLE IF NOT EXISTS `page_alias` (
  `id` int(11) NOT NULL DEFAULT '0',
  `pagename` varchar(255) NOT NULL DEFAULT '',
  `actualname` varchar(255) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `page_counter`
--

DROP TABLE IF EXISTS `page_counter`;
CREATE TABLE IF NOT EXISTS `page_counter` (
  `pagename` varchar(255) NOT NULL DEFAULT '',
  `counter` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `page_privacy`
--

DROP TABLE IF EXISTS `page_privacy`;
CREATE TABLE IF NOT EXISTS `page_privacy` (
`id` mediumint(9) NOT NULL,
  `pagename` varchar(255) NOT NULL,
  `view_role` int(11) DEFAULT NULL,
  `view_privilege` int(11) DEFAULT NULL,
  `edit_role` int(11) DEFAULT NULL,
  `edit_privilege` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `player`
--

DROP TABLE IF EXISTS `player`;
CREATE TABLE IF NOT EXISTS `player` (
`id` int(11) NOT NULL,
  `fullname` varchar(255) NOT NULL DEFAULT '',
  `name` varchar(255) NOT NULL DEFAULT '',
  `url` varchar(255) NOT NULL DEFAULT ''
) ENGINE=InnoDB AUTO_INCREMENT=1047 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `privileges`
--

DROP TABLE IF EXISTS `privileges`;
CREATE TABLE IF NOT EXISTS `privileges` (
`id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `weight` smallint(6) NOT NULL DEFAULT '0',
  `lowpriv` int(11) NOT NULL DEFAULT '0',
  `privilege` int(11) DEFAULT NULL,
  `is_system` tinyint(1) NOT NULL DEFAULT '0',
  `unassignable` tinyint(1) NOT NULL DEFAULT '0',
  `rankname` varchar(255) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=87 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `profile`
--

DROP TABLE IF EXISTS `profile`;
CREATE TABLE IF NOT EXISTS `profile` (
`id` int(11) NOT NULL,
  `uri` varchar(255) NOT NULL DEFAULT '',
  `postdata` text,
  `duration` decimal(10,6) NOT NULL DEFAULT '0.000000',
  `usertime` decimal(10,6) NOT NULL DEFAULT '0.000000',
  `time` int(11) NOT NULL DEFAULT '0'
) ENGINE=MyISAM AUTO_INCREMENT=11803941 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ramaddresses`
--

DROP TABLE IF EXISTS `ramaddresses`;
CREATE TABLE IF NOT EXISTS `ramaddresses` (
`id` int(11) NOT NULL,
  `addrset` int(11) NOT NULL,
  `address` bigint(20) NOT NULL,
  `datatype` enum('byte','word','dword','float','Q12.4','Q20.12','Q20.4','Q28.4','Q8.8','Q16.8','Q24.8','Q16.16','tbyte') NOT NULL DEFAULT 'word',
  `signed` enum('signed','unsigned','hex') NOT NULL DEFAULT 'signed',
  `endian` enum('big','little','host') NOT NULL,
  `description` varchar(255) NOT NULL,
  `domain` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=1822 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ramaddresses_domains`
--

DROP TABLE IF EXISTS `ramaddresses_domains`;
CREATE TABLE IF NOT EXISTS `ramaddresses_domains` (
`domain` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `system` int(11) DEFAULT NULL
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ramaddresses_log`
--

DROP TABLE IF EXISTS `ramaddresses_log`;
CREATE TABLE IF NOT EXISTS `ramaddresses_log` (
`sequence_id` bigint(20) NOT NULL,
  `user` int(11) NOT NULL,
  `timestamp` bigint(20) NOT NULL,
  `set_id` int(11) NOT NULL,
  `addr_id` int(11) DEFAULT NULL,
  `changetype` char(1) NOT NULL,
  `old_value` varchar(255) DEFAULT NULL,
  `new_value` varchar(255) DEFAULT NULL
) ENGINE=InnoDB AUTO_INCREMENT=13187 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ramaddresses_sets`
--

DROP TABLE IF EXISTS `ramaddresses_sets`;
CREATE TABLE IF NOT EXISTS `ramaddresses_sets` (
`addrset` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `nonce` char(32) DEFAULT NULL,
  `system` int(11) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=119 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ranks`
--

DROP TABLE IF EXISTS `ranks`;
CREATE TABLE IF NOT EXISTS `ranks` (
`id` int(11) NOT NULL,
  `name` varchar(50) NOT NULL DEFAULT '',
  `assign_privilege` int(11) NOT NULL,
  `weight` smallint(6) NOT NULL DEFAULT '0',
  `supercede_role` int(11) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=46 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ranks_variant`
--

DROP TABLE IF EXISTS `ranks_variant`;
CREATE TABLE IF NOT EXISTS `ranks_variant` (
  `baserank` enum('user','player') NOT NULL,
  `minarg` int(11) NOT NULL,
  `rank` varchar(255) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `rating`
--

DROP TABLE IF EXISTS `rating`;
CREATE TABLE IF NOT EXISTS `rating` (
  `name` varchar(255) NOT NULL DEFAULT '',
  `description` varchar(255) NOT NULL DEFAULT '',
  `priority` tinyint(4) NOT NULL DEFAULT '100'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `rejections`
--

DROP TABLE IF EXISTS `rejections`;
CREATE TABLE IF NOT EXISTS `rejections` (
  `id` int(11) NOT NULL COMMENT 'submission id of rejected submission',
  `reason` int(11) NOT NULL COMMENT 'reason for rejection'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `rejections_reasons`
--

DROP TABLE IF EXISTS `rejections_reasons`;
CREATE TABLE IF NOT EXISTS `rejections_reasons` (
  `id` int(11) NOT NULL,
  `text` varchar(255) NOT NULL,
  `priority` smallint(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `role_maintenancelog`
--

DROP TABLE IF EXISTS `role_maintenancelog`;
CREATE TABLE IF NOT EXISTS `role_maintenancelog` (
`id` int(11) NOT NULL,
  `userid` int(11) NOT NULL,
  `timestamp` int(11) NOT NULL,
  `type` enum('R','A','M') NOT NULL,
  `content` varchar(255) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=240 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `role_privilege`
--

DROP TABLE IF EXISTS `role_privilege`;
CREATE TABLE IF NOT EXISTS `role_privilege` (
  `role` int(11) NOT NULL,
  `privilege` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
CREATE TABLE IF NOT EXISTS `roles` (
`id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `assign_privilege` int(11) DEFAULT NULL,
  `weight` smallint(6) NOT NULL DEFAULT '0',
  `login` int(11) NOT NULL,
  `rank` int(11) NOT NULL DEFAULT '0',
  `rankname` varchar(255) NOT NULL,
  `undeleteable` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `roms`
--

DROP TABLE IF EXISTS `roms`;
CREATE TABLE IF NOT EXISTS `roms` (
`rom_id` int(11) NOT NULL,
  `md5` char(32) NOT NULL,
  `sha1` char(40) NOT NULL,
  `description` varchar(255) NOT NULL,
  `addrset` int(11) DEFAULT NULL,
  `gn_id` int(11) DEFAULT NULL,
  `type` char(1) NOT NULL DEFAULT 'G'
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `session`
--

DROP TABLE IF EXISTS `session`;
CREATE TABLE IF NOT EXISTS `session` (
  `id` char(32) NOT NULL DEFAULT '',
  `starttime` int(11) NOT NULL DEFAULT '0',
  `lasttime` int(11) NOT NULL DEFAULT '0',
  `sessdata` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `site_constants`
--

DROP TABLE IF EXISTS `site_constants`;
CREATE TABLE IF NOT EXISTS `site_constants` (
  `constant` int(11) NOT NULL,
  `value` bigint(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `site_links`
--

DROP TABLE IF EXISTS `site_links`;
CREATE TABLE IF NOT EXISTS `site_links` (
  `text_id` int(11) NOT NULL DEFAULT '-1',
  `dest_page` varchar(255) NOT NULL DEFAULT '',
  `link_count` smallint(6) NOT NULL DEFAULT '1',
  `excerpt` varchar(255) DEFAULT NULL,
  `link_type` enum('normal','autogenerated') NOT NULL DEFAULT 'normal'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `site_text`
--

DROP TABLE IF EXISTS `site_text`;
CREATE TABLE IF NOT EXISTS `site_text` (
`id` int(11) NOT NULL,
  `type` enum('M','P','S') CHARACTER SET latin1 NOT NULL DEFAULT 'P',
  `movieid` int(11) NOT NULL DEFAULT '-1',
  `pagename` varchar(255) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `submissionid` int(11) NOT NULL DEFAULT '-1',
  `minoredit` enum('N','Y') CHARACTER SET latin1 NOT NULL DEFAULT 'N',
  `whyedit` text NOT NULL,
  `revision` smallint(6) NOT NULL DEFAULT '1',
  `userid` int(11) NOT NULL,
  `timestamp` int(11) NOT NULL DEFAULT '0',
  `description` longtext NOT NULL,
  `obsoleted_by` int(11) NOT NULL DEFAULT '-1',
  `ipaddr` varchar(255) CHARACTER SET latin1 NOT NULL DEFAULT '',
  `num_lines` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=77618 DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `site_text_search`
--

DROP TABLE IF EXISTS `site_text_search`;
CREATE TABLE IF NOT EXISTS `site_text_search` (
  `site_text_id` int(11) NOT NULL DEFAULT '0',
  `title` text NOT NULL,
  `title_metaphone` text NOT NULL,
  `class_metaphone` text NOT NULL,
  `description` text NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `sort_url`
--

DROP TABLE IF EXISTS `sort_url`;
CREATE TABLE IF NOT EXISTS `sort_url` (
`id` int(11) NOT NULL,
  `timestamp` int(11) NOT NULL,
  `expiretime` int(11) NOT NULL,
  `url` varchar(255) DEFAULT NULL
) ENGINE=MEMORY AUTO_INCREMENT=1282 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `submission`
--

DROP TABLE IF EXISTS `submission`;
CREATE TABLE IF NOT EXISTS `submission` (
`id` int(11) NOT NULL,
  `userid` int(11) NOT NULL,
  `timestamp` int(11) NOT NULL DEFAULT '0',
  `systemid` int(11) NOT NULL,
  `gamename` varchar(255) NOT NULL DEFAULT '',
  `nickname` varchar(255) NOT NULL,
  `gameversion` varchar(255) NOT NULL DEFAULT '',
  `romname` varchar(255) NOT NULL DEFAULT '',
  `authorname` varchar(255) NOT NULL DEFAULT '',
  `authornick` varchar(255) NOT NULL DEFAULT '',
  `frames` int(10) NOT NULL,
  `length` decimal(10,3) NOT NULL DEFAULT '0.000',
  `rerecords` int(11) NOT NULL DEFAULT '-1',
  `alerts` text,
  `status` enum('N','P','R','Y','C','Q','O','K','S','J') NOT NULL,
  `statusby` varchar(255) NOT NULL DEFAULT '',
  `ipaddr` varchar(255) NOT NULL DEFAULT '',
  `content` longblob,
  `subdate` int(11) NOT NULL DEFAULT '0',
  `judged_by` int(11) NOT NULL DEFAULT '-1',
  `judgedate` int(11) NOT NULL DEFAULT '-1',
  `gn_id` int(11) DEFAULT NULL COMMENT 'id from gamename',
  `emuversion` varchar(255) DEFAULT NULL,
  `intended_tier` int(11) DEFAULT NULL
) ENGINE=InnoDB AUTO_INCREMENT=5807 DEFAULT CHARSET=latin1;

--
-- Triggers `submission`
--
DROP TRIGGER IF EXISTS `submission_insert`;
DELIMITER //
CREATE TRIGGER `submission_insert` BEFORE INSERT ON `submission`
 FOR EACH ROW BEGIN
    SET NEW.timestamp=UNIX_TIMESTAMP(NOW());
  END
//
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `submission_editors`
--

DROP TABLE IF EXISTS `submission_editors`;
CREATE TABLE IF NOT EXISTS `submission_editors` (
  `userid` int(11) NOT NULL,
  `submissionid` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `submission_rating`
--

DROP TABLE IF EXISTS `submission_rating`;
CREATE TABLE IF NOT EXISTS `submission_rating` (
  `userid` int(11) NOT NULL,
  `submissionid` int(11) NOT NULL,
  `ratingname` varchar(255) NOT NULL,
  `value` decimal(5,2) NOT NULL DEFAULT '5.00'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `svnmap`
--

DROP TABLE IF EXISTS `svnmap`;
CREATE TABLE IF NOT EXISTS `svnmap` (
  `author` varchar(255) NOT NULL,
  `username` varchar(255) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `system`
--

DROP TABLE IF EXISTS `system`;
CREATE TABLE IF NOT EXISTS `system` (
`id` int(11) NOT NULL,
  `abbr` varchar(8) NOT NULL DEFAULT '',
  `name` varchar(255) NOT NULL DEFAULT '',
  `image_width` int(11) NOT NULL DEFAULT '256',
  `image_height1` int(11) NOT NULL DEFAULT '224',
  `image_height2` int(11) NOT NULL DEFAULT '240',
  `ratio_low` decimal(5,2) NOT NULL DEFAULT '0.30',
  `ratio_high` decimal(5,2) NOT NULL DEFAULT '10.10',
  `monochrome` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=44 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `system_codes`
--

DROP TABLE IF EXISTS `system_codes`;
CREATE TABLE IF NOT EXISTS `system_codes` (
  `parser` varchar(4) NOT NULL,
  `code` varchar(255) NOT NULL,
  `code2` varchar(255) NOT NULL,
  `regcode` varchar(255) NOT NULL,
  `system` int(11) NOT NULL,
  `region` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `system_framerate`
--

DROP TABLE IF EXISTS `system_framerate`;
CREATE TABLE IF NOT EXISTS `system_framerate` (
  `systemid` int(11) NOT NULL,
  `regionid` int(11) NOT NULL,
  `framerate` double NOT NULL,
  `preliminary` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `system_rules`
--

DROP TABLE IF EXISTS `system_rules`;
CREATE TABLE IF NOT EXISTS `system_rules` (
  `parser` char(4) NOT NULL,
  `priority` int(11) NOT NULL,
  `rule` text NOT NULL,
  `system` int(11) NOT NULL,
  `region` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `system_screenshot`
--

DROP TABLE IF EXISTS `system_screenshot`;
CREATE TABLE IF NOT EXISTS `system_screenshot` (
  `systemid` int(11) NOT NULL,
  `width` int(11) NOT NULL,
  `height` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `todo_list`
--

DROP TABLE IF EXISTS `todo_list`;
CREATE TABLE IF NOT EXISTS `todo_list` (
`id` int(11) NOT NULL,
  `description` text NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `priority` int(11) NOT NULL,
  `status` varchar(128) NOT NULL,
  `complete_flag` tinyint(1) NOT NULL DEFAULT '0',
  `visibility` enum('all','staff','admins','coders','devels','code_admins') NOT NULL DEFAULT 'all'
) ENGINE=MyISAM AUTO_INCREMENT=25 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `tvc_upload_stats`
--

DROP TABLE IF EXISTS `tvc_upload_stats`;
CREATE TABLE IF NOT EXISTS `tvc_upload_stats` (
  `uploader` int(11) NOT NULL,
  `amount` int(11) unsigned NOT NULL,
  `size` bigint(20) unsigned NOT NULL,
  `modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `tvc_uploads`
--

DROP TABLE IF EXISTS `tvc_uploads`;
CREATE TABLE IF NOT EXISTS `tvc_uploads` (
  `md5` char(32) COLLATE utf8_unicode_ci NOT NULL,
  `uploader` int(11) NOT NULL,
  `filename` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `filesize` bigint(20) unsigned NOT NULL,
  `title` varchar(100) COLLATE utf8_unicode_ci NOT NULL,
  `description` varchar(5000) COLLATE utf8_unicode_ci NOT NULL,
  `tags` varchar(500) COLLATE utf8_unicode_ci NOT NULL,
  `status` char(1) COLLATE utf8_unicode_ci NOT NULL COMMENT '(W)aiting, (U)ploading, (Q)ueued, (S)ideloading, (D)one',
  `imd5` char(120) COLLATE utf8_unicode_ci DEFAULT NULL,
  `ipos` bigint(20) unsigned NOT NULL DEFAULT '0',
  `ytid` char(11) COLLATE utf8_unicode_ci DEFAULT NULL,
  `modified` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `updater_ip`
--

DROP TABLE IF EXISTS `updater_ip`;
CREATE TABLE IF NOT EXISTS `updater_ip` (
`id` tinyint(4) NOT NULL,
  `ip` varchar(39) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `uploaders`
--

DROP TABLE IF EXISTS `uploaders`;
CREATE TABLE IF NOT EXISTS `uploaders` (
`id` int(11) NOT NULL,
  `uploader` varchar(255) NOT NULL,
  `site` varchar(255) NOT NULL,
  `user` int(11) DEFAULT NULL
) ENGINE=InnoDB AUTO_INCREMENT=231 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user_files`
--

DROP TABLE IF EXISTS `user_files`;
CREATE TABLE IF NOT EXISTS `user_files` (
`file_id` bigint(20) NOT NULL,
  `file_uid` int(11) NOT NULL,
  `file_name` varchar(255) NOT NULL,
  `file_content` longblob NOT NULL,
  `file_class` varchar(1) NOT NULL,
  `file_type` varchar(16) NOT NULL,
  `file_ts` bigint(20) NOT NULL,
  `file_system` int(11) DEFAULT NULL,
  `file_length` decimal(10,3) NOT NULL,
  `file_frames` int(11) NOT NULL DEFAULT '0',
  `file_rerecords` bigint(20) NOT NULL,
  `file_title` varchar(255) NOT NULL,
  `file_description` text NOT NULL,
  `file_log_len` int(11) NOT NULL,
  `file_phys_len` int(11) NOT NULL,
  `file_gn_id` int(11) DEFAULT NULL,
  `file_hidden` tinyint(4) NOT NULL DEFAULT '0',
  `file_warnings` text NOT NULL,
  `file_views` int(10) unsigned NOT NULL DEFAULT '0',
  `file_downloads` int(10) unsigned NOT NULL DEFAULT '0',
  `file_comments` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB AUTO_INCREMENT=44787462186883190 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user_files_comments`
--

DROP TABLE IF EXISTS `user_files_comments`;
CREATE TABLE IF NOT EXISTS `user_files_comments` (
`id` int(11) NOT NULL,
  `file_id` bigint(20) NOT NULL,
  `ip` varchar(255) NOT NULL,
  `parent` int(11) DEFAULT NULL,
  `title` varchar(255) NOT NULL,
  `text` text NOT NULL,
  `timestamp` bigint(20) NOT NULL,
  `userid` int(11) NOT NULL
) ENGINE=InnoDB AUTO_INCREMENT=384 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user_maintenancelog`
--

DROP TABLE IF EXISTS `user_maintenancelog`;
CREATE TABLE IF NOT EXISTS `user_maintenancelog` (
`id` int(11) NOT NULL,
  `userid` int(11) NOT NULL,
  `editorid` int(11) NOT NULL,
  `timestamp` int(11) NOT NULL,
  `type` enum('R','H','A','N','P') NOT NULL,
  `content` text
) ENGINE=InnoDB AUTO_INCREMENT=7397 DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user_player`
--

DROP TABLE IF EXISTS `user_player`;
CREATE TABLE IF NOT EXISTS `user_player` (
  `userid` int(11) NOT NULL,
  `playerid` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user_privilege`
--

DROP TABLE IF EXISTS `user_privilege`;
CREATE TABLE IF NOT EXISTS `user_privilege` (
  `user` int(11) NOT NULL,
  `privilege` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user_rank`
--

DROP TABLE IF EXISTS `user_rank`;
CREATE TABLE IF NOT EXISTS `user_rank` (
  `user` int(11) NOT NULL,
  `rank` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `user_role`
--

DROP TABLE IF EXISTS `user_role`;
CREATE TABLE IF NOT EXISTS `user_role` (
  `user` int(11) NOT NULL,
  `role` int(11) NOT NULL,
  `date` int(11) NOT NULL DEFAULT '-1'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE IF NOT EXISTS `users` (
`id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL DEFAULT '',
  `role` set('nobody','player','editor','vestededitor','publisher','judge','adminassistant','admin','senior','starman','encoder') NOT NULL DEFAULT 'player',
  `authority` decimal(10,2) NOT NULL DEFAULT '1.00',
  `auth_sum` decimal(10,5) NOT NULL DEFAULT '1.00000',
  `homepage` varchar(255) NOT NULL DEFAULT '',
  `lastchange` int(11) NOT NULL DEFAULT '0',
  `createtime` int(11) NOT NULL DEFAULT '0',
  `donated` decimal(10,4) NOT NULL DEFAULT '0.0000',
  `points` decimal(13,5) NOT NULL DEFAULT '0.00000'
) ENGINE=InnoDB AUTO_INCREMENT=6319 DEFAULT CHARSET=latin1;

--
-- Triggers `users`
--
DROP TRIGGER IF EXISTS `users_insert`;
DELIMITER //
CREATE TRIGGER `users_insert` BEFORE INSERT ON `users`
 FOR EACH ROW BEGIN
    SET NEW.lastchange=UNIX_TIMESTAMP(NOW());
    SET NEW.createtime=UNIX_TIMESTAMP(NOW());
  END
//
DELIMITER ;
DROP TRIGGER IF EXISTS `users_update`;
DELIMITER //
CREATE TRIGGER `users_update` BEFORE UPDATE ON `users`
 FOR EACH ROW BEGIN
    SET NEW.lastchange=UNIX_TIMESTAMP(NOW());
  END
//
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `youtube_data`
--

DROP TABLE IF EXISTS `youtube_data`;
CREATE TABLE IF NOT EXISTS `youtube_data` (
  `movieid` int(11) NOT NULL DEFAULT '-1',
  `ytid` char(11) NOT NULL COMMENT 'YT video ID',
  `special_tags` varchar(500) NOT NULL DEFAULT '' COMMENT 'Special tags',
  `special_title` varchar(48) NOT NULL DEFAULT '' COMMENT 'Special title',
  `special_comment` text NOT NULL,
  `uploader` int(11) NOT NULL,
  `privacy` char(1) NOT NULL DEFAULT 'P'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `ytu_log`
--

DROP TABLE IF EXISTS `ytu_log`;
CREATE TABLE IF NOT EXISTS `ytu_log` (
  `ytid` char(11) NOT NULL,
  `ts` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `command` text NOT NULL,
  `status` int(11) NOT NULL,
  `output` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `access_analyzer`
--
ALTER TABLE `access_analyzer`
 ADD KEY `a` (`accesstime`) USING HASH, ADD KEY `i` (`ipv6prefix`,`ipaddr`) USING HASH;

--
-- Indexes for table `access_blocks`
--
ALTER TABLE `access_blocks`
 ADD KEY `i` (`ipv6prefix`,`ipaddr`) USING HASH, ADD KEY `b` (`blockbegin`) USING BTREE, ADD KEY `e` (`blockend`) USING BTREE;

--
-- Indexes for table `api_keys`
--
ALTER TABLE `api_keys`
 ADD PRIMARY KEY (`id`), ADD KEY `user` (`user`);

--
-- Indexes for table `api_privileges`
--
ALTER TABLE `api_privileges`
 ADD UNIQUE KEY `key_2` (`key`,`privilege`), ADD KEY `key` (`key`), ADD KEY `privilege` (`privilege`);

--
-- Indexes for table `api_sessions`
--
ALTER TABLE `api_sessions`
 ADD PRIMARY KEY (`id`), ADD KEY `pkey` (`pkey`);

--
-- Indexes for table `awards`
--
ALTER TABLE `awards`
 ADD UNIQUE KEY `award_index` (`UserID`,`MovieID`,`Award`,`Year`), ADD KEY `MovieID` (`MovieID`), ADD KEY `Award` (`Award`);

--
-- Indexes for table `awards_classes`
--
ALTER TABLE `awards_classes`
 ADD PRIMARY KEY (`award`), ADD UNIQUE KEY `shortname` (`shortname`), ADD UNIQUE KEY `description` (`description`);

--
-- Indexes for table `classtype`
--
ALTER TABLE `classtype`
 ADD PRIMARY KEY (`id`);

--
-- Indexes for table `donate`
--
ALTER TABLE `donate`
 ADD PRIMARY KEY (`username`);

--
-- Indexes for table `filetype`
--
ALTER TABLE `filetype`
 ADD PRIMARY KEY (`filetype`), ADD KEY `c` (`filetypeclass`);

--
-- Indexes for table `flagstype`
--
ALTER TABLE `flagstype`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `name` (`name`), ADD UNIQUE KEY `key` (`key`), ADD KEY `privilegeid` (`privilegeid`);

--
-- Indexes for table `game`
--
ALTER TABLE `game`
 ADD PRIMARY KEY (`id`), ADD KEY `s` (`systemid`), ADD KEY `d` (`derived_from`), ADD KEY `gn` (`gamename`);

--
-- Indexes for table `gamename`
--
ALTER TABLE `gamename`
 ADD PRIMARY KEY (`gn_id`), ADD UNIQUE KEY `displayname` (`displayname`,`sys_id`), ADD KEY `searchkey` (`searchkey`), ADD KEY `sys_id` (`sys_id`);

--
-- Indexes for table `gamename_group`
--
ALTER TABLE `gamename_group`
 ADD UNIQUE KEY `gn_id` (`gn_id`,`group`), ADD KEY `gn_id_2` (`gn_id`), ADD KEY `group` (`group`);

--
-- Indexes for table `gamename_groupname`
--
ALTER TABLE `gamename_groupname`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `searchkey` (`searchkey`), ADD UNIQUE KEY `name` (`name`);

--
-- Indexes for table `gamename_link`
--
ALTER TABLE `gamename_link`
 ADD UNIQUE KEY `gn_id` (`gn_id`,`link`), ADD KEY `link` (`link`), ADD KEY `linktype` (`linktype`);

--
-- Indexes for table `gamename_linkclass`
--
ALTER TABLE `gamename_linkclass`
 ADD PRIMARY KEY (`type`), ADD UNIQUE KEY `description` (`description`), ADD KEY `dual` (`dual`);

--
-- Indexes for table `img_access_log`
--
ALTER TABLE `img_access_log`
 ADD PRIMARY KEY (`id`,`url_md5`) USING HASH, ADD KEY `u` (`url_md5`(4)) USING HASH, ADD KEY `c` (`counter`) USING BTREE, ADD KEY `ti` (`timestamp`);

--
-- Indexes for table `img_size_cache`
--
ALTER TABLE `img_size_cache`
 ADD PRIMARY KEY (`url_md5`) USING HASH, ADD KEY `ti` (`timestamp`) USING BTREE, ADD KEY `bf` (`blobfile`(2));

--
-- Indexes for table `movie`
--
ALTER TABLE `movie`
 ADD PRIMARY KEY (`id`), ADD KEY `p` (`playerid`), ADD KEY `s` (`systemid`), ADD KEY `o` (`obsoleted_by`), ADD KEY `u` (`submissionid`), ADD KEY `r` (`recommended`), ADD KEY `i` (`inpure`), ADD KEY `t` (`lastchange`), ADD KEY `g` (`gameid`), ADD KEY `pd` (`pubdate`), ADD KEY `pb` (`published_by`), ADD KEY `tier` (`tier`);

--
-- Indexes for table `movie_class`
--
ALTER TABLE `movie_class`
 ADD PRIMARY KEY (`movieid`,`classid`,`value`), ADD KEY `c` (`classid`);

--
-- Indexes for table `movie_file`
--
ALTER TABLE `movie_file`
 ADD PRIMARY KEY (`id`), ADD KEY `m` (`movieid`), ADD KEY `filetime` (`filetime`), ADD KEY `y` (`typech`), ADD KEY `uploader` (`uploader`);

--
-- Indexes for table `movie_file_storage`
--
ALTER TABLE `movie_file_storage`
 ADD PRIMARY KEY (`filename`);

--
-- Indexes for table `movie_flag`
--
ALTER TABLE `movie_flag`
 ADD UNIQUE KEY `movie` (`movie`,`flag`), ADD KEY `flag` (`flag`);

--
-- Indexes for table `movie_formats`
--
ALTER TABLE `movie_formats`
 ADD PRIMARY KEY (`format`), ADD KEY `alias` (`alias`), ADD KEY `filetype` (`filetype`);

--
-- Indexes for table `movie_maintenancelog`
--
ALTER TABLE `movie_maintenancelog`
 ADD PRIMARY KEY (`id`), ADD KEY `m` (`movieid`), ADD KEY `u` (`userid`), ADD KEY `ti` (`timestamp`), ADD KEY `t` (`type`);

--
-- Indexes for table `movie_rating`
--
ALTER TABLE `movie_rating`
 ADD PRIMARY KEY (`userid`,`movieid`,`ratingname`(4)), ADD KEY `m` (`movieid`), ADD KEY `r` (`ratingname`(4)), ADD KEY `ratingname` (`ratingname`);

--
-- Indexes for table `movie_search`
--
ALTER TABLE `movie_search`
 ADD UNIQUE KEY `m` (`movieid`), ADD FULLTEXT KEY `gm` (`gamename`), ADD FULLTEXT KEY `nn` (`nickname`), ADD FULLTEXT KEY `gn` (`gamenick`);

--
-- Indexes for table `movie_streaming`
--
ALTER TABLE `movie_streaming`
 ADD PRIMARY KEY (`id`), ADD KEY `m` (`movieid`), ADD KEY `u` (`userid`), ADD KEY `ue` (`user_enabled`), ADD KEY `uet` (`user_modtime`), ADD KEY `en` (`server_enabled`), ADD KEY `ent` (`server_modtime`), ADD KEY `is` (`is_streaming`);

--
-- Indexes for table `movie_tiers`
--
ALTER TABLE `movie_tiers`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `name` (`name`), ADD KEY `privilegeid` (`privilegeid`);

--
-- Indexes for table `page_alias`
--
ALTER TABLE `page_alias`
 ADD PRIMARY KEY (`id`), ADD KEY `p` (`pagename`(32)), ADD KEY `a` (`actualname`(32));

--
-- Indexes for table `page_counter`
--
ALTER TABLE `page_counter`
 ADD PRIMARY KEY (`pagename`);

--
-- Indexes for table `page_privacy`
--
ALTER TABLE `page_privacy`
 ADD PRIMARY KEY (`id`), ADD KEY `pn` (`pagename`), ADD KEY `vr` (`view_role`), ADD KEY `vp` (`view_privilege`), ADD KEY `er` (`edit_role`), ADD KEY `ep` (`edit_privilege`);

--
-- Indexes for table `player`
--
ALTER TABLE `player`
 ADD PRIMARY KEY (`id`);

--
-- Indexes for table `privileges`
--
ALTER TABLE `privileges`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `name` (`name`), ADD KEY `privilege` (`privilege`);

--
-- Indexes for table `profile`
--
ALTER TABLE `profile`
 ADD PRIMARY KEY (`id`), ADD KEY `uri` (`uri`(32));

--
-- Indexes for table `ramaddresses`
--
ALTER TABLE `ramaddresses`
 ADD PRIMARY KEY (`id`), ADD KEY `domain` (`domain`), ADD KEY `addrset` (`addrset`);

--
-- Indexes for table `ramaddresses_domains`
--
ALTER TABLE `ramaddresses_domains`
 ADD PRIMARY KEY (`domain`), ADD UNIQUE KEY `name` (`name`,`system`), ADD KEY `system` (`system`);

--
-- Indexes for table `ramaddresses_log`
--
ALTER TABLE `ramaddresses_log`
 ADD PRIMARY KEY (`sequence_id`), ADD KEY `timestamp` (`timestamp`), ADD KEY `user` (`user`);

--
-- Indexes for table `ramaddresses_sets`
--
ALTER TABLE `ramaddresses_sets`
 ADD PRIMARY KEY (`addrset`), ADD KEY `system` (`system`);

--
-- Indexes for table `ranks`
--
ALTER TABLE `ranks`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `name` (`name`), ADD KEY `assign_privilege` (`assign_privilege`);

--
-- Indexes for table `ranks_variant`
--
ALTER TABLE `ranks_variant`
 ADD PRIMARY KEY (`baserank`,`minarg`);

--
-- Indexes for table `rating`
--
ALTER TABLE `rating`
 ADD PRIMARY KEY (`name`), ADD UNIQUE KEY `description` (`description`);

--
-- Indexes for table `rejections`
--
ALTER TABLE `rejections`
 ADD PRIMARY KEY (`id`,`reason`), ADD KEY `reason` (`reason`);

--
-- Indexes for table `rejections_reasons`
--
ALTER TABLE `rejections_reasons`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `text` (`text`);

--
-- Indexes for table `role_maintenancelog`
--
ALTER TABLE `role_maintenancelog`
 ADD PRIMARY KEY (`id`), ADD KEY `userid` (`userid`);

--
-- Indexes for table `role_privilege`
--
ALTER TABLE `role_privilege`
 ADD PRIMARY KEY (`role`,`privilege`), ADD KEY `r` (`role`), ADD KEY `p` (`privilege`);

--
-- Indexes for table `roles`
--
ALTER TABLE `roles`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `name` (`name`), ADD KEY `ap` (`assign_privilege`);

--
-- Indexes for table `roms`
--
ALTER TABLE `roms`
 ADD PRIMARY KEY (`rom_id`), ADD UNIQUE KEY `md5` (`md5`,`sha1`), ADD KEY `gn_id` (`gn_id`), ADD KEY `addrset` (`addrset`);

--
-- Indexes for table `session`
--
ALTER TABLE `session`
 ADD PRIMARY KEY (`id`), ADD KEY `lt_index` (`lasttime`);

--
-- Indexes for table `site_constants`
--
ALTER TABLE `site_constants`
 ADD PRIMARY KEY (`constant`);

--
-- Indexes for table `site_links`
--
ALTER TABLE `site_links`
 ADD PRIMARY KEY (`text_id`,`dest_page`), ADD KEY `d` (`dest_page`(32)), ADD KEY `lt` (`link_type`);

--
-- Indexes for table `site_text`
--
ALTER TABLE `site_text`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `prim` (`pagename`,`revision`) USING BTREE, ADD KEY `m` (`movieid`), ADD KEY `r` (`revision`), ADD KEY `u` (`userid`), ADD KEY `t` (`timestamp`), ADD KEY `s` (`submissionid`), ADD KEY `o` (`obsoleted_by`), ADD KEY `p` (`pagename`(32)), ADD KEY `y` (`type`);

--
-- Indexes for table `site_text_search`
--
ALTER TABLE `site_text_search`
 ADD UNIQUE KEY `s` (`site_text_id`), ADD FULLTEXT KEY `t` (`title`), ADD FULLTEXT KEY `d` (`description`), ADD FULLTEXT KEY `tm` (`title_metaphone`), ADD FULLTEXT KEY `cm` (`class_metaphone`);

--
-- Indexes for table `sort_url`
--
ALTER TABLE `sort_url`
 ADD PRIMARY KEY (`id`);

--
-- Indexes for table `submission`
--
ALTER TABLE `submission`
 ADD PRIMARY KEY (`id`), ADD KEY `u` (`userid`), ADD KEY `t` (`timestamp`), ADD KEY `s` (`systemid`), ADD KEY `a` (`status`), ADD KEY `sd` (`subdate`), ADD KEY `gn_id` (`gn_id`), ADD KEY `intended_tier` (`intended_tier`), ADD KEY `judged_by` (`judged_by`);

--
-- Indexes for table `submission_editors`
--
ALTER TABLE `submission_editors`
 ADD UNIQUE KEY `userid` (`userid`,`submissionid`), ADD KEY `submissionid` (`submissionid`);

--
-- Indexes for table `submission_rating`
--
ALTER TABLE `submission_rating`
 ADD PRIMARY KEY (`userid`,`submissionid`,`ratingname`(4)), ADD KEY `s` (`submissionid`), ADD KEY `r` (`ratingname`(4)), ADD KEY `ratingname` (`ratingname`);

--
-- Indexes for table `svnmap`
--
ALTER TABLE `svnmap`
 ADD PRIMARY KEY (`author`), ADD KEY `n` (`username`);

--
-- Indexes for table `system`
--
ALTER TABLE `system`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `abbr` (`abbr`), ADD UNIQUE KEY `name` (`name`);

--
-- Indexes for table `system_codes`
--
ALTER TABLE `system_codes`
 ADD PRIMARY KEY (`parser`,`code2`,`regcode`), ADD KEY `system` (`system`);

--
-- Indexes for table `system_framerate`
--
ALTER TABLE `system_framerate`
 ADD UNIQUE KEY `systemid` (`systemid`,`regionid`);

--
-- Indexes for table `system_rules`
--
ALTER TABLE `system_rules`
 ADD PRIMARY KEY (`parser`,`priority`), ADD KEY `system` (`system`);

--
-- Indexes for table `system_screenshot`
--
ALTER TABLE `system_screenshot`
 ADD PRIMARY KEY (`systemid`,`width`,`height`), ADD KEY `systemid` (`systemid`);

--
-- Indexes for table `todo_list`
--
ALTER TABLE `todo_list`
 ADD PRIMARY KEY (`id`);

--
-- Indexes for table `tvc_upload_stats`
--
ALTER TABLE `tvc_upload_stats`
 ADD PRIMARY KEY (`uploader`);

--
-- Indexes for table `tvc_uploads`
--
ALTER TABLE `tvc_uploads`
 ADD PRIMARY KEY (`md5`), ADD KEY `modified` (`modified`), ADD KEY `uploader` (`uploader`);

--
-- Indexes for table `updater_ip`
--
ALTER TABLE `updater_ip`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `ip` (`ip`);

--
-- Indexes for table `uploaders`
--
ALTER TABLE `uploaders`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `uploader` (`uploader`,`site`), ADD KEY `user` (`user`);

--
-- Indexes for table `user_files`
--
ALTER TABLE `user_files`
 ADD PRIMARY KEY (`file_id`), ADD KEY `file_uid` (`file_uid`), ADD KEY `file_gn_id` (`file_gn_id`), ADD KEY `file_system` (`file_system`);

--
-- Indexes for table `user_files_comments`
--
ALTER TABLE `user_files_comments`
 ADD PRIMARY KEY (`id`), ADD KEY `parent` (`parent`), ADD KEY `file_id` (`file_id`), ADD KEY `userid` (`userid`);

--
-- Indexes for table `user_maintenancelog`
--
ALTER TABLE `user_maintenancelog`
 ADD PRIMARY KEY (`id`), ADD KEY `u` (`userid`), ADD KEY `e` (`editorid`), ADD KEY `ti` (`timestamp`), ADD KEY `t` (`type`);

--
-- Indexes for table `user_player`
--
ALTER TABLE `user_player`
 ADD PRIMARY KEY (`userid`,`playerid`), ADD KEY `p` (`playerid`);

--
-- Indexes for table `user_privilege`
--
ALTER TABLE `user_privilege`
 ADD PRIMARY KEY (`user`,`privilege`), ADD KEY `u` (`user`), ADD KEY `p` (`privilege`);

--
-- Indexes for table `user_rank`
--
ALTER TABLE `user_rank`
 ADD PRIMARY KEY (`user`,`rank`), ADD KEY `rank` (`rank`);

--
-- Indexes for table `user_role`
--
ALTER TABLE `user_role`
 ADD PRIMARY KEY (`user`,`role`), ADD KEY `u` (`user`), ADD KEY `r` (`role`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
 ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `n` (`name`(16)), ADD KEY `h` (`homepage`(8)), ADD KEY `t` (`lastchange`), ADD KEY `c` (`createtime`);

--
-- Indexes for table `youtube_data`
--
ALTER TABLE `youtube_data`
 ADD UNIQUE KEY `ytid` (`ytid`), ADD KEY `uploader` (`uploader`);

--
-- Indexes for table `ytu_log`
--
ALTER TABLE `ytu_log`
 ADD PRIMARY KEY (`ytid`), ADD KEY `ts` (`ts`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `api_keys`
--
ALTER TABLE `api_keys`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=3;
--
-- AUTO_INCREMENT for table `api_sessions`
--
ALTER TABLE `api_sessions`
MODIFY `id` bigint(20) unsigned NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=100;
--
-- AUTO_INCREMENT for table `awards_classes`
--
ALTER TABLE `awards_classes`
MODIFY `award` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=48;
--
-- AUTO_INCREMENT for table `classtype`
--
ALTER TABLE `classtype`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=9058;
--
-- AUTO_INCREMENT for table `flagstype`
--
ALTER TABLE `flagstype`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=9;
--
-- AUTO_INCREMENT for table `game`
--
ALTER TABLE `game`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `gamename`
--
ALTER TABLE `gamename`
MODIFY `gn_id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=1993;
--
-- AUTO_INCREMENT for table `gamename_groupname`
--
ALTER TABLE `gamename_groupname`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=207;
--
-- AUTO_INCREMENT for table `movie`
--
ALTER TABLE `movie`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=3619;
--
-- AUTO_INCREMENT for table `movie_file`
--
ALTER TABLE `movie_file`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=31221;
--
-- AUTO_INCREMENT for table `movie_maintenancelog`
--
ALTER TABLE `movie_maintenancelog`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=56827;
--
-- AUTO_INCREMENT for table `movie_streaming`
--
ALTER TABLE `movie_streaming`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `movie_tiers`
--
ALTER TABLE `movie_tiers`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=4;
--
-- AUTO_INCREMENT for table `page_privacy`
--
ALTER TABLE `page_privacy`
MODIFY `id` mediumint(9) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `player`
--
ALTER TABLE `player`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=1047;
--
-- AUTO_INCREMENT for table `privileges`
--
ALTER TABLE `privileges`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=87;
--
-- AUTO_INCREMENT for table `profile`
--
ALTER TABLE `profile`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=11803941;
--
-- AUTO_INCREMENT for table `ramaddresses`
--
ALTER TABLE `ramaddresses`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=1822;
--
-- AUTO_INCREMENT for table `ramaddresses_domains`
--
ALTER TABLE `ramaddresses_domains`
MODIFY `domain` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=75;
--
-- AUTO_INCREMENT for table `ramaddresses_log`
--
ALTER TABLE `ramaddresses_log`
MODIFY `sequence_id` bigint(20) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=13187;
--
-- AUTO_INCREMENT for table `ramaddresses_sets`
--
ALTER TABLE `ramaddresses_sets`
MODIFY `addrset` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=119;
--
-- AUTO_INCREMENT for table `ranks`
--
ALTER TABLE `ranks`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=46;
--
-- AUTO_INCREMENT for table `role_maintenancelog`
--
ALTER TABLE `role_maintenancelog`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=240;
--
-- AUTO_INCREMENT for table `roles`
--
ALTER TABLE `roles`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=15;
--
-- AUTO_INCREMENT for table `roms`
--
ALTER TABLE `roms`
MODIFY `rom_id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=5;
--
-- AUTO_INCREMENT for table `site_text`
--
ALTER TABLE `site_text`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=77618;
--
-- AUTO_INCREMENT for table `sort_url`
--
ALTER TABLE `sort_url`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=1282;
--
-- AUTO_INCREMENT for table `submission`
--
ALTER TABLE `submission`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=5807;
--
-- AUTO_INCREMENT for table `system`
--
ALTER TABLE `system`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=44;
--
-- AUTO_INCREMENT for table `todo_list`
--
ALTER TABLE `todo_list`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=25;
--
-- AUTO_INCREMENT for table `updater_ip`
--
ALTER TABLE `updater_ip`
MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=5;
--
-- AUTO_INCREMENT for table `uploaders`
--
ALTER TABLE `uploaders`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=231;
--
-- AUTO_INCREMENT for table `user_files`
--
ALTER TABLE `user_files`
MODIFY `file_id` bigint(20) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=44787462186883190;
--
-- AUTO_INCREMENT for table `user_files_comments`
--
ALTER TABLE `user_files_comments`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=384;
--
-- AUTO_INCREMENT for table `user_maintenancelog`
--
ALTER TABLE `user_maintenancelog`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=7397;
--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
MODIFY `id` int(11) NOT NULL AUTO_INCREMENT,AUTO_INCREMENT=6319;
--
-- Constraints for dumped tables
--

--
-- Constraints for table `api_keys`
--
ALTER TABLE `api_keys`
ADD CONSTRAINT `api_keys_ibfk_1` FOREIGN KEY (`user`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `api_privileges`
--
ALTER TABLE `api_privileges`
ADD CONSTRAINT `api_privileges_ibfk_3` FOREIGN KEY (`key`) REFERENCES `api_keys` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `api_privileges_ibfk_4` FOREIGN KEY (`privilege`) REFERENCES `privileges` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `api_sessions`
--
ALTER TABLE `api_sessions`
ADD CONSTRAINT `api_sessions_ibfk_1` FOREIGN KEY (`pkey`) REFERENCES `api_keys` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `awards`
--
ALTER TABLE `awards`
ADD CONSTRAINT `awards_ibfk_1` FOREIGN KEY (`MovieID`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `awards_ibfk_2` FOREIGN KEY (`UserID`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `awards_ibfk_3` FOREIGN KEY (`Award`) REFERENCES `awards_classes` (`award`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `flagstype`
--
ALTER TABLE `flagstype`
ADD CONSTRAINT `flagstype_ibfk_1` FOREIGN KEY (`privilegeid`) REFERENCES `privileges` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `game`
--
ALTER TABLE `game`
ADD CONSTRAINT `game_ibfk_2` FOREIGN KEY (`derived_from`) REFERENCES `game` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `game_ibfk_3` FOREIGN KEY (`systemid`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `gamename`
--
ALTER TABLE `gamename`
ADD CONSTRAINT `gamename_ibfk_1` FOREIGN KEY (`sys_id`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `gamename_group`
--
ALTER TABLE `gamename_group`
ADD CONSTRAINT `gamename_group_ibfk_1` FOREIGN KEY (`gn_id`) REFERENCES `gamename` (`gn_id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `gamename_group_ibfk_2` FOREIGN KEY (`group`) REFERENCES `gamename_groupname` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `gamename_link`
--
ALTER TABLE `gamename_link`
ADD CONSTRAINT `gamename_link_ibfk_1` FOREIGN KEY (`link`) REFERENCES `gamename` (`gn_id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `gamename_link_ibfk_2` FOREIGN KEY (`linktype`) REFERENCES `gamename_linkclass` (`type`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `gamename_link_ibfk_3` FOREIGN KEY (`gn_id`) REFERENCES `gamename` (`gn_id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `gamename_linkclass`
--
ALTER TABLE `gamename_linkclass`
ADD CONSTRAINT `gamename_linkclass_ibfk_1` FOREIGN KEY (`dual`) REFERENCES `gamename_linkclass` (`type`) ON UPDATE CASCADE;

--
-- Constraints for table `movie`
--
ALTER TABLE `movie`
ADD CONSTRAINT `movie_ibfk_10` FOREIGN KEY (`published_by`) REFERENCES `users` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `movie_ibfk_3` FOREIGN KEY (`submissionid`) REFERENCES `submission` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `movie_ibfk_7` FOREIGN KEY (`tier`) REFERENCES `movie_tiers` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `movie_ibfk_8` FOREIGN KEY (`playerid`) REFERENCES `player` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `movie_ibfk_9` FOREIGN KEY (`systemid`) REFERENCES `system` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `movie_class`
--
ALTER TABLE `movie_class`
ADD CONSTRAINT `movie_class_ibfk_1` FOREIGN KEY (`movieid`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_class_ibfk_2` FOREIGN KEY (`classid`) REFERENCES `classtype` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `movie_file`
--
ALTER TABLE `movie_file`
ADD CONSTRAINT `movie_file_ibfk_1` FOREIGN KEY (`movieid`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_file_ibfk_3` FOREIGN KEY (`typech`) REFERENCES `filetype` (`filetype`) ON UPDATE CASCADE,
ADD CONSTRAINT `movie_file_ibfk_4` FOREIGN KEY (`uploader`) REFERENCES `uploaders` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `movie_flag`
--
ALTER TABLE `movie_flag`
ADD CONSTRAINT `movie_flag_ibfk_1` FOREIGN KEY (`movie`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_flag_ibfk_2` FOREIGN KEY (`flag`) REFERENCES `flagstype` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `movie_formats`
--
ALTER TABLE `movie_formats`
ADD CONSTRAINT `movie_formats_ibfk_1` FOREIGN KEY (`filetype`) REFERENCES `filetype` (`filetype`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_formats_ibfk_2` FOREIGN KEY (`alias`) REFERENCES `movie_formats` (`format`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `movie_maintenancelog`
--
ALTER TABLE `movie_maintenancelog`
ADD CONSTRAINT `movie_maintenancelog_ibfk_1` FOREIGN KEY (`movieid`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_maintenancelog_ibfk_2` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `movie_rating`
--
ALTER TABLE `movie_rating`
ADD CONSTRAINT `movie_rating_ibfk_2` FOREIGN KEY (`movieid`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_rating_ibfk_4` FOREIGN KEY (`ratingname`) REFERENCES `rating` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_rating_ibfk_5` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `movie_streaming`
--
ALTER TABLE `movie_streaming`
ADD CONSTRAINT `movie_streaming_ibfk_1` FOREIGN KEY (`movieid`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `movie_streaming_ibfk_2` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `movie_tiers`
--
ALTER TABLE `movie_tiers`
ADD CONSTRAINT `movie_tiers_ibfk_1` FOREIGN KEY (`privilegeid`) REFERENCES `privileges` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `page_privacy`
--
ALTER TABLE `page_privacy`
ADD CONSTRAINT `page_privacy_ibfk_1` FOREIGN KEY (`view_privilege`) REFERENCES `privileges` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `page_privacy_ibfk_2` FOREIGN KEY (`edit_privilege`) REFERENCES `privileges` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `page_privacy_ibfk_3` FOREIGN KEY (`view_role`) REFERENCES `roles` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `page_privacy_ibfk_4` FOREIGN KEY (`edit_role`) REFERENCES `roles` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `privileges`
--
ALTER TABLE `privileges`
ADD CONSTRAINT `privileges_ibfk_1` FOREIGN KEY (`privilege`) REFERENCES `privileges` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `ramaddresses`
--
ALTER TABLE `ramaddresses`
ADD CONSTRAINT `ramaddresses_ibfk_1` FOREIGN KEY (`addrset`) REFERENCES `ramaddresses_sets` (`addrset`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `ramaddresses_ibfk_2` FOREIGN KEY (`domain`) REFERENCES `ramaddresses_domains` (`domain`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `ramaddresses_domains`
--
ALTER TABLE `ramaddresses_domains`
ADD CONSTRAINT `ramaddresses_domains_ibfk_1` FOREIGN KEY (`system`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `ramaddresses_log`
--
ALTER TABLE `ramaddresses_log`
ADD CONSTRAINT `ramaddresses_log_ibfk_1` FOREIGN KEY (`user`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `ramaddresses_sets`
--
ALTER TABLE `ramaddresses_sets`
ADD CONSTRAINT `ramaddresses_sets_ibfk_1` FOREIGN KEY (`system`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `ranks`
--
ALTER TABLE `ranks`
ADD CONSTRAINT `ranks_ibfk_1` FOREIGN KEY (`assign_privilege`) REFERENCES `privileges` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `rejections`
--
ALTER TABLE `rejections`
ADD CONSTRAINT `rejections_ibfk_1` FOREIGN KEY (`reason`) REFERENCES `rejections_reasons` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `rejections_ibfk_2` FOREIGN KEY (`id`) REFERENCES `submission` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `role_maintenancelog`
--
ALTER TABLE `role_maintenancelog`
ADD CONSTRAINT `role_maintenancelog_ibfk_1` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `role_privilege`
--
ALTER TABLE `role_privilege`
ADD CONSTRAINT `role_privilege_ibfk_1` FOREIGN KEY (`privilege`) REFERENCES `privileges` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `role_privilege_ibfk_2` FOREIGN KEY (`role`) REFERENCES `roles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `roles`
--
ALTER TABLE `roles`
ADD CONSTRAINT `roles_ibfk_1` FOREIGN KEY (`assign_privilege`) REFERENCES `privileges` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `roms`
--
ALTER TABLE `roms`
ADD CONSTRAINT `roms_ibfk_1` FOREIGN KEY (`gn_id`) REFERENCES `gamename` (`gn_id`) ON DELETE SET NULL ON UPDATE CASCADE,
ADD CONSTRAINT `roms_ibfk_2` FOREIGN KEY (`addrset`) REFERENCES `ramaddresses_sets` (`addrset`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `site_links`
--
ALTER TABLE `site_links`
ADD CONSTRAINT `site_links_ibfk_1` FOREIGN KEY (`text_id`) REFERENCES `site_text` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `site_text`
--
ALTER TABLE `site_text`
ADD CONSTRAINT `site_text_ibfk_2` FOREIGN KEY (`movieid`) REFERENCES `movie` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `site_text_ibfk_3` FOREIGN KEY (`submissionid`) REFERENCES `submission` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `site_text_ibfk_4` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `submission`
--
ALTER TABLE `submission`
ADD CONSTRAINT `submission_ibfk_4` FOREIGN KEY (`systemid`) REFERENCES `system` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `submission_ibfk_5` FOREIGN KEY (`judged_by`) REFERENCES `users` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `submission_ibfk_6` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `submission_ibfk_7` FOREIGN KEY (`gn_id`) REFERENCES `gamename` (`gn_id`) ON DELETE SET NULL ON UPDATE CASCADE,
ADD CONSTRAINT `submission_ibfk_8` FOREIGN KEY (`intended_tier`) REFERENCES `movie_tiers` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `submission_editors`
--
ALTER TABLE `submission_editors`
ADD CONSTRAINT `submission_editors_ibfk_1` FOREIGN KEY (`submissionid`) REFERENCES `submission` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `submission_editors_ibfk_2` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `submission_rating`
--
ALTER TABLE `submission_rating`
ADD CONSTRAINT `submission_rating_ibfk_2` FOREIGN KEY (`submissionid`) REFERENCES `submission` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `submission_rating_ibfk_4` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `submission_rating_ibfk_5` FOREIGN KEY (`ratingname`) REFERENCES `rating` (`name`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `system_codes`
--
ALTER TABLE `system_codes`
ADD CONSTRAINT `system_codes_ibfk_1` FOREIGN KEY (`system`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `system_framerate`
--
ALTER TABLE `system_framerate`
ADD CONSTRAINT `system_framerate_ibfk_1` FOREIGN KEY (`systemid`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `system_rules`
--
ALTER TABLE `system_rules`
ADD CONSTRAINT `system_rules_ibfk_1` FOREIGN KEY (`system`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `system_screenshot`
--
ALTER TABLE `system_screenshot`
ADD CONSTRAINT `system_screenshot_ibfk_1` FOREIGN KEY (`systemid`) REFERENCES `system` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `tvc_upload_stats`
--
ALTER TABLE `tvc_upload_stats`
ADD CONSTRAINT `tvc_upload_stats_ibfk_1` FOREIGN KEY (`uploader`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `tvc_uploads`
--
ALTER TABLE `tvc_uploads`
ADD CONSTRAINT `tvc_uploads_ibfk_1` FOREIGN KEY (`uploader`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `uploaders`
--
ALTER TABLE `uploaders`
ADD CONSTRAINT `uploaders_ibfk_1` FOREIGN KEY (`user`) REFERENCES `users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `user_files`
--
ALTER TABLE `user_files`
ADD CONSTRAINT `user_files_ibfk_1` FOREIGN KEY (`file_gn_id`) REFERENCES `gamename` (`gn_id`) ON DELETE SET NULL ON UPDATE CASCADE,
ADD CONSTRAINT `user_files_ibfk_2` FOREIGN KEY (`file_system`) REFERENCES `system` (`id`) ON UPDATE CASCADE,
ADD CONSTRAINT `user_files_ibfk_3` FOREIGN KEY (`file_uid`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `user_files_comments`
--
ALTER TABLE `user_files_comments`
ADD CONSTRAINT `user_files_comments_ibfk_4` FOREIGN KEY (`file_id`) REFERENCES `user_files` (`file_id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `user_files_comments_ibfk_5` FOREIGN KEY (`parent`) REFERENCES `user_files_comments` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
ADD CONSTRAINT `user_files_comments_ibfk_6` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `user_maintenancelog`
--
ALTER TABLE `user_maintenancelog`
ADD CONSTRAINT `user_maintenancelog_ibfk_1` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `user_maintenancelog_ibfk_2` FOREIGN KEY (`editorid`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

--
-- Constraints for table `user_player`
--
ALTER TABLE `user_player`
ADD CONSTRAINT `user_player_ibfk_2` FOREIGN KEY (`playerid`) REFERENCES `player` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `user_player_ibfk_3` FOREIGN KEY (`userid`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `user_privilege`
--
ALTER TABLE `user_privilege`
ADD CONSTRAINT `user_privilege_ibfk_2` FOREIGN KEY (`privilege`) REFERENCES `privileges` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `user_privilege_ibfk_3` FOREIGN KEY (`user`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `user_rank`
--
ALTER TABLE `user_rank`
ADD CONSTRAINT `user_rank_ibfk_1` FOREIGN KEY (`rank`) REFERENCES `ranks` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `user_rank_ibfk_2` FOREIGN KEY (`user`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `user_role`
--
ALTER TABLE `user_role`
ADD CONSTRAINT `user_role_ibfk_2` FOREIGN KEY (`role`) REFERENCES `roles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
ADD CONSTRAINT `user_role_ibfk_3` FOREIGN KEY (`user`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `youtube_data`
--
ALTER TABLE `youtube_data`
ADD CONSTRAINT `youtube_data_ibfk_1` FOREIGN KEY (`uploader`) REFERENCES `users` (`id`) ON UPDATE CASCADE;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
