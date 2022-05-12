# NestDashboard

Very basic website to show Nest camera feeds.

For more information: https://developers.google.com/nest/device-access/get-started

Set the following enviromental variables:

- `PROJECTID` Create a Google smart home project here: https://console.nest.google.com/device-access
- `CLIENTID` Your google api client id - https://console.cloud.google.com/apis/credentials
- `SECRET` Your google api account client secret

## Docker

```bash

docker run -d -p 80:80 -e PROJECTID=123xyz456 -e CLIENTID=googleclientid.com -e SECRET=clientsecret ghcr.io/jamescoverdale/nestdashboard:latest

```
 
