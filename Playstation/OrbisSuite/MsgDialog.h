#pragma once
#include <orbis/MsgDialog.h>

class MsgDialog
{
public:
	static OrbisMsgDialogResult DialogResult;

	static void Open(const char* Msg, bool Blocking = true, OrbisMsgDialogButtonType ButtonType = ORBIS_MSG_DIALOG_BUTTON_TYPE_OK, OrbisMsgDialogMode Mode = ORBIS_MSG_DIALOG_MODE_USER_MSG, const char* Button1 = "", const char* Button2 = "");
	static void Close();
	static OrbisCommonDialogStatus Status();
	static bool Result();

private:
	static bool DialogRunning;
};