
# OrbisLib2 Beginners Guide
Hey! This is a writeup on the OrbisLib2 API for C# to help users get started. Not everything is yet added, but will be in the coming days.


## Getting Started
### Pre-Requisites
To get started using the OrbisLib2 API, there are 3 pre-requisites.
- First and foremost, you need to have [Visual Studio](https://visualstudio.microsoft.com/ "Visual Studio") installed with .NET desktop development aswell as the[ .NET 6.0 (or newer) runtime.](https://dotnet.microsoft.com/en-us/download/dotnet/6.0 " .NET 6.0 (or newer) runtime.") (or your favorite IDE/Editor of choice)
- Secondly, you need to install Orbis Suite on your PC. The download can be found on GitHub releases, or for the newest build (as of 2/23/23) [here](https://cdn.discordapp.com/attachments/1065843387846103111/1077818094338187374/Orbis_Suite_Setup.exe "here"). To run this make sure you install .NET 6.0 from above and [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework ".NET Framework").
- Last but not least, you need to install and run Orbis Suite on your jailbroken PS4 console. The .PKG file can be found in `C:/Program Files (x86)/Orbis Suite` once you install Orbis Suite on your computer.

### Creating a Project
Now that we have all of our pre-requisites installed, lets create a new project. From this point on, I am going to assume that you have basic knowledge of using your IDE/editor aswell as basic C# knowledge.

Go ahead and create yourself a new .NET project. I am going to choose to create a Windows Forms Application so we can create a simple GUI for visual input/output, but feel free to use whatever type of project you prefer.

Now that we have our new project created, we need to add OrbisLib2 as a reference. If you are using Visual Studio, you do this by going to `Project > Add Project Reference > Browse` and then locating the OrbisLib2.dll file. The dll file can be found in `C:/Program Files (x86)/Orbis Suite` just like the PS4 PKG.

OrbisLib also references some SQLite libraries, so we will also need to install the required packages using NuGet or by manually adding them to our project.

<img src="https://i.imgur.com/kSlItfK.png" height="250">

### Adding Our Console
Now that our project is set up, we need to add a PS4 console to our target list before doing anything. There are a couple ways you can do this.
- Open Orbis Neighborhood and add your console from the targets tab.
- Manually create a new target using the `NewTarget()` function inside the TargetManager class.

For the easiest approach, I am going to go with the first option. This option is the most user friendly and is probably the option most users are going to resort to anyways.

<img src="https://i.imgur.com/bkpaTxl.png" height="250">

### Connecting to Our Console
Now that we have our console added as a target, you may be wondering how we connect to it. Well, connecting to the console is as simple as being on the same network. That's it.
As long as Orbis Suite is running on your PS4 and is on the same network as your computer, Orbis Suite on your computer will automatically search for and connect to the console. (Thanks OSM for the awesome background service!)

<img src="https://i.imgur.com/dqPo6rN.png" height="250">

## Writing Our First Code
Now that we have everything set up properly, let's get started with our first few lines of code. Remember, this isn't a programming lesson. If you are having trouble understanding some terms and/or code, it's best to go learn the basics of C# and then come back to this later.

### Finding and Selecting a Target
To get started, we are going to find and select our target PS4 console. There are a couple ways of doing this:
- Using the `ShowDialog()` function inside the `Dialog.SelectTarget` class.
- Iterate through the targets list and add them to a listbox.

I am going to go with the second method as the first requires you to add SimpleUI and some WPF libraries as dependencies (since I am using WinForms and not WPF) and I want to keep the project as clean as possible.

Anyways, let's start by placing a listbox and a button on our fresh Windows Form window. (Controls can be found in the toolbox).

<img src="https://i.imgur.com/aCWo36J.png" height="250">

I have named these `targetListBox` and `refreshTargetsBtn` respectively.

Now, we need to actually write some code to make these controls function how we want. Let's double click the refresh button to create a `Click` event. We need to iterate through the targets list and add them to our listbox. This is easily done by using a `foreach` loop, as seen below. Oh, and don't forget to clear the listbox before looping through the targets.

```c#
private void refreshTargetsBtn_Click(object sender, EventArgs e)
{
    targetListBox.Items.Clear(); //Clears the listbox so we don't have duplicate items upon refresh.
    foreach(Target target in TargetManager.Targets) //Loops through the list of targets.
    {
        targetListBox.Items.Add(target.Name); //For each target found in the list, add it to the listbox.
    }
}
```

Let's run our application and test out our new code. Upon clicking the refresh button, we can see all of our targets appear in the listbox. If you don't see any targets, you either have an error in your code or you forgot to [add a target](#adding-our-console) earlier.

<img src="https://i.imgur.com/rxw4BCv.png" height="250">

Now that we have our refresh button working and we can see all of our targets, we need a way of selecting one. This is as simple as using the listbox's `selectedIndexChanged` event. Yet again, double click the listbox to create this event. This time we are going to select the target by it's name based on what is highlighted/selected in the listbox.

We are also going to want a way to confirm that the above code works. For testing purposes, let's create a messagebox after we set the selected target.

This is all done using the following code.

```c#
private void targetListBox_SelectedIndexChanged(object sender, EventArgs e)
{
    //Find the target using the selected item's text and set it to the SelectedTarget property.
    TargetManager.SelectedTarget = TargetManager.GetTarget(targetListBox.GetItemText(targetListBox.SelectedItem));
    MessageBox.Show(TargetManager.SelectedTarget.Name); //Pop a messagebox with the selected target's name.
}
```

When you select a target in the listbox, you should now get a message with the target's name.

<img src="https://i.imgur.com/E2QAyVN.png" height="250">

Congratulations! You successfully found and selected a target PS4. Let's move onto some of the more entertaining stuff, like controlling our console.
