const fs = require('fs');
const { matrix, multiply, norm } = require('mathjs');
const math = require('mathjs');
const _ = require('lodash');
const fileToOpen = 'test_matrix_1.json';

const matrixRawData = JSON.parse(fs.readFileSync('./RawInput/' + fileToOpen));
var totalNodeCount = Object.keys(matrixRawData).length;

var hArray = []; 
var aArray = [];

_.forEach(Object.keys(matrixRawData), (key) => 
{
    var nextNodeOutlinkCount = matrixRawData[key].length;
    var nextColumn = math.zeros(totalNodeCount);
    var nextValue = (1/nextNodeOutlinkCount);

    _.forEach(matrixRawData[key], (outlink) =>
    {
        var index = _.findIndex(Object.keys(matrixRawData), x => x === outlink);
        nextColumn._data[index] = nextValue;
    });

    hArray.push(nextColumn._data);

    if (nextNodeOutlinkCount === 0)
    {
        nextColumn = math.multiply(math.ones(totalNodeCount), (1/totalNodeCount));
        aArray.push(nextColumn._data);
    }
    else
    {
        aArray.push(math.zeros(totalNodeCount)._data);
    }
});

var H = math.matrix(hArray);
var A = math.matrix(aArray);
var finalRanks = [];
var iterationsVerusAlpha = [];

for(var alpha = 0.0; alpha <= 1.0; alpha = alpha + 0.05)
{
    var hAlpha = math.multiply(H, alpha);
    var aAlpha = math.multiply(A, alpha);
    var finalFactor = ((1-alpha)/totalNodeCount)*1;
    var G = math.add(math.add(hAlpha, aAlpha), finalFactor);
    
    var iNext = math.matrix(math.zeros(totalNodeCount));
    iNext._data[0] = 1.0;
    
    var iterations = 0;
    
    var iMinusOne = iNext;
    iNext = math.multiply(iNext, G);
    
    while (math.norm(math.subtract(iMinusOne, iNext), 'fro') > 0.00001)
    {
        iMinusOne = iNext;
        iNext = math.multiply(iNext, G);
        iterations++;
    }

    var innerRank = [];
    
    _.forEach(Object.keys(matrixRawData), (key) => {
        var index = _.findIndex(Object.keys(matrixRawData), x => x === key);
        var nextEntry = {};
        nextEntry['node'] = key;
        nextEntry['rank'] = iNext._data[index];
        innerRank.push(nextEntry);
    })

    var lessInnerRank = {};
    innerRank = _.orderBy(innerRank, 'rank', 'desc');
    lessInnerRank[alpha] = innerRank;
    finalRanks.push(lessInnerRank);

    var nextIvA = {};
    nextIvA[alpha] = iterations;
    iterationsVerusAlpha.push(nextIvA);
}

rankFileName = './RawOutput/finalRanks_' + fileToOpen;
ivaFileName = './RawOutput/iterationsVersusAlpha_' + fileToOpen;

fs.writeFile(rankFileName, JSON.stringify(finalRanks, null, 1), (err) => {
    if(err)
    {
        console.log("error writing final ranks");
    }
    else
    {
        console.log("final ranks saved");
    }
})

fs.writeFile(ivaFileName, JSON.stringify(iterationsVerusAlpha, null, 1), (err) => {
    if(err)
    {
        console.log("error writing iterations v alpha");
    }
    else
    {
        console.log("iterationsVersusAlpha saved");
    }
})