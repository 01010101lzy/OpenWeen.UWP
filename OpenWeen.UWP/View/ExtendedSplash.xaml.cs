﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenWeen.Core.Model;
using OpenWeen.UWP.BackgroundTask;
using OpenWeen.UWP.Common;
using OpenWeen.UWP.Common.Helpers;
using OpenWeen.UWP.Shared.Common;
using OpenWeen.UWP.Shared.Common.Helpers;
using OpenWeen.UWP.ToastNotificationTask;
using PropertyChanged;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace OpenWeen.UWP.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    partial class ExtendedSplash
    {
        internal Rect splashImageRect;
        private static SplashScreen splash;
        internal bool dismissed = false;
        internal Frame rootFrame = new Frame();
        private double ScaleFactor;
        public string StateText
        {
            get { return StateTextblock.Text; }
            set { StateTextblock.Text = value; }
        }

        public ExtendedSplash(SplashScreen splashscreen, bool loadState)
        {
            InitializeComponent();
            Window.Current.SizeChanged += ExtendedSplash_OnResize;
            if (splash == null && splashscreen != null)
            {
                splash = splashscreen;
            }
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            ScaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            if (splash != null)
            {
                splash.Dismissed += DismissedEventHandler;
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }
            InitTransitions();
            RestoreStateAsync(loadState);
        }
        

        private void InitTransitions()
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new DrillInNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            rootFrame.ContentTransitions = collection;
        }

        private async Task InitEmotion()
        {
            StateText = "正在初始化表情";
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync("emotion.json");
            if (file == null)
                return;
            var text = File.ReadAllText(file.Path);
            StaticResource.Emotions = JsonHelper.FromJson<List<EmotionModel>>(text);
       }

        private async Task<bool> CheckForLogin()
        {
            try
            {
                do
                {
                    Core.Api.Entity.AccessToken = SettingHelper.GetListSetting<string>(SettingNames.AccessToken, isThrowException: true).ToList()[Settings.SelectedUserIndex];
                    if (string.IsNullOrEmpty(Core.Api.Entity.AccessToken))
                        throw new Core.Exception.InvalidAccessTokenException();
                    if (string.IsNullOrEmpty((await Core.Api.User.Account.GetLimitStatus()).Error))
                        break;
                    var list = SettingHelper.GetListSetting<string>(SettingNames.AccessToken, isThrowException: true).ToList();
                    list.RemoveAt(Settings.SelectedUserIndex);
                    Settings.SelectedUserIndex = 0;
                    SettingHelper.SetListSetting(SettingNames.AccessToken, list);
                    if (list.Count == 0)
                        return false;
                } while (true);
                return true;
            }
            catch (Exception e)// when (e is Core.Exception.InvalidAccessTokenException || e is SettingException)
            {
                return false;
            }

        }

        private void RestoreStateAsync(bool loadState)
        {
            if (loadState)
            {
                // TODO: write code to load state
            }
        }

        private void PositionImage()
        {

            extendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.Left);
            extendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Top);
            if (StaticResource.IsPhone)
            {
                extendedSplashImage.Height = splashImageRect.Height / ScaleFactor;
                extendedSplashImage.Width = splashImageRect.Width / ScaleFactor;
            }
            else
            {
                extendedSplashImage.Height = splashImageRect.Height;
                extendedSplashImage.Width = splashImageRect.Width;
            }
        }
        

        private void ExtendedSplash_OnResize(object sender, WindowSizeChangedEventArgs e)
        {
            if (splash != null)
            {
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }
        }

        private async void DismissedEventHandler(SplashScreen sender, object e)
        {
            dismissed = true;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 DismissExtendedSplash();
             });
        }

        private async Task InitUid()
        {
            StateText = "正在初始化用户数据";
            StaticResource.Uid = long.Parse(await Core.Api.User.Account.GetUid());
        }

        private async void DismissExtendedSplash()
        {
            Window.Current.SizeChanged -= ExtendedSplash_OnResize;
            StateText = "正在检查后台通知";
            await BackgroundHelper.Register<UpdateUnreadCountTask>(new TimeTrigger(15, false));
            await BackgroundHelper.Register<ToastNotificationBackgroundTask>(new ToastNotificationActionTrigger());
            await InitEmotion();
            var cahcesize = rootFrame.CacheSize;
            rootFrame.CacheSize = 0;
            rootFrame.CacheSize = cahcesize;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (await CheckForLogin())
            {
                await InitUid();
                rootFrame.Navigate(typeof(MainPage));
            }
            else
            {
                rootFrame.Navigate(typeof(LoginPage));
            }
            SystemNavigationManager.GetForCurrentView().BackRequested += ExtendedSplash_BackRequested;
            rootFrame.Navigated += RootFrame_Navigated;
            Window.Current.Content = rootFrame;
        }

        private void ExtendedSplash_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
            else
            {
                Application.Current.Exit();
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (rootFrame.CanGoBack)
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
    }
}