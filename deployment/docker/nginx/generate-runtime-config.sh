#!/bin/sh
# Runs automatically on container start (nginx's official entrypoint executes every script in
# /docker-entrypoint.d/ before starting nginx). Writes the API_URL env var into a static JS file
# so the frontend can pick up a new API URL with just a container restart - no image rebuild.
set -e
: "${API_URL:=}"
envsubst '${API_URL}' < /etc/nginx/templates/runtime-config.js.template > /usr/share/nginx/html/runtime-config.js
