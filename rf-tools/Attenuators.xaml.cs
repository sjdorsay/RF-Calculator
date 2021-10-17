using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.ComponentModel;
using System.Windows;
using Image = iText.Layout.Element.Image;
using TextAlignment = iText.Layout.Properties.TextAlignment;

// iText7 PDF .NET Module

namespace rf_tools
{
    public class AttenuatorParameters : INotifyPropertyChanged
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

        readonly AttenuatorParameters pi_atten = new AttenuatorParameters();
        readonly AttenuatorParameters tee_atten = new AttenuatorParameters();
        readonly AttenuatorParameters btee_atten = new AttenuatorParameters();
        readonly AttenuatorParameters refl_atten = new AttenuatorParameters();

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

        /** Calculate Attenuation of PI Attenuator **/
        private double Atten_PI_GetAttenuation(double r1, double r2, double r3)
        {
            double imped;
            double a;
            double atten;
            double[] ABCD = new double[4];

            ABCD[0] = 1 + (r2 / r1);
            ABCD[1] = r2;
            ABCD[3] = 1 + (r2 / r1);
            ABCD[2] = (ABCD[3] + 1) / r1;

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

            ABCD[0] = 1 + (r2 / r1);
            ABCD[1] = r2;
            ABCD[3] = 1 + (r2 / r1);
            ABCD[2] = (ABCD[3] + 1) / r1;

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
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

                pi_atten.Attenuation = Atten_PI_GetAttenuation(r1, r2, r2);
                pi_atten.Impedance = Atten_PI_GetImpedance(r1, r2, r2);
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

                tol = pi_atten.Tolerance/100;

                double tempAtten;
                double tempImpedance;

                // Find the maximum and minimum attenuation and impedance
                for (int i = 0; i < 8; i++)
                {
                    tempAtten = Atten_PI_GetAttenuation(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r2 * (1 + (2 * (4 & i) - 1) * tol)
                        );
                    
                    tempImpedance = Atten_PI_GetImpedance(
                        r1 * (1 + (2 * (1 & i) - 1) * tol),
                        r2 * (1 + (2 * (2 & i) - 1) * tol),
                        r2 * (1 + (2 * (4 & i) - 1) * tol)
                        );

                    if (tempAtten < attenuation[0]) attenuation[0] = tempAtten;
                    if (tempAtten > attenuation[2]) attenuation[2] = tempAtten;

                    if (tempImpedance < impedance[0]) impedance[0] = tempImpedance;
                    if (tempImpedance > impedance[2]) impedance[2] = tempImpedance;
                }

                Gen_Report(attenuation, impedance);
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
            ABCD[1] = r1 * (ABCD[0] + 1);
            ABCD[2] = 1 / r2;
            ABCD[3] = 1 + (r1 / r2);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedanceof Tee Attenuator **/
        private double Atten_TEE_GetImpedance(double r1, double r2, double r3)
        {
            double imped;
            double[] ABCD = new double[4];

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 * (ABCD[0] + 1);
            ABCD[2] = 1 / r2;
            ABCD[3] = 1 + (r1 / r2);

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
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

            if(genReport)
            {

            }
        }

        private void Gen_Report(double[] atten, double[] imped)
        {
            // Get path to user documents assuming C drive
            string UserName = Environment.UserName;
            string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

            // Must have write permissions to the path folder
            PdfWriter writer = new PdfWriter(DocuPath + "Attenuator_Report.pdf");
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

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

            // Section 1 - Material Properties
            header.Add("1. Attenuation");

            body.Add("min_atten = " + atten[0] + " dB\n");
            body.Add("typ_atten = " + atten[1] + " dB\n");
            body.Add("max_atten = " + atten[2] + " dB\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 1 - Impedance Properties
            header.Add("2. Impedance");

            body.Add("min_imped = " + imped[0] + " Ohm\n");
            body.Add("typ_imped = " + imped[1] + " Ohm\n");
            body.Add("max_imped = " + imped[2] + " Ohm\n");

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

            System.Diagnostics.Process.Start(filePath + "\\HelpGuide\\attenuator.html");
        }

        /** Report Generation Checkbox Handler **/
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            genReport = true;
        }

        /** Report Generation Checkbox Handler **/
        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            genReport = false;
        }

        /** Tab Selection Selection Change Handler **/
        private void attTabCtrl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            genReport = false;
        }
    }
}
