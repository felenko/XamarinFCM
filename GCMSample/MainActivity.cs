using System;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using BackendlessAPI;
using BackendlessAPI.Async;
using BackendlessAPI.Exception;
using BackendlessAPI.Messaging;
using Java.Util;

namespace GCMSample
{
	[Activity (Label="@string/app_name", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		const int PLAY_SERVICES_RESOLUTION_REQUEST = 9000;
		const string TAG = "MainActivity";

		BroadcastReceiver mRegistrationBroadcastReceiver;
		ProgressBar mRegistrationProgressBar;
		TextView mInformationTextView;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.activity_main);

			mRegistrationProgressBar = FindViewById<ProgressBar> (Resource.Id.registrationProgressBar);
			mRegistrationBroadcastReceiver = new BroadcastReceiver ();
			mRegistrationBroadcastReceiver.Receive += (sender, e) => {
				mRegistrationProgressBar.Visibility = ViewStates.Gone;
				var sharedPreferences =	PreferenceManager.GetDefaultSharedPreferences ((Context)sender);
				var sentToken = sharedPreferences.GetBoolean (QuickstartPreferences.SENT_TOKEN_TO_SERVER, false);
				mInformationTextView.Text = sentToken ? GetString (Resource.String.gcm_send_message) : GetString (Resource.String.token_error_message);
			};
			mInformationTextView = FindViewById<TextView> (Resource.Id.informationTextView);

		    RegistrationIntentService.TokenReceived += (sender, token) =>
		    {
		        RegisterDeviceInBackendless(token);
		    };

            if (CheckPlayServices ()) {
				var intent = new Intent (this, typeof(RegistrationIntentService));
				StartService (intent);
			}

            InitApi();
        }
        
        private void InitApi()
        {
            String appId = "A3D96FA2-7314-2543-FF66-0B60549D7300";
            String secretKey = "C52669F7-B0BC-4D8F-FF19-D120A11B7500";
            Backendless.URL = "http://api.backendless.com";
            try
            {
                Backendless.InitApp(appId, secretKey);
                
            }
            catch (Exception e)
            {
                Console.Write($"Error {e.Message}");
            }
        }

        private void errorHandler(BackendlessFault fault)
        {
            Toast.MakeText(this, "error registering", ToastLength.Short);
        }


        public void RegisterDeviceInBackendless(string token)
        {
            String id = null;
            id = Build.Serial;
            var OS_VERSION = (Build.VERSION.SdkInt).ToString();
            var OS = "ANDROID";
            try
            {
                id = "504de480-1bec-4aed-bbf6-26e20a2128b8"; //UUID.RandomUUID().ToString();
            }
            catch (Exception e)
            {
               Console.Write($"Error {e.Message}");
            }

            var DEVICE_ID = id;
            //"174677789761"
            var deviceReg = new DeviceRegistration();
            deviceReg.Os = OS;
            deviceReg.OsVersion = OS_VERSION;
            deviceReg.Expiration = DateTime.Now.AddHours(3);
            deviceReg.DeviceId = DEVICE_ID;
            deviceReg.DeviceToken = token;
            Backendless.Messaging.DeviceRegistration = deviceReg;

            Backendless.Messaging.RegisterDevice(token, "default", new AsyncCallback<string>(responseHanlder, errorHandler));
        }

        private void responseHanlder(string response)
        {
            //Toast.MakeText(this, "Registered", ToastLength.Short);
            //Toast.MakeText(this, Backendless.Messaging.DeviceRegistration.DeviceId, ToastLength.Short).Show();
            string id = Backendless.Messaging.DeviceRegistration.DeviceId;
        }
        protected override void OnResume ()
		{
			base.OnResume ();
			LocalBroadcastManager.GetInstance (this).RegisterReceiver (mRegistrationBroadcastReceiver,
				new IntentFilter (QuickstartPreferences.REGISTRATION_COMPLETE));
		}

		protected override void OnPause ()
		{
			LocalBroadcastManager.GetInstance (this).UnregisterReceiver (mRegistrationBroadcastReceiver);
			base.OnPause ();
		}

		bool CheckPlayServices ()
		{
			int resultCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable (this);
			if (resultCode != ConnectionResult.Success) {
				if (GooglePlayServicesUtil.IsUserRecoverableError (resultCode)) {
					GooglePlayServicesUtil.GetErrorDialog (resultCode, this,
						PLAY_SERVICES_RESOLUTION_REQUEST).Show ();
				} else {
					Log.Info (TAG, "This device is not supported.");
					Finish ();
				}
				return false;
			}
			return true;
		}

		class BroadcastReceiver : Android.Content.BroadcastReceiver
		{
			public EventHandler<BroadcastEventArgs> Receive { get; set; }

			public override void OnReceive (Context context, Intent intent)
			{
				if (Receive != null)
					Receive (context, new BroadcastEventArgs (intent));
			}
		}

		class BroadcastEventArgs : EventArgs
		{
			public Intent Intent { get; private set; }

			public BroadcastEventArgs (Intent intent)
			{
				Intent = intent;
			}
		}
	}
}


