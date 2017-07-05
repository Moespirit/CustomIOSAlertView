# CustomIOSAlertView

[CustomIOSAlertView](https://github.com/wimagguc/ios-custom-alertview) for Xamarin

# Custom iOS AlertView

`v0.9.5`

`support for iOS7+`

The addSubview is not available in UIAlertView since iOS7. The view hierarchy for this class is private and must not be modified.

As a solution, this class creates an iOS-style dialog which you can extend with any UIViews or buttons. The animations and the looks are copied too and no images or other resources are needed.

![A demo screen](https://github.com/wimagguc/ios-custom-alertview/blob/master/Docs/screen.png?raw=true)

## Install
[![NuGet](https://img.shields.io/nuget/v/Moespirit.Xamarin.iOSControls.CustomIOSAlertView.svg?style=flat)](https://www.nuget.org/packages/Moespirit.Xamarin.iOSControls.CustomIOSAlertView/)

## Change notes

* Fixed rotation for IOS8

* Removed 7 from the class name. Just use CustomIOSAlertView from now on, like: [[CustomIOSAlertView alloc] init];

* The initWithParentView method is now deprecated. Please use the init method instead, where you don't need to pass a parent view at all. **In case the init doesn't work for you, please leave a note or open an issue here.**

## Quick start guide

1. Create the UIView object `changed`

    ```
    CustomIOSAlertView alertView = new CustomIOSAlertView();
    ```

2. Add some custom content to the alert view (optional)

    ```
    UIView customView ..;

    alertView.ContainerView=customView;
    ```

3. Display the dialog

    ```
    alertView.Show();
    ```

## More functions

* Close the dialog

    ```
    alertView.Close();
    ```

* To add more buttons, pass a list of titles

    ```
    alertView.ButtonTitles=new NSString[]{new NSString("Button1"),new NSString("Button2"),new NSString("Button3")}
    ```

* You can remove all buttons by passing nil

    ```
    alertView.ButtonTitles=null;
    ```

* You can enable or disable the iOS7 parallax effects on the alert view

    ```
    alertView.UseMotionEffects=true;
    ```

* Handle button clicks with a custom delegate


    Add the delegate methods:

    ```
    alertView.OnButtonTouchUpInside+=(sender,intargs)=>{...};
    ```

* Handle button clicks with a code block

    ```
alertView.OnButtonTouchUpInside+=(sender,intargs)=>{alertView.Close()};
    ```


## Todos

This is a really quick implementation, and there are a few things missing:

* Adding more buttons: they don't exactly match the look with that of on iOS7

* Rotation: rotates wrong with the keyboard on


