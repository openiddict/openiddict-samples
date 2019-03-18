// +++++++++++++++++++++++++++++++++++++++++
// The signing keys come from Azure Active Directory key discovery:
// https://login.windows.net/common/discovery/keys

// Update this file with new keys if the console shows this error: 
// "No key matching kid or alg found in signing keys"

// This is a TypeScript file instead of a JSON file
// because the Aurelia CLI's default project requires
// additional configuration to load JSON files.
// +++++++++++++++++++++++++++++++++++++++++

export default {
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "2KVcuzqAidOLqWSaol7wgFRGCYo",
      "x5t": "2KVcuzqAidOLqWSaol7wgFRGCYo",
      "n": "40MK4ih03cjonv5Zz2PmxjkyAuQZlm5TEsCkcSYiGBYhVJLIyAz567Q2uvkW4jKUmsqD9Ic4l4vAW5hk4Qx9FRVwpF7BRMgEqYguqWDn53nrO1hkBO6GbrQHlunVFSVRAxnQZN6nP3GlL2E7gy_kZEHFHnGgEoI4XvjF9W4c2ST_CdtX9iCDC3zYWsabwKmJeNuYXPLrVWanopsUNp0kOKPaaYgJLDMAkShW-SUvNwv_hV_Te_eXxoGQj9I98OObqTnl2p4Ob6cQg39tpZuzszZa02Qlc14_Lx1HQaR2WFuARIQgl1JUtZ4EW3x5XQlCpRdw8KOHCkkTz2OseHDIIQ",
      "e": "AQAB",
      "x5c": [
        "MIIDBTCCAe2gAwIBAgIQQm0sN9lDrblM/7U/vYMVmTANBgkqhkiG9w0BAQsFADAtMSswKQYDVQQDEyJhY2NvdW50cy5hY2Nlc3Njb250cm9sLndpbmRvd3MubmV0MB4XDTE3MDkwNjAwMDAwMFoXDTE5MDkwNzAwMDAwMFowLTErMCkGA1UEAxMiYWNjb3VudHMuYWNjZXNzY29udHJvbC53aW5kb3dzLm5ldDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAONDCuIodN3I6J7+Wc9j5sY5MgLkGZZuUxLApHEmIhgWIVSSyMgM+eu0Nrr5FuIylJrKg/SHOJeLwFuYZOEMfRUVcKRewUTIBKmILqlg5+d56ztYZATuhm60B5bp1RUlUQMZ0GTepz9xpS9hO4Mv5GRBxR5xoBKCOF74xfVuHNkk/wnbV/Yggwt82FrGm8CpiXjbmFzy61Vmp6KbFDadJDij2mmICSwzAJEoVvklLzcL/4Vf03v3l8aBkI/SPfDjm6k55dqeDm+nEIN/baWbs7M2WtNkJXNePy8dR0GkdlhbgESEIJdSVLWeBFt8eV0JQqUXcPCjhwpJE89jrHhwyCECAwEAAaMhMB8wHQYDVR0OBBYEFNISA3dtAzEd0muqNDbWm3kvNlJDMA0GCSqGSIb3DQEBCwUAA4IBAQClLLoAvg3dYqWO63Z6O5L7yataGcilmL3YUqCFoRKsuwej2T833qyc1iLG0iWCGeWAUonKXuGwfCSSSj2E3ksLtgV6xmuMl+NuVPpRpQo+38n+OxUoWKu963dMxnORFENEqKW0pMioipMk/HBaW3aJWyH1oT2rZ3KhFm67SFjKscF8ShAE82tQQIFwEFAXjMItW2oZVGDz3vDOaJN5xC8rfA6xkXTdcCuzy74SalKkLhpBO8S3XIOBVRZw+l0Koog8YNqhsvGsGS+hGXXNlCZTg0I1tR3g2DcSuHRcuTZKh7Z7XPPsDgleNirtvYFEvdvD4K2I7gb2H1xQn87oYAIX"
      ]
    },
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "2S4SCVGs8Sg9LS6AqLIq6DpW-g8",
      "x5t": "2S4SCVGs8Sg9LS6AqLIq6DpW-g8",
      "n": "oZ-QQrNuB4ei9ATYrT61ebPtvwwYWnsrTpp4ISSp6niZYb92XM0oUTNgqd_C1vGN8J-y9wCbaJWkpBf46CjdZehrqczPhzhHau8WcRXocSB1u_tuZhv1ooAZ4bAcy79UkeLiG60HkuTNJJC8CfaTp1R97szBhuk0Vz5yt4r5SpfewIlBCnZUYwkDS172H9WapQu-3P2Qjh0l-JLyCkdrhvizZUk0atq5_AIDKRU-A0pRGc-EZhUL0LqUMz6c6M2s_4GnQaScv44A5iZUDD15B6e8Apb2yARohkWmOnmRcTVfes8EkfxjzZEzm3cNkvP0ogILyISHKlkzy2OmlU6iXw",
      "e": "AQAB",
      "x5c": [
        "MIIDKDCCAhCgAwIBAgIQBHJvVNxP1oZO4HYKh+rypDANBgkqhkiG9w0BAQsFADAjMSEwHwYDVQQDExhsb2dpbi5taWNyb3NvZnRvbmxpbmUudXMwHhcNMTYxMTE2MDgwMDAwWhcNMTgxMTE2MDgwMDAwWjAjMSEwHwYDVQQDExhsb2dpbi5taWNyb3NvZnRvbmxpbmUudXMwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQChn5BCs24Hh6L0BNitPrV5s+2/DBhaeytOmnghJKnqeJlhv3ZczShRM2Cp38LW8Y3wn7L3AJtolaSkF/joKN1l6GupzM+HOEdq7xZxFehxIHW7+25mG/WigBnhsBzLv1SR4uIbrQeS5M0kkLwJ9pOnVH3uzMGG6TRXPnK3ivlKl97AiUEKdlRjCQNLXvYf1ZqlC77c/ZCOHSX4kvIKR2uG+LNlSTRq2rn8AgMpFT4DSlEZz4RmFQvQupQzPpzozaz/gadBpJy/jgDmJlQMPXkHp7wClvbIBGiGRaY6eZFxNV96zwSR/GPNkTObdw2S8/SiAgvIhIcqWTPLY6aVTqJfAgMBAAGjWDBWMFQGA1UdAQRNMEuAEDUj0BrjP0RTbmoRPTRMY3WhJTAjMSEwHwYDVQQDExhsb2dpbi5taWNyb3NvZnRvbmxpbmUudXOCEARyb1TcT9aGTuB2Cofq8qQwDQYJKoZIhvcNAQELBQADggEBAGnLhDHVz2gLDiu9L34V3ro/6xZDiSWhGyHcGqky7UlzQH3pT5so8iF5P0WzYqVtogPsyC2LPJYSTt2vmQugD4xlu/wbvMFLcV0hmNoTKCF1QTVtEQiAiy0Aq+eoF7Al5fV1S3Sune0uQHimuUFHCmUuF190MLcHcdWnPAmzIc8fv7quRUUsExXmxSX2ktUYQXzqFyIOSnDCuWFm6tpfK5JXS8fW5bpqTlrysXXz/OW/8NFGq/alfjrya4ojrOYLpunGriEtNPwK7hxj1AlCYEWaRHRXaUIW1ByoSff/6Y6+ZhXPUe0cDlNRt/qIz5aflwO7+W8baTS4O8m/icu7ItE="
      ]
    }
  ]
}
