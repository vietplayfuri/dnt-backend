#!/bin/sh
psql -U postgres -c "drop database if exists costs";
psql -U postgres -c "create database costs";
flyway-4.2.0/flyway migrate
