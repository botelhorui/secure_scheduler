The user can create and remove calendars, to which events can be added, removed, edited also and can share calendars with other users.

The threat analsys STRIDE: it is possible spoofing a user/server identity; Tampering of calendar data in a server; Repudiation of the application operations, most importantly, the confirmation of a shared scheduled event; Calendar data privacy breach. 

Building a secure system depends on the security goals, on our specific system the privacy of users calendar data was the leading goal. For this reason we choose to separate the data manipulation from the data storage, by designing a client application that only handles private data, and a server application that only handles public data and stores encrypted data. After arriving at a solution design we started by trying to implement the system as a web application, but it turned out to be unfeasible doing cryptography in Javascript. We then focused on using Windows Crypto API which turned out to be good enough.

Check the report.pdf, UML.PNG and ClientServerRunningPrintscreen.png

Used: C#, .NET Framework, Microsoft Visual Studio, WCF, STRIDE ANALYSIS, OPENSSL, RSA, X509, TLS 1.0, ECDH, SHA1, AES, PBKDF2
