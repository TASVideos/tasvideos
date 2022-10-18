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

DELETE FROM public.ip_bans;
DELETE FROM public.media_posts;
DELETE FROM public.user_disallows;
DELETE FROM public."__EFMigrationsHistory";
DELETE FROM public.auto_history;

-- Trim user files
DELETE FROM public.user_file_comments;

DROP TABLE IF EXISTS _user_files;
CREATE TEMPORARY TABLE _user_files (id bigint primary key);
INSERT INTO _user_files
	SELECT uf.id
	FROM public.user_files uf
	WHERE uf.hidden = false
	ORDER BY LENGTH(uf.content)
	LIMIT 4;

DELETE
FROM public.user_files uf
WHERE uf.id NOT IN (SELECT uif.id from _user_files uif);

--Trim Wiki
DELETE FROM public.wiki_pages WHERE child_id is not null;
UPDATE public.wiki_pages SET markup = '[TODO]: Wiped as sample data' WHERE LENGTH(markup) > 1500 AND page_name <> 'Movies';

DELETE FROM public.wiki_referrals; --TODO: maybe only delete referrals for deleted wiki pages and preserve existing

--Clear User data
DELETE FROM public.forum_topic_watches;
DELETE FROM public.private_messages;
DELETE
FROM public.publication_ratings pr
WHERE (SELECT public_ratings from users WHERE id = pr.user_id) = false;

 --Trim Publications and Submissions
TRUNCATE TABLE publication_maintenance_logs;

--Terrible hack, but without it, resulting script can be run due to fk containts on obsoleted_by_id, need to investigate
DELETE FROM public.publications WHERE obsoleted_by_id is not null;

UPDATE public.publications SET movie_file = E'\\000'::bytea;
UPDATE public.submissions SET movie_file = E'\\000'::bytea;

--Trim down forum data

--Terrible hack, but without it, the resulting script can't be run due to fk constraints, need to investigate
UPDATE forum_topics SET submission_id = null;

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
		LIMIT 50
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
	OR u.id = 505 --TVA
	OR u.id = 3788 --TASVideosGrue
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

--Update call tracking columns, we do not want to expose these
DO $$
DECLARE execute_query text;
BEGIN
	DROP TABLE IF EXISTS _tracking_columns;
	CREATE TEMPORARY TABLE _tracking_columns (col citext, tab citext);
	INSERT INTO _tracking_columns
	SELECT
		column_name, table_name
	FROM information_schema.columns
	WHERE column_name = 'create_user_name';
	execute_query := (SELECT string_agg('UPDATE ' || tab || ' SET ' || col || ' = ''a''', ';') FROM _tracking_columns);
	EXECUTE(execute_query);
END $$;