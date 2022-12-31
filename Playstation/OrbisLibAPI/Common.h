#pragma once

#include <stack>
#include <functional>
#include <errno.h>
#include <map>
#include <algorithm>
#include <vector>
#include <string>
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <stddef.h>
#include <stdint.h>
#include <signal.h>
#include <orbis/libkernel.h>
#include <orbis/libmonovm.h>
#include <orbis/RegMgr.h>
#include <orbis/Net.h>
#include <orbis/NetCtl.h>
#include <orbis/SystemService.h>
#include <orbis/UserService.h>
#include <orbis/Sysmodule.h>
#include <orbis/AppInstUtil.h>
#include <orbis/SysCore.h>

#include "SocketListener.h"
#include "Utilities.h"
#include "GoldHEN.h"
#include "../../Misc/libjbc.h"
#include "Sockets.h"
#include "System.h"
#include "Flash.h"

#include "APIPackets.h"
#include "LncUtil.h"
#include "ShellCoreUtil.h"
#include "SystemMonitor.h"

//#define FIRMWARE_505
#define FIRMWARE_900