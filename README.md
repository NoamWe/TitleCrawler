# TitleCrawler
## How to run
1. run `docker run -p 6379:6379 -d redis`
2. run `docker run -d --hostname my-rabbit --name my-rabbit -p 15672:15672 -p  5672:5672 rabbitmq:3-management`
3. clone this repo
4. cd into the root folder of the project

## running node craweler
1. `cd crawler`
2. `npm install`
3. `node app.js`

## running the master c#
1. build and run

## API
Use swagger on localhost/5001
1. GET /api/urls/titles?lastMinutes=<SOMEINTEGER> if no query parameter is given then the value will be taken from appsettings
2. POST /api/urls/crawl 
  
