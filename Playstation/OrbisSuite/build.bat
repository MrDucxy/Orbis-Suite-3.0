SETLOCAL EnableDelayedExpansion

Rem Libraries to link in
set libraries=-lc++ -lc -lSceSysModule -lkernel -lSceVideoOut -lSceSystemService -lSceSysCore -lSceSystemStateMgr -lSceNet -lScePad -lSceUserService -lSceRegMgr -lSceFreeType -lSceMsgDialog -lSceCommonDialog -lGoldHEN_Hook -lSceLncUtil

Rem Read the script arguments into local vars
set intdir=%1
set targetname=%~2
set outputPath=%3

set outputElf=%intdir%%targetname%.elf
set outputOelf=%intdir%%targetname%.oelf

Rem Compile object files for all the source files
for %%f in (*.cpp) do (
    clang++ -cc1 -triple x86_64-scei-ps4-elf -I"%OO_PS4_TOOLCHAIN%\\include" -I"%OO_PS4_TOOLCHAIN%\\include\\c++\\v1" -I"..\\..\\External\\GoldHEN_Plugins_SDK\\include" -I"..\\..\\External\\ps4-libjbc" -DORBISLIB_DEBUG -emit-obj -o %intdir%\%%~nf.o %%~nf.cpp
)

Rem Get a list of object files for linking
set obj_files=
for %%f in (%1\\*.o) do set obj_files=!obj_files! .\%%f

Rem Link the input ELF
ld.lld -m elf_x86_64 -pie --script "%OO_PS4_TOOLCHAIN%\link.x" --eh-frame-hdr -o "%outputElf%" "-L%OO_PS4_TOOLCHAIN%\\lib" "-L..\\..\\External\\GoldHEN_Plugins_SDK" %libraries% --verbose "%OO_PS4_TOOLCHAIN%\lib\crt1.o" %obj_files% "..\\..\\External\\ps4-libjbc\\jbc.o"

Rem Create the eboot
%OO_PS4_TOOLCHAIN%\bin\windows\create-fself.exe -in "%outputElf%" --out "%outputOelf%" --eboot "eboot.bin"

Rem Cleanup
copy "eboot.bin" %outputPath%\Playstation\Build\pkg\eboot.bin
del "eboot.bin"

REM Generate the script. Will overwrite any existing temp.txt
REM echo open 192.168.0.55 1337> temp.txt
REM echo open 192.168.0.55 2121> temp.txt
REM echo anonymous>> temp.txt
REM echo anonymous>> temp.txt
REM echo cd "/data/app/">> temp.txt
REM echo send "%outputPath%\Playstation\Build\pkg\eboot.bin">> temp.txt
REM echo quit>> temp.txt
REM 
REM REM Launch FTP and pass it the script
REM ftp -s:temp.txt
REM 
REM REM Clean up.
REM del temp.txt