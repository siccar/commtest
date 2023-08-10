# CommTest

A suite of test to confirm commisioning of a Siccar Installation

## Requirements

You will need to install the SiccarCmd dotnet tooling addin, this should be configured in the git cloned directory but will need to be restored:

    > cd commtest/
    > dotnet siccarcmd 

It may be useful if you are a developer or work with mulitple installations to be aware of the use of system defaults, these can be found in most cases you can over ride settings in these files  

    > cd ~/.siccar
    > ls
        Directory: C:\Users\<%user%>\.siccar

    Mode                 LastWriteTime         Length Name
    ----                 -------------         ------ ----
    -a---          24/07/2023    16:52            538 appsettings.json
    -a---          20/07/2023    13:56           2830 auth.cache

The auth.cache file is a store JWT if using the CLI 'dotnet siccarcmd auth login' user authentication flow.

## Basic Ping Pong Test

The ping pong test excercises the base functions of a Siccar installation. It does this by:
- creating two wallets, 
- publishes a blueprint which is just a circular two step blueprint
- sends a random true/false field from Participant 1 to Particiapnt 2 and back again.

It can be configured to carry diffferent payload sizes or loops etc but a standard run will confirm correct commisioning of an installation. 

There are various parameters - most commonly used is '-r' '--register' to reuse a register already created on the system.

a typical run should look like this :

    PS C:\projects\siccar\commtest\bin\Debug\net7.0> dotnet siccarcmd auth login
    Logged in, access cached
    PS C:\projects\siccar\commtest\bin\Debug\net7.0> .\CommTest.exe pingpong
    Siccar System Checker v2.0
    Examining configuration and connectivity
    Testing against installation : https://localhost:8443/
    Authenticating using Client Credentials
    Using Token : eyJhbGciOiJSUzI1NiIsImtpZCI6IkEyNkI4RERFMkQ3NkVGN0FCQzc1MzA2QkI1QkJGQUQwNjA0N0NBRDFSUzI1NiIsInR5cCI6ImF0K2p3dCIsIng1dCI6Im9tdU4zaTEyNzNxOGRUQnJ0YnY2MEdCSHl0RSJ9.eyJuYmYiOjE2OTE2NzAzNjcsImV4cCI6MTY5MTY3Mzk2NywiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6ODQ0MyIsImNsaWVudF9pZCI6InNpY2Nhci1hZG1pbi11aS1jbGllbnQiLCJyb2xlIjpbImJsdWVwcmludC5hZG1pbiIsImJsdWVwcmludC5wdWJsaXNoIiwiYmx1ZXByaW50LmF1dGhvcmlzZXIiLCJpbnN0YWxsYXRpb24uYWRtaW4iLCJpbnN0YWxsYXRpb24ucmVhZGVyIiwiaW5zdGFsbGF0aW9uLmJpbGxpbmciLCJyZWdpc3Rlci5jcmVhdG9yIiwicmVnaXN0ZXIubWFpbnRhaW5lciIsInJlZ2lzdGVyLmFkbWluIiwicmVnaXN0ZXIucmVhZGVyIiwidGVuYW50LmJpbGxpbmciLCJ0ZW5hbnQuYWRtaW4iLCJ0ZW5hbnQuYXBwLmFkbWluIiwid2FsbGV0LnVzZXIiLCJ3YWxsZXQub3duZXIiLCJ3YWxsZXQuZGVsZWdhdGUiXSwidGVuYW50IjoiMGFmZWJiMGEtZjNmOS00OGQyLTlhZTAtMjkzNzU1ZTg4M2JkIiwic3ViIjoic2ljY2FyLWFkbWluLXVpLWNsaWVudCIsImp0aSI6IjY4RUIzMzRFRjNCRjM5MDgxRUQwQTU1RTg4QURGRTI5IiwiaWF0IjoxNjkxNjcwMzY3LCJzY29wZSI6WyJibHVlcHJpbnQuYWRtaW4iLCJyZWdpc3Rlci5tYWludGFpbmVyIiwidGVuYW50LmJpbGxpbmciLCJ3YWxsZXQudXNlciJdfQ.fs506U1kvgdG85uR4hpz3-4BchjqWLwZD453q3giBErZyRLPzBXYLxf4wq--PndQ9ODkfwaiaw-kL-hFSDymFS2-Q_X5FP1D4NAS0SfBvuBK0zqJPH9jN00ESdpAMHYuca8sX9MN32lGBtI2WMr0Kpr-ivHmmjokcvNoZn7qhdIMMRamSMEKVbfUR4fBPP1SXYhc_s9xRCkvEla7r7pl713oCBwBjM2rxAXZ3ICQbAAuXnBKTsxGdqLOPpgKvUN3MUNlexgB0q6A40GP95lN_iKNK85_TXJfS8cd5ZSUaMLao6WblKKQagzCVCyEHn1ATNe7agsw-gmKQTTSSwMiWg
    Running PingPong test...
         Test loops : 10
         Starting payload is 0 characters
         Increasing the payload by 0 characters
    Initialising Register:
    Created new Register : 20b6f21e3744440eba34b0d07f463214
    Initialising Wallets:
    Created new Wallet[0] : ws1jjzzrxkx96d38cwha4ptvunmy9x09msnpftkjm5dggm75w6phaj0qmkzjn0
    Created new Wallet[1] : ws1jdfr83j0d2qjk4wj4m7lq8wju594vwe4ppau39q90n03cm7zh5nvs3gq7wa
    Blueprint created and published, TXId  : fbfe55369fd6763dfbdb119f9fd6f78641a00ac00613460c09a6fc182b023b6f
    Getting starting action:....................
        Confirmed Start Action : P1 sends to P2
    Sending Inital Action Ping Action on TxId : ba4c7836f44375ee4d0933c97a5a4c5a704e184bae6602ac12b1f9270754c6ce
     Processed 1 Action Pong Action on TxId : 3153bc89a5f25f614e87adca65faffe970d78a8e90d2d1cadaef92f0795dbc5e in : GetAction 545.7248 ms: SubmitAction 793 ms @ 1:26:31
     Processed 2 Action Pong Action on TxId : 7cf16e4cac5110c6c5ab322eb81daf6a72561689f791b6fc3e21928022f5cf64 in : GetAction 374.0138 ms: SubmitAction 576 ms @ 1:26:41
     Processed 3 Action Pong Action on TxId : 2a641bf113e36b8799aa00aace3728353fbc8217be3a4c025b7c46619270f55a in : GetAction 266.3791 ms: SubmitAction 437 ms @ 1:26:50
     Processed 4 Action Pong Action on TxId : f7169f49570e9f7f56df37e381c9dc359cee7f5bae986b744139f03ad1981e1c in : GetAction 295.4964 ms: SubmitAction 805 ms @ 1:27:01
     Processed 5 Action Pong Action on TxId : 2c7e7fa70ed833d1e920428a949cce8a6750763cc055b98e65dd8b2af7cea797 in : GetAction 367.237 ms: SubmitAction 666 ms @ 1:27:11
     Processed 6 Action Pong Action on TxId : 9536db4b59f2207b066c1548cfac9e713407b555b387a3532112226d928bed6e in : GetAction 278.1041 ms: SubmitAction 441 ms @ 1:27:20
     Processed 7 Action Pong Action on TxId : 128f6358181fb51f47a378bd0e60cea6f872a91501dc538533e68499430a3638 in : GetAction 274.6863 ms: SubmitAction 522 ms @ 1:27:30
     Processed 8 Action Pong Action on TxId : be5f9eec672153077bc5ad8bb823c250cb852dfa0ce9ac7c954090edf12bcd1b in : GetAction 287.3336 ms: SubmitAction 538 ms @ 1:27:41
     Processed 9 Action Pong Action on TxId : 14d89a45cc354196b17a667e9bb0a4206f43b2876af9bae9a72103ccf9040656 in : GetAction 251.1308 ms: SubmitAction 557 ms @ 1:27:51
     Processed 10 Action Pong Action on TxId : 4e4d1164495a13d15ad32164fc88b8f13f0de56790caaa4736f951cb0170ea3d in : GetAction 324.0367 ms: SubmitAction 572 ms @ 1:28:01
         PingPong tests completed in 00:01:52.9419543

    PS C:\projects\siccar\commtest\bin\Debug\net7.0

Any error due to authentication or unable to submit requests will error locally, where there are timeouts or service Authorisation errors then the appropriate service log should be consulted to find the nature of the error. 