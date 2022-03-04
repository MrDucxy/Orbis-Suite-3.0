SHELL=/bin/bash

none:
	printf "\e[1m\e[31mPlease Choose a firmware!! EG. \"make 505\" \033[37m\n"

build:
	cmd.exe /C Increment.bat
	
	+$(MAKE) -C ShellCode clean
	+$(MAKE) -C ShellCode
	+$(MAKE) -C Kernel clean
	+$(MAKE) -C Kernel
	+$(MAKE) -C Kernel copy
	printf "\e[1m\e[32m[%0.2f Build Sucess!]\033[37m\n" "$$(($(VERSION) * 1))e-2"

505:
	+$(MAKE) build VERSION=505

672:
	+$(MAKE) build VERSION=672

702:
	+$(MAKE) build VERSION=702

755:
	+$(MAKE) build VERSION=755

900:
	+$(MAKE) build VERSION=900

all:
	+$(MAKE) build VERSION=505
	+$(MAKE) build VERSION=672
	+$(MAKE) build VERSION=702
	+$(MAKE) build VERSION=755
	+$(MAKE) build VERSION=900

	@/bin/echo -e "\e[1m\e[32m[All Build Sucess!]\033[37m"

clean:
	+$(MAKE) -C ShellCode clean
	+$(MAKE) -C Kernel clean
