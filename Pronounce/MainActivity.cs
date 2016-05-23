using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using Java.Util;
using Android.Support.V7.App;
using Android.Views;
using Android.Media;
using Android.Content;
using Android.Util;

namespace Pronounce
{
    [Activity(Label = "Pronounce", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Pronounce")]
    public class MainActivity : AppCompatActivity, TextToSpeech.IOnInitListener
    {
        private TextToSpeech tts;
        EditText editText;
        SeekBar _seekBarAlarm;
        AudioManager mgr = null;

        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {

        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            SetContentView(Resource.Layout.Main);
            var toolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);
            //Toolbar will now take on default Action Bar characteristics
            SetActionBar(toolbar);
            //You can now use and reference the ActionBar
            ActionBar.Title = "PRONOUNCE";


            tts = new TextToSpeech(this.ApplicationContext, this);
            tts.SetLanguage(Java.Util.Locale.Default);

            Button button = FindViewById<Button>(Resource.Id.MyButton);
            Button clear_button = FindViewById<Button>(Resource.Id.button1);

            // Spinner
            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1);

            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.Languages, Android.Resource.Layout.SimpleSpinnerItem);


            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            //Clear button
            clear_button.Click += delegate
            {
                editText.Text = "";
            };

            //Speak button
            button.Click += Button_Click;

        }
        // Overflow button
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Overflow, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            editText = FindViewById<EditText>(Resource.Id.editText1);
            string text1 = editText.Text;


            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                tts.Speak(text1, QueueMode.Flush, null, null);
            }
            else
            {
#pragma warning disable
                tts.Speak(text1, QueueMode.Flush, null);
            }
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;


            string toast = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

            Toast.MakeText(this, toast, ToastLength.Long).Show();

            if (toast == "French")
            {
                tts.SetLanguage(Locale.French);
            }
            else if (toast == "German")
            {
                tts.SetLanguage(Locale.German);
            }
            else if (toast == "English (US)")
            {
                tts.SetLanguage(Locale.Us);
            }
            else if (toast == "English (GB)")
            {
                tts.SetLanguage(Locale.Uk);
            }
            else if (toast == "Italian")
            {
                tts.SetLanguage(Locale.Italian);
            }
            else if (toast == "Japanese")
            {
                tts.SetLanguage(Locale.Japanese);
            }
            else if (toast == "Korean")
            {
                tts.SetLanguage(Locale.Korean);
            }
        }
    }
}



