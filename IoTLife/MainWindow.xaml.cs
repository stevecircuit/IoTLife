using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IoTLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        //*******************************NUMERIC INPUT HANDLING****************************************
        private static readonly Regex _regex = new Regex("[^0-9.,]+");
        private static readonly Regex coma = new Regex("[,]+");
        //regex that matches disallowed text
        private static bool IsTextAllowed( string text)
        {
            return !_regex.IsMatch(text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void PreviewTextInputHandler(Object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }
        //*********************************************************************************************

        //*****************************CALCULATE*******************************************************
        private void Calc_btn_Click(object sender, RoutedEventArgs e)
        {
            //result.Text = slp_current_avg_prefix.SelectedValue.ToString() ;
            var textboxes = new[] { batt_cap, act_time, act_current_avg, slp_time, slp_current_avg };
            if (textboxes.Any(x => string.IsNullOrWhiteSpace(x.Text)))
            {
                MessageBox.Show("Values must be set!\n(Except self-discharge and cutoff limit)", "Correct input data!");
            }
            else
            {
                double activeCurrentDividerAmp = 0.0;
                switch(act_current_prefix.SelectedValue.ToString())
                {
                    case "mA":
                        activeCurrentDividerAmp = 1000.0;
                        break;
                    case "uA":
                        activeCurrentDividerAmp = 1000000.0;
                        break;
                    case "nA":
                        activeCurrentDividerAmp = 1000000000.0;
                        break;
                }
                double sleepCurrentDividerToAMP = 0.0;
                switch (slp_current_avg_prefix.SelectedValue.ToString())
                {
                    case "mA":
                        sleepCurrentDividerToAMP = 1000.0;
                        break;
                    case "uA":
                        sleepCurrentDividerToAMP = 1000000.0;
                        break;
                    case "nA":
                        sleepCurrentDividerToAMP = 1000000000.0;
                        break;
                }
                double activeTimeDividerToH = 0.0;
                switch (act_time_prefix.SelectedValue.ToString())
                {
                    case "h":
                        activeTimeDividerToH = 1.0;
                        break;
                    case "m":
                        activeTimeDividerToH = 60.0;
                        break;
                    case "s":
                        activeTimeDividerToH = 3600.0;
                        break;
                }
                double sleepTimeDividerToH = 0.0;
                switch (slp_time_prefix.SelectedValue.ToString())
                {
                    case "h":
                        sleepTimeDividerToH = 1.0;
                        break;
                    case "m":
                        sleepTimeDividerToH = 60.0;
                        break;
                    case "s":
                        sleepTimeDividerToH = 3600.0;
                        break;
                }
                double batteryCapapcityDividerToAh = 0.0;
                switch (batt_cap_prefix.SelectedValue.ToString())
                {
                    case "Ah":
                        batteryCapapcityDividerToAh = 1.0;
                        break;
                    case "mAh":
                        batteryCapapcityDividerToAh = 1000.0;
                        break;
                }

                double activeConsumption = (double.Parse(act_current_avg.Text, System.Globalization.CultureInfo.InvariantCulture) / activeCurrentDividerAmp) * (double.Parse(act_time.Text, System.Globalization.CultureInfo.InvariantCulture) / activeTimeDividerToH);
                double sleepCurrent = double.Parse(slp_current_avg.Text, System.Globalization.CultureInfo.InvariantCulture) / sleepCurrentDividerToAMP;
                double period = (double.Parse(slp_time.Text, System.Globalization.CultureInfo.InvariantCulture) / sleepTimeDividerToH) + (double.Parse(act_time.Text, System.Globalization.CultureInfo.InvariantCulture) / activeTimeDividerToH);
                double totalCurrent = sleepCurrent + (activeConsumption / period);
                double capacity = double.Parse(batt_cap.Text, System.Globalization.CultureInfo.InvariantCulture)/(batteryCapapcityDividerToAh);
                double totalCurrent1 = (activeConsumption + (sleepCurrent * (double.Parse(slp_time.Text, System.Globalization.CultureInfo.InvariantCulture) / sleepTimeDividerToH))) / ((double.Parse(act_time.Text, System.Globalization.CultureInfo.InvariantCulture) / activeTimeDividerToH) + (double.Parse(slp_time.Text, System.Globalization.CultureInfo.InvariantCulture) / sleepTimeDividerToH));

                double selfDischargeOffsetCurrent = 0.0;
                if (batt_cutdwn_pcnt.Text.Length > 0)
                {
                    double cutoff_pnct = double.Parse(batt_cutdwn_pcnt.Text, System.Globalization.CultureInfo.InvariantCulture);
                    capacity = capacity * ((100.0 - cutoff_pnct)/100.0);
                }
                if (batt_self_discharge_pcnt.Text.Length > 0)
                {
                    if(double.Parse(batt_self_discharge_pcnt.Text, System.Globalization.CultureInfo.InvariantCulture) != 0.0)
                    {
                        selfDischargeOffsetCurrent = capacity * double.Parse(batt_self_discharge_pcnt.Text, System.Globalization.CultureInfo.InvariantCulture) / 100.0 / 8760.0;
                        totalCurrent = totalCurrent + selfDischargeOffsetCurrent;
                        totalCurrent1 = totalCurrent1 + selfDischargeOffsetCurrent;
                    }
                }

                double timeToLive = capacity / totalCurrent;
                double timeToLive1 = capacity / totalCurrent1;
                double liveAVG = (timeToLive + timeToLive1) / 2.0;
                string outPut = string.Format("{0:N2}", (liveAVG / 24 / 365));
                result.Text = "Estimated battery life: "+ outPut.ToString() + " year(s).";
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text = textBox.Text.Replace(',','.'); //replace coma to dot becouse System.Globalization.CultureInfo.InvariantCulture doesn't worked
            textBox.CaretIndex = textBox.Text.Length; //set cursor back to the end

            //replace duplicated decimal points
            int count = 0;
            foreach (char c in textBox.Text)
                if (c == '.') count++;
            if(count > 1)
            {
                textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1, 1);
            }
            //

            if (result.Text.Length > 0)
            {
                result.Text = "Values changed!";
                //todo: recalculate
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(result != null) //nullreference exeption if not exist
            {
                result.Text = "Units changed!";
                //todo: recalculate
            }
        }

    }
}
