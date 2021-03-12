-- Instructions
-- Run a full Import
-- Run this script
-- In SMMS, Pick TASVideos database in object explorer
--      - run Tasks -> Generate scripts
--		- Pick all table
--		- Go to Advanced -> Types of data to script, and change from Schema only to Data only
--		- Save as SampleData.sql
--		- Run
--		- zip SampleData-MsSql.sql and replace SampleData-MsSql.zip

use [TASVideos]

DELETE FROM game_ram_address_domains
DELETE FROM game_ram_addresses
DELETE FROM ip_bans
DELETE FROM media_posts
DELETE FROM user_disallows

-- Trim user files
DELETE FROM user_file_comments

DECLARE @UserFiles as Table (id bigint primary key)
INSERT INTO @UserFiles
	SELECT TOP 6 uf.id
	FROM user_files uf
	WHERE uf.hidden = 0
	ORDER BY len(uf.content)
	
DELETE uf
FROM user_files uf
LEFT JOIN @UserFiles iuf on uf.id = iuf.id
WHERE iuf.id IS NULL

UPDATE user_files SET content = 0x0

--Trim Wiki
DELETE FROM wiki_pages WHERE child_id is not null
DECLARE @WikiPagesToDelete as Table (id int primary key)
INSERT INTO @WikiPagesToDelete
	SELECT id
	FROM wiki_pages
	WHERE is_deleted = 1
	OR (page_name NOT LIKE 'InternalSystem%' AND len(markup) < 18)

DELETE FROM wiki_pages WHERE id IN (SELECT id FROM @WikiPagesToDelete)

UPDATE wiki_pages SET markup = '[TODO]: Wiped as sample data' WHERE len(markup) > 1500 AND page_name <> 'Movies'

DELETE FROM wiki_referrals --TODO: maybe only delete referrals for deleted wiki pages and preserve existing

--Trim Publications and Submissions
DECLARE @Publications as TABLE (id int primary key, submission_id int, wiki_content_id int)
INSERT INTO @Publications
	SELECT id = p.id, submission_id = p.submission_id, wiki_content_id = p.wiki_content_id
	FROM publications p
	WHERE p.obsoleted_by_id IS NULL
	AND id > 3600

DECLARE @PublicationWikis as TABLE (id int primary key)
INSERT INTO @PublicatioNWikis
	SELECT wp.id
	FROM wiki_pages wp
	LEFT JOIN @Publications p on wp.id = p.wiki_content_id
	WHERE p.id IS NULL
	
DELETE p
	FROM publications p
	LEFT JOIN @Publications ipu on p.id = ipu.id
	WHERE ipu.id IS NULL

DELETE wp
	FROM wiki_pages wp
	LEFT JOIN @Publications p on wp.id = p.wiki_content_id
	WHERE wp.page_name LIKE 'InternalSystem/PublicationContent/M%'
	AND p.id IS NULL

DELETE pa
	FROM publication_authors pa
	LEFT JOIN @Publications ipu on pa.publication_id = ipu.id
	WHERE ipu.id IS NULL
	
DELETE pa
	FROM publication_awards pa
	LEFT JOIN @Publications ipu on pa.publication_id = ipu.id
	WHERE ipu.id IS NULL

DELETE pf
	FROM publication_files pf
	LEFT JOIN @Publications ipu on pf.publication_id = ipu.id
	WHERE ipu.id IS NULL

DELETE pf
	FROM publication_flags pf
	LEFT JOIN @Publications ipu on pf.publication_id = ipu.id
	WHERE ipu.id IS NULL

DELETE pr
	FROM publication_ratings pr
	LEFT JOIN @Publications ipu on pr.publication_id = ipu.id
	WHERE ipu.id IS NULL

DELETE pu
	FROM publication_urls pu	
	LEFT JOIN @Publications ipu on pu.publication_id = ipu.id
	WHERE ipu.id IS NULL

DECLARE @Submissions as TABLE (id int primary key, wiki_content_id int)
INSERT INTO @Submissions
	SELECT id = s.id, wiki_content_id = s.wiki_content_id
	FROM submissions s
	JOIN @Publications p on s.id = p.submission_id

DELETE s
	FROM submissions s
	LEFT JOIN @Submissions isu ON s.id = isu.id
	WHERE isu.id IS NULL

DELETE wp
	FROM wiki_pages wp
	LEFT JOIN @Submissions s on wp.id = s.wiki_content_id
	WHERE wp.page_name LIKE 'InternalSystem/SubmissionContent/S%'
	AND s.id IS NULL

DELETE sa
	FROM submission_authors sa
	LEFT JOIN @Submissions isu on sa.submission_id = isu.id
	WHERE isu.id IS NULL

DELETE ssh
	FROM submission_status_history ssh
	LEFT JOIN @Submissions isu on ssh.submission_id = isu.id
	WHERE isu.id IS NULL

UPDATE publications SET movie_file = 0x0
UPDATE submissions SET movie_file = 0x0

--Delete unncessary Games
DECLARE @Games as Table (id int primary key)
INSERT INTO @Games
	SELECT DISTINCT id = g.id
	FROM Games g
	LEFT JOIN publications p on g.id = p.game_id
	LEFT JOIN submissions s ON g.id =  s.game_id
	LEFT JOIN user_files uf on g.id = uf.game_id
	WHERE p.id IS NULL
	AND s.id IS NULL
	AND uf.id IS NULL
	AND g.id > 0

DELETE gg
	FROM game_genres gg
	LEFT JOIN @Games g on gg.game_id = g.id
	WHERE g.id IS NULL
		
DELETE FROM g
	FROM Games g
	JOIN @Games ig on g.id = ig.id

--Trim down forum data
DECLARE @Topics as Table (id int primary key)
INSERT INTO @Topics 
	SELECT topic.id
	FROM forums f
	CROSS APPLY (
		SELECT
		TOP 4 *
		FROM forum_topics t
		WHERE t.forum_id = f.id
		ORDER BY t.create_timestamp DESC
	) topic


DECLARE @Posts as Table (id int primary key)
INSERT INTO @Posts
	SELECT post.id
	FROM @Topics t
	CROSS APPLY (
		SELECT
		TOP 4 *
		FROM forum_posts p
		WHERE t.id = p.topic_id
		ORDER BY p.create_timestamp -- First posts instead of last because we need the post that started the topic
	) post

DECLARE @Polls as TABLE (id int primary key)
INSERT INTO @Polls
	SELECT p.id
	FROM forum_polls p
	JOIN @Topics t ON p.topic_id = t.id

DECLARE @PollOptions as TABLE (id int primary key)
INSERT INTO @PollOptions
	SELECT po.id
	FROM forum_poll_options po
	JOIN @Polls p on po.poll_id = p.id

DECLARE @PollOptionVotes as TABLE (id int primary key)
INSERT INTO @PollOptionVotes
	SELECT pov.id
	FROM forum_poll_option_votes pov
	JOIN @PollOptions po on pov.poll_option_id = po.id

DELETE pov
FROM forum_poll_option_votes pov
LEFT JOIN @PollOptionVotes ipov on pov.id = ipov.id
WHERE ipov.id IS NULL

DELETE po
FROM forum_poll_options po
LEFT JOIN @PollOptions ipo on po.id = ipo.id
WHERE ipo.id IS NULL

DELETE p
FROM forum_posts p
LEFT JOIN @Posts iposts on p.id = iposts.id
WHERE iposts.id IS NULL

DELETE t
FROM forum_topics t
LEFT JOIN @Topics it ON t.id = it.id
WHERE it.id IS NULL

DELETE p
FROM forum_polls p
LEFT JOIN @Polls ip on p.id = ip.id
WHERE ip.id IS NULL

--Clear User data
DELETE FROM forum_topic_watches
DELETE FROM private_messages

-- Delete Users who have not contributed to any current data, and have no roles
DECLARE @ActiveUsers As Table (id int primary key)
INSERT INTO @ActiveUsers
	SELECT u.id
	FROM users u
	WHERE EXISTS (SELECT 1 FROM user_awards ua WHERE ua.user_id = u.id)
	OR EXISTS (SELECT 1 FROM user_files uf where uf.author_id = u.id)
	OR EXISTS (SELECT 1 FROM submissions s WHERE s.publisher_id = u.id)
	OR EXISTS (SELECT 1 FROM submissions s WHERE s.judge_id = u.id)
	OR EXISTS (SELECT 1 FROM submissions s WHERE s.submitter_id = u.id)
	OR EXISTS (SELECT 1 FROM submission_authors sa where sa.user_id = u.id)
	OR EXISTS (SELECT 1 FROM publication_authors pa where pa.user_id = u.id)
	OR EXISTS (SELECT 1 FROM publication_ratings pr WHERE pr.user_id = u.id)
	OR EXISTS (SELECT 1 FROM forum_posts fp WHERE fp.poster_id = u.id)
	OR EXISTS (SELECT 1 FROM forum_topics ft WHERE ft.poster_id = u.id)
	
DELETE ur
	FROM user_roles ur
	LEFT JOIN @ActiveUsers iu on ur.user_id = iu.id
	WHERE iu.id IS NULL

DELETE u
	FROM users u
	LEFT JOIN @ActiveUsers au on u.id = au.id
	WHERE au.id IS NULL

UPDATE users 
	SET Signature = NULL,
		legacy_password = 'caecb26de1c989826750c7c478a9401d', -- We don't want to make these public
		email = null -- We dont' want to make these public either
