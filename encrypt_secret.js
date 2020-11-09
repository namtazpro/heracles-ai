var myArgs = process.argv.slice(2);
const sodium = require('tweetsodium')
const value = myArgs[0];
console.log(value);
const key =  myArgs[1];
console.log(key;
const keyBytes = Buffer.from(key, 'base64');
const messageBytes = Buffer.from(value);
const encryptedBytes = sodium.seal(messageBytes, keyBytes);
const encrypted = Buffer.from(encryptedBytes).toString('base64')
console.log(encrypted);
