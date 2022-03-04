#pragma once
#include <orbis/MsgDialog.h>

class MsgDialog
{
public:
	static SceMsgDialogResult DialogResult;

	static void Open(const char* Msg, bool Blocking = true, SceMsgDialogButtonType ButtonType = SCE_MSG_DIALOG_BUTTON_TYPE_OK, SceMsgDialogMode Mode = SCE_MSG_DIALOG_MODE_USER_MSG, const char* Button1 = "", const char* Button2 = "");
	static void Close();
	static SceCommonDialogStatus Status();
	static bool Result();

private:
	static bool DialogRunning;
};