-- Does the initial migration of branches into game goals
DO $$
DECLARE
	sub record;
	pub record;
	tempGameGoal record;
	baseLineId integer := (SELECT id from goals WHERE display_name = 'baseline');
	tempGoalId integer;
BEGIN 
	IF baseLineId IS NULL THEN
		RAISE NOTICE 'baseline does not exist, creating';
		INSERT INTO goals (display_name, create_timestamp, last_update_timestamp) VALUES ('baseline', NOW(), NOW());
		SELECT id INTO baseLineId FROM goals WHERE display_name = 'baseline';
		RAISE NOTICE 'Inserted baseLine goal: %', baseLineId;
	ELSE
		RAISE NOTICE 'baseline exist, skipping insert';
	END IF;
	
	RAISE NOTICE 'baselineId %', baseLineId;
	
	RAISE NOTICE '--------------------------';
	RAISE NOTICE '---- Submission Goals ----';
	RAISE NOTICE '--------------------------';
	DROP TABLE IF EXISTS _submissions;
	CREATE TEMPORARY TABLE _submissions (id int primary key, game_id int, branch citext);
	INSERT INTO _submissions
		SELECT id, game_id, TRIM(REPLACE(branch, '"', '')) AS branch
		FROM submissions
		WHERE game_goal_id IS NULL
		AND game_id IS NOT NULL
		ORDER BY id;
	
	FOR sub in SELECT id, game_id, branch FROM _submissions LOOP
		RAISE NOTICE '-- Submission % --', sub.id;
		-- Handle Goal Record
		IF sub.branch IS NULL THEN
			tempGoalId = baseLineId;
			--RAISE NOTICE 'Base line submission % using baseLine goal id: %', sub.id, tempGoalId;
		ELSE
			--RAISE NOTICE 'Non-baseline goal, finding goal record';
			SELECT id INTO tempGoalId FROM goals where display_name = sub.branch;
			IF tempGoalId IS NULL THEN
				--RAISE NOTICE 'goal does not exist, creating';
				INSERT INTO goals (display_name, create_timestamp, last_update_timestamp) VALUES (sub.branch, NOW(), NOW());
				SELECT id INTO tempGoalId FROM goals where display_name = sub.branch;
				IF tempGoalId IS NULL THEN
					RAISE EXCEPTION 'FAILED TO INSERT GOAL %', sub.branch;
				END IF;
				--RAISE NOTICE 'INSERTED goal, id: %', tempGoalId;
			END IF;
		END IF;
		
		-- Handle Game Record
		RAISE NOTICE 'Begin Game Goal';
		SELECT game_id, goal_id, id INTO tempGameGoal FROM game_goals WHERE game_id = sub.game_id AND goal_id = tempGoalId;
		IF tempGameGoal IS NULL THEN
			--RAISE NOTICE 'GameGoal does not already exist, INSERTING';
			INSERT INTO game_goals (game_id, goal_id) VALUES (sub.game_id, tempGoalId);
			SELECT game_id, goal_id, id INTO tempGameGoal FROM game_goals WHERE game_id = sub.game_id AND goal_id = tempGoalId;
			IF tempGameGoal IS NULL THEN
				RAISE EXCEPTION 'FAILED TO INSERT GAME GOAL FOR %', sub.branch;
			END IF;
			--RAISE NOTICE 'INSERTED game_goal % %', tempGameGoal.game_id, tempGameGoal.goal_id;
		END IF;
		
		UPDATE submissions SET game_goal_id = tempGameGoal.id WHERE id = sub.id;
	END LOOP;
	
	RAISE NOTICE '--------------------------';
	RAISE NOTICE '---- Publication Goals ----';
	RAISE NOTICE '--------------------------';
	DROP TABLE IF EXISTS _publications;
	CREATE TEMPORARY TABLE _publications (id int primary key, game_id int, branch citext);
	INSERT INTO _publications
		SELECT id, game_id, TRIM(REPLACE(branch, '"', '')) AS branch
		FROM publications
		WHERE game_goal_id IS NULL
		AND game_id IS NOT NULL
		--AND id < 1000
		ORDER BY id;
	
	FOR pub in SELECT id, game_id, branch FROM _publications LOOP
		--RAISE NOTICE '-- Publications % --', pub.id;
		-- Handle Goal Record
		IF pub.branch IS NULL THEN
			tempGoalId = baseLineId;
			--RAISE NOTICE 'Base line publication % using baseLine goal id: %', pub.id, tempGoalId;
		ELSE
			--RAISE NOTICE 'Non-baseline goal, finding goal record';
			SELECT id INTO tempGoalId FROM goals where display_name = pub.branch;
			IF tempGoalId IS NULL THEN
				--RAISE NOTICE 'goal does not exist, creating';
				INSERT INTO goals (display_name, create_timestamp, last_update_timestamp) VALUES (pub.branch, NOW(), NOW());
				SELECT id INTO tempGoalId FROM goals where display_name = pub.branch;
				IF tempGoalId IS NULL THEN
					RAISE EXCEPTION 'FAILED TO INSERT GOAL %', pub.branch;
				END IF;
				--RAISE NOTICE 'INSERTED goal, id: %', tempGoalId;
			END IF;
		END IF;
		
		-- Handle Game Record
		--RAISE NOTICE 'Begin Game Goal';
		SELECT game_id, goal_id, id INTO tempGameGoal FROM game_goals WHERE game_id = pub.game_id AND goal_id = tempGoalId;
		IF tempGameGoal IS NULL THEN
			--RAISE NOTICE 'GameGoal does not already exist, INSERTING';
			INSERT INTO game_goals (game_id, goal_id) VALUES (pub.game_id, tempGoalId);
			SELECT game_id, goal_id, id INTO tempGameGoal FROM game_goals WHERE game_id = pub.game_id AND goal_id = tempGoalId;
			IF tempGameGoal IS NULL THEN
				RAISE EXCEPTION 'FAILED TO INSERT GAME GOAL FOR %', pub.branch;
			END IF;
			--RAISE NOTICE 'INSERTED game_goal % %', tempGameGoal.game_id, tempGameGoal.goal_id;
		END IF;
		
		UPDATE publications SET game_goal_id = tempGameGoal.id WHERE id = pub.id;
	END LOOP;
END $$

--SELECT * FROM goals
--SELECT * FROM game_goals
--UPDATE submissions SET game_goal_id = NULL
--UPDATE publications SET game_goal_id = NULL
--DELETE FROM goals
--DELETE FROM game_goals

