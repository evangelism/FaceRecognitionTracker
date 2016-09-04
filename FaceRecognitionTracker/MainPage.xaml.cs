﻿using Microsoft.Azure.Devices.Client;
using Microsoft.ProjectOxford.Emotion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FaceRecognitionTracker
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture MC;
        DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(3) };
        EmotionServiceClient Oxford = new EmotionServiceClient(Config.OxfordAPIKey);

        FaceDetectionEffect FaceDetector;
        VideoEncodingProperties VideoProps;

        bool IsFacePresent = false;

        DeviceClient iothub;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Init();
            iothub = DeviceClient.CreateFromConnectionString(Config.DeviceConnectionString);
            await iothub.OpenAsync();
            dt.Tick += GetEmotions;
            dt.Start();
        }

        private async Task Init()
        {
            MC = new MediaCapture();
            var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var camera = cameras.First();
            var settings = new MediaCaptureInitializationSettings() { VideoDeviceId = camera.Id };
            await MC.InitializeAsync(settings);
            ViewFinder.Source = MC;

            // Create face detection
            var def = new FaceDetectionEffectDefinition();
            def.SynchronousDetectionEnabled = false;
            def.DetectionMode = FaceDetectionMode.HighPerformance;
            FaceDetector = (FaceDetectionEffect)(await MC.AddVideoEffectAsync(def, MediaStreamType.VideoPreview));
            FaceDetector.FaceDetected += FaceDetectedEvent;
            FaceDetector.DesiredDetectionInterval = TimeSpan.FromMilliseconds(100);
            FaceDetector.Enabled = true;

            await MC.StartPreviewAsync();
            var props = MC.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            VideoProps = props as VideoEncodingProperties;
        }

        private async void FaceDetectedEvent(FaceDetectionEffect sender, FaceDetectedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HighlightDetectedFace(args.ResultFrame.DetectedFaces.FirstOrDefault()));
        }

        private async Task HighlightDetectedFace(DetectedFace face)
        {
            var cx = ViewFinder.ActualWidth / VideoProps.Width;
            var cy = ViewFinder.ActualHeight / VideoProps.Height;

            if (face == null)
            {
                FaceRect.Visibility = Visibility.Collapsed;
                IsFacePresent = false;
            }
            else
            {
                FaceRect.Margin = new Thickness(cx * face.FaceBox.X, cy * face.FaceBox.Y, 0, 0);
                FaceRect.Width = cx * face.FaceBox.Width;
                FaceRect.Height = cy * face.FaceBox.Height;
                FaceRect.Visibility = Visibility.Visible;
                IsFacePresent = true;
            }
        }

        async void GetEmotions(object sender, object e)
        {
            if (!IsFacePresent) return;
            dt.Stop();
            var ms = new MemoryStream();
            try
            {
                await MC.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), ms.AsRandomAccessStream());
            }
            catch
            {
                dt.Start();
                return;
            }
            ms.Position = 0L;
            var ms1 = new MemoryStream();
            ms.CopyTo(ms1);
            ms.Position = 0L;
            ms1.Position = 0L;
            var Emo = await Oxford.RecognizeAsync(ms);
            if (Emo != null && Emo.Length > 0)
            {
                var Face = Emo[0];
                var s = JsonConvert.SerializeObject(Face.Scores);
                System.Diagnostics.Debug.WriteLine(s);
                var b = Encoding.UTF8.GetBytes(s);
                await iothub.SendEventAsync(new Message(b));
            }
            dt.Start();
        }
    }
}

