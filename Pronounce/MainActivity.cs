using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using Android.Support.V7.App;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Views;
using Android.Media;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Content.Res;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Java.Util;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using Java.Lang;
using System.Threading.Tasks;

namespace Pronounce
{
    [Activity(Label = "Pronounce", MainLauncher = true, Icon = "@drawable/icon1", Theme = "@style/Pronounce")]
    public class MainActivity : AppCompatActivity, TextToSpeech.IOnInitListener
    {
        private TextToSpeech tts;
        EditText editText;
        SeekBar _seekBarAlarm;
        AudioManager mgr = null;
        private DrawerLayout mDrawerLayout;
        private NavigationView _navigationView;
        List<string> items;
        ArrayAdapter<string> adapter;
        Java.Util.Locale lang;
        Android.Support.V7.Widget.Toolbar toolbar;


        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {
            Button languageButton = FindViewById<Button>(Resource.Id.languageButton);
            languageButton.Text = Pronounce.Helpers.Settings.Language;
            Toast.MakeText(this, Pronounce.Helpers.Settings.Language, ToastLength.Long).Show();
            lang = Java.Util.Locale.GetAvailableLocales().FirstOrDefault(t => t.DisplayLanguage == Helpers.Settings.Language);
            tts.SetLanguage(lang);

            //Get available languages
            var langAvailable = new List<string>();
            var localesAvailable = Java.Util.Locale.GetAvailableLocales().ToList();
            foreach (var locale in localesAvailable)
            {
                var res = tts.IsLanguageAvailable(locale);
                switch (res)
                {
                    case LanguageAvailableResult.Available:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                    case LanguageAvailableResult.CountryAvailable:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                    case LanguageAvailableResult.CountryVarAvailable:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                }
            }

            langAvailable = langAvailable.OrderBy(t => t).Distinct().ToList();
            var listLanguages = FindViewById<ListView>(Resource.Id.listoflanguages);

            var adapter2 = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, langAvailable);
            listLanguages.Adapter = adapter2;

            listLanguages.ItemClick += async (object sender, AdapterView.ItemClickEventArgs e) =>
        {
            Helpers.Settings.Language = langAvailable[(int)e.Id];
            Toast.MakeText(this, Helpers.Settings.Language, ToastLength.Long).Show();

            // if we get an error, default to the default language
            if (status == OperationResult.Error)
                tts.SetLanguage(Java.Util.Locale.Default);
            // if the listener is ok, set the lang
            if (status == OperationResult.Success)
            {
                lang = Java.Util.Locale.GetAvailableLocales().FirstOrDefault(t => t.DisplayLanguage == Helpers.Settings.Language);
                tts.SetLanguage(lang);
                languageButton.Text = Pronounce.Helpers.Settings.Language;
            }

            await Task.Delay(500);
            mDrawerLayout.CloseDrawers();
        };
   }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            SetContentView(Resource.Layout.Main);


            //Status bar
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            }

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.my_toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Pronounce";
            SupportActionBar ab = SupportActionBar;
            ab.SetHomeAsUpIndicator(Resource.Drawable.ic_menu1);
            ab.SetDisplayHomeAsUpEnabled(true);


            //History
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            editText = FindViewById<EditText>(Resource.Id.editText1);
            items = new List<string>(new[] { "History" });
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, items);
            listView.Adapter = adapter;
            FindViewById<Button>(Resource.Id.MyButton).Click += HandleClick;

            listView.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs args)
            {
                editText.Text = ((TextView)args.View).Text;
            };



            //Drawer
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }

            // Volume bar setup
            mgr = (AudioManager)GetSystemService(Context.AudioService);

            ////
            _seekBarAlarm = FindViewById<SeekBar>(Resource.Id.seekBar1);

            // modify the ring
            initBar(_seekBarAlarm, Android.Media.Stream.Music);

            /// <summary>
            /// initBar
            /// </summary>
            /// <param name="bar"></param>
            /// <param name="stream"></param>
            /// 


            //tts
            tts = new TextToSpeech(this.ApplicationContext, this);
            tts.SetLanguage(Java.Util.Locale.Default);

            Button dbutton = FindViewById<Button>(Resource.Id.MyButton);
            Button clear_button = FindViewById<Button>(Resource.Id.button1);
            editText = FindViewById<EditText>(Resource.Id.editText1);


            //Bottom sheet

            LinearLayout sheet = FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
            BottomSheetBehavior bottomSheetBehavior = BottomSheetBehavior.From(sheet);

            var metrics = Resources.DisplayMetrics;
            var heightInDp = ConvertPixelsToDp(metrics.HeightPixels);

            if (heightInDp <= 731)
            {
                // it's a phone
                bottomSheetBehavior.PeekHeight = 120;
            }
            else
            {
                // it's a tablet
                bottomSheetBehavior.PeekHeight = 360;
            }

            bottomSheetBehavior.Hideable = false;

            bottomSheetBehavior.SetBottomSheetCallback(new MyBottomSheetCallBack());


            //Speak button
            dbutton.Click += Button_Click;


            //Clear button
            clear_button.Click += delegate
            {
                if (!string.IsNullOrEmpty(editText.Text))
                {
                    editText.Text = "";
                }
            };

            //Clear history button
            Button clear_history = FindViewById<Button>(Resource.Id.button3);
            clear_history.Click += delegate
            {
                adapter.Clear();
                adapter.NotifyDataSetChanged();
            };


            //setup navigation view
            _navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

        }

        //
        //   METHODS
        //

        // Pixel conversion to dp
        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }



        //History
        protected void HandleClick(object sender, EventArgs e)
        {
            editText = FindViewById<EditText>(Resource.Id.editText1);
            adapter.Add(editText.Text);
            adapter.NotifyDataSetChanged();
        }

        //Open drawer
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }


        // Drawer contents
        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {
                e.MenuItem.SetChecked(true);
                mDrawerLayout.CloseDrawers();
            };
        }


        // Overflow button
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Overflow, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        // Volume bar
        private void initBar(SeekBar bar, Android.Media.Stream stream)
        {
            //
            bar.Max = mgr.GetStreamMaxVolume(stream);
            bar.Progress = mgr.GetStreamVolume(stream);

            //
            bar.SetOnSeekBarChangeListener(new VolumeListener(mgr, stream));
        }

        public class VolumeListener : Java.Lang.Object, SeekBar.IOnSeekBarChangeListener
        {
            AudioManager audio;
            Android.Media.Stream theStream;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="mgr"></param>
            public VolumeListener(AudioManager mgr, Android.Media.Stream stream)
            {
                // /!\ is it a correct way to pass information in the constructor ?
                theStream = stream;
                audio = mgr;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="seekBar"></param>
            /// <param name="progress"></param>
            /// <param name="fromUser"></param>
            public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
            {
                if (fromUser)
                {
                    string.Format("The user adjusted the value of the SeekBar to {0}", seekBar.Progress);

                    // play sound
                    audio.SetStreamVolume(theStream, progress, VolumeNotificationFlags.PlaySound);

                }
            }
            public void OnStartTrackingTouch(SeekBar seekBar)
            {
                System.Diagnostics.Debug.WriteLine("Tracking changes.");
            }
            public void OnStopTrackingTouch(SeekBar seekBar)
            {
                System.Diagnostics.Debug.WriteLine("Stopped tracking changes.");
            }
        }

        // Speak
        private void Button_Click(object sender, EventArgs e)
        {

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

        public class MyBottomSheetCallBack : BottomSheetBehavior.BottomSheetCallback
        {
            public override void OnSlide(View bottomSheet, float slideOffset)
            {
                //Sliding
            }

            public override void OnStateChanged(View bottomSheet, int newState)
            {
                //State changed
            }
        }
    }
}
