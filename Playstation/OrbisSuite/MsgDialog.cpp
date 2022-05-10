#include "Common.h"
#include "MsgDialog.h"

#include <orbis/MsgDialog.h>

/*
	|========================= Dialog =========================|
	|	Made with love by OSM / Greg :)						   |
	|														   |
	|	TODO:												   |
	|		- Add Call back functionality for non blocking	   |
	|														   |
	|==========================================================|
*/

OrbisMsgDialogResult MsgDialog::DialogResult;

bool MsgDialog::DialogRunning;

OrbisMsgDialogParam DialogParam;
OrbisMsgDialogUserMessageParam UserMsgParam;
OrbisMsgDialogButtonsParam ButtonsParam;

void MsgDialog::Open(const char* Msg, bool Blocking, OrbisMsgDialogButtonType ButtonType, OrbisMsgDialogMode Mode, const char* Button1, const char* Button2)
{
	//klog("MsgDialog::Open()\n");

	OrbisUserServiceUserId userID;
	OrbisUserServiceInitializeParams param;
	param.priority = ORBIS_KERNEL_PRIO_FIFO_LOWEST;
	sceUserServiceInitialize(&param);
	sceUserServiceGetInitialUser(&userID);

	// Init
	sceMsgDialogParamInitialize(&DialogParam);
	memset(&UserMsgParam, 0, sizeof(UserMsgParam));
	memset(&ButtonsParam, 0, sizeof(ButtonsParam));
	DialogParam.userMsgParam = &UserMsgParam;
	DialogRunning = false;

	// Setup Params
	DialogParam.userId = userID;
	UserMsgParam.msg = Msg;
	UserMsgParam.buttonType = ButtonType;
	DialogParam.mode = Mode;
	if (ButtonType == ORBIS_MSG_DIALOG_BUTTON_TYPE_2BUTTONS)
	{
		ButtonsParam.msg1 = Button1;
		ButtonsParam.msg2 = Button2;
		UserMsgParam.buttonsParam = &ButtonsParam;
	}

	int res = sceMsgDialogOpen(&DialogParam);
	if (res != 0)
	{
		klog("sceMsgDialogOpen() Failed with %llX\n", res);
		return;
	}

	DialogRunning = true;

	if (Blocking)
	{
		klog("Waiting...\n");
		while (sceMsgDialogUpdateStatus() != ORBIS_COMMON_DIALOG_STATUS_FINISHED) { sceKernelUsleep(100); }
	}
}

void MsgDialog::Close()
{
	int res = 0;
	if ((res = sceMsgDialogClose()) != 0)
	{
		klog("Oh no... sceMsgDialogClose() Failed with 0x%llX\n", res);
		return;
	}

	DialogRunning = false;
}

OrbisCommonDialogStatus MsgDialog::Status()
{
	if (DialogRunning)
		return sceMsgDialogUpdateStatus();
	else
		return ORBIS_COMMON_DIALOG_STATUS_FINISHED;
}

bool MsgDialog::Result()
{
	if (DialogRunning)
	{
		int res = 0;
		memset(&DialogResult, 0, sizeof(DialogResult));
		if (0 > (res = sceMsgDialogGetResult(&DialogResult)))
		{
			klog("sceMsgDialogGetResult() Failed with 0x%llX\n", res);
			return false;
		}

		// Clean up.
		Close();

		return true;
	}
	
	return false;
}