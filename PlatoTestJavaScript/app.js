const readline = require('node:readline');
const { stdin: input, stdout: output } = require('node:process');

const rl = readline.createInterface({ input, output });

rl.question('What do you think of Node.js? ', (answer) =>
{
    console.log(`Thank you for your valuable feedback: ${answer}`);
    rl.close();
});