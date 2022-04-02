-- Instructions
-- Run a full import
-- Run this script
-- In PgAdmin, right click on the database and click Backup.
--		Enter sample-data.sql in the SampleData folder as the filename
--		Select Plain as the Format
--		Select UTF8 as the encoding
--		In the Dump options tab
--			Click data only - Yes
--			Schema only - No
--			Use column inserts - Yes
--			Use Insert Commands - Yes
--		Run
--		gzip sample-data.sql.gz and replace temp file

DELETE FROM public.game_ram_address_domains;
DELETE FROM public.game_ram_addresses;
DELETE FROM public.ip_bans;
DELETE FROM public.media_posts;
DELETE FROM public.user_disallows;
DELETE FROM public."__EFMigrationsHistory";
-- Trim user files
DELETE FROM public.user_file_comments;

DROP TABLE IF EXISTS _user_files;
CREATE TEMPORARY TABLE _user_files (id bigint primary key);
INSERT INTO _user_files
	SELECT uf.id
	FROM public.user_files uf
	WHERE uf.hidden = false
	ORDER BY LENGTH(uf.content)
	LIMIT 10;

DELETE
FROM public.user_files uf
WHERE uf.id NOT IN (SELECT uif.id from _user_files uif);

--Trim Wiki
DELETE FROM public.wiki_pages WHERE child_id is not null;

DROP TABLE IF EXISTS _wiki_pages_to_delete;
CREATE TEMPORARY TABLE _wiki_pages_to_delete (id int primary key);
INSERT INTO _wiki_pages_to_delete
	SELECT id
	FROM public.wiki_pages
	WHERE is_deleted = true and child_id IS NOT NULL
	OR (page_name NOT LIKE 'InternalSystem%' AND LENGTH(markup) < 18);
	
DELETE
FROM public.wiki_pages wp
WHERE wp.id IN (SELECT twp.id from _wiki_pages_to_delete twp);

UPDATE public.wiki_pages SET markup = '[TODO]: Wiped as sample data' WHERE LENGTH(markup) > 1500 AND page_name <> 'Movies';

DELETE FROM public.wiki_referrals; --TODO: maybe only delete referrals for deleted wiki pages and preserve existing

--Clear User data
DELETE FROM public.forum_topic_watches;
UPDATE public.users SET signature = NULL;
UPDATE public.users SET legacy_password = NULL; --We don't want this data public
DELETE FROM public.private_messages;

 --Trim Publications and Submissions
TRUNCATE TABLE publication_maintenance_logs;
DROP TABLE IF EXISTS _publications;
CREATE TEMPORARY TABLE _publications (id int primary key, submission_id int, wiki_content_id int);
INSERT INTO _publications
 	SELECT p.id, p.submission_Id, p.wiki_content_id
	FROM public.publications p
	WHERE p.obsoleted_by_id IS NULL
	AND p.id > 3600;
	
DROP TABLE IF EXISTS _publication_wikis;
CREATE TEMPORARY TABLE _publication_wikis (id int primary key);
INSERT INTO _publication_wikis
	SELECT wp.id
	FROM public.wiki_pages wp
	LEFT JOIN _publications p on wp.id = p.wiki_content_id
	WHERE p.id IS NULL;

DROP TABLE IF EXISTS _publication_ratings;
CREATE TEMPORARY TABLE _publication_ratings (user_id int, publication_id int);
INSERT INTO _publication_ratings
	SELECT pr.user_id, pr.publication_id
	FROM public.publication_ratings pr
	JOIN public.users u on pr.user_id = u.id
	LEFT JOIN _publications p on pr.publication_id = p.id
	WHERE p is NULL
	OR u.public_ratings = true;

DELETE
FROM public.publications p
WHERE p.id NOT IN (SELECT id from _publications);

DELETE
FROM public.wiki_pages wp
WHERE wp.id NOT IN (SELECT wiki_content_id from _publications)
AND wp.page_name LIKE 'InternalSystem/PublicationContent/M%';

DELETE
FROM public."publication_authors" pa
WHERE pa.publication_id NOT IN (SELECT Id from _publications);

DELETE
FROM public.publication_awards pa
WHERE pa.publication_id NOT IN (SELECT Id from _publications);

DELETE
FROM public.publication_files pf
WHERE pf.publication_id NOT IN (SELECT Id from _publications);

DELETE
FROM public.publication_flags pf
WHERE pf.publication_id NOT IN (SELECT Id from _publications);

DELETE
FROM public.publication_ratings pr
USING _publication_ratings prr
WHERE pr.user_id = prr.user_id AND pr.publication_id = prr.publication_id;

DELETE
FROM public.publication_urls pu
WHERE pu.publication_id NOT IN (SELECT Id from _publications);

DROP TABLE IF EXISTS _submissions;
CREATE TEMPORARY TABLE _submissions (id int primary key, wiki_content_id int);

INSERT INTO _submissions
	SELECT s.id, s.wiki_content_id
	FROM public.submissions s
	JOIN _publications p on s.id = p.submission_id;

UPDATE forum_topics
	SET submission_id = null
	FROM forum_topics f
	LEFT JOIN _submissions isu on f.submission_id = isu.id
	WHERE isu.id IS NULL;

DELETE
FROM public.submissions s
WHERE s.id NOT IN (SELECT id from _submissions);

DELETE
FROM public.wiki_pages wp
WHERE wp.id NOT IN (SELECT wiki_content_id from _submissions)
AND wp.page_name LIKE 'InternalSystem/SubmissionContent/S%';

DELETE
FROM public.submission_authors sa
WHERE sa.submission_id NOT IN (SELECT Id from _submissions);

DELETE
FROM public.submission_status_history ssh
WHERE ssh.submission_id NOT IN (SELECT id from _submissions);

UPDATE public.publications SET movie_file = E'\\000'::bytea;
UPDATE public.submissions SET movie_file = E'\\000'::bytea;

--Delete unncessary Games
DROP TABLE IF EXISTS _games;
CREATE TEMPORARY TABLE _games (id int primary key);
INSERT INTO _games
	SELECT DISTINCT g.id
	FROM public.games g
	LEFT JOIN public.publications p on g.id = p.game_id
	LEFT JOIN public.submissions s ON g.id = s.game_id
	LEFT JOIN public.user_files uf on g.id = uf.game_id
	WHERE p.id IS NULL
	AND s.id IS NULL
	AND uf.id IS NULL
	AND g.id > 0;

DELETE
FROM public.game_genres gg
WHERE gg.game_id NOT IN (SELECT Id from _games);

DELETE
FROM public.games g
WHERE g.id IN (SELECT id from _games);

--Trim down forum data
DROP TABLE IF EXISTS _topics;
CREATE TEMPORARY TABLE _topics (id int primary key);
INSERT INTO _topics
	SELECT topic.id
	FROM public.forums f
	JOIN LATERAL (
		SELECT *
		FROM public.forum_topics t
		WHERE t.forum_id = f.id
		ORDER BY t.create_timestamp DESC
		LIMIT 4
	) AS topic ON topic.forum_id = f.id
	WHERE f.restricted = false;
	
DROP TABLE IF EXISTS _posts;
CREATE TEMPORARY TABLE _posts (id int primary key);
INSERT INTO _posts
	SELECT post.Id
	FROM _topics t
	JOIN LATERAL (
		SELECT *
		FROM public.forum_posts p
		WHERE t.id = p.topic_id
		ORDER BY p.create_timestamp -- First posts instead of last because we need the post that started the topic
		LIMIT 4
	) post on post.topic_id = t.id;
	
DROP TABLE IF EXISTS _polls;
CREATE TEMPORARY TABLE _polls (id int primary key);
INSERT INTO _polls
	SELECT p.Id
	FROM public.forum_polls p
	JOIN _topics t ON p.topic_id = t.id;

DROP TABLE IF EXISTS _poll_options;
CREATE TEMPORARY TABLE _poll_options (id int primary key);
INSERT INTO _poll_options
	SELECT po.id
	FROM public.forum_poll_options po
	JOIN _polls p on po.poll_id = p.id;

DELETE
FROM public.forum_poll_option_votes pov;

DELETE
FROM public.forum_poll_options po
WHERE po.id NOT IN (SELECT id from _poll_options);

DELETE
FROM public.forum_posts p
WHERE p.id NOT IN (SELECT id FROM _posts);

DELETE
FROM public.forum_topics t
WHERE t.id NOT IN (SELECT id FROM _topics);

DELETE
FROM public.forum_polls p
WHERE p.id NOT IN (SELECT id from _polls);

 --No reason to make this data available
UPDATE forum_posts SET ip_address = '8.8.8.8';
UPDATE forum_poll_option_votes SET ip_address = '8.8.8.8';

-- Delete Users who have not contributed to any current data, and have no roles
TRUNCATE TABLE user_maintenance_logs;
DROP TABLE IF EXISTS _active_users;
CREATE TEMPORARY TABLE _active_users (id int primary key);
INSERT INTO _active_users
	SELECT u.id
	FROM public.users u
	WHERE EXISTS (SELECT 1 FROM public.user_awards ua WHERE ua.user_id = u.id)
	OR EXISTS (SELECT 1 FROM public.user_files uf WHERE uf.author_id = u.id)
	OR EXISTS (SELECT 1 FROM public.submissions s WHERE s.publisher_id = u.id)
	OR EXISTS (SELECT 1 FROM public.submissions s WHERE s.judge_id = u.id)
	OR EXISTS (SELECT 1 FROM public.submissions s WHERE s.submitter_id = u.id)
	OR EXISTS (SELECT 1 FROM public.submission_authors sa WHERE sa.user_id = u.id)
	OR EXISTS (SELECT 1 FROM public.publication_authors pa WHERE pa.user_id = u.id)
	OR EXISTS (SELECT 1 FROM public.publication_ratings pr WHERE pr.user_id = u.id)
	OR EXISTS (SELECT 1 FROM public.forum_posts fp WHERE fp.poster_id = u.id)
	OR EXISTS (SELECT 1 FROM public.forum_topics ft WHERE ft.poster_id = u.id)
	OR EXISTS (SELECT 1 FROM public.forum_poll_option_votes v WHERE v.user_id = u.id)
	OR EXISTS (SELECT 1 FROM public.wiki_pages wp WHERE wp.author_id = u.id);

DELETE
FROM public.user_roles ur
WHERE ur.user_id NOT IN (SELECT id FROM _active_users);

DELETE
FROM public.users u
WHERE u.id NOT IN (SELECT id FROM _active_users);

UPDATE public.users 
	SET signature = NULL,
		password_hash = 'AQAAAAEAACcQAAAAEJ0432uxp/JdMT51+b1SqQRq52JyKAiumPqKr/LO7Z73Kctz/eu5GZLonouiGmo0ww==', -- We don't want to make the real password public
		email = 'tasvideos@example.com', --We don't want these public, but a valid email address is necessary for many user operations
		normalized_email = null, -- We don't want to make these public either
		last_logged_in_time_stamp = NOW(),
		time_zone_iD = 'UTC';

TRUNCATE TABLE public.user_claims;