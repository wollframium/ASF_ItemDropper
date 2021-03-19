# ASF Item Dropper

# DISCLAIMER

This plugin is provided on AS-IS basis, without any guarantee at all. Author is not responsible for any harm, direct or indirect, that may be caused by using this plugin. You use this plugin at your own risk.

## Introduction

This plugin for [ASF](https://github.com/JustArchiNET/ArchiSteamFarm/) allows you to trigger ingame item drops for idled games. Works only with ASF v4.0+ (make sure to check actual required version in release notes).

## Installation

- download .zip file from [latest release](https://github.com/webben-de/ASF_ItemDropper/releases/latest), in most cases you need `ASF-ItemDropper.zip`, but if you use ASF-generic-netf.zip (you really need a strong reason to do that) download `ASF-ItemDropper-netf.zip`.
- unpack downloaded .zip file to `plugins` folder inside your ASF folder.
- (re)start ASF, you should get a message indicating that plugin loaded successfully.

## Usage

After installation, you can use those commands (only for accounts with Master+ permissions):

### `istart <bots> <appids>`

### `istop`

### `idrop <bots> <appid> <drop definition list>`

Triggers an item drop on single or on multiple bots.

- `idrop bot1,bot2,bot3 987654 12345` triggers drops on the given bot names
- `idrop bot1..bot3 987654 12345` triggers drops on the given bot names
- `idrop bot1 987654 12345` triggers drop on the given single bot
- `idrop 987654 12345` triggers drop on the bot, where the command was sent to

### `idrops <bots> <appid> <drop definition list>`

Same syntax and same function as **IDROP** command only less output for smaller displays like mobile phones or tablets.

### `idroppt <bots> <appid>`

Displays the overall playtime of a game on single or on multiple bots.


- `idroppt bot1,bot2,bot3 987654` display playtime of the given bot names
- `idroppt bot1..bot3 987654` display playtime of the given bot names
- `idroppt bot1 987654` display playtime of a single bot
- `idroppt 987654` display playtime of the bot, where the command was sent to

### `idropdeflist`

Command to read and print content of file **idropdeflist.txt**, which must be located in same directory as the plugin DLL itself.
