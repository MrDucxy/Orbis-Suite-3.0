LoaderShellCode = ../Kernel/Resources/LoaderShellCode.bin
LoaderShellCodes = LoaderShellCode.s
UnloaderShellCode = ../Kernel/Resources/UnLoaderShellCode.bin
UnloaderShellCodes = UnLoaderShellCode.s

all: clean $(LoaderShellCode)
all: clean $(UnloaderShellCode)

$(LoaderShellCode):
	nasm -f bin -o $(LoaderShellCode) $(LoaderShellCodes)

$(UnloaderShellCode):
	nasm -f bin -o $(UnloaderShellCode) $(UnloaderShellCodes)
	
.PHONY: clean
clean:
	rm -f $(LoaderShellCode) 
	rm -f $(UnloaderShellCode) 