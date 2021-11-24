﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TelegramDeliverer.ViewModels
{
    public class InputBox
    {

        Window Box = new Window();//window for the inputbox
        FontFamily font = new FontFamily(System.Drawing.Text.GenericFontFamilies.SansSerif);//font for the whole inputbox
        int FontSize = 14;//fontsize for the input
        StackPanel sp1 = new StackPanel();// items container
        string title = "";//title as heading
        string boxcontent;//title
        string defaulttext = "";//default textbox content
        string errormessage = "Error";//error messagebox content
        string errortitle = "Error";//error messagebox heading title
        string okbuttontext = "אישור";//Ok button content
        string CancelButtonText = "ביטול";
        Brush BoxBackgroundColor = Brushes.WhiteSmoke;// Window Background
        Brush InputBackgroundColor = Brushes.Ivory;// Textbox Background
        bool clickedOk = false;
        TextBox input = new TextBox();
        Button ok = new Button();
        Button cancel = new Button();
        bool inputreset = false;

        string output { get; set; }
        public InputBox(string content)
        {
            output = null;
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            windowdef();
        }

        public InputBox(string content, string Htitle, string DefaultText)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            try
            {
                title = Htitle;
            }
            catch
            {
                title = "Error!";
            }
            try
            {
                defaulttext = DefaultText;
            }
            catch
            {
                DefaultText = "Error!";
            }
            windowdef();
        }

        public InputBox(string content, string Htitle, string Font, int Fontsize)
        {
            try
            {
                boxcontent = content;
            }
            catch { boxcontent = "Error!"; }
            try
            {
                font = new FontFamily(Font);
            }
            catch { font = new FontFamily("Tahoma"); }
            try
            {
                title = Htitle;
            }
            catch
            {
                title = "Error!";
            }
            if (Fontsize >= 1)
                FontSize = Fontsize;
            windowdef();
        }

        private void windowdef()// window building - check only for window size
        {
            Box.Height = 100;// Box Height
            Box.Width = 450;// Box Width
            Box.Title = title;
            Box.Content = sp1;
            Box.Closing += Box_Closing;
            Box.WindowStyle = WindowStyle.None;
            Box.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            TextBlock content = new TextBlock();
            content.TextWrapping = TextWrapping.Wrap;
            content.Background = null;
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.Text = boxcontent;
            content.FontSize = FontSize;
            sp1.Children.Add(content);


            input.FontSize = FontSize;
            input.HorizontalAlignment = HorizontalAlignment.Center;
            input.Text = defaulttext;
            input.MinWidth = 200;
            input.MouseEnter += input_MouseDown;
            input.KeyDown += input_KeyDown;

            sp1.Children.Add(input);

            ok.Width = 70;
            ok.Height = 30;
            ok.Click += ok_Click;
            ok.Content = okbuttontext;

            cancel.Width = 70;
            cancel.Height = 30;
            cancel.Click += cancel_Click;
            cancel.Content = CancelButtonText;

            WrapPanel gboxContent = new WrapPanel();
            gboxContent.HorizontalAlignment = HorizontalAlignment.Center;

            sp1.Children.Add(gboxContent);
            gboxContent.Children.Add(ok);
            gboxContent.Children.Add(cancel);

            input.Focus();
        }

        void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //validation
        }

        private void input_MouseDown(object sender, MouseEventArgs e)
        {
            if ((sender as TextBox).Text == defaulttext && inputreset == false)
            {
                (sender as TextBox).Text = null;
                inputreset = true;
            }
        }

        private void input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && clickedOk == false)
            {
                e.Handled = true;
                ok_Click(input, null);
            }

            if (e.Key == Key.Escape)
            {
                cancel_Click(input, null);
            }
        }

        void ok_Click(object sender, RoutedEventArgs e)
        {
            clickedOk = true;
            foreach (var c in input.Text)
            {
                if (c < '0' || c > '9')
                {
                    MessageBox.Show("אנא הקלד ספרות בלבד");
                    return;
                }
            }
            if (input.Text == defaulttext || input.Text == "")
                MessageBox.Show(errormessage, errortitle, MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                Box.Close();
                output = input.Text;
            }
            clickedOk = false;
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            Box.Close();
            output = null;
        }

        public string ShowDialog()
        {
            Box.ShowDialog();
            return output;
        }
    }
}
