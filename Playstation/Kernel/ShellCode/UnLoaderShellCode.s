BITS 64
DEFAULT REL

magic: db 'SHEL'
entry: dq shellcode

thr_initial: dq 0
ShellCodeComplete: db 0
ShouldCallExit: db 0

ModuleHandle: dq 0 
Result: dq 0

; Addresses / Misc
sceKernelUsleep: dq 0
asceKernelStopUnloadModule: dq 0
libkernel: dq 0
str_libkernel: db 'libkernel.sprx', 0
str_libkernelweb: db 'libkernel_web.sprx', 0
str_libkernelsys: db 'libkernel_sys.sprx', 0
str_sceKernelSleep: db 'sceKernelUsleep', 0
str_sceKernelStopUnloadModule: db 'sceKernelStopUnloadModule', 0

str_scePthreadCreate: db 'scePthreadCreate', 0
scePthreadCreate: dq 0
str_scePthreadAttrSetstacksize: db 'scePthreadAttrSetstacksize', 0
scePthreadAttrSetstacksize: dq 0
str_scePthreadAttrInit: db 'scePthreadAttrInit', 0
scePthreadAttrInit: dq 0

hthread: dq 0
scePthreadAttr: dq 0
str_threadName: db 'Orbis SPRX UnLoader', 0

; Work around for oosdk
amodule_stop: dq 0
str_module_stop: db 'module_stop', 0

; Main shellcode function.
shellcode:
	; load thread into fs
	mov rdi, qword [thr_initial]
	mov rsi, qword [rdi]
	mov rdi, qword [rsi + 0x1E0]
	call amd64_set_fsbase

    ; get libkernel handle
	mov rcx, 0
	lea rdx, [libkernel]
	mov rsi, 0
	lea rdi, [str_libkernel]
	call sys_dynlib_load_prx
	test rax, rax
	je resolve

	mov rcx, 0
	lea rdx, [libkernel]
	mov rsi, 0
	lea rdi, [str_libkernelweb]
	call sys_dynlib_load_prx
	test rax, rax
	je resolve

	mov rcx, 0
	lea rdx, [libkernel]
	mov rsi, 0
	lea rdi, [str_libkernelsys]
	call sys_dynlib_load_prx

resolve:
	; resolve sceKernelUsleep
	lea rdx, [sceKernelUsleep]
	lea rsi, [str_sceKernelSleep]
	mov rdi, qword [libkernel]
	call sys_dynlib_dlsym

	; resolve sceKernelStopUnloadModule
	lea rdx, [asceKernelStopUnloadModule]
	lea rsi, [str_sceKernelStopUnloadModule]
	mov rdi, qword [libkernel]
	call sys_dynlib_dlsym

	; resolve scePthreadCreate
	lea rdx, [scePthreadCreate]
	lea rsi, [str_scePthreadCreate]
	mov rdi, qword [libkernel]
	call sys_dynlib_dlsym

    ; resolve scePthreadAttrSetstacksize
	lea rdx, [scePthreadAttrSetstacksize]
	lea rsi, [str_scePthreadAttrSetstacksize]
	mov rdi, qword [libkernel]
	call sys_dynlib_dlsym

    ; resolve scePthreadAttrInit
	lea rdx, [scePthreadAttrInit]
	lea rsi, [str_scePthreadAttrInit]
	mov rdi, qword [libkernel]
	call sys_dynlib_dlsym

	call StopModule
    call sceKernelStopUnloadModule

	; Set the Flag we are done.
    mov byte [ShellCodeComplete], 1

	; Shutdown the thread.
	mov rdi, 0
	call sys_thr_exit
	retn

StopModule:
    ; Check to see if we shouldnt call the exit manually.
	cmp byte[ShouldCallExit], 0
	je EndofModuleStop

    ; Manually calling Exit.
    lea rdx, [amodule_stop]
	lea rsi, [str_module_stop]
	mov rdi, qword [ModuleHandle]
	call sys_dynlib_dlsym
	
	cmp dword[amodule_stop], 0
	jle EndofModuleStop

	mov r12, qword [amodule_stop]
	call r12

EndofModuleStop:
    xor eax, eax
	retn

; Sub function
sceKernelStopUnloadModule:
	xor r9, r9
    xor r8, r8
    xor rcx, rcx
    xor rdx, rdx
    xor rsi, rsi
	mov rdi, [ModuleHandle]
	mov r12, qword [asceKernelStopUnloadModule]
	call r12
	mov qword [Result], rax
	xor eax, eax
	retn

; Sub function
sys_dynlib_load_prx:
	mov rax, 594
	mov r10, rcx
	syscall
	retn

; Sub function
sys_dynlib_dlsym:
	mov rax, 591
	mov r10, rcx
	syscall
	retn

; Sub function
sys_thr_exit:
	mov rax, 431
	mov r10, rcx
	syscall
	retn

; Sub function
sys_sysarch:
	mov rax, 165
	mov r10, rcx
	syscall
	retn

; Sub function
amd64_set_fsbase:
	push rbp
	mov rbp, rsp
	push rbx
	sub rsp, 0x18

	mov [rbp - 0x18], rdi

	lea rsi, [rbp - 0x18]
	mov edi, 129
	call sys_sysarch

	add rsp, 0x18
	pop rbx
	pop rbp
	retn