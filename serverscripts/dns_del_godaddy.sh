#!/bin/bash

source ~tasvideos/.secrets/GODADDY_VARIABLES

domain="tasvideos.org"                      # your domain
type="TXT"                                  # Record type A, CNAME, MX, etc.
name="_acme-challenge"                      # name of record to update
ttl="3600"                                  # Time to Live min value 600

headers="Authorization: sso-key $key:$secret"

curl -X DELETE \
"https://api.godaddy.com/v1/domains/$domain/records/$type/$name" \
-H "accept: application/json" \
-H "Content-Type: application/json" \
-H "$headers"


