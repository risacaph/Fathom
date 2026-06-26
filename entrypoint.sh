#! /bin/bash

# Removed - This is causing issues for Synology users
## Set default UID and GID for Fathom but allow overrides
#PUID=${PUID:-0}
#PGID=${PGID:-0}
#
## Add Fathom group if it doesn't already exist
#if [[ -z "$(getent group "$PGID" | cut -d':' -f1)" ]]; then
#    groupadd -o -g "$PGID" fathom
#fi
#
## Add Fathom user if it doesn't already exist
#if [[ -z "$(getent passwd "$PUID" | cut -d':' -f1)" ]]; then
#    useradd -o -u "$PUID" -g "$PGID" -d /fathom fathom
#fi

#Checks if the config file exists, and creates it if it does not
if [ ! -f "/fathom/config/appsettings.json" ]; then
    echo "Fathom configuration file does not exist, copying from temp..."
    cp /tmp/config/appsettings.json /fathom/config/appsettings.json
    if [ -f "/fathom/config/appsettings.json" ]; then
        echo "Copy completed successfully, starting app..."
    else
        echo "Copy failed, check folder permissions. Exiting..."
        exit
    fi
fi

echo "Starting Fathom"
echo ls -l "/fathom/config/appsettings.json"

exec ./Fathom

#if [[ "$PUID" -eq 0 ]]; then
#    # Run as root
#    ./Fathom
#else
#    # Set ownership on config dir if running non-root and current ownership is different
#    if [[ ! "$(stat -c %u /fathom/config)" = "$PUID" ]]; then
#        echo "Specified PUID differs from Fathom config dir ownership, updating permissions now..."
#        if [[ ! "$(stat -c %g /fathom/config)" = "$PGID" ]]; then
#            chown -R "$PUID":"$PGID" /fathom/config
#        else
#            chown -R "$PUID" /fathom/config
#        fi
#
#    elif [[ ! "$(stat -c %g /fathom/config)" = "$PGID" ]]; then
#        echo "Specified PGID differs from Fathom config dir ownership, updating permissions now..."
#        chgrp -R "$PGID" /fathom/config
#    fi
#
#    # Run as non-root user
#    su -l fathom -c ./Fathom
#fi
