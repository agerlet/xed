#!/bin/bash

docker stop $(docker ps -q)
docker rm $(docker ps -a -q)