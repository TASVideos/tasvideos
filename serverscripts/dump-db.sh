#!/bin/bash
pg_dump TASVideos | gzip -9  > /home/tasvideos/db_dump/dump.sql.gz
