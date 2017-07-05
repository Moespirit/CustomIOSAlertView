using Foundation;
using System;
using UIKit;
using CoreGraphics;
using System.Linq;
using CoreAnimation;

namespace Moespirit.Xamarin.iOSControls
{
    public class CustomIOSAlertView : UIView
    {
        static readonly Version NSFoundationVersionNumber = new Version(UIDevice.CurrentDevice.SystemVersion);
        const float kCustomIOSAlertViewDefaultButtonHeight = 50;
        const float kCustomIOSAlertViewDefaultButtonSpacerHeight = 1;
        const float kCustomIOSAlertViewCornerRadius = 7;
        const float kCustomIOS7MotionEffectExtent = 10.0f;


        float buttonHeight = 0;
        float buttonSpacerHeight = 0;

        public UIView ParentView { get; set; }
        public UIView ContainerView { get; set; }
        public UIView DialogView { get; set; }
        public event Action<CustomIOSAlertView, int> OnButtonTouchUpInside;
        public NSString[] ButtonTitles { get; set; }
        public bool UseMotionEffects { get; set; }
        public bool CloseOnTouchUpOutside { get; set; }
        public CustomIOSAlertView()
        {
            init();
        }
        public CustomIOSAlertView(UIView parentView)
        {
            init();
            Frame = parentView.Frame;
            this.ParentView = parentView;
        }
        private void init()
        {
            Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Size.Width, UIScreen.MainScreen.Bounds.Size.Height);
            UseMotionEffects = false;
            CloseOnTouchUpOutside = false;
            ButtonTitles = new NSString[] { new NSString("Close") };
            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString(UIDevice.OrientationDidChangeNotification), deviceOrientationDidChange, null);
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString(UIKeyboard.WillShowNotification), keyboardWillShow, null);
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString(UIKeyboard.WillHideNotification), keyboardWillHide, null);
        }
        static readonly Version NSFoundationVersionNumber_iOS_7_1 = new Version(7, 1);
        public void Show()
        {
            DialogView = createContainerView();

            DialogView.Layer.ShouldRasterize = true;
            DialogView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;

            Layer.ShouldRasterize = true;
            Layer.RasterizationScale = UIScreen.MainScreen.Scale;
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                applyMotionEffects();
            }
            BackgroundColor = UIColor.FromRGBA(0,0,0,0);
            AddSubview(DialogView);

            // Can be attached to a view or to the top most window
            // Attached to a view:
            if (ParentView!=null)
            {
                ParentView.AddSubview(this);
                // Attached to the top most window

            }
            else
            {
                // On iOS7, calculate with orientation
                if(NSFoundationVersionNumber<= NSFoundationVersionNumber_iOS_7_1)
                {
                    var interfaceOrientation = UIApplication.SharedApplication.StatusBarOrientation;
                    switch (interfaceOrientation)
                    {
                        case UIInterfaceOrientation.LandscapeLeft:
                            Transform = CGAffineTransform.MakeRotation(new nfloat(Math.PI * 270.0 / 180.0));
                            break;
                        case UIInterfaceOrientation.LandscapeRight:
                            Transform = CGAffineTransform.MakeRotation(new nfloat(Math.PI * 90.0 / 180.0));
                            break;
                        case UIInterfaceOrientation.PortraitUpsideDown:
                            Transform = CGAffineTransform.MakeRotation(new nfloat(Math.PI * 180.0 / 180.0));
                            break;
                        default:
                            break;
                    }
                    Frame = new CGRect(0, 0, Frame.Size.Width, Frame.Size.Height);

                }
                else
                {
                    CGSize screenSize = countScreenSize();
                    CGSize dialogSize = countDialogSize();
                    CGSize keyboardSize = new CGSize(0, 0);

                    DialogView.Frame = new CGRect((screenSize.Width - dialogSize.Width) / 2, (screenSize.Height - keyboardSize.Height - dialogSize.Height) / 2, dialogSize.Width, dialogSize.Height);
                }
                UIApplication.SharedApplication.Windows.FirstOrDefault().AddSubview(this);
            }

            DialogView.Layer.Opacity = 0.5f;
            DialogView.Layer.Transform = CATransform3D.MakeScale(1.3f, 1.3f, 1.0f);

            Animate(0.2f, 0.0, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0.4f);
                DialogView.Layer.Opacity = 1.0f;
                DialogView.Layer.Transform = CATransform3D.MakeScale(1, 1, 1);
            }, null);

        }
        public void customIOS7dialogButtonTouchUpInside(object sender, EventArgs index)
        {
            var ciav = (UIButton)sender;
            customIOS7dialogButtonTouchUpInside(this, (int)ciav.Tag);

            OnButtonTouchUpInside?.Invoke(this, (int)ciav.Tag);
        }
        public void customIOS7dialogButtonTouchUpInside(CustomIOSAlertView sender, int index)
        {
            System.Diagnostics.Debug.WriteLine($"Button Clicked! {index} {sender.Tag}");
            Close();
        }
        public void Close()
        {
            CATransform3D currentTransform = DialogView.Layer.Transform;

            if(NSFoundationVersionNumber<=NSFoundationVersionNumber_iOS_7_1)
            {
                float startRotation = ((NSNumber)DialogView.ValueForKeyPath(new NSString( "layer.transform.rotation.z"))).FloatValue;
                CATransform3D rotation = CATransform3D.MakeRotation(new nfloat( -startRotation + Math.PI * 270.0 / 180.0), 0.0f, 0.0f, 0.0f);

                DialogView.Layer.Transform = rotation.Concat( CATransform3D.MakeScale(1, 1, 1));
            }

            DialogView.Layer.Opacity = 1.0f;

            UIView.Animate(0.2f, 0.0, UIViewAnimationOptions.TransitionNone, () =>
            {
                BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 0.0f);
                DialogView.Layer.Transform = currentTransform.Concat(CATransform3D.MakeScale(0.6f, 0.6f, 1.0f));
                DialogView.Layer.Opacity = 0;
            }, () =>
            {
                foreach(var v in Subviews)
                {
                    v.RemoveFromSuperview();
                }
                RemoveFromSuperview();
            });
        }
        void setSubView(UIView subView)
        {
            ContainerView = subView;
        }

        UIView createContainerView()
        {
            if (ContainerView == null)
            {
                ContainerView = new UIView( new CGRect(0, 0, 300, 150));
            }

            CGSize screenSize = countScreenSize();
            CGSize dialogSize = countDialogSize();

            // For the black background
            Frame = new CGRect(0, 0, screenSize.Width, screenSize.Height);

            // This is the dialog's container; we attach the custom content and the buttons to this one
            UIView dialogContainer = new UIView(new CGRect((screenSize.Width - dialogSize.Width) / 2, (screenSize.Height - dialogSize.Height) / 2, dialogSize.Width, dialogSize.Height));

            // First, we style the dialog to match the iOS7 UIAlertView >>>
            CAGradientLayer gradient = (CAGradientLayer)CAGradientLayer.Create();
            gradient.Frame = dialogContainer.Bounds;
            gradient.Colors = new CGColor[] { new CGColor(new nfloat(218.0 / 255.0), new nfloat(218.0 / 255.0), new nfloat(218.0 / 255.0), new nfloat(1.0f)),
            new CGColor(new nfloat(233.0 / 255.0), new nfloat(233.0 / 255.0), new nfloat(233.0 / 255.0), new nfloat(1.0f)),
            new CGColor(new nfloat(218.0 / 255.0), new nfloat(218.0 / 255.0), new nfloat(218.0 / 255.0), new nfloat(1.0f))};

            float cornerRadius = kCustomIOSAlertViewCornerRadius;
            gradient.CornerRadius = cornerRadius;
            dialogContainer.Layer.InsertSublayer(gradient, 0);

            dialogContainer.Layer.CornerRadius = cornerRadius;
            dialogContainer.Layer.BorderColor = new CGColor(new nfloat(198.0 / 255.0), new nfloat(198.0 / 255.0), new nfloat(198.0 / 255.0), new nfloat(1.0f));
            dialogContainer.Layer.BorderWidth = 1;
            dialogContainer.Layer.ShadowRadius = cornerRadius + 5;
            dialogContainer.Layer.ShadowOpacity = 0.1f;
            dialogContainer.Layer.ShadowOffset = new CGSize(0 - (cornerRadius + 5) / 2, 0 - (cornerRadius + 5) / 2);
            dialogContainer.Layer.ShadowColor = UIColor.Black.CGColor;
            dialogContainer.Layer.ShadowPath = UIBezierPath.FromRoundedRect(dialogContainer.Bounds, dialogContainer.Layer.CornerRadius).CGPath;

            // There is a line above the button
            UIView lineView = new UIView(new CGRect(0, dialogContainer.Bounds.Size.Height - buttonHeight - buttonSpacerHeight, dialogContainer.Bounds.Size.Width, buttonSpacerHeight));
            lineView.BackgroundColor = new UIColor(new nfloat(198.0 / 255.0), new nfloat(198.0 / 255.0), new nfloat(198.0 / 255.0), new nfloat(1.0f));
            dialogContainer.AddSubview(lineView);
            // ^^^

            // Add the custom container if there is any
            dialogContainer.AddSubview(ContainerView);

            // Add the buttons too
            addButtonsToView(dialogContainer);

            return dialogContainer;
        }
        void addButtonsToView(UIView container)
        {
            if(ButtonTitles==null)
            {
                return;
            }

            nfloat buttonWidth = container.Bounds.Size.Width / ButtonTitles.Length;

            for(int i = 0; i < ButtonTitles.Length; i++)
            {
                UIButton closeButton = new UIButton(UIButtonType.Custom);

                closeButton.Frame = new CGRect(i * buttonWidth, container.Bounds.Size.Height - buttonHeight, buttonWidth, buttonHeight);

                closeButton.TouchUpInside += customIOS7dialogButtonTouchUpInside;
                closeButton.Tag = i;

                closeButton.SetTitle(ButtonTitles[i], UIControlState.Normal);
                closeButton.SetTitleColor(new UIColor(0.0f, 0.5f, 1.0f, 1.0f),UIControlState.Normal);
                closeButton.SetTitleColor(new UIColor(0.2f, 0.2f, 0.2f, 0.5f), UIControlState.Highlighted);
                closeButton.TitleLabel.Font = UIFont.BoldSystemFontOfSize(14.0f);
                closeButton.TitleLabel.Lines = 0;
                closeButton.TitleLabel.TextAlignment = UITextAlignment.Center;
                closeButton.Layer.CornerRadius = kCustomIOSAlertViewCornerRadius;

                container.AddSubview(closeButton);

            }
        }

        CGSize countScreenSize()
        {
            if (ButtonTitles != null && ButtonTitles.Length > 0)
            {
                buttonHeight = kCustomIOSAlertViewDefaultButtonHeight;
                buttonSpacerHeight = kCustomIOSAlertViewDefaultButtonSpacerHeight;
            }
            else
            {
                buttonHeight = 0;
                buttonSpacerHeight = 0;
            }

            var screenWidth = UIScreen.MainScreen.Bounds.Size.Width;
            var screenHeight = UIScreen.MainScreen.Bounds.Size.Height;

            if (NSFoundationVersionNumber <= NSFoundationVersionNumber_iOS_7_1)
            {
                UIInterfaceOrientation interfaceOrientation = UIApplication.SharedApplication.StatusBarOrientation;
                if (interfaceOrientation == UIInterfaceOrientation.LandscapeLeft && interfaceOrientation == UIInterfaceOrientation.LandscapeRight)
                {
                    var tmp = screenWidth;
                    screenWidth = screenHeight;
                    screenHeight = tmp;
                }
            }

            return new CGSize(screenWidth, screenHeight);
        }
        CGSize countDialogSize()
        {
            var dialogWidth = ContainerView.Frame.Size.Width;
            var dialogHeight = ContainerView.Frame.Size.Height + buttonHeight + buttonSpacerHeight;

            return new CGSize(dialogWidth, dialogHeight);
           
        }
        static readonly Version NSFoundationVersionNumber_iOS_6_1 = new Version(6, 1);
        void applyMotionEffects()
        {
            if (NSFoundationVersionNumber <= NSFoundationVersionNumber_iOS_6_1)
            {
                return;
            }

            UIInterpolatingMotionEffect horizontalEffect = new UIInterpolatingMotionEffect("center.x", UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis);

            horizontalEffect.MinimumRelativeValue = new NSNumber( -kCustomIOS7MotionEffectExtent);
            horizontalEffect.MaximumRelativeValue = new NSNumber(kCustomIOS7MotionEffectExtent);

            UIInterpolatingMotionEffect verticalEffect = new UIInterpolatingMotionEffect("center.y", UIInterpolatingMotionEffectType.TiltAlongVerticalAxis);

            verticalEffect.MinimumRelativeValue = new NSNumber(-kCustomIOS7MotionEffectExtent);
            verticalEffect.MaximumRelativeValue = new NSNumber(kCustomIOS7MotionEffectExtent);

            UIMotionEffectGroup motionEffectGroup = new UIMotionEffectGroup();
            motionEffectGroup.MotionEffects = new UIMotionEffect[] { horizontalEffect, verticalEffect };

            DialogView.AddMotionEffect(motionEffectGroup);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIDevice.OrientationDidChangeNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillShowNotification, null);

        }

        // Rotation changed, on iOS7
        void changeOrientationForIOS7()
        {
            var interfaceOrientation = UIApplication.SharedApplication.StatusBarOrientation;

            float startRotation = ((NSNumber)ValueForKeyPath(new NSString("layer.transform.rotation.z"))).FloatValue;
            CGAffineTransform rotation;

            switch (interfaceOrientation)
            {
                case UIInterfaceOrientation.LandscapeLeft:
                    rotation = CGAffineTransform.MakeRotation(new nfloat(-startRotation + Math.PI * 270.0 / 180.0));
                    break;
                case UIInterfaceOrientation.LandscapeRight:
                    rotation = CGAffineTransform.MakeRotation(new nfloat(-startRotation + Math.PI * 90.0 / 180.0));
                    break;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    rotation = CGAffineTransform.MakeRotation(new nfloat(-startRotation + Math.PI * 180.0 / 180.0));
                    break;
                default:
                    rotation = CGAffineTransform.MakeRotation(new nfloat(-startRotation + 0.0));
                    break;
            }

            Animate(0.2f, 0.0, UIViewAnimationOptions.TransitionNone, () =>
            {
                DialogView.Transform = rotation;
            }, null);
        }

        // Rotation changed, on iOS8
        void changeOrientationForIOS8(NSNotification notification)
        {
            var screenWidth = UIScreen.MainScreen.Bounds.Size.Width;
            var screenHeight = UIScreen.MainScreen.Bounds.Size.Height;

            Animate(0.2f, 0.0, UIViewAnimationOptions.TransitionNone, () =>
            {
                CGSize dialogSize = countDialogSize();
                //var nsv = ((NSValue)notification.UserInfo.ObjectForKey(UIKeyboard.FrameBeginUserInfoKey));
                //CGSize keyboardSize = nsv == null ? CGSize.Empty : nsv.CGRectValue.Size;
                Frame = new CGRect(0, 0, screenWidth, screenHeight);
                DialogView.Frame = new CGRect((screenWidth - dialogSize.Width) / 2, (screenHeight - keyboardSize.Height - dialogSize.Height) / 2, dialogSize.Width, dialogSize.Height);
            }, null);
        }

        // Handle device orientation changes
        void deviceOrientationDidChange(NSNotification notification)
        {
            // If dialog is attached to the parent view, it probably wants to handle the orientation change itself
            if (ParentView != null)
                return;

            if (NSFoundationVersionNumber <= NSFoundationVersionNumber_iOS_7_1)
            {
                changeOrientationForIOS7();
            }
            else
            {
                changeOrientationForIOS8(notification);
            }
        }
        CGSize keyboardSize;//added by yinyue200
        void keyboardWillShow(NSNotification notification)
        {
            var screenSize = countScreenSize();
            var dialogSize = countDialogSize();
            keyboardSize = ((NSValue)notification.UserInfo.ObjectForKey(UIKeyboard.FrameBeginUserInfoKey)).CGRectValue.Size;

            UIInterfaceOrientation interfaceOrientation = UIApplication.SharedApplication.StatusBarOrientation;
            switch (interfaceOrientation)
            {
                case UIInterfaceOrientation.LandscapeLeft:
                case UIInterfaceOrientation.LandscapeRight:
                    var tmp = keyboardSize.Height;
                    keyboardSize.Height = keyboardSize.Width;
                    keyboardSize.Width = tmp;
                    break;
                default:
                    break;
            }

            Animate(0.2f, 0.0, UIViewAnimationOptions.TransitionNone, () =>
            {
                DialogView.Frame = new CGRect((screenSize.Width - dialogSize.Width) / 2, (screenSize.Height - keyboardSize.Height - dialogSize.Height) / 2, dialogSize.Width, dialogSize.Height);
            }, null);
        }
        void keyboardWillHide(NSNotification notification)
        {
            keyboardSize = CGSize.Empty;//added by yinyue200
            var screenSize = countScreenSize();
            var dialogSize = countDialogSize();

            Animate(0.2f, 0.0, UIViewAnimationOptions.TransitionNone, () =>
            {
                DialogView.Frame = new CGRect((screenSize.Width - dialogSize.Width) / 2, (screenSize.Height - dialogSize.Height) / 2, dialogSize.Width, dialogSize.Height);
            }, null);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            if (!CloseOnTouchUpOutside)
                return;

            UITouch touch = (UITouch)touches.AnyObject;
            if(touch.View is CustomIOSAlertView)
            {
                Close();
            }
        }

    }

}
