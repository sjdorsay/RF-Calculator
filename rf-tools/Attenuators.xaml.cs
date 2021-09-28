using System;
using System.ComponentModel;
using System.Windows;

// iText7 PDF .NET Module

namespace rf_tools
{
    public class Attenuator : INotifyPropertyChanged
    {
        private double res1;
        private double res2;
        private double res3;
        private double res4;

        private double impedance;

        private double attenuation;
        private double tolerance;

        public double Res1
        {
            get { return res1; }
            set { res1 = value; NotifyPropertyChanged("Res1"); }
        }

        public double Res2
        {
            get { return res2; }
            set { res2 = value; NotifyPropertyChanged("Res2"); }
        }

        public double Res3
        {
            get { return res3; }
            set { res3 = value; NotifyPropertyChanged("Res3"); }
        }

        public double Res4
        {
            get { return res4; }
            set { res4 = value; NotifyPropertyChanged("Res4"); }
        }

        public double Impedance
        {
            get { return impedance; }
            set { impedance = value; NotifyPropertyChanged("Impedance"); }
        }

        public double Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; NotifyPropertyChanged("Attenuation"); }
        }

        public double Tolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    /// <summary>
    /// Interaction logic for attenuators.xaml
    /// </summary>
    public partial class Attenuators : Window
    {
        private bool attenRunSynthesis = false;
        private bool attenRunEvaluate = false;

        private bool genReport = false;

        readonly Attenuator pi_atten = new Attenuator();
        readonly Attenuator tee_atten = new Attenuator();
        readonly Attenuator btee_atten = new Attenuator();
        readonly Attenuator refl_atten = new Attenuator();

        /** Attenuator Initilazation Function **/
        public Attenuators()
        {
            InitializeComponent();

            attPiTab.DataContext = pi_atten;
            attTeeTab.DataContext = tee_atten;
            attBTeeTab.DataContext = btee_atten;
            attReflTab.DataContext = refl_atten;
        }

        /** Calculate Standard Resistance Value **/
        private double GetStdResistor(double resistance, double tolerance)
        {
            // Default resistor is 0 Ohms
            double stdRes = 0;
            int esr;

            // Initialize Auxiliary Variables
            double whole;
            int inte;
            double deci;

            switch (tolerance)
            {
                case 0.1:
                    esr = 192;
                    break;
                case 0.5:
                    esr = 192;
                    break;
                case 1:
                    esr = 96;
                    break;
                case 2:
                    esr = 48;
                    break;
                case 5:
                    esr = 24;
                    break;
                default:
                    esr = 96;
                    break;
            }

            // Calculate the value increments
            double inc = Math.Pow(10, 1 / esr);

            // Find the closest power
            if (resistance != 0)
            {
                whole = Math.Log10(resistance);
                inte = (int)whole;
                deci = whole - inte;

                double exp = 2 + (Math.Round(esr * deci) / esr);

                stdRes = Math.Round(Math.Pow(10, exp));
                stdRes = stdRes * Math.Pow(10, inte - 2);
            }

            return stdRes;
        }

        /** PI Attenuator - Synthesis / Evalute **/
        public void atten_pi_run()
        {
            // Initialize resistor values
            double r1 = 0;
            double r2 = 1e6;

            double a;
            double imped;
            double tol;

            if (attenRunSynthesis)
            {
                a = Math.Pow(10, pi_atten.Attenuation / 20);
                imped = pi_atten.Impedance;
                tol = pi_atten.Tolerance;

                if (1 < a)
                {
                    r1 = imped * (a + 1) / (a - 1);
                    r2 = imped * (Math.Pow(a, 2) - 1) / (2 * a);
                }

                pi_atten.Res1 = GetStdResistor(r1, tol);
                pi_atten.Res2 = GetStdResistor(r2, tol);
            }

            if (attenRunEvaluate)
            {
                r1 = pi_atten.Res1;
                r2 = pi_atten.Res2;

                double A = 1 + (r2 / r1);
                double B = r2;
                double D = 1 + (r2 / r1);
                double C = (D + 1) / r1;

                imped = Math.Sqrt(A * B / (C * D));

                a = (A + B / imped + C * imped + D) / 2;
                double atten = 20 * Math.Log10(Math.Abs(a));

                pi_atten.Attenuation = atten;
                pi_atten.Impedance = imped;
            }

            if (genReport)
            {

            }
        }

        /** Tee Attenuator - Synthesis / Evalute **/
        public void atten_tee_run()
        {
            // Initialize resistor values for a 0 dB attenuator
            double r1 = 0;
            double r2 = 1e6;

            double a;
            double imped;
            double tol;

            if (attenRunSynthesis)
            {
                a = Math.Pow(10, tee_atten.Attenuation / 20);
                imped = tee_atten.Impedance;
                tol = tee_atten.Tolerance;

                // Generate ideal resistor values
                if (1 < a)
                {
                    r1 = imped * (a - 1) / (a + 1);
                    r2 = 2 * imped * a / (Math.Pow(a, 2) - 1);
                }

                tee_atten.Res1 = GetStdResistor(r1, tol);
                tee_atten.Res2 = GetStdResistor(r2, tol);
            }

            if (attenRunEvaluate)
            {
                r1 = tee_atten.Res1;
                r2 = tee_atten.Res2;

                double A = 1 + (r1 / r2);
                double B = r1 * (A + 1);
                double C = 1 / r2;
                double D = 1 + (r1 / r2);

                imped = Math.Sqrt(D * B / (C * A));

                a = (A + B / imped + C * imped + D) / 2;
                double atten = 20 * Math.Log10(Math.Abs(a));

                tee_atten.Attenuation = atten;
                tee_atten.Impedance = imped;
            }
        }

        /** Bridged Tee Attenuator - Synthesis / Evalute **/
        public void atten_btee_run()
        {
            // Initialize resistor values for a 0 dB attenuator
            double r1 = 1e6;
            double r2 = 0;
            double r3 = 0;
            double r4 = 1e6;

            // Instantiate useful variables
            double a;
            double imped;
            double tol;

            if (attenRunSynthesis)
            {
                a = Math.Pow(10, btee_atten.Attenuation / 20);
                imped = btee_atten.Impedance;
                tol = btee_atten.Tolerance;

                // Generate ideal resistor values
                if (1 < a)
                {
                    r1 = imped * (a - 1);
                    r2 = imped;
                    r3 = imped;
                    r4 = imped / (a - 1);
                }

                btee_atten.Res1 = GetStdResistor(r1, tol);
                btee_atten.Res2 = GetStdResistor(r2, tol);
                btee_atten.Res3 = GetStdResistor(r3, tol);
                btee_atten.Res4 = GetStdResistor(r4, tol);
            }

            if (attenRunEvaluate)
            {
                r1 = btee_atten.Res1;
                r2 = btee_atten.Res2;
                r3 = btee_atten.Res3;
                r4 = btee_atten.Res4;

                // Calculate supplemental equations for determining ABCD matrix
                double x0 = r1 + r3 + r2 * (1 + (r3 / r4));
                double x1 = r4 * (r1 + r2) + r3 * (r2 + r4);
                double x2 = x0 - r1;

                // Find the ABCD matrix parameters
                double A = 1 + r1 * (r2 / x1);
                double B = r1 * (x2 / x0);
                double C = (r1 + r2 + r3) / x1;
                double D = 1 + r1 * (r3 / x1);

                // Use the ABCD matrix to solve for the impedance value
                imped = Math.Sqrt(D * B / (C * A));

                // Use the ABCD matrix to solve for the attenuation
                a = (A + B / imped + C * imped + D) / 2;
                double atten = 20 * Math.Log10(Math.Abs(a));

                // Update properties with new values
                btee_atten.Attenuation = atten;
                btee_atten.Impedance = imped;
            }
        }

        /** Reflection Attenuator - Synthesis / Evalute **/
        public void atten_refl_run()
        {
            // Initialize resistor values for a 0 dB attenuator
            double r1 = 1e6;
            double r2 = 0;

            double a;
            double imped;
            double tol;

            if (attenRunSynthesis)
            {
                a = Math.Pow(10, refl_atten.Attenuation / 20);
                imped = refl_atten.Impedance;
                tol = refl_atten.Tolerance;

                // Generate ideal resistor values
                if (1 < a)
                {
                    r1 = imped * (a - 1) / (a + 1);
                    r2 = imped * (a + 1) / (a - 1);
                }

                refl_atten.Res1 = GetStdResistor(r1, tol);
                refl_atten.Res2 = GetStdResistor(r2, tol);
            }

            if (attenRunEvaluate)
            {
                r1 = refl_atten.Res1;
                r2 = refl_atten.Res2;

                imped = 50;
                double atten = 20 * Math.Log10(1.4125);

                refl_atten.Attenuation = atten;
                refl_atten.Impedance = imped;
            }
        }

        /** Synthesis Click Handler **/
        private void atten_synthesis_click(object sender, RoutedEventArgs e)
        {
            attenRunSynthesis = true;
            attenRunEvaluate = false;

            // Assign the permitivity value
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    atten_pi_run();
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    atten_tee_run();
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    atten_btee_run();
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:
                    atten_refl_run();
                    break;

                /**Default: PI Attenuator**/
                default:
                    atten_pi_run();
                    break;
            }

            attenRunSynthesis = false;
            attenRunEvaluate = false;
        }

        /** Evaluate Click Handler **/
        private void atten_evaluate_click(object sender, RoutedEventArgs e)
        {
            attenRunSynthesis = false;
            attenRunEvaluate = true;

            // Assign the permitivity value
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    atten_pi_run();
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    atten_tee_run();
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    atten_btee_run();
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:
                    atten_refl_run();
                    break;

                /**Default: PI Attenuator**/
                default:
                    atten_pi_run();
                    break;
            }

            attenRunSynthesis = false;
            attenRunEvaluate = false;
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            genReport = true;
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            genReport = false;
        }
    }
}
