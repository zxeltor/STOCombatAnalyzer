# Star Trek Online Combat Analyzer ![Latest application version](https://img.shields.io/badge/beta.2-v1.0.0-blue)

#### License and Disclaimer
We are not affiliated, associated, authorized, endorsed by, or in any way officially connected with the game Star Trek Online, or any of its subsidiaries or its affiliates. The official Star Trek Online website can be found at [https://www.playstartrekonline.com/](https://www.playstartrekonline.com/en/)

Please see license info [here](https://github.com/zxeltor/STOCombatAnalyzer?tab=Apache-2.0-1-ov-file).

#### Revision History and Updates
Please more information please visit the [WIKI](https://github.com/zxeltor/STOCombatAnalyzer/wiki)

## Overview
This is a PC application used to parse and analyze Star Trek Online combat logs. I became interested in trying out STO random elites, but I was curious if my Space DPS was up to snuff. After a little research, I found a reddit post which had a brief outline of the combat log, and the available fields. I figured doing a real-time DPS analyzer would be futile. until I could wrap my head around the combat log an it's fields.

## Building
The source in this repo is wrapped up in a Visual Studio 2022 solution. You should be able to clone this repo localy, then build and run from inside of Visual Studio.

You could also run the dotnet cli build and run commands from inside the zxeltor.StoCombatAnalyser.Interface project folder as well.

## Release and package installer
I've been playing with a ClickOnce installer. You can download and install the latest from this link: [https://starfleet.engineer/StoCombatAnalyzer/Publish.html](https://starfleet.engineer/StoCombatAnalyzer/Publish.html)
Note: The app and installer are signed with a localy generated certificate. Since it's not from a known publisher, you'll get warnings during the install process about it coming from an untrusted publisher.

## Quick Start
After you have a successful build, do the following to get started using the application.

- Start the application
- Under the Settings tab, set the following settings:
  - **CombatLogPath**: Set this to match the STO log folder on your local machine.
    - If you click the **select** button, a folder select dialog will appear to help you select a folder.
    - If you click the **detect** button, the application will attempt to get the STO install folder from the windows registry. A dialog box will appear to let you know if it was succesfull, and will update the field with the STO log folder if it was.
  - **CombatLogPathFilePattern**: Set a file pattern used to select one or more combat log files.
    - This search pattern supports wildcards so more then one file can be selected.
  - (Optional) **MaxNumberOfCombatsToDisplay** You can set the number of combat instances to display in the UI after you parse the logs.
    - If you set this to 0 or less, it will display all combat instances

![Settings Tab](zxeltor.StoCombatAnalyser.Interface/Images/StoCombatAnalyzerScreenShot_Settings.jpg)
- Switch to the Log File Analyzer tab:
- Click the **Parse Log(s)** button. If successful, you should see something similar to the following.
  - A dialog will appear and inform you of success, or failure. If a failure occurs, there should be details on what to do.

![Main UI](zxeltor.StoCombatAnalyser.Interface/Images/StoCombatAnalyzerScreenShot.jpg)
