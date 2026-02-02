const crypto = require('crypto');

const teamId = 'W9934B29TZ';
const clientId = 'com.stg.haircuthistory.service';
const keyId = '8K6HSV953M';
const privateKey = `-----BEGIN PRIVATE KEY-----
MIGTAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBHkwdwIBAQQgIOwL9h5SSiz7DygB
F4PZfwrmbP8WZVWgL1ltvITLs0igCgYIKoZIzj0DAQehRANCAAQvH1AQmYxKBcIa
2UlYM1OwGElp1Rt/0iRId2y8nl9H7MqfHaCTBkrIbY0RfKTbLkrHgag+EHkODcCx
ZUb0JZWD
-----END PRIVATE KEY-----`;

// JWT Header
const header = {
  alg: 'ES256',
  kid: keyId,
  typ: 'JWT'
};

// JWT Payload (valid for 6 months)
const now = Math.floor(Date.now() / 1000);
const payload = {
  iss: teamId,
  iat: now,
  exp: now + (86400 * 180), // 180 days
  aud: 'https://appleid.apple.com',
  sub: clientId
};

function base64url(data) {
  return Buffer.from(JSON.stringify(data))
    .toString('base64')
    .replace(/=/g, '')
    .replace(/\+/g, '-')
    .replace(/\//g, '_');
}

const headerB64 = base64url(header);
const payloadB64 = base64url(payload);
const signatureInput = `${headerB64}.${payloadB64}`;

const sign = crypto.createSign('SHA256');
sign.update(signatureInput);
const signature = sign.sign(privateKey, 'base64')
  .replace(/=/g, '')
  .replace(/\+/g, '-')
  .replace(/\//g, '_');

const jwt = `${signatureInput}.${signature}`;
console.log('Apple Client Secret (valid for 180 days):');
console.log(jwt);
