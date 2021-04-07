using Android.App;
using Android.Content;
using Android.OS;
using System.Net;
using Java.Lang;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using AndroidX.Work;
using Android.Preferences;
using Xamarin.Essentials;

namespace ApartmentSearch
{
	class NotificationWorker : Worker
    {
        static readonly int NOTIFICATION_ID = 1000;
        static readonly string CHANNEL_ID = "notification";
        internal static readonly string APARTMENT_KEY = "apartment";

        bool newer = false;
        string url_string;

        public NotificationWorker(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {

        }

        public override Result DoWork()
        {
            //ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
            //ISharedPreferencesEditor editor = preferences.Edit();

            WebClient client = new WebClient();
            string url, apartment_string;

            try
            {
                url = "https://www.dba.dk/boliger/lejebolig/lejelejlighed/reg-koebenhavn-og-omegn/?reg=nordsjaelland&sort=listingdate-desc&pris=(0-7000)";

                url_string = client.DownloadString(url);

                url_string = url_string.Substring(url_string.IndexOf("Product"));
                url_string = url_string.Substring(url_string.IndexOf("url") + 7, (url_string.IndexOf("offers") - 18) - (url_string.IndexOf("url") + 7));

                apartment_string = client.DownloadString(url_string).Replace("&#230;", "æ").Replace("&#248;", "ø").Replace("&#229;", "å");
                apartment_string = apartment_string.Substring(apartment_string.IndexOf("description") + 22);
                apartment_string = apartment_string.Substring(0, apartment_string.IndexOf("meta") - 10);

                if (Preferences.Get("key", "default") == "default")
                {
                    //editor.PutString("key", url_string);
                    //editor.Apply();
                    Preferences.Set("key", url_string);
                    newer = true;
                }
                else
                {
                    if (Preferences.Get("key", "default") != url_string)
                    {
                        Preferences.Set("key", url_string);
                        newer = true;
                    }
                    else
                        newer = false;
                }

            }
            catch (System.Exception)
            {
                throw;
            }

            if (newer == true)
            {
                var uri = Android.Net.Uri.Parse(url_string);

                var valuesForActivity = new Bundle();
                valuesForActivity.PutString(APARTMENT_KEY, url_string);

                var resultIntent = new Intent(Intent.ActionView, uri);
                resultIntent.PutExtras(valuesForActivity);

                var stackBuilder = TaskStackBuilder.Create(Application.Context);
                stackBuilder.AddParentStack(Class.FromType(typeof(MainActivity)));
                stackBuilder.AddNextIntent(resultIntent);

                var resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

                var builder = new NotificationCompat.Builder(Application.Context, CHANNEL_ID)
                                .SetAutoCancel(true)
                                .SetContentIntent(resultPendingIntent)
                                .SetContentTitle("dba")
                                .SetSmallIcon(Resource.Drawable.icon)
                                .SetContentText(apartment_string);

                var notificationManager = NotificationManagerCompat.From(Application.Context);
                notificationManager.Notify(NOTIFICATION_ID, builder.Build());
            }
            //else
            //    ShowToast();

            return Result.InvokeSuccess();
        }

        //public void ShowToast()
        //{
        //    Handler mainHandler = new Handler(Looper.MainLooper);
        //    Java.Lang.Runnable runnableToast = new Java.Lang.Runnable(() =>
        //    {
        //        Toast.MakeText(Application.Context, "running", ToastLength.Short).Show();
        //    });

        //    mainHandler.Post(runnableToast);
        //}
	}
}