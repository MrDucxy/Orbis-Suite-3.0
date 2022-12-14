#pragma once

class LncUtil
{
public:
	static int Init();
	static int sceLncUtilGetAppId(const char* TitleId);

private:
	static uint64_t LibraryBaseAddress;
	static int(*_sceLncUtilGetAppId)(const char* titleId);
};
