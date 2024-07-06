# VSDebugPro

Enhanced debugging for C/C++, C#/.NET. Dump blocks of data, load data in memory and more.

[![HitCount](https://hits.dwyl.com/ovidiuvio/VSDebugPro.svg?style=flat-square&show=unique)](http://hits.dwyl.com/ovidiuvio/VSDebugPro) [![Build status](https://ci.appveyor.com/api/projects/status/y1b8p5ncabjbv4kn?svg=true)](https://ci.appveyor.com/project/ovidiuvio/vsdebugpro)
<a href="https://raw.githubusercontent.com/ovidiuvio/VSDebugPro/master/LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="License" /></a>

**[www.vsdebug.pro](http://www.vsdebug.pro)**

<img src="/assets/console.png" width="85%"/>

###### Available for:

`VS2010 - VS2022`, `C/C++`, `C#`, `x86/64`, `Arm64/Arm64EC`

###### Features:
---

- Save memory blocks from `Minidumps`
- Supports `remote debugging` sessions
- Works with `Visual Studio` for `ARM`
- Works with ARM programs while debugging
- Works with x64 programs running emulated on `ARM64EC`
- Works with x86/x64 targets
- Compatible with any programming language in Visual Studio that implements the standard debugger interface
- Console commands:

    ```
        help  Provides help information for commands.
       about  Opens the about window.
       alias  Alias allows a more familiar command or name to execute a long string
    settings  Opens product settings dialog.
     dumpmem  Memory dump utility.
     loadmem  Load memory utility.
      memcpy  Memory copy utility.
      memset  Fills a block of memory with a pattern.
        diff  Memory diff.
      malloc  Allocates memory in the process heap.
        free  Free memory allocated with malloc.
        exec  Executes commands from a specified YAML file with Mustache templating.
    ```

 - [Batch commands](https://www.vsdebug.pro/pages/docs/exec.html)

    ```
    exec <yamlFilePath> [arg1] [arg2] ... [argN]
    ```
    
    ```
    variables:
    var1: value1
    var2: value2
    commands:
    - command1 {{var1}} {{var2}}
    - command2 {{var1}} {{var2}}
    ```
- [Memory dump](https://www.vsdebug.pro/pages/docs/dumpmem.html)

    ```
    dumpmem [options] <filename> <address> <size>
    ```

- [Memory write](https://www.vsdebug.pro/pages/docs/loadmem.html)

    ```
    loadmem <file> <address> <size>
    ```

- [Memory copy](https://www.vsdebug.pro/pages/docs/memcpy.html)

    ```
    memcpy <dst> <src> <size>
    ```

- [Write memory with a pattern](https://www.vsdebug.pro/pages/docs/memset.html)

    ```
    memset <dst> <val> <size>
    ```

- Memory diff

    ```
    <diff> <addr1> <addr2> <size>
    ```
