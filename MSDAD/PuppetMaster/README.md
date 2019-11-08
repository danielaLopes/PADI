#Instructions on how to run a PuppetMaster
The PuppetMaster.exe file is in bin/Debug/

Run in command line:
PuppetMaster.exe [pcsPoolFile] [configFile]

The config file is where the commands must be, after executing those commands, more commands can be run via command line.
Commands must follow a specific order, first AddRoom's, then Server's, then Client's commands. After that, any command besides those can be ran.