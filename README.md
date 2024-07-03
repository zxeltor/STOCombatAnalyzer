# Star Trek Online Combat Analyzer [![GitHub release (latest by date)](https://img.shields.io/github/v/release/zxeltor/STOCombatAnalyzer)](https://github.com/zxeltor/STOCombatAnalyzer/releases/latest)

* [Overview](#overview)
* [Building](#building)
* [Download](#download)
* [Quick Start](#quick-start)
* [Wiki](#wiki)
* [Disclaimer](#disclaimer)

---
## Overview
This is a PC application used to parse and analyze Star Trek Online combat logs. I became interested in trying out STO random elites, but I was curious if my Space DPS was up to snuff. After a little research, I found a reddit post which had a brief outline of the combat log, and the available fields. I figured this application would give me the opportunity to wrap my head around the combat log and it's fields.

This app displays a breakdown of damage types, using calculations based on the absolute value of the magnitude field from the STO combat log file entries. Calculations for DPS, Total Damage, and Max Damage are displayed. Information like resists and such aren't included.

See the [wiki](https://github.com/zxeltor/STOCombatAnalyzer/wiki) for more details.

---
## Building
The source in this repo is wrapped up in a Visual Studio 2022 solution. You should be able to clone this repo localy, then build and run from inside of Visual Studio.

You could also run the dotnet cli build and run commands from inside the zxeltor.StoCombatAnalyser.Interface project folder as well.

---

## Download
See the [wiki](https://github.com/zxeltor/STOCombatAnalyzer/wiki) for more details.

---

## Quick Start
After you have a successful build, do the following to get started using the application.

- Start the application
- Under the Settings tab, set the following settings:
  - **CombatLogPath**: Set this to match the STO log folder on your local machine.
    - If you click the **select** button, a folder select dialog will appear to help you select a folder.
    - If you click the **detect** button, the application will attempt to get the STO install folder from the windows registry. A dialog box will appear to let you know if it was succesfull, and will update the field with the STO log folder if it was.
  - **CombatLogPathFilePattern**: Set a file pattern used to select one or more combat log files.
    - This search pattern supports wildcards so more then one file can be selected.
  - (Optional) **MyCharacter**: An identifier used to select a player when a new combat instance is chosen.
    - This comes in handy when browsing through multiple combat instances via the Combat List dropdown. For example, I have mine set to @zxeltor so it will pick all of my characters, intead of specific character.
  - (Optional) **HowFarBackForCombat**: How far back in hours to pull combat log data.
  - (Optional) **PurgeCombatLogs**: Enable combat log folder purge at application startup.
    - Note: If only one combat log exists, it won't be purged regardless of how old it is.
  - (Optional) **HowLongToKeepLogs**: How long to keep logs in days, before they are purged.
  - (Optional) **DebugLogging**: Enables debug logging. This also enables/disables a few information dialogs in the UI.
    - **Open Log File**: This button attempts to open the log file for the application in your default text file viewer/editor.

- Switch to the Log File Analyzer tab:
- Click the ![Parse Log(s)](https://github.com/zxeltor/STOCombatAnalyzer/blob/master/zxeltor.StoCombatAnalyser.Interface/Images/glyphicons-82-refresh.png) "Parse Log(s)" button. This parses the STO combat logs. If successful, a dialog will appear and inform you of success, or failure. If a failure occurs, there should be details on what to do.
- After a successfull parse, you can choose a combat instance from the Combat List dropdown.
- If all goes well, you should see something similar to what's displayed in Figure 1.

Figure 1: What the UI looks like after a successfull parse, and a combat instance was selected in the Combat List dropdown.
![The main tab](https://github.com/zxeltor/STOCombatAnalyzer/blob/master/zxeltor.StoCombatAnalyser.Interface/Images/StoCombatAnalyzerScreenShot.jpg)

---

## Wiki
For more detailed information on this application, and release informtation, keep an eye on the [wiki](https://github.com/zxeltor/STOCombatAnalyzer/wiki)

---

## Disclaimer
This software and any related documentation is provided “as is” without warranty of any kind, either express or implied, including, without limitation, the implied warranties of merchantability, fitness for a particular purpose, or non-infringement. Licensee accepts any and all risk arising out of use or performance of Software

**Note:** We are not affiliated, associated, authorized, endorsed by, or in any way officially connected with the game Star Trek Online, or any of its subsidiaries or its affiliates. The official Star Trek Online website can be found at [https://www.playstartrekonline.com/](https://www.playstartrekonline.com/en/)
