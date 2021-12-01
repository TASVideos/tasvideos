#!/bin/bash

source ~tasvideos/.secrets/GODADDY_VARIABLES

echo $1 - $2

domain="tasvideos.org"                      # your domain
type="TXT"                                  # Record type A, CNAME, MX, etc.
name="_acme-challenge"                      # name of record to update
ttl="3600"                                  # Time to Live min value 600

headers="Authorization: sso-key $key:$secret"

data="[ { \"data\": \"$2\", \"ttl\": $ttl } ]"

# Adding/updating tasvidos.org challenge
curl -X PUT \
"https://api.godaddy.com/v1/domains/$domain/records/$type/$name" \
-H "accept: application/json" \
-H "Content-Type: application/json" \
-H "$headers" \
-d "$data"

