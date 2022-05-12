# NestDashboard

Very basic website to show Nest camera feeds.

Set the following enviromental variables:

- `PROJECTID` Create a Google smart home project here: http
- `CLIENTID` Your google api client id
- `SECRET` Your google api account client secret

## Docker

```bash

docker run -d -p 80:80 -e PROJECTID:123xyz456 -e CLIENTID:googleclientid.com -e SECRET:clientsecret ghcr.io/jamescoverdale/nestdashboard:latest

```
