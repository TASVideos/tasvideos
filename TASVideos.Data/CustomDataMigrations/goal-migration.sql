-- Does the initial migration of branches into game goals
DO $$
DECLARE
	sub record;
	pub record;
	tempBranch citext;
	tempGameGoal record;
	tempGoalId integer;
BEGIN 
	
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
		--RAISE NOTICE '-- Submission % --', sub.id;
		tempBranch = sub.branch;
		
		-- Handle Goal Record
		IF sub.branch IS NULL THEN
			tempBranch = 'baseline';			
		END IF;
		
		SELECT game_id, id INTO tempGameGoal FROM game_goals WHERE game_id = sub.game_id AND display_name = tempBranch;
		IF tempGameGoal IS NULL THEN					
			--RAISE NOTICE 'GameGoal does not already exist, INSERTING';
			INSERT INTO game_goals (game_id, display_name) VALUES (sub.game_id, tempBranch);
			SELECT game_id, display_name, id INTO tempGameGoal FROM game_goals WHERE game_id = sub.game_id AND display_name = tempBranch;
			IF tempGameGoal IS NULL THEN
				RAISE EXCEPTION 'FAILED TO INSERT GAME GOAL FOR %', sub.branch;
			END IF;
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
		ORDER BY id;
	
	FOR pub in SELECT id, game_id, branch FROM _publications LOOP			
		--RAISE NOTICE '-- Submission % --', sub.id;
		tempBranch = pub.branch;
		
		-- Handle Goal Record
		IF pub.branch IS NULL THEN
			tempBranch = 'baseline';			
		END IF;
		
		SELECT game_id, id INTO tempGameGoal FROM game_goals WHERE game_id = pub.game_id AND display_name = tempBranch;
		IF tempGameGoal IS NULL THEN					
			--RAISE NOTICE 'GameGoal does not already exist, INSERTING';
			INSERT INTO game_goals (game_id, display_name) VALUES (pub.game_id, tempBranch);
			SELECT game_id, display_name, id INTO tempGameGoal FROM game_goals WHERE game_id = pub.game_id AND display_name = tempBranch;
			IF tempGameGoal IS NULL THEN
				RAISE EXCEPTION 'FAILED TO INSERT GAME GOAL FOR %', pub.branch;
			END IF;
		END IF;
		
		UPDATE publications SET game_goal_id = tempGameGoal.id WHERE id = pub.id; 
	END LOOP;
END $$

--SELECT * FROM game_goals
--UPDATE submissions SET game_goal_id = NULL
--UPDATE publications SET game_goal_id = NULL
--DELETE FROM game_goals

