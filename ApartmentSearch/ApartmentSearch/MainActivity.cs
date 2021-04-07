using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using AndroidX.Work;

namespace ApartmentSearch
{
    [Activity(Label = "ApartmentSearch", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity
    {
        static readonly string CHANNEL_ID = "notification";
        internal static readonly string APARTMENT_KEY = "apartment";

        string url_string;

        protected override void OnCreate(Bundle bundle)
        {   
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            CreateNotificationChannel();

            var button = FindViewById<Button>(Resource.Id.MyButton);
            button.Click += ButtonOnClick;

            PeriodicWorkRequest taxWorkRequest = PeriodicWorkRequest.Builder.From<NotificationWorker>(TimeSpan.FromMinutes(10)).Build();
            WorkManager.Instance.Enqueue(taxWorkRequest);

            WebClient client = new WebClient();
            string url;

            try
            {
                url = "https://www.dba.dk/boliger/lejebolig/lejelejlighed/reg-koebenhavn-og-omegn/?reg=nordsjaelland&sort=listingdate-desc&pris=(0-7000)";

                url_string = client.DownloadString(url);

                url_string = url_string.Substring(url_string.IndexOf("Product"));
                url_string = url_string.Substring(url_string.IndexOf("url") + 7, (url_string.IndexOf("offers") - 18) - (url_string.IndexOf("url") + 7));
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        void ButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (url_string != null)
            {
                var uri = Android.Net.Uri.Parse(url_string);
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var name = Resources.GetString(Resource.String.channel_name);
            var description = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
                          {
                              Description = description
                          };

            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            WorkManager.Instance.CancelAllWork();
        }
    }
}
