﻿using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using TextAlignment = iText.Layout.Properties.TextAlignment;

// iText7 PDF .NET Module

namespace rf_tools
{
    public class Attenuator : INotifyPropertyChanged
    {
        private double impedance;

        private double attenuation;
        private double tolerance;

        private bool genReport;

        public bool GenReport
        {
            get { return genReport; }
            set { genReport = value; NotifyPropertyChanged("GenReport"); }
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
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class PiAttenuator : Attenuator
    {
        private double res1;
        private double res2;

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
    }

    public class TeeAttenuator : Attenuator
    {
        private double res1;
        private double res2;

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
    }

    public class BTeeAttenuator : Attenuator
    {
        private double res1;
        private double res2;
        private double res3;
        private double res4;

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
    }

    public class ReflAttenuator : Attenuator
    {
        private double res1;
        private double res2;

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
    }

    /// <summary>
    /// Interaction logic for attenuators.xaml
    /// </summary>
    public partial class Attenuators : Window
    {
        private bool attenRunSynthesis = false;
        private bool attenRunEvaluate = false;

        private readonly PiAttenuator pi_atten = new PiAttenuator();
        private readonly TeeAttenuator tee_atten = new TeeAttenuator();
        private readonly BTeeAttenuator btee_atten = new BTeeAttenuator();
        private readonly ReflAttenuator refl_atten = new ReflAttenuator();

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
                stdRes *= Math.Pow(10, inte - 2);
            }

            return stdRes;
        }

        /** Calculate Attenuation of PI Attenuator **/
        private double Atten_PI_GetAttenuation(double r1, double r2, double r3)
        {
            double imped;
            double a;
            double atten;
            double[] ABCD = new double[4];

            ABCD[0] = 1 + (r2 / r3);
            ABCD[1] = r2;
            ABCD[3] = 1 + (r2 / r1);
            ABCD[2] = (1 / r1) + ABCD[3] * (1 / r3);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedanceof PI Attenuator **/
        private double Atten_PI_GetImpedance(double r1, double r2, double r3)
        {
            double imped;
            double[] ABCD = new double[4];

            ABCD[0] = 1 + (r2 / r3);
            ABCD[1] = r2;
            ABCD[3] = 1 + (r2 / r1);
            ABCD[2] = (1 / r1) + ABCD[3] * (1 / r3);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
        }

        /** Export LTSpice of PI Attenuator **/
        private void Atten_PI_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            string UserName = Environment.UserName;
            string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

            adapter.AddSource(1, 0, new LTSpiceCoords(-48, 16, 0));
            adapter.AddResistor(1, 0, new LTSpiceCoords(0, 16, 0),
                string.Format("{{mc({0}, tolR)}}", pi_atten.Res1));
            adapter.AddResistor(2, 1, new LTSpiceCoords(144, 0, 90),
                string.Format("{{mc({0}, tolR)}}", pi_atten.Res2));
            adapter.AddResistor(2, 0, new LTSpiceCoords(128, 16, 0),
                string.Format("{{mc({0}, tolR)}}", pi_atten.Res1));
            adapter.AddResistor(2, 0, new LTSpiceCoords(176, 16, 0), "50");

            adapter.AddNetSim(0, -128, 100000000, 4000000000, 100);
            adapter.AddParameter(0, -64, "tolR", (float)pi_atten.Tolerance / 100);

            File.WriteAllText(DocuPath + "pi_attenuator.asc", adapter.ToString());
        }

        /** PI Attenuator - Synthesis / Evalute **/
        public void Atten_PI_Run()
        {
            // Initialize resistor values
            double r1 = 0;
            double r2 = 1e6;

            double a;
            double imped;
            double tol;

            bool genReport = pi_atten.GenReport;

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

                pi_atten.Attenuation = Atten_PI_GetAttenuation(r1, r2, r1);
                pi_atten.Impedance = Atten_PI_GetImpedance(r1, r2, r1);
            }

            if (genReport)
            {
                double[] attenuation = new double[3];

                attenuation[0] = pi_atten.Attenuation;
                attenuation[1] = pi_atten.Attenuation;
                attenuation[2] = pi_atten.Attenuation;

                double[] impedance = new double[3];

                impedance[0] = pi_atten.Impedance;
                impedance[1] = pi_atten.Impedance;
                impedance[2] = pi_atten.Impedance;

                tol = pi_atten.Tolerance / 100;

                double tempAtten;
                double tempImpedance;

                int Ntot = 8;

                // Find the maximum and minimum attenuation and impedance
                for (int i = 0; i < Ntot; i++)
                {
                    tempAtten = Atten_PI_GetAttenuation(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r1 * (1 + (2 * (4 & i) - 1) * tol));

                    tempImpedance = Atten_PI_GetImpedance(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r1 * (1 + (2 * (4 & i) - 1) * tol));

                    if (tempAtten < attenuation[0]) attenuation[0] = tempAtten;
                    if (tempAtten > attenuation[2]) attenuation[2] = tempAtten;

                    if (tempImpedance < impedance[0]) impedance[0] = tempImpedance;
                    if (tempImpedance > impedance[2]) impedance[2] = tempImpedance;
                }

                Gen_Report(attenuation, impedance, new double[] { r1, r2, r1 });
            }
        }

        /** Calculate Attenuation of Tee Attenuator **/
        private double Atten_TEE_GetAttenuation(double r1, double r2, double r3)
        {
            double imped;
            double a;
            double atten;
            double[] ABCD = new double[4];

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 + r3 * ABCD[0];
            ABCD[2] = 1 / r2;
            ABCD[3] = 1 + (r3 / r2);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedance of Tee Attenuator **/
        private double Atten_TEE_GetImpedance(double r1, double r2, double r3)
        {
            double imped;
            double[] ABCD = new double[4];

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 + r3 * ABCD[0];
            ABCD[2] = 1 / r2;
            ABCD[3] = 1 + (r3 / r2);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
        }

        /** Export LTSpice of TEE Attenuator **/
        private void Atten_TEE_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            string UserName = Environment.UserName;
            string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

            adapter.AddSource(1, 0, new LTSpiceCoords(-48, 16, 0));
            adapter.AddResistor(2, 1, new LTSpiceCoords(96, 0, 90),
                string.Format("{{mc({0}, tolR)}}", tee_atten.Res1));
            adapter.AddResistor(2, 0, new LTSpiceCoords(80, 16, 0),
                string.Format("{{mc({0}, tolR)}}", tee_atten.Res2));
            adapter.AddResistor(3, 2, new LTSpiceCoords(224, 0, 90),
                string.Format("{{mc({0}, tolR)}}", tee_atten.Res1));
            adapter.AddResistor(3, 0, new LTSpiceCoords(208, 16, 0), "50");

            adapter.AddNetSim(0, -128, 100000000, 4000000000, 100);
            adapter.AddParameter(0, -64, "tolR", (float)tee_atten.Tolerance / 100);

            File.WriteAllText(DocuPath + "tee_attenuator.asc", adapter.ToString());
        }

        /** Tee Attenuator - Synthesis / Evalute **/
        public void Atten_TEE_Run()
        {
            // Initialize resistor values for a 0 dB attenuator
            double r1 = 0;
            double r2 = 1e6;

            double a;
            double imped;
            double tol;

            bool genReport = tee_atten.GenReport;

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

                tee_atten.Attenuation = Atten_TEE_GetAttenuation(r1, r2, r1);
                tee_atten.Impedance = Atten_TEE_GetImpedance(r1, r2, r1);
            }

            if (genReport)
            {
                double[] attenuation = new double[3];

                attenuation[0] = tee_atten.Attenuation;
                attenuation[1] = tee_atten.Attenuation;
                attenuation[2] = tee_atten.Attenuation;

                double[] impedance = new double[3];

                impedance[0] = tee_atten.Impedance;
                impedance[1] = tee_atten.Impedance;
                impedance[2] = tee_atten.Impedance;

                tol = tee_atten.Tolerance / 100;

                double tempAtten;
                double tempImpedance;

                int Ntot = 8;

                // Find the maximum and minimum attenuation and impedance
                for (int i = 0; i < Ntot; i++)
                {
                    tempAtten = Atten_TEE_GetAttenuation(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r1 * (1 + (2 * (4 & i) - 1) * tol));

                    tempImpedance = Atten_TEE_GetImpedance(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r1 * (1 + (2 * (4 & i) - 1) * tol));

                    if (tempAtten < attenuation[0]) attenuation[0] = tempAtten;
                    if (tempAtten > attenuation[2]) attenuation[2] = tempAtten;

                    if (tempImpedance < impedance[0]) impedance[0] = tempImpedance;
                    if (tempImpedance > impedance[2]) impedance[2] = tempImpedance;
                }

                Gen_Report(attenuation, impedance, new double[] { r1, r2, r1 });
            }
        }

        /** Calculate Attenuation of BTee Attenuator **/
        private double Atten_BTEE_GetAttenuation(double r1, double r2, double r3, double r4)
        {
            double imped;
            double a;
            double atten;
            double[] ABCD = new double[4];

            // Calculate supplemental equations for determining ABCD matrix
            double x0 = r1 + r3 + r2 * (1 + (r3 / r4));
            double x1 = r4 * (r1 + r2) + r3 * (r2 + r4);
            double x2 = x0 - r1;

            // Find the ABCD matrix parameters
            ABCD[0] = 1 + r1 * (r2 / x1);
            ABCD[1] = r1 * (x2 / x0);
            ABCD[2] = (r1 + r2 + r3) / x1;
            ABCD[3] = 1 + r1 * (r3 / x1);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedanceof BTee Attenuator **/
        private double Atten_BTEE_GetImpedance(double r1, double r2, double r3, double r4)
        {
            double imped;
            double[] ABCD = new double[4];

            // Calculate supplemental equations for determining ABCD matrix
            double x0 = r1 + r3 + r2 * (1 + (r3 / r4));
            double x1 = r4 * (r1 + r2) + r3 * (r2 + r4);
            double x2 = x0 - r1;

            // Find the ABCD matrix parameters
            ABCD[0] = 1 + r1 * (r2 / x1);
            ABCD[1] = r1 * (x2 / x0);
            ABCD[2] = (r1 + r2 + r3) / x1;
            ABCD[3] = 1 + r1 * (r3 / x1);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
        }

        /** Export LTSpice of BTEE Attenuator **/
        private void Atten_BTEE_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            string UserName = Environment.UserName;
            string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

            adapter.AddSource(1, 0, new LTSpiceCoords(-48, 16, 0));
            adapter.AddResistor(2, 1, new LTSpiceCoords(96, 0, 90),
                string.Format("{{mc({0}, tolR)}}", btee_atten.Res2));
            adapter.AddResistor(2, 0, new LTSpiceCoords(80, 16, 0),
                string.Format("{{mc({0}, tolR)}}", btee_atten.Res4));
            adapter.AddResistor(3, 2, new LTSpiceCoords(224, 0, 90),
                string.Format("{{mc({0}, tolR)}}", btee_atten.Res3));
            adapter.AddResistor(3, 1, new LTSpiceCoords(144, -32, 90),
                string.Format("{{mc({0}, tolR)}}", btee_atten.Res1));
            adapter.AddResistor(3, 0, new LTSpiceCoords(208, 16, 0), "50");

            adapter.AddNetSim(0, -128, 100000000, 4000000000, 100);
            adapter.AddParameter(0, -64, "tolR", (float)btee_atten.Tolerance / 100);

            File.WriteAllText(DocuPath + "btee_attenuator.asc", adapter.ToString());
        }

        /** Bridged Tee Attenuator - Synthesis / Evalute **/
        public void Atten_BTEE_Run()
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

            bool genReport = btee_atten.GenReport;

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

                // Update properties with new values
                btee_atten.Attenuation = Atten_BTEE_GetAttenuation(r1, r2, r3, r4);
                btee_atten.Impedance = Atten_BTEE_GetImpedance(r1, r2, r3, r4);
            }

            if (genReport)
            {
                double[] attenuation = new double[4];

                attenuation[0] = btee_atten.Attenuation;
                attenuation[1] = btee_atten.Attenuation;
                attenuation[2] = btee_atten.Attenuation;

                double[] impedance = new double[4];

                impedance[0] = btee_atten.Impedance;
                impedance[1] = btee_atten.Impedance;
                impedance[2] = btee_atten.Impedance;

                tol = btee_atten.Tolerance / 100;

                double tempAtten;
                double tempImpedance;

                int Ntot = 16;

                // Find the maximum and minimum attenuation and impedance
                for (int i = 0; i < Ntot; i++)
                {
                    tempAtten = Atten_BTEE_GetAttenuation(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r3 * (1 + (2 * (4 & i) - 1) * tol),
                        r4 * (1 + (2 * (8 & i) - 1) * tol));

                    tempImpedance = Atten_BTEE_GetImpedance(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r3 * (1 + (2 * (4 & i) - 1) * tol),
                        r4 * (1 + (2 * (8 & i) - 1) * tol));

                    if (tempAtten < attenuation[0]) attenuation[0] = tempAtten;
                    if (tempAtten > attenuation[2]) attenuation[2] = tempAtten;

                    if (tempImpedance < impedance[0]) impedance[0] = tempImpedance;
                    if (tempImpedance > impedance[2]) impedance[2] = tempImpedance;
                }

                Gen_Report(attenuation, impedance, new double[] { r1, r2, r3, r4 });
            }
        }

        /** Calculate Attenuation of Reflection Attenuator **/
        private double Atten_Refl_GetAttenuation(double r1, double r2)
        {
            double imped;
            double a;
            double atten;

            imped = Math.Sqrt(r1 * r2);

            a = (imped + r1) / (imped - r1);
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedanceof Reflection Attenuator **/
        private double Atten_Refl_GetImpedance(double r1, double r2)
        {
            double imped = Math.Sqrt(r1 * r2);

            return imped;
        }

        /** Reflection Attenuator - Synthesis / Evalute **/
        public void Atten_Refl_Run()
        {
            // Initialize resistor values for a 0 dB attenuator
            double r1 = 1e6;
            double r2 = 0;

            double a;
            double imped;
            double tol;

            bool genReport = refl_atten.GenReport;

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

                refl_atten.Attenuation = Atten_Refl_GetAttenuation(r1, r2); ;
                refl_atten.Impedance = Atten_Refl_GetImpedance(r1, r2); ;
            }

            if (genReport)
            {
                double[] attenuation = new double[4];

                attenuation[0] = refl_atten.Attenuation;
                attenuation[1] = refl_atten.Attenuation;
                attenuation[2] = refl_atten.Attenuation;

                double[] impedance = new double[4];

                impedance[0] = refl_atten.Impedance;
                impedance[1] = refl_atten.Impedance;
                impedance[2] = refl_atten.Impedance;

                tol = refl_atten.Tolerance / 100;

                double tempAtten;
                double tempImpedance;

                int Ntot = 2 ^ attenuation.Length;

                // Find the maximum and minimum attenuation and impedance
                for (int i = 0; i < Ntot; i++)
                {
                    tempAtten = Atten_Refl_GetAttenuation(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol));

                    tempImpedance = Atten_Refl_GetImpedance(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol));

                    if (tempAtten < attenuation[0]) attenuation[0] = tempAtten;
                    if (tempAtten > attenuation[2]) attenuation[2] = tempAtten;

                    if (tempImpedance < impedance[0]) impedance[0] = tempImpedance;
                    if (tempImpedance > impedance[2]) impedance[2] = tempImpedance;
                }

                Gen_Report(attenuation, impedance, new double[] { r1, r2 });
            }
        }

        /** Report Generation Function **/
        private void Gen_Report(double[] atten, double[] imped, double[] resList)
        {
            // Get path to user documents assuming C drive
            string UserName = Environment.UserName;
            string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

            // Must have write permissions to the path folder
            PdfWriter writer = new PdfWriter(DocuPath + "Attenuator_Report.pdf");
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            double a = Math.Pow(10, atten[1] / 20);
            double maxPwr = 18 + 10 * (2.647 / (2.647 + a));

            /** Setup Document Formats **/
            // Setup Title formatting
            Paragraph title = new Paragraph("Attenuator Report")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(24);

            // Setup Header formatting
            Paragraph header = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(16);

            // Setup body formatting
            Paragraph body = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(12);

            /** Generature Document **/
            // Add title
            document.Add(title);

            // Section 1 - Attenuator
            header.Add("1. Attenuator");

            for (int i = 0; i < resList.Length; i++)
                body.Add(string.Format("R{0} = {1} Ohm\n", i + 1, resList[i]));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 2 - Attenuation
            header.Add("2. Attenuation");

            body.Add("Minimum: " + atten[0] + " dB\n");
            body.Add("Nominal: " + atten[1] + " dB\n");
            body.Add("Maximum: " + atten[2] + " dB\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 3 - Impedance Properties
            header.Add("3. Impedance");

            body.Add("Minimum: " + imped[0] + " Ohm\n");
            body.Add("Nominal: " + imped[1] + " Ohm\n");
            body.Add("Maximum: " + imped[2] + " Ohm\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 4 - Power Handling
            header.Add("4. Power Handling");

            body.Add("Maximum: " + maxPwr + " dBm\n");
            body.Add("Note: The maximum input power assumes resistors rated for 1/16 W, at room temperature and 0% derated.\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            document.Close();
        }

        /** Synthesis Click Handler **/
        private void Atten_Synthesis_Click(object sender, RoutedEventArgs e)
        {
            attenRunSynthesis = true;
            attenRunEvaluate = false;

            // Assign the permitivity value
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    Atten_PI_Run();
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    Atten_TEE_Run();
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    Atten_BTEE_Run();
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:
                    Atten_Refl_Run();
                    break;

                /**Default: PI Attenuator**/
                default:
                    Atten_PI_Run();
                    break;
            }

            attenRunSynthesis = false;
            attenRunEvaluate = false;
        }

        /** Evaluate Click Handler **/
        private void Atten_Evaluate_Click(object sender, RoutedEventArgs e)
        {
            attenRunSynthesis = false;
            attenRunEvaluate = true;

            // Assign the permitivity value
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    Atten_PI_Run();
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    Atten_TEE_Run();
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    Atten_BTEE_Run();
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:
                    Atten_Refl_Run();
                    break;

                /**Default: PI Attenuator**/
                default:
                    Atten_PI_Run();
                    break;
            }

            attenRunSynthesis = false;
            attenRunEvaluate = false;
        }

        /** Help Button Click Handler **/
        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.CurrentDirectory;

            _ = System.Diagnostics.Process.Start(filePath + "\\HelpGuide\\attenuator.html");
        }

        /** Report Generation Checkbox Handler **/
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            bool genReport = true;

            // Enable report generation
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    pi_atten.GenReport = genReport;
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    tee_atten.GenReport = genReport;
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    btee_atten.GenReport = genReport;
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:
                    refl_atten.GenReport = genReport;
                    break;

                /**Default: PI Attenuator**/
                default:
                    pi_atten.GenReport = genReport;
                    break;
            }
        }

        /** Report Generation Checkbox Handler **/
        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            bool genReport = false;

            // Disable report generation
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    pi_atten.GenReport = genReport;
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    tee_atten.GenReport = genReport;
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    btee_atten.GenReport = genReport;
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:
                    refl_atten.GenReport = genReport;
                    break;

                /**Default: PI Attenuator**/
                default:
                    pi_atten.GenReport = genReport;
                    break;
            }
        }

        /** LTSpice Export Handler **/
        private void Export_LTSpice(object sender, RoutedEventArgs e)
        {
            // Assign the permitivity value
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    Atten_PI_Export_LTSpice();
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    Atten_TEE_Export_LTSpice();
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    Atten_BTEE_Export_LTSpice();
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:

                    break;

                /**Default: PI Attenuator**/
                default:
                    Atten_PI_Run();
                    break;
            }
        }
    }
}
