#pragma once

enum SceNotificationRequestType
{
	NotificationRequest = 0,
	SystemNotification = 1,
	SystemNotificationWithUserId = 2,
	SystemNotificationWithDeviceId = 3,
	SystemNotificationWithDeviceIdRelatedToUser = 4,
	SystemNotificationWithText = 5,
	SystemNotificationWithTextRelatedToUser = 6,
	SystemNotificationWithErrorCode = 7,
	SystemNotificationWithAppId = 8,
	SystemNotificationWithAppName = 9,
	SystemNotificationWithAppInfo = 9,
	SystemNotificationWithAppNameRelatedToUser = 10,
	SystemNotificationWithParams = 11,
	SendSystemNotificationWithUserName = 12,
	SystemNotificationWithUserNameInfo = 13,
	SendAddressingSystemNotification = 14,
	AddressingSystemNotificationWithDeviceId = 15,
	AddressingSystemNotificationWithUserName = 16,
	AddressingSystemNotificationWithUserId = 17,

	UNK_1 = 100,
	TrcCheckNotificationRequest = 101,
	NpDebugNotificationRequest = 102,
	UNK_2 = 102,
};

typedef struct
{
	enum SceNotificationRequestType type;
	int reqId;
	int priority;
	int msgId;
	int targetId;
	int userId;
	int unk1;
	int unk2;
	int appId;
	int errorNum;
	int unk3;
	unsigned char useIconImageUri;
	char message[1024];
	char iconUri[1024];
	char unk[1024];
} SceNotificationRequest;

extern int(*sceKernelDebugOutText)(int dbg_channel, const char* text, ...);
extern int(*sceKernelSendNotificationRequest)(int device, SceNotificationRequest* req, size_t size, int blocking);
extern int(*sceKernelGetModuleInfo)(SceKernelModule handle, SceDbgModuleInfo* info);

bool LoadModules();
void klog(const char* fmt, ...);
void Notify(const char* MessageFMT, ...);
void NotifyCustom(const char* IconURI, const char* MessageFMT, ...);
bool CopySflash();