# PgAdmin Config

This is used for spinning up the docker image.

The `.pgadmin-data` directory contains PgAdmin's database.

If you change the `servers.json` you may need to delete the `.pgadmin-data` directory to see your changes.

See:
* https://www.pgadmin.org/docs/pgadmin4/development/container_deployment.html
* https://github.com/postgres/pgadmin4/blob/master/web/pgadmin/setup/tests/servers.json
