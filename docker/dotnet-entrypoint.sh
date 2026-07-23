#!/bin/sh
set -eu
PORT="${PORT:-8080}"
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
exec dotnet "$APP_DLL"
