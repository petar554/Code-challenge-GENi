function countContainingRanges(input) {
    const pairs = input.split('\n');
    console.log(pairs);
    
    let containingCount = 0;
    let overlappingCount = 0;

    for (const pair of pairs) {
        const [range1, range2] = pair.split(',');

        const [start1, end1] = range1.split('-').map(Number);
        const [start2, end2] = range2.split('-').map(Number);
        
        if ((start1 >= start2 && end1 <= end2) || (start2 >= start1 && end2 <= end1)) {
            containingCount++;
        }

        // overlapping ranges
        if (start1 < end2 && start2 < end1) {
            overlappingCount++;
        }
    }

    return { containingCount, overlappingCount };
}

// the functionality has been tested with a larger amount of data.
const sampleInputData = [
    '2-4,6-8',
    '2-3,4-5',
    '5-7,7-9',
    '2-8,3-7',
    '6-6,4-6',
    '2-6,4-8',
].join('\n');

const result = countContainingRanges(sampleInputData);
console.log("Containing Count:", result.containingCount);
console.log("Overlapping Count:", result.overlappingCount);

