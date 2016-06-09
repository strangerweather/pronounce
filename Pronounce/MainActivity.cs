using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech.Tts;
using Android.Support.V7.App;
using Android.Views;
using Android.Media;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Content.Res;
using Android.Support.V4.Widget;
using Java.Util;
using System.Collections.Generic;
using System.Linq;
using Android.Util;

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


        
        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {

            //Get available languages
            var langAvailable = new List<string>();
            var localesAvailable = Java.Util.Locale.GetAvailableLocales().ToList();
            foreach (var locale in localesAvailable)
            {
                var res = tts.IsLanguageAvailable(locale);
                switch (res)
                {
                    case LanguageAvailableResult.Available:
                        langAvailable.Add(locale.DisplayName);
                        break;
                    case LanguageAvailableResult.CountryAvailable:
                        langAvailable.Add(locale.DisplayName);
                        break;
                    case LanguageAvailableResult.CountryVarAvailable:
                        langAvailable.Add(locale.DisplayName);
                        break;
                }
            }

            langAvailable = langAvailable.OrderBy(t => t).Distinct().ToList();
            var listLanguages = FindViewById<ListView>(Resource.Id.listoflanguages);
            var adapter2 = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, langAvailable);
            listLanguages.Adapter = adapter2;

            listLanguages.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>
        {
            lang = Java.Util.Locale.GetAvailableLocales().FirstOrDefault(t => t.DisplayLanguage == langAvailable[(int)e.Id]);
            // Do something with a click
            var languageSelected = langAvailable[(int)e.Id];
            Toast.MakeText(this, languageSelected, ToastLength.Long).Show();
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

            var toolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);
            //Toolbar will now take on default Action Bar characteristics
            SetActionBar(toolbar);
            //You can now use and reference the ActionBar
            ActionBar.Title = "Pronounce";
            Android.App.ActionBar ab = ActionBar;
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

            ////handle navigation
            //_navigationView.NavigationItemSelected += (sender, e) =>
            //{            
            //    e.MenuItem.SetChecked(true);

            //    switch (e.MenuItem.ItemId)
            //    {
            //        case Resource.Id.albanian:
            //            Toast.MakeText(this, "Albanian", ToastLength.Long).Show();
            //            Locale Albanian = new Locale("sq", "AL");
            //            tts.SetLanguage(Albanian);
            //            break;
            //        case Resource.Id.arabic:
            //            Toast.MakeText(this, "Arabic", ToastLength.Long).Show();
            //            Locale Arabic = new Locale("ar", "EG");
            //            tts.SetLanguage(Arabic);
            //            break;
            //        case Resource.Id.belarusian:
            //            Toast.MakeText(this, "Belarusian", ToastLength.Long).Show();
            //            Locale Belarusian = new Locale("be", "BY");
            //            tts.SetLanguage(Belarusian);
            //            break;
            //        case Resource.Id.bulgarian:
            //            Toast.MakeText(this, "Bulgarian", ToastLength.Long).Show();
            //            Locale Bulgarian = new Locale("bg", "BG");
            //            tts.SetLanguage(Bulgarian);
            //            break;
            //        case Resource.Id.catalan:
            //            Toast.MakeText(this, "Catalan", ToastLength.Long).Show();
            //            Locale Catalan = new Locale("ca", "ES");
            //            tts.SetLanguage(Catalan);
            //            break;
            //        case Resource.Id.chinese:
            //            Toast.MakeText(this, "Chinese ", ToastLength.Long).Show();
            //            Locale Chinese = new Locale("zh", "CN");
            //            tts.SetLanguage(Chinese);
            //            break;
            //        case Resource.Id.croatian:
            //            Toast.MakeText(this, "Croatian ", ToastLength.Long).Show();
            //            Locale Croatian = new Locale("hr", "HR");
            //            tts.SetLanguage(Croatian);
            //            break;
            //        case Resource.Id.czech:
            //            Toast.MakeText(this, "Czech ", ToastLength.Long).Show();
            //            Locale Czech = new Locale("cs", "CZ");
            //            tts.SetLanguage(Czech);
            //            break;
            //        case Resource.Id.danish:
            //            Toast.MakeText(this, "Danish ", ToastLength.Long).Show();
            //            Locale Danish = new Locale("da", "DK");
            //            tts.SetLanguage(Danish);
            //            break;
            //        case Resource.Id.dutch:
            //            Toast.MakeText(this, "Dutch ", ToastLength.Long).Show();
            //            Locale Dutch = new Locale("nl", "NL");
            //            tts.SetLanguage(Dutch);
            //            break;
            //        case Resource.Id.englishGB:
            //            Toast.MakeText(this, "English (GB) ", ToastLength.Long).Show();
            //            tts.SetLanguage(Locale.Uk);
            //            break;
            //        case Resource.Id.englishUS:
            //            Toast.MakeText(this, "English (US) ", ToastLength.Long).Show();
            //            tts.SetLanguage(Locale.Us);
            //            break;
            //        case Resource.Id.estonian:
            //            Toast.MakeText(this, "Estonian ", ToastLength.Long).Show();
            //            Locale Estonian = new Locale("et", "EE");
            //            tts.SetLanguage(Estonian);
            //            break;
            //        case Resource.Id.finnish:
            //            Toast.MakeText(this, "Finnish ", ToastLength.Long).Show();
            //            Locale Finnish = new Locale("fi", "FI");
            //            tts.SetLanguage(Finnish);
            //            break;
            //        case Resource.Id.french:
            //            Toast.MakeText(this, "French", ToastLength.Long).Show();
            //            tts.SetLanguage(Locale.French);
            //            break;
            //        case Resource.Id.german:
            //            Toast.MakeText(this, "German", ToastLength.Long).Show();
            //            tts.SetLanguage(Locale.German);
            //            break;
            //        case Resource.Id.greek:
            //            Toast.MakeText(this, "Greek", ToastLength.Long).Show();
            //            Locale Greek = new Locale("el", "GR");
            //            tts.SetLanguage(Greek);
            //            break;
            //        case Resource.Id.hebrew:
            //            Toast.MakeText(this, "Hebrew", ToastLength.Long).Show();
            //            Locale Hebrew = new Locale("iw", "IL");
            //            tts.SetLanguage(Hebrew);
            //            break;
            //        case Resource.Id.hindi:
            //            Toast.MakeText(this, "Hindi", ToastLength.Long).Show();
            //            Locale Hindi = new Locale("hi", "IN");
            //            tts.SetLanguage(Hindi);
            //            break;
            //        case Resource.Id.hungarian:
            //            Toast.MakeText(this, "Hungarian", ToastLength.Long).Show();
            //            Locale Hungarian = new Locale("hu", "HU");
            //            tts.SetLanguage(Hungarian);
            //            break;
            //        case Resource.Id.icelandic:
            //            Toast.MakeText(this, "Icelandic", ToastLength.Long).Show();
            //            Locale Icelandic = new Locale("is", "IS");
            //            tts.SetLanguage(Icelandic);
            //            break;
            //        case Resource.Id.indonesian:
            //            Toast.MakeText(this, "Indonesian", ToastLength.Long).Show();
            //            Locale Indonesian = new Locale("in", "ID");
            //            tts.SetLanguage(Indonesian);
            //            break;
            //        case Resource.Id.irish:
            //            Toast.MakeText(this, "Irish", ToastLength.Long).Show();
            //            Locale Irish = new Locale("ga", "IE");
            //            tts.SetLanguage(Irish);
            //            break;
            //        case Resource.Id.italian:
            //            Toast.MakeText(this, "Italian", ToastLength.Long).Show();
            //            tts.SetLanguage(Locale.Italian);
            //            break;
            //        case Resource.Id.japanese:
            //            Toast.MakeText(this, "Japanese ", ToastLength.Long).Show();
            //            Locale Japanese = new Locale("ja", "JP");
            //            tts.SetLanguage(Japanese);
            //            break;
            //        case Resource.Id.korean:
            //            Toast.MakeText(this, "Korean", ToastLength.Long).Show();
            //            tts.SetLanguage(Locale.Korean);
            //            break;
            //        case Resource.Id.latvian:
            //            Toast.MakeText(this, "Latvian ", ToastLength.Long).Show();
            //            Locale Latvian = new Locale("lv", "LV");
            //            tts.SetLanguage(Latvian);
            //            break;
            //        case Resource.Id.lithuanian:
            //            Toast.MakeText(this, "Lithuanian ", ToastLength.Long).Show();
            //            Locale Lithuanian = new Locale("it", "LT");
            //            tts.SetLanguage(Lithuanian);
            //            break;
            //        case Resource.Id.macedonian:
            //            Toast.MakeText(this, "Macedonian ", ToastLength.Long).Show();
            //            Locale Macedonian = new Locale("mk", "MK");
            //            tts.SetLanguage(Macedonian);
            //            break;
            //        case Resource.Id.malay:
            //            Toast.MakeText(this, "Malay ", ToastLength.Long).Show();
            //            Locale Malay = new Locale("ms", "MY");
            //            tts.SetLanguage(Malay);
            //            break;
            //        case Resource.Id.maltese:
            //            Toast.MakeText(this, "Maltese ", ToastLength.Long).Show();
            //            Locale Maltese = new Locale("mt", "MT");
            //            tts.SetLanguage(Maltese);
            //            break;
            //        case Resource.Id.norwegian:
            //            Toast.MakeText(this, "Norwegian  ", ToastLength.Long).Show();
            //            Locale Norwegian = new Locale("no", "NO");
            //            tts.SetLanguage(Norwegian);
            //            break;
            //        case Resource.Id.polish:
            //            Toast.MakeText(this, "Polish ", ToastLength.Long).Show();
            //            Locale Polish = new Locale("pl", "PL");
            //            tts.SetLanguage(Polish);
            //            break;
            //        case Resource.Id.portuguese:
            //            Toast.MakeText(this, "Portuguese", ToastLength.Long).Show();
            //            Locale Portuguese = new Locale("pt", "PT");
            //            tts.SetLanguage(Portuguese);
            //            break;
            //        case Resource.Id.romanian:
            //            Toast.MakeText(this, "Romanian", ToastLength.Long).Show();
            //            Locale Romanian = new Locale("ro", "RO");
            //            tts.SetLanguage(Romanian);
            //            break;
            //        case Resource.Id.russian:
            //            Toast.MakeText(this, "Russian", ToastLength.Long).Show();
            //            Locale Russian = new Locale("ru", "RU");
            //            tts.SetLanguage(Russian);
            //            break;
            //        case Resource.Id.serbian:
            //            Toast.MakeText(this, "Serbian ", ToastLength.Long).Show();
            //            Locale Serbian = new Locale("sr", "RS");
            //            tts.SetLanguage(Serbian);
            //            break;
            //        case Resource.Id.slovak:
            //            Toast.MakeText(this, "Slovak", ToastLength.Long).Show();
            //            Locale Slovak = new Locale("sk", "SK");
            //            tts.SetLanguage(Slovak);
            //            break;
            //        case Resource.Id.slovenian:
            //            Toast.MakeText(this, "Slovenian", ToastLength.Long).Show();
            //            Locale Slovenian = new Locale("sl", "SL");
            //            tts.SetLanguage(Slovenian);
            //            break;
            //        case Resource.Id.spanish:
            //            Toast.MakeText(this, "Spanish", ToastLength.Long).Show();
            //            Locale Spanish = new Locale("es", "ES");
            //            tts.SetLanguage(Spanish);
            //            break;
            //        case Resource.Id.swedish:
            //            Toast.MakeText(this, "Swedish", ToastLength.Long).Show();
            //            Locale Swedish = new Locale("sv", "SE");
            //            tts.SetLanguage(Swedish);
            //            break;
            //        case Resource.Id.thaiWD:
            //            Toast.MakeText(this, "Thai (Western Digits)", ToastLength.Long).Show();
            //            Locale ThaiWD = new Locale("th", "TH");
            //            tts.SetLanguage(ThaiWD);
            //            break;
            //        case Resource.Id.thaiTD:
            //            Toast.MakeText(this, "Thai (Thai Digits)", ToastLength.Long).Show();
            //            Locale ThaiTD = new Locale("th", "TH_TH");
            //            tts.SetLanguage(ThaiTD);
            //            break;
            //        case Resource.Id.turkish:
            //            Toast.MakeText(this, "Turkish", ToastLength.Long).Show();
            //            Locale Turkish = new Locale("tr", "TR");
            //            tts.SetLanguage(Turkish);
            //            break;
            //        case Resource.Id.ukrainian:
            //            Toast.MakeText(this, "Ukrainian", ToastLength.Long).Show();
            //            Locale Ukrainian = new Locale("uk", "UA");
            //            tts.SetLanguage(Ukrainian);
            //            break;
            //        case Resource.Id.vietnamese:
            //            Toast.MakeText(this, "Vietnamese", ToastLength.Long).Show();
            //            Locale Vietnamese = new Locale("vi", "VN");
            //            tts.SetLanguage(Vietnamese);
            //            break;
            //    }
            //};
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