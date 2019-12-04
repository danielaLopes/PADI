#Instructions on how to run a Client
The Client.exe file is in bin/Debug/

Run in command line:
Client.exe <username> <clientUrl> <serverUrl> <scriptFile> <nBackupServers> <backupServers> <otherClientsUrls>

We assume the client will receive f backup server urls so that the system can tolerate f faults.

Then it'll run the commands in scriptFile and after that, more commands can be inserted via command line.
