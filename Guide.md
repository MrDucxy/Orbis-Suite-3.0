# OrbisLib2 Beginners Guide
Hey! This is a writeup on the OrbisLib2 API for C# to help users get started. Not everything is yet added, but will be in the coming days.


## Getting Started
###Pre-Requisites
To get started using the OrbisLib2 API, there are 3 pre-requisites.
- First and foremost, you need to have [Visual Studio](https://visualstudio.microsoft.com/ "Visual Studio") installed with .NET desktop development aswell as the[ .NET 6.0 (or newer) runtime.](https://dotnet.microsoft.com/en-us/download/dotnet/6.0 " .NET 6.0 (or newer) runtime.") (or your favorite IDE/Editor of choice)
- Secondly, you need to install Orbis Suite on your PC. The download can be found on GitHub releases, or for the newest build (as of 2/23/23) [here](https://cdn.discordapp.com/attachments/1065843387846103111/1077818094338187374/Orbis_Suite_Setup.exe "here"). To run this make sure you install .NET 6.0 from above and [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework ".NET Framework").
- Last but not least, you need to install and run Orbis Suite on your jailbroken PS4 console. The .PKG file can be found in `C:/Program Files (x86)/Orbis Suite` once you install Orbis Suite on your computer.

###Creating a Project
Now that we have all of our pre-requisites installed, lets create a new project. From this point on, I am going to assume that you have basic knowledge of using your IDE/editor aswell as basic C# knowledge.

Go ahead and create yourself a new .NET project. I am going to choose to create a Windows Forms Application so we can create a simple GUI for visual input/output, but feel free to use whatever type of project you prefer.

Now that we have our new project created, we need to add OrbisLib2 as a reference. If you are using Visual Studio, you do this by going to `Project > Add Project Reference > Browse` and then locating the OrbisLib2.dll file. The dll file can be found in `C:/Program Files (x86)/Orbis Suite` just like the PS4 PKG.

###Adding Our Console
Now that our project is set up, we need to add a PS4 console to our target list before doing anything. There are a couple ways you can do this.
- Open Orbis Neighborhood and add your console from the targets tab.
- Manually create a new target using the `NewTarget()` function inside the TargetManager class.

For the easiest approach, I am going to go with the first option. This option is the most user friendly and is probably the option most users are going to resort to anyways.

<img src="https://i.imgur.com/bkpaTxl.png" height="250">

###Connecting to Our Console
Now that we have our console added as a target, you may be wondering how we connect to it. Well, connecting to the console is as simple as being on the same network. That's it.
As long as Orbis Suite is running on your PS4 and is on the same network as your computer, Orbis Suite on your computer will automatically search for and connect to the console. (Thanks OSM for the awesome background service!)

<img src="https://i.imgur.com/dqPo6rN.png" height="250">

##Writing Our First Code
got bored at this part, will add more later :D
