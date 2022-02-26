using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using rf_tools.Adapters;

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

        /***************************/
        /** PI Attenuator Section **/
        /***************************/

        /** -------------------------- **/
        /** Synthesis Function Section **/
        /** -------------------------- **/

        /** Calculate Series Resistor of PI Attenuator **/
        private double Atten_PI_Synth_GetSeriesRes()
        {
            double rser = 0;
            double a = Math.Pow(10, pi_atten.Attenuation / 20);
            double imped = pi_atten.Impedance;
            double tol = pi_atten.Tolerance;

            if (1 < a)
            {
                rser = imped * (Math.Pow(a, 2) - 1) / (2 * a);
            }

            return CommonFunctions.GetStdResistor(rser, tol);
        }

        /** Calculate Shunt Resistor of PI Attenuator **/
        private double Atten_PI_Synth_GetShuntRes()
        {
            double rshunt = 1e6;
            double a = Math.Pow(10, pi_atten.Attenuation / 20);
            double imped = pi_atten.Impedance;
            double tol = pi_atten.Tolerance;

            if (1 < a)
            {
                rshunt = imped * (a + 1) / (a - 1);
            }

            return CommonFunctions.GetStdResistor(rshunt, tol);
        }

        /** ------------------------- **/
        /** Evaluate Function Section **/
        /** ------------------------- **/

        /** Calculate Attenuation of PI Attenuator **/
        private double Atten_PI_Eval_GetAttenuation()
        {
            double atten;

            double r1, r2;

            // The attenuation can be calculated using the PI attenuator
            // algorithm with one adjustment.
            // 1. We can assume r1 = r3 since it follows for all the worst cases scenarios
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;

            atten = Atten_PI_GetAttenuation(r1, r2, r1);

            return atten;
        }

        /** Calculate Attenuation of PI Attenuator **/
        private double Atten_PI_Eval_GetWorstAttenuation()
        {
            double attenLow, attenHigh, attenNom;
            double attenWorst;

            double r1, r2;

            // The attenuation can be calculated using the PI attenuator
            // algorithm with one adjustment.
            // 1. We can assume r1 = r3 since it follows for all the worst cases scenarios

            // Highest Attenuation
            r1 = pi_atten.Res1 * (1 - pi_atten.Tolerance / 100);
            r2 = pi_atten.Res2 * (1 + pi_atten.Tolerance / 100);

            attenHigh = Atten_PI_GetAttenuation(r1, r2, r1);

            // Lowest Attenuation
            r1 = pi_atten.Res1 * (1 + pi_atten.Tolerance / 100);
            r2 = pi_atten.Res2 * (1 - pi_atten.Tolerance / 100);

            attenLow = Atten_PI_GetAttenuation(r1, r2, r1);

            // Nominal Attenuation
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;

            attenNom = Atten_PI_GetAttenuation(r1, r2, r1);

            attenWorst = (attenHigh - attenNom) > (attenNom - attenLow) ? attenHigh : attenLow;

            return attenWorst;
        }

        /** Calculate Input Impedanceof PI Attenuator **/
        private double Atten_PI_Eval_GetImpedance()
        {
            double imped;

            double r1, r2, r3;

            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;
            r3 = pi_atten.Res1;

            imped = Atten_PI_GetImpedance(r1, r2, r3);

            return imped;
        }

        /** Calculate Input Impedanceof PI Attenuator **/
        private double Atten_PI_Eval_GetWorstImpedance()
        {
            // TO DO:
            // The run function has a nice way to find worst-case.
            // check this out
            double impedLow, impedHigh, impedNom;
            double impedWorst;

            double r1, r2, r3;

            // Nominal Impedance
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;
            r3 = pi_atten.Res1;

            impedNom = Atten_PI_GetImpedance(r1, r2, r3);

            // Highest Impedance
            r1 = pi_atten.Res1 * (1 + pi_atten.Tolerance / 100);
            r2 = pi_atten.Res2 * (1 + pi_atten.Tolerance / 100);
            r3 = pi_atten.Res1 * (1 + pi_atten.Tolerance / 100);

            impedHigh = Atten_PI_GetImpedance(r1, r2, r3);

            // Lowest Impedance
            r1 = pi_atten.Res1 * (1 - pi_atten.Tolerance / 100);
            r2 = pi_atten.Res2 * (1 - pi_atten.Tolerance / 100);
            r3 = pi_atten.Res1 * (1 - pi_atten.Tolerance / 100);

            impedLow = Atten_PI_GetImpedance(r1, r2, r3);

            // Find the worst-case impedance
            impedWorst = (impedHigh - impedNom) > (impedNom - impedLow) ? impedHigh : impedLow;

            return impedWorst;
        }

        /** -------------------------- **/
        /** Auxiliary Function Section **/
        /** -------------------------- **/

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

        /** Calculate Input Impedanceof PI Attenuator **/
        private double Atten_PI_GetPowerRating()
        {
            double a;
            double pwrRating;

            a = Math.Pow(10, pi_atten.Attenuation / 20);
            pwrRating = 0.0625 * (a + 1) / (a - 1);
            pwrRating = 0.0625 * a * a / (a - 1);

            return pwrRating;
        }

        /** Export LTSpice of PI Attenuator **/
        private void Atten_PI_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            string DocuPath = CommonFunctions.SaveFile(".asc", "LTSpice File|.asc");

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

            File.WriteAllText(DocuPath, adapter.ToString());
        }

        /** Generate and Export PDF Report **/
        private void Atten_PI_Export_Report()
        {
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // Gather ideal values
            _ = pi_atten.Attenuation;
            _ = pi_atten.Impedance;
            _ = pi_atten.Res1;
            _ = pi_atten.Res2;

            // Gather performance values
            _ = Atten_PI_Eval_GetAttenuation();
            _ = Atten_PI_Eval_GetWorstAttenuation();
            _ = Atten_PI_Eval_GetImpedance();
            _ = Atten_PI_Eval_GetWorstImpedance();

            // Get other data
            _ = Atten_PI_GetPowerRating();
        }

        /** PI Attenuator - Synthesis / Evalute **/
        public void Atten_PI_Run()
        {
            // Initialize resistor values
            double r1 = 1e6;
            double r2 = 0;

            double tol;

            bool genReport = pi_atten.GenReport;

            if (attenRunSynthesis)
            {
                // Call resistor get functions
                pi_atten.Res1 = Atten_PI_Synth_GetShuntRes();
                pi_atten.Res2 = Atten_PI_Synth_GetSeriesRes();
            }

            if (attenRunEvaluate)
            {
                // Calculate attenuation and impedance based on resistors provided
                pi_atten.Attenuation = Atten_PI_Eval_GetAttenuation();
                pi_atten.Impedance = Atten_PI_Eval_GetImpedance();
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

        /****************************/
        /** Tee Attenuator Section **/
        /****************************/
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

        /** Generate and Export PDF Report **/
        private void Atten_TEE_Export_Report()
        {
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");
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

                tee_atten.Res1 = CommonFunctions.GetStdResistor(r1, tol);
                tee_atten.Res2 = CommonFunctions.GetStdResistor(r2, tol);
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

        /************************************/
        /** Bridged Tee Attenuator Section **/
        /************************************/
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
        
        /** Generate and Export PDF Report **/
        private void Atten_BTEE_Export_Report()
        {
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");
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

                btee_atten.Res1 = CommonFunctions.GetStdResistor(r1, tol);
                btee_atten.Res2 = CommonFunctions.GetStdResistor(r2, tol);
                btee_atten.Res3 = CommonFunctions.GetStdResistor(r3, tol);
                btee_atten.Res4 = CommonFunctions.GetStdResistor(r4, tol);
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

        /***********************************/
        /** Reflection Attenuator Section **/
        /***********************************/
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

        /** Generate and Export PDF Report **/
        private void Atten_Refl_Export_Report()
        {
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");
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

                refl_atten.Res1 = CommonFunctions.GetStdResistor(r1, tol);
                refl_atten.Res2 = CommonFunctions.GetStdResistor(r2, tol);
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

        /*****************************/
        /** Export Function Section **/
        /*****************************/

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

        /** LTSpice Export Handler **/
        private void Export_Report(object sender, RoutedEventArgs e)
        {
            Atten_Synthesis_Click(sender, e);
            Atten_Evaluate_Click(sender, e);

            // Assign the permitivity value
            switch (attTabCtrl.SelectedIndex)
            {
                /**Tab 0: PI Attenuator**/
                case 0:
                    Atten_PI_Export_Report();
                    break;

                /**Tab 1: Tee Attenuator**/
                case 1:
                    Atten_TEE_Export_Report();
                    break;

                /**Tab 2: Bridged Tee Attenuator**/
                case 2:
                    Atten_BTEE_Export_Report();
                    break;

                /**Tab 3: Reflection Attenuator**/
                case 3:
                    Atten_Refl_Export_Report();
                    break;

                /**Default: PI Attenuator**/
                default:
                    Atten_PI_Export_Report();
                    break;
            }
        }

        /***********************************/
        /** GUI Interface Handler Section **/
        /***********************************/

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
    }
}
