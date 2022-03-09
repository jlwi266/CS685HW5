First, start the Metrics API (requires node.js):

cd Metrics-API
npm install
node metrics.js

Then, open another console and start the crawler (requires dotnet core):

cd Crawler-App
dotnet restore
dotnet run