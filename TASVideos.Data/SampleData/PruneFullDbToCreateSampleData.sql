-- Instructions
-- Run a full Import
-- Run this script
-- In SMMS, run Tasks -> Generate scripts
--		- Pick all table
--		- Go to Advanced and pick Data
--		- Save as SampleData.sql
--		- Run
--		- zip SampleData.sql and replace SampleData.zip

use [TASVideos]

DELETE FROM MediaPosts

-- Trim user files
DELETE FROM UserFileComments

DECLARE @UserFiles as Table (Id bigint primary key)
INSERT INTO @UserFiles
	SELECT TOP 6 uf.Id
	FROM UserFiles uf
	WHERE uf.Hidden = 0
	ORDER BY len(uf.Content)
	
DELETE uf
FROM UserFiles uf
LEFT JOIN @UserFiles iuf on uf.Id = iuf.Id
WHERE iuf.Id IS NULL

UPDATE UserFiles SET Content = 0x0

--Trim Wiki
DELETE FROM WikiPages WHERE ChildId is not null
DECLARE @WikiPagesToDelete as Table (Id int primary key)
INSERT INTO @WikiPagesToDelete
	SELECT ID
	FROM WikiPages
	WHERE IsDeleted = 1
	OR (PageName NOT LIKE 'InternalSystem%' AND len(Markup) < 18)

DELETE FROM WikiPages WHERE ID IN (SELECT Id FROM @WikiPagesToDelete)


UPDATE WikiPages SET Markup = '[TODO]: Wiped as sample data' WHERE len(Markup) > 1500 AND PageName <> 'Movies'

DELETE FROM WikiReferrals --TODO: maybe only delete referrals for deleted wiki pages and preserve existing

--Clear User data
DELETE FROM ForumTopicWatches
UPDATE [USER] SET Signature = NULL
UPDATE [USER] SET LegacyPassword = NULL --We don't want this data public
DELETE FROM PrivateMessages

 --Trim Publications and Submissions
 DECLARE @Publications as TABLE (Id int primary key, SubmissionId int, WikiContentId int)
INSERT INTO @Publications
	SELECT Id = p.Id, SubmissionId = p.SubmissionId, WikiContentId = p.WikiContentId
	FROM Publications p
	WHERE p.ObsoletedById IS NULL
	AND ID > 3600

DECLARE @PublicationWikis as TABLE (Id int primary key)
INSERT INTO @PublicatioNWikis
	SELECT wp.Id
	FROM WikiPages wp
	LEFT JOIN @Publications p on wp.Id = p.WikiContentId
	WHERE p.Id IS NULL
	
DELETE p
	FROM Publications p
	LEFT JOIN @Publications ipu on p.Id = ipu.Id
	WHERE ipu.Id IS NULL

DELETE wp
	FROM WikiPages wp
	LEFT JOIN @Publications p on wp.Id = p.WikiContentId
	WHERE wp.PageName LIKE 'InternalSystem/PublicationContent/M%'
	AND p.Id IS NULL

DELETE pa
	FROM PublicationAuthors pa
	LEFT JOIN @Publications ipu on pa.PublicationId = ipu.Id
	WHERE ipu.Id IS NULL
	
DELETE pa
	FROM PublicationAwards pa
	LEFT JOIN @Publications ipu on pa.PublicationId = ipu.Id
	WHERE ipu.Id IS NULL

DELETE pf
	FROM PublicationFiles pf
	LEFT JOIN @Publications ipu on pf.PublicationId = ipu.Id
	WHERE ipu.Id IS NULL

DELETE pf
	FROM PublicationFlags pf
	LEFT JOIN @Publications ipu on pf.PublicationId = ipu.Id
	WHERE ipu.Id IS NULL

DELETE pr
	FROM PublicationRatings pr
	LEFT JOIN @Publications ipu on pr.PublicationId = ipu.Id
	WHERE ipu.Id IS NULL

DECLARE @Submissions as TABLE (Id int primary key, WikiContentId int)
INSERT INTO @Submissions
	SELECT Id = s.Id, WikiContentId = s.WikiContentId
	FROM Submissions s
	JOIN @Publications p on s.Id = p.SubmissionId

DELETE s
	FROM Submissions s
	LEFT JOIN @Submissions isu ON s.Id = isu.Id
	WHERE isu.Id IS NULL

DELETE wp
	FROM WikiPages wp
	LEFT JOIN @Submissions s on wp.Id = s.WikiContentId
	WHERE wp.PageName LIKE 'InternalSystem/SubmissionContent/S%'
	AND s.Id IS NULL

DELETE sa
	FROM SubmissionAuthors sa
	LEFT JOIN @Submissions isu on sa.SubmissionId = isu.Id
	WHERE isu.Id IS NULL

DELETE ssh
	FROM SubmissionStatusHistory ssh
	LEFT JOIN @Submissions isu on ssh.SubmissionId = isu.Id
	WHERE isu.Id IS NULL

UPDATE Publications SET MovieFile = 0x0
UPDATE Submissions SET MovieFile = 0x0

--Delete unncessary Games
DECLARE @Games as Table (Id int primary key)
INSERT INTO @Games
	SELECT DISTINCT Id = g.Id
	FROM Games g
	LEFT JOIN Publications p on g.Id = p.GameId
	LEFT JOIN Submissions s ON g.Id =  s.GameId
	LEFT JOIN UserFiles uf on g.Id = uf.GameId
	WHERE p.Id IS NULL
	AND s.Id IS NULL
	AND uf.ID IS NULL
	AND g.Id > 0

DELETE gg
	FROM GameGenres gg
	LEFT JOIN @Games g on gg.GameId = g.Id
	WHERE g.Id IS NULL
		
DELETE FROM g
	FROM Games g
	JOIN @Games ig on g.Id = ig.Id

--Trim down forum data
DECLARE @Topics as Table (Id int primary key)
INSERT INTO @Topics 
	SELECT topic.Id
	FROM Forums f
	CROSS APPLY (
		SELECT
		TOP 4 *
		FROM ForumTopics t
		WHERE t.ForumId = f.Id
		ORDER BY t.CreateTimeStamp DESC
	) topic


DECLARE @Posts as Table (Id int primary key)
INSERT INTO @Posts
	SELECT post.Id
	FROM @Topics t
	CROSS APPLY (
		SELECT
		TOP 4 *
		FROM ForumPosts p
		WHERE t.Id = p.TopicId
		ORDER BY p.CreateTimeStamp -- First posts instead of last because we need the post that started the topic
	) post

DECLARE @Polls as TABLE (Id int primary key)
INSERT INTO @Polls
	SELECT p.Id
	FROM ForumPolls p
	JOIN @Topics t ON p.TopicId = t.Id

DECLARE @PollOptions as TABLE (Id int primary key)
INSERT INTO @PollOptions
	SELECT po.Id
	FROM ForumPollOptions po
	JOIN @Polls p on po.PollId = p.Id

DECLARE @PollOptionVotes as TABLE (Id int primary key)
INSERT INTO @PollOptionVotes
	SELECT pov.Id
	FROM ForumPollOptionVotes pov
	JOIN @PollOptions po on pov.PollOptionId = po.Id

DELETE pov
FROM ForumPollOptionVotes pov
LEFT JOIN @PollOptionVotes ipov on pov.Id = ipov.Id
WHERE ipov.Id IS NULL

DELETE po
FROM ForumPollOptions po
LEFT JOIN @PollOptions ipo on po.Id = ipo.Id
WHERE ipo.Id IS NULL

DELETE p
FROM ForumPosts p
LEFT JOIN @Posts iposts on p.Id = iposts.Id
WHERE iposts.Id IS NULL

DELETE t
FROM ForumTopics t
LEFT JOIN @Topics it ON t.Id = it.Id
WHERE it.Id IS NULL


-- Delete Users who have not contributed to any current data, and have no roles
DECLARE @ActiveUsers As Table (Id int primary key)
INSERT INTO @ActiveUsers
	SELECT u.Id
	FROM [User] u
	WHERE EXISTS (SELECT 1 FROM UserAwards ua WHERE ua.UserId = u.Id)
	OR EXISTS (SELECT 1 FROM UserFiles uf where uf.AuthorId = u.Id)
	OR EXISTS (SELECT 1 FROM Submissions s WHERE s.PublisherId = u.Id)
	OR EXISTS (SELECT 1 FROM Submissions s WHERE s.JudgeId = u.Id)
	OR EXISTS (SELECT 1 FROM SubmissionAuthors sa where sa.UserId = u.Id)
	OR EXISTS (SELECT 1 FROM PublicationAuthors pa where pa.UserId = u.Id)
	OR EXISTS (SELECT 1 FROM PublicationRatings pr WHERE pr.UserId = u.id)
	OR EXISTS (SELECT 1 FROM ForumPosts fp WHERE fp.PosterId = u.Id)
	OR EXISTS (SELECT 1 FROM ForumTopics ft WHERE ft.PosterId = u.Id)

DELETE ur
	FROM UserRoles ur
	LEFT JOIN @ActiveUsers iu on ur.UserId = iu.Id
	WHERE iu.Id IS NULL

DELETE u
	FROM [User] u
	LEFT JOIN @ActiveUsers au on u.Id = au.Id
	WHERE au.Id IS NULL

UPDATE [User] 
	SET Signature = NULL,
		LegacyPassword = NULL, -- We don't want to make these public
		Email = null -- We dont' want to amek these public either

