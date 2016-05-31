﻿using System;
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
        RadioGroup rg1;
        RadioGroup rg2;


        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {

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


            //Drawer
            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }

            //RadioGroups
            rg1 = (RadioGroup)FindViewById(Resource.Id.RadioGroup1);
            rg2 = (RadioGroup)FindViewById(Resource.Id.RadioGroup2);
            rg1.CheckedChange += Rg1_CheckedChange;
            rg2.CheckedChange += Rg2_CheckedChange;


            int chkId1 = rg1.CheckedRadioButtonId;
            int chkId2 = rg2.CheckedRadioButtonId;
            int realCheck = chkId1 == -1 ? chkId2 : chkId1;

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

            Button button = FindViewById<Button>(Resource.Id.MyButton);
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
                bottomSheetBehavior.PeekHeight = 100;
            }
            else
            {
                // it's a tablet
                bottomSheetBehavior.PeekHeight = 360;
            }

            bottomSheetBehavior.Hideable = false;

            bottomSheetBehavior.SetBottomSheetCallback(new MyBottomSheetCallBack());


            ////Speak button
            //button.Click += Button_Click;

            //Clear button
            clear_button.Click += delegate
            {
                if (!string.IsNullOrEmpty(editText.Text))
                {
                    editText.Text = "";
                }
            };
        }

        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
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
        //RadioGroups
        void Rg1_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            if (e.CheckedId != -1)
            {
                rg2.CheckedChange -= Rg2_CheckedChange;
                rg2.ClearCheck();
                rg2.CheckedChange += Rg2_CheckedChange;
            }
        }
        void Rg2_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            if (e.CheckedId != -1)
            {
                rg1.CheckedChange -= Rg1_CheckedChange;
                rg1.ClearCheck();
                rg1.CheckedChange += Rg1_CheckedChange;
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


//        // Choose language
//        public override bool OnOptionsItemSelected(IMenuItem item)
//        {
//            return base.OnOptionsItemSelected(item);
//        }

//        private void Button_Click(object sender, EventArgs e)
//        {
            
//            string text1 = editText.Text;


//            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
//            {
//                tts.Speak(text1, QueueMode.Flush, null, null);
//            }
//            else
//            {
//#pragma warning disable
//                tts.Speak(text1, QueueMode.Flush, null);
//            }
//        }



        //string toast = string.Format("{0}", spinner.GetItemAtPosition(e.Position));

        //Toast.MakeText(this, toast, ToastLength.Long).Show();

        //if (toast == "French")
        //{
        //    tts.SetLanguage(Locale.French);
        //}
        //else if (toast == "German")
        //{
        //    tts.SetLanguage(Locale.German);
        //}
        //else if (toast == "English (US)")
        //{
        //    tts.SetLanguage(Locale.Us);
        //}
        //else if (toast == "English (GB)")
        //{
        //    tts.SetLanguage(Locale.Uk);
        //}
        //else if (toast == "Italian")
        //{
        //    tts.SetLanguage(Locale.Italian);
        //}
        //else if (toast == "Japanese")
        //{
        //    tts.SetLanguage(Locale.Japanese);
        //}
        //else if (toast == "Korean")
        //{
        //    tts.SetLanguage(Locale.Korean);
        //}

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










