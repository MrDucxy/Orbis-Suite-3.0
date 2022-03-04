#include "../../Common.hpp"
#include "Patches.hpp"
#include "../Resolver/Resolver.hpp"

//
// Credits to Mira for the patches. Was just easier to slap them in :P
//

void Install_Patches()
{
    if(!gpKernelBase)
        return;

    klog("Disable WP");
    cpu_disable_wp();

    #if defined(SOFTWARE_VERSION_505) || defined(SOFTWARE_VERSION_NA)
    klog("Patches 5.05");
    Install_505();
    #elif defined(SOFTWARE_VERSION_672)
    Install_672();
    #elif defined(SOFTWARE_VERSION_702)
    Install_702();
    #elif defined(SOFTWARE_VERSION_755)
    Install_755();
    #elif defined(SOFTWARE_VERSION_900)
    Install_900();
    #endif

    klog("Enable WP");
    cpu_enable_wp();

    klog("Install_Patches() -> Sucess!");
}