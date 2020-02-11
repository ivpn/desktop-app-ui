# SparkleSharp

C# bindings for Sparkle

## Installation instructions

- Install Visual Studio for macOS.
- Clone this repository (preferrably as a submodule in your main project)

  ```sh
  git submodule add https://github.com/rainycape/SparkleSharp.git
  ```
- From your Xamarin.Mac project, right click on the solution and select *Add* -> *Add Existing Project* and select **Sparkle.csproj**
- Right click on your Xamarin.Mac project's References and select *Edit References*
- Go to to *Projects* tab and check the box for the Sparkle project
- Finally, follow [Sparkle's own documentation](https://sparkle-project.org/documentation/) to set your appcast URL and add a "Check for Updates" menu item
