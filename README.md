# Star Trek Online Combat Analyzer [![GitHub release (latest by date)](https://img.shields.io/github/v/release/zxeltor/STOCombatAnalyzer)](https://github.com/zxeltor/STOCombatAnalyzer/releases/latest)

* [Overview](#overview)
* [Building](#building)
* [Download](#download)
* [Screen Shots](#screen-shots)
* [Wiki](#wiki)
* [Disclaimer](#disclaimer)

---
## Overview
This is a PC application used to parse and analyze Star Trek Online combat logs. I became interested in trying out STO random elites, but I was curious if my Space DPS was up to snuff. After a little research, I found a reddit post which had a brief outline of the combat log, and the available fields. I figured this application would give me the opportunity to wrap my head around the combat log and it's fields.

This app displays a breakdown of various damage/ability [metrics](https://github.com/zxeltor/STOCombatAnalyzer/wiki/Player-Metrics).

See the [wiki](https://github.com/zxeltor/STOCombatAnalyzer/wiki) for more details.

This is a companion project of [STOCombatRealtime](https://github.com/zxeltor/zxeltor.StoCombat.Realtime)). STOCombatRealtime provides a real-time combat data overlay for STO.

---
## Building
The source in this repo is wrapped up in a Visual Studio 2022 solution. You should be able to clone this repo localy, then build and run from inside of Visual Studio.

You could also run the dotnet cli build and run commands from inside the zxeltor.StoCombatAnalyser.Interface project folder as well.

---

## Download
See the [wiki](https://github.com/zxeltor/STOCombatAnalyzer/wiki#installer) for more details.

---

## Screen Shots
Figure 1: What the UI looks like after a successfull parse, and a combat instance was selected in the Combat List dropdown.
![The main tab](https://github.com/zxeltor/STOCombatAnalyzer/blob/master/zxeltor.StoCombat.Analyser/Images/StoCombatAnalyzerScreenShot.jpg)

---

## Wiki
For more detailed information on this application, and release informtation, keep an eye on the [wiki](https://github.com/zxeltor/STOCombatAnalyzer/wiki)

---

## Disclaimer
This software and any related documentation is provided “as is” without warranty of any kind, either express or implied, including, without limitation, the implied warranties of merchantability, fitness for a particular purpose, or non-infringement. Licensee accepts any and all risk arising out of use or performance of Software

**Note:** We are not affiliated, associated, authorized, endorsed by, or in any way officially connected with the game Star Trek Online, or any of its subsidiaries or its affiliates. The official Star Trek Online website can be found at [https://www.playstartrekonline.com/](https://www.playstartrekonline.com/en/)
