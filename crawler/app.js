var q = 'jobsQueue';
var resultsQueue = "resultsQueue"

var open = require('amqplib').connect('amqp://localhost');

const asyncRedis = require("async-redis");
const client = asyncRedis.createClient();

const axios = require('axios').default;

const parseTitle = (body) => {
    let match = body.match(/<title>([^<]*)<\/title>/) // regular expression to parse contents of the <title> tag
    if (!match || typeof match[1] !== 'string')
      throw new Error('Unable to parse the title tag')
    return match[1]
  }

open.then(function (conn) {
    return conn.createChannel();
}).then(function (ch) {
    return ch.assertQueue(q).then(function (ok) {
        return ch.consume(q, function (msg) {
            if (msg !== null) {
                console.log(msg.content.toString());
                ch.ack(msg);

                const msgJson = JSON.parse(msg.content.toString());
                client.get(msgJson.Url)
                    .then(function (value) {
                        //if not already crawled
                        if (!value) {
                            //crawl
                            axios.get(`https://${msgJson.Url}`)
                                .then(function (response) {
                                    // handle success
                                    console.log(response);
                                    
                                    const title = parseTitle(response.data)                    

                                    //save to redis 
                                    client.set(msgJson.Url, title)
                                        .then(function (ok) {
                                            open.then(function (conn) {
                                                return conn.createChannel();
                                            }).then(function (ch) {
                                                return ch.assertQueue(resultsQueue).then(function (ok) {
                                            
                                                    //send response to master
                                                    return ch.sendToQueue(resultsQueue, Buffer.from(JSON.stringify({ url: msgJson.Url, title })));
                                                });
                                            }).catch(console.warn);
                                        })
                                })
                                .catch(function (error) {
                                    // handle error
                                    console.log(error);
                                })
                        }
                        console.log(ok)
                    })
            }
        });
    });
}).catch(console.warn);


