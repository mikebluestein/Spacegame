﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Xna.Framework;

using CocosSharp;
using SpaceGameCommon;
using Amazon.Device.GameController;

namespace Spacegame
{
    [Activity (
        Label = "SpaceGameAndroid",
        AlwaysRetainTaskState = true,
        Icon = "@drawable/icon",
        Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Landscape,
        LaunchMode = LaunchMode.SingleInstance,
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)
    ]
    public class MainActivity : AndroidGameActivity
    {
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            var application = new CCApplication (); 
            application.ApplicationDelegate = new GameAppDelegate ();
            SetContentView (application.AndroidContentView);

            GameController.Init (this);

            application.StartGame ();
        }

        public override bool OnGenericMotionEvent (MotionEvent e)
        {
            bool handled = false;
            try {
                handled = GameController.OnGenericMotionEvent (e);
            } catch (GameController.DeviceNotFoundException) {
            }
            return handled || base.OnGenericMotionEvent (e);
        }
    }
}