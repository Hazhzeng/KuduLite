#!/bin/bash

PROCESS="$1"
PROCANDARGS=$*

mkdir -p /home/LogFiles/webssh

while :
do
    RESULT=`pgrep ${PROCESS}`

    if [ "${RESULT:-null}" = null ]; then
            echo "${PROCESS} not running, starting "$PROCANDARGS
            # Ensure std logs go to `/dev/null` and error logs are recorded per instance
            $PROCANDARGS 1> /dev/null 2> /home/LogFiles/webssh/$COMPUTERNAME_err.log &
    fi
    sleep 10
done
