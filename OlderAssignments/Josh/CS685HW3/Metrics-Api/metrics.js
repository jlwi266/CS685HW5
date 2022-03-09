const express = require('express');
const bodyparser = require('body-parser');
const app = express();
const port = 8080;

var pages = []
var errors = []
var files = []

var startTime = null;
var endTime = null;

app.use(bodyparser.json({limit: "25mb"}));

app.get('/metrics', (req, res) => 
{
    resObj = {
        "startTime": startTime,
        "endTime": endTime,
        "pageCount": pages.length,
        "errorCount": errors.length,
        "fileCount": files.length,
        "pages": pages,
        "errors": errors,
        "files": files
    }

    res.send(resObj)
});

app.post('/starttime', (req, res) => 
{
    pages = [];
    files = [];
    errors = [];
    endTime = null;

    startTime = req.body.time;
    res.send("success");
});

app.post('/endtime', (req, res) => 
{
    endTime = req.body.time;
    res.send("success");
});

app.post('/page', (req, res) => 
{
    pages.push(req.body);
    res.send("success");
});

app.post('/error', (req, res) => 
{
    errors.push(req.body);
    res.send("success");
});

app.post('/file', (req, res) => 
{
    files.push(req.body);
    res.send("success");
});

app.listen(port, () => {
  console.log(`metrics API listening at http://localhost:${port}`)
})