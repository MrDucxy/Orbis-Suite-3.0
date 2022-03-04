CC        	:=    clang++
AS        	:=    clang++
OBJCOPY   	:=    objcopy
ODIR    	:=    build
SDIR    	:=    source
RDIR	    :=    Resources
IDIRS    	:=    -Iinclude -I$(FREEBSD_INCLUDES) -I../../../Misc
CFLAGS    	:=    $(IDIRS) -O3 -s -w -std=gnu++11 -ffunction-sections -fdata-sections -fno-builtin -fno-exceptions -fno-asynchronous-unwind-tables -nostartfiles -nostdlib -w -masm=intel -march=btver2 -mtune=btver2 -m64 -mabi=sysv -mcmodel=small -mstackrealign -fPIE -DVERSION_$(VERSION)
LFLAGS    	:=    -Xlinker -T Linker -Wl,--build-id=none -mstackrealign -pie -Wl,--gc-sections
SFLAGS  	:=    -nostartfiles -nostdlib -masm=intel -march=btver2 -mtune=btver2 -m64 -mabi=sysv -mcmodel=small
CFILES    	:=    $(shell find $(SDIR) -name \*.cpp)
SFILES    	:=    $(shell find $(SDIR) -name \*.s)
RFILES      :=    $(shell find $(RDIR) -name \*.bin)
OBJS	    :=	  $(patsubst $(SDIR)/%.c, $(ODIR)/%.o, $(CFILES)) $(patsubst $(SDIR)/%.s, $(ODIR)/%.o, $(SFILES)) $(patsubst $(RDIR)/%.bin, $(ODIR)/%.o, $(RFILES))

TARGET = $(shell basename "$(CURDIR)").elf

$(TARGET): $(ODIR) $(OBJS)
	$(CC) crt0.s $(OBJS) -o $(TARGET) $(CFLAGS) $(LFLAGS)

$(ODIR)/%.o: $(SDIR)/%.cpp
	mkdir -p $(dir $@)
	$(CC) -c -o $@ $< $(CFLAGS)

$(ODIR)/%.o: $(SDIR)/%.s
	$(AS) -c -o $@ $< $(SFLAGS)

$(ODIR)/%.o: $(RDIR)/%.bin
	mkdir -p $(dir $@)
	ld -r -b binary -o $@ $<
#	objdump -t $@

$(ODIR):
	@mkdir $@

.PHONY: copy
.PHONY: clean

clean:
	rm -f $(TARGET) $(ODIR)/*.o

copy:
	cp $(shell basename "$(CURDIR)").elf ../../Build/pkg/Kernel/$(shell basename "$(CURDIR)").$(VERSION).ELF

	echo open 192.168.0.55 2121> temp.txt
	echo user anonymous anonymous>> temp.txt
	echo put \"$(shell pwd)/$(shell basename "$(CURDIR)").elf\" \"/data/Orbis Suite/$(shell basename "$(CURDIR)").$(VERSION).ELF\">> temp.txt
	echo bye>> temp.txt

	ftp -n < temp.txt