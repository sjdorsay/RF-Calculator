using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using TextAlignment = iText.Layout.Properties.TextAlignment;

/* To Do:
 * [ ] Create LTSpice export tool for reflective attenuator
 * [x] Create Report export tool for reflective attenuator
 * [x] Add Power rating for BTEE, and TEE
 * [x] Verify and review the generated reports
 * [ ] Verify and review the generated circuits
 * [ ] Update the attenuator HTML document
 */
namespace rf_tools
{
    internal class Attenuator : INotifyPropertyChanged
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

    internal class PiAttenuator : Attenuator
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

    internal class TeeAttenuator : Attenuator
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

    internal class BTeeAttenuator : Attenuator
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

    internal class ReflAttenuator : Attenuator
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

        private readonly int nDigits = 3;

        /** Attenuator Initilazation Function **/
        public Attenuators()
        {
            InitializeComponent();

            attPiTab.DataContext = pi_atten;
            attTeeTab.DataContext = tee_atten;
            attBTeeTab.DataContext = btee_atten;
            attReflTab.DataContext = refl_atten;
        }

        /*******************/
        /** PI Attenuator **/
        /*******************/
        #region PI
        /** ------------------- **/
        /** Synthesis Functions **/
        /** ------------------- **/
        /** Calculate Series Resistor of PI Attenuator **/
        private double Atten_PI_Synth_GetSeriesRes()
        {
            // Instantiate variables
            double rser = 0;
            double a = Math.Pow(10, pi_atten.Attenuation / 20);
            double imped = pi_atten.Impedance;
            double tol = pi_atten.Tolerance;

            // If a = 1 there will be calulation errors.
            // The a = 1 case corresponds to 0 dB attenuator.
            // To include 0 dB in the calculator the rser has
            // an inital value of 0 Ohms
            if (1 < a)
            {
                rser = imped * (Math.Pow(a, 2) - 1) / (2 * a);
            }

            // Calculate the industry standard resistor value
            // with the selected tolerance
            return CommonFunctions.GetStdResistor(rser, tol);
        }

        /** Calculate Shunt Resistor of PI Attenuator **/
        private double Atten_PI_Synth_GetShuntRes()
        {
            // Instantiate variables
            double rshunt = 1e6;
            double a = Math.Pow(10, pi_atten.Attenuation / 20);
            double imped = pi_atten.Impedance;
            double tol = pi_atten.Tolerance;

            // If a = 1 there will be calulation errors.
            // The a = 1 case corresponds to 0 dB attenuator.
            // To include 0 dB in the calculator the rshunt has
            // an inital value of 1 MOhms
            if (1 < a)
            {
                rshunt = imped * (a + 1) / (a - 1);
            }

            // Calculate the industry standard resistor value
            // with the selected tolerance
            return CommonFunctions.GetStdResistor(rshunt, tol);
        }

        /** ------------------ **/
        /** Evaluate Functions **/
        /** ------------------ **/
        /** Calculate Attenuation of PI Attenuator **/
        private double Atten_PI_Eval_GetAttenuation()
        {
            // Instantiate variables
            double atten;
            double r1, r2;

            // The attenuation can be calculated using the PI attenuator
            // We can assume r1 = r3
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;

            // Calculate nominal attenuation
            atten = Atten_PI_GetAttenuation(r1, r2, r1);

            // Return the attenuation value
            return atten;
        }

        /** Calculate Attenuation of PI Attenuator **/
        private double Atten_PI_Eval_GetWorstAttenuation()
        {
            // Instantiate variables
            double attenLow, attenHigh, attenNom;
            double attenWorst;
            double r1, r2;
            double tol = pi_atten.Tolerance / 100.0;

            // Nominal Resistor Values to be scaled by the tolerance
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;

            // Start all the variables off at the nominal value
            attenNom = Atten_PI_GetAttenuation(r1, r2, r1);

            // Find the maximum and minimum attenuation
            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r1 }, Atten_PI_GetAttenuation);
            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r1 }, Atten_PI_GetAttenuation);

            // Find the worst-case value by comparing the results
            attenWorst = (attenHigh - attenNom) > (attenNom - attenLow) ? attenHigh : attenLow;

            return attenWorst;
        }

        /** Calculate Minimum Attenuation of TEE Attenuator **/
        private double Atten_PI_Eval_GetMinAttenuation()
        {
            // Instantiate variables
            double attenLow;
            double tol;
            double r1, r2, r3;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;
            r3 = pi_atten.Res1;

            // Convert tolerance to decimal (from %)
            tol = pi_atten.Tolerance / 100.0;

            // Get the minimum attenuation
            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r3 }, Atten_PI_GetAttenuation);

            return attenLow;
        }

        /** Calculate Maximum Attenuation of TEE Attenuator **/
        private double Atten_PI_Eval_GetMaxAttenuation()
        {
            // Instantiate variables
            double attenHigh;
            double tol;
            double r1, r2, r3;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;
            r3 = pi_atten.Res1;

            // Convert tolerance to decimal (from %)
            tol = pi_atten.Tolerance / 100.0;

            // Get the maximum attenuation
            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r3 }, Atten_PI_GetAttenuation);

            return attenHigh;
        }

        /** Calculate Input Impedanceof PI Attenuator **/
        private double Atten_PI_Eval_GetImpedance()
        {
            // Instantiate variables
            double imped;
            double r1, r2, r3;

            // Gather resistor values
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;
            r3 = pi_atten.Res1;

            // Use the resistor values to find the impedance
            imped = Atten_PI_GetImpedance(r1, r2, r3);

            return imped;
        }

        /** Calculate Input Impedanceof PI Attenuator **/
        private double Atten_PI_Eval_GetWorstImpedance()
        {
            // Instantiate variables
            double impedLow, impedHigh, impedNom;
            double impedWorst;
            double r1, r2;
            double tol = pi_atten.Tolerance / 100.0;

            // Nominal Resistor Values to be scaled by the tolerance
            r1 = pi_atten.Res1;
            r2 = pi_atten.Res2;

            // Start all the variables off at the nominal value
            impedNom = Atten_PI_GetImpedance(r1, r2, r1);

            // Find the maximum and minimum attenuation
            impedLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r1 }, Atten_PI_GetImpedance);
            impedHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r1 }, Atten_PI_GetImpedance);

            // Find the worst-case value by comparing the results
            impedWorst = (impedHigh - impedNom) > (impedNom - impedLow) ? impedHigh : impedLow;

            return impedWorst;
        }

        /** -------------------------- **/
        /** Auxiliary Function Section **/
        /** -------------------------- **/
        /** Calculate Attenuation of PI Attenuator **/
        private double Atten_PI_GetAttenuation(params double[] rList)
        {
            double imped;
            double a;
            double atten;
            double r1, r2, r3;
            double[] ABCD = new double[4];

            // Split params into respective values
            r1 = rList[0];
            r2 = rList[1];
            r3 = rList[2];

            // Calculate the ABCD parameters
            ABCD[0] = 1 + (r2 / r3);
            ABCD[1] = r2;
            ABCD[3] = 1 + (r2 / r1);
            ABCD[2] = (1 / r1) + ABCD[3] * (1 / r3);

            // Use the ABCD matrix to calculate the impedance
            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            // Use the ABCD matrix to calculate the attenuation
            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedanceof PI Attenuator **/
        private double Atten_PI_GetImpedance(params double[] rList)
        {
            double imped;
            double r1, r2, r3;
            double[] ABCD = new double[4];

            // Split params into respective values
            r1 = rList[0];
            r2 = rList[1];
            r3 = rList[2];

            // Calculate the ABCD parameters
            ABCD[0] = 1 + (r2 / r3);
            ABCD[1] = r2;
            ABCD[3] = 1 + (r2 / r1);
            ABCD[2] = (1 / r1) + ABCD[3] * (1 / r3);

            // Use the ABCD matrix to calculate the impedance
            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
        }

        /** Calculate Maximum Power Rating PI Attenuator **/
        private double Atten_PI_GetPowerRating()
        {
            double a;
            double pwrRating;

            a = Math.Pow(10, pi_atten.Attenuation / 20);
            
            pwrRating = 0.0625 * a * a / (a - 1);

            return pwrRating;
        }

        /** Export LTSpice of PI Attenuator **/
        private void Atten_PI_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            string docPath = CommonFunctions.SaveFile(".asc", "LTSpice File|.asc");
            
            adapter.AddSource(1, 0, new LTSpiceCoords(-48, 16, 0));

            adapter.AddResistor(1, 0, new LTSpiceCoords(0, 16, 0),
                string.Format("{{mc({0}, tolR)}}", pi_atten.Res1));
            
            adapter.AddResistor(2, 1, new LTSpiceCoords(144, 0, 90),
                string.Format("{{mc({0}, tolR)}}", pi_atten.Res2));
            
            adapter.AddResistor(2, 0, new LTSpiceCoords(128, 16, 0),
                string.Format("{{mc({0}, tolR)}}", pi_atten.Res1));
            
            adapter.AddResistor(2, 0, new LTSpiceCoords(176, 16, 0), "50");

            File.WriteAllText(docPath, adapter.ToString());
        }

        /** Generate and Export PDF Report **/
        private void Atten_PI_Export_Report()
        {
            // Get file path to user documents
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // Round all values to the number of digits
            int digits = 3;

            // Must have write permissions to the path folder
            PdfWriter writer = new PdfWriter(docPath);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            /** Setup Document Formatting **/
            // Setup Title formatting
            Paragraph title = new Paragraph("PI Attenuator Report")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(24);

            // Setup Sub-Title formatting
            Paragraph subtitle = new Paragraph("By RF Design Toolkit")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBold()
               .SetItalic()
               .SetFontSize(20);

            // Setup Header formatting
            Paragraph header = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetUnderline()
               .SetBold()
               .SetFontSize(16);

            // Setup body formatting
            Paragraph body = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(12);

            /** Generate Document **/
            // Add title
            document.Add(title);
            document.Add(subtitle);

            // Section 1 - Attenuator
            header.Add("1. Attenuator Design");

            // Gather ideal values
            body.Add(string.Format("Atten = {0} dB\n", Math.Round(pi_atten.Attenuation, digits)));
            body.Add(string.Format("Z0 = {0} Ohm\n", Math.Round(pi_atten.Impedance, digits)));
            body.Add("\n");
            body.Add(string.Format("The resistors have been calculated using {0}% standard resistor values.\n", pi_atten.Tolerance));
            body.Add(string.Format("R1 = {0} Ohm\n", pi_atten.Res1));
            body.Add(string.Format("R2 = {0} Ohm\n", pi_atten.Res2));
            body.Add(string.Format("R1 = {0} Ohm\n", pi_atten.Res1));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 2 - Attenuator
            header.Add("2. Attenuator Performance");

            // Gather ideal values
            body.Add(string.Format("Attenuation\n"));
            body.Add(string.Format("min = {0} dB\n", Math.Round(Atten_PI_Eval_GetMinAttenuation(), digits)));
            body.Add(string.Format("nom = {0} dB\n", Math.Round(Atten_PI_Eval_GetAttenuation(), digits)));
            body.Add(string.Format("max = {0} dB\n", Math.Round(Atten_PI_Eval_GetMaxAttenuation(), digits)));
            body.Add("\n");
            body.Add(string.Format("Characteristic Impedance\n"));
            body.Add(string.Format("nom = {0} Ohm\n", Math.Round(Atten_PI_Eval_GetImpedance(), digits)));
            body.Add(string.Format("worst = {0} Ohm\n", Math.Round(Atten_PI_Eval_GetWorstImpedance(), digits)));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 3 - Power Handling
            header.Add("3. Power Handling");

            double pwr_mw = Atten_PI_GetPowerRating() * 1000;
            double pwr_dbm = 10 * Math.Log10(pwr_mw);

            body.Add("Maximum:\n");
            body.Add(Math.Round(pwr_mw, digits) + " mW\n");
            body.Add(Math.Round(pwr_dbm, digits) + " dBm\n\n");
            body.Add("Note: \nThe maximum input power assumes resistors rated for 1/16 W, at room temperature, and 0% derated.\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            document.Close();
        }

        /** PI Attenuator - Synthesis / Evalute **/
        public void Atten_PI_Run()
        {
            if (attenRunSynthesis)
            {
                // Call resistor get functions
                pi_atten.Res1 = Atten_PI_Synth_GetShuntRes();
                pi_atten.Res2 = Atten_PI_Synth_GetSeriesRes();
            }

            if (attenRunEvaluate)
            {
                // Calculate attenuation and impedance based on resistors provided
                pi_atten.Attenuation = Math.Round(Atten_PI_Eval_GetAttenuation(), nDigits);
                pi_atten.Impedance = Math.Round(Atten_PI_Eval_GetImpedance(), nDigits);
            }
        }
        #endregion

        /****************************/
        /** Tee Attenuator Section **/
        /****************************/
        #region TEE
        /** -------------------------- **/
        /** Synthesis Function Section **/
        /** -------------------------- **/

        /** Calculate Series Resistor of TEE Attenuator **/
        private double Atten_TEE_Synth_GetSeriesRes()
        {
            // Initialize resistor values for a 0 dB attenuator
            double rser = 0;
            double a;
            double imped;
            double tol;

            // Calculate the series resistance based on attenuation
            a = Math.Pow(10, tee_atten.Attenuation / 20);
            imped = tee_atten.Impedance;
            tol = tee_atten.Tolerance;

            // Generate ideal resistor values
            if (1 < a)
            {
                rser = imped * (a - 1) / (a + 1);
            }

            // Convert resistance to standard value
            return CommonFunctions.GetStdResistor(rser, tol);
        }

        /** Calculate Shunt Resistor of TEE Attenuator **/
        private double Atten_TEE_Synth_GetShuntRes()
        {
            // Initialize resistor values for a 0 dB attenuator
            double rshunt = 1e6;
            double a;
            double imped;
            double tol;

            // Calculate the shunt resistance based on attenuation
            a = Math.Pow(10, tee_atten.Attenuation / 20);
            imped = tee_atten.Impedance;
            tol = tee_atten.Tolerance;

            // Generate ideal resistor values
            if (1 < a)
            {
                rshunt = 2 * imped * a / (Math.Pow(a, 2) - 1);
            }

            // Convert resistance to standard value
            return CommonFunctions.GetStdResistor(rshunt, tol);
        }

        /** ------------------------- **/
        /** Evaluate Function Section **/
        /** ------------------------- **/

        /** Calculate Attenuation of TEE Attenuator **/
        private double Atten_TEE_Eval_GetAttenuation()
        {
            double atten;

            double r1, r2;

            // The attenuation can be calculated using the TEE attenuator
            // algorithm with one adjustment.
            // 1. We can assume r3 = r1
            r1 = tee_atten.Res1;
            r2 = tee_atten.Res2;

            atten = Atten_TEE_GetAttenuation(r1, r2, r1);

            return atten;
        }

        /** Calculate Attenuation of TEE Attenuator **/
        private double Atten_TEE_Eval_GetWorstAttenuation()
        {
            // Instantiate variables
            double attenLow, attenHigh, attenNom;
            double attenWorst;
            double r1, r2;
            double tol = tee_atten.Tolerance / 100.0;

            // Nominal Resistor Values to be scaled by the tolerance
            r1 = tee_atten.Res1;
            r2 = tee_atten.Res2;

            // Start all the variables off at the nominal value
            attenNom = Atten_TEE_GetAttenuation(r1, r2, r1);

            // Find the maximum and minimum attenuation
            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r1 }, Atten_TEE_GetAttenuation);
            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r1 }, Atten_TEE_GetAttenuation);

            // Find the worst-case value by comparing the results
            attenWorst = (attenHigh - attenNom) > (attenNom - attenLow) ? attenHigh : attenLow;

            return attenWorst;
        }

        /** Calculate Minimum Attenuation of TEE Attenuator **/
        private double Atten_TEE_Eval_GetMinAttenuation()
        {
            // Instantiate variables
            double attenLow;
            double tol;
            double r1, r2, r3;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = tee_atten.Res1;
            r2 = tee_atten.Res2;
            r3 = tee_atten.Res1;

            // Convert tolerance to decimal (from %)
            tol = tee_atten.Tolerance / 100.0;

            // Get the minimum attenuation
            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r3 }, Atten_TEE_GetAttenuation);

            return attenLow;
        }

        /** Calculate Maximum Attenuation of TEE Attenuator **/
        private double Atten_TEE_Eval_GetMaxAttenuation()
        {
            // Instantiate variables
            double attenHigh;
            double tol;
            double r1, r2, r3;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = tee_atten.Res1;
            r2 = tee_atten.Res2;
            r3 = tee_atten.Res1;

            // Convert tolerance to decimal (from %)
            tol = tee_atten.Tolerance / 100.0;

            // Get the maximum attenuation
            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r3 }, Atten_TEE_GetAttenuation);

            return attenHigh;
        }

        /** Calculate Input Impedanceof TEE Attenuator **/
        private double Atten_TEE_Eval_GetImpedance()
        {
            double imped;

            double r1, r2, r3;

            r1 = tee_atten.Res1;
            r2 = tee_atten.Res2;
            r3 = tee_atten.Res1;

            imped = Atten_TEE_GetImpedance(r1, r2, r3);

            return imped;
        }

        /** Calculate Input Impedance of TEE Attenuator **/
        private double Atten_TEE_Eval_GetWorstImpedance()
        {
            // Instantiate variables
            double impedLow, impedHigh, impedNom;
            double impedWorst;

            double r1, r2;
            double tol = tee_atten.Tolerance / 100.0;

            // Nominal Resistor Values to be scaled by the tolerance
            r1 = tee_atten.Res1;
            r2 = tee_atten.Res2;

            // Start all the variables off at the nominal value
            impedNom = Atten_TEE_GetAttenuation(r1, r2, r1);

            // Find the maximum and minimum attenuation
            impedLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r1 }, Atten_TEE_GetImpedance);
            impedHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r1 }, Atten_TEE_GetImpedance);

            // Find the worst-case value by comparing the results
            impedWorst = (impedHigh - impedNom) > (impedNom - impedLow) ? impedHigh : impedLow;

            return impedWorst;
        }

        /** -------------------------- **/
        /** Auxiliary Function Section **/
        /** -------------------------- **/

        /** Calculate Attenuation of Tee Attenuator **/
        private double Atten_TEE_GetAttenuation(params double[] rList)
        {
            double imped;
            double a;
            double atten;
            double r1, r2, r3;
            double[] ABCD = new double[4];

            // Split params into respective values
            r1 = rList[0];
            r2 = rList[1];
            r3 = rList[2];

            // Calculate ABCD parameters of the TEE Attenuator
            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 + r3 * ABCD[0];
            ABCD[2] = 1 / r2;
            ABCD[3] = 1 + (r3 / r2);

            // Use ABCD parameters to calculate the impedance
            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            // Use ABCD parameters and impedance to calculate the attenuation
            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedance of Tee Attenuator **/
        private double Atten_TEE_GetImpedance(params double[] rList)
        {
            double imped;
            double r1, r2, r3;
            double[] ABCD = new double[4];

            // Split params into respective values
            r1 = rList[0];
            r2 = rList[1];
            r3 = rList[2];

            // Calculate ABCD parameters of the TEE Attenuator
            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 + r3 * ABCD[0];
            ABCD[2] = 1 / r2;
            ABCD[3] = 1 + (r3 / r2);

            // Use ABCD parameters to calculate the impedance
            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
        }

        /** Calculate Input Impedanceof TEE Attenuator **/
        private double Atten_TEE_GetPowerRating()
        {
            // TO DO
            double a;
            double pwrRating;

            a = Math.Pow(10, tee_atten.Attenuation / 20);
            
            pwrRating = 0.0625 * a * a / (a - 1);

            return pwrRating;
        }

        /** Export LTSpice of TEE Attenuator **/
        private void Atten_TEE_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();
            
            // Get file path to save file
            string docPath = CommonFunctions.SaveFile(".asc", "LTSpice Schematic|.asc");

            //Add signal source
            adapter.AddSource(1, 0, new LTSpiceCoords(-48, 16, 0));

            // Add TEE Attenuator resistors
            adapter.AddResistor(2, 1, new LTSpiceCoords(96, 0, 90),
                string.Format("{{mc({0}, tolR)}}", tee_atten.Res1));
            adapter.AddResistor(2, 0, new LTSpiceCoords(80, 16, 0),
                string.Format("{{mc({0}, tolR)}}", tee_atten.Res2));
            adapter.AddResistor(3, 2, new LTSpiceCoords(224, 0, 90),
                string.Format("{{mc({0}, tolR)}}", tee_atten.Res1));

            // Add Load resistor
            adapter.AddResistor(3, 0, new LTSpiceCoords(208, 16, 0), "50");

            // Setup simulation
            adapter.AddNetSim(0, -128, 100000000, 4000000000, 100);
            adapter.AddParameter(0, -64, "tolR", (float)tee_atten.Tolerance / 100);

            // Save text to file
            File.WriteAllText(docPath, adapter.ToString());
        }

        /** Generate and Export PDF Report **/
        private void Atten_TEE_Export_Report()
        {
            // Get file path to user documents
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // Round all values to the number of digits
            int digits = 3;

            // Must have write permissions to the path folder
            PdfWriter writer = new PdfWriter(docPath);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            /** Setup Document Formatting **/
            // Setup Title formatting
            Paragraph title = new Paragraph("TEE Attenuator Report")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(24);

            // Setup Sub-Title formatting
            Paragraph subtitle = new Paragraph("By RF Design Toolkit")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBold()
               .SetItalic()
               .SetFontSize(20);

            // Setup Header formatting
            Paragraph header = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetUnderline()
               .SetBold()
               .SetFontSize(16);

            // Setup body formatting
            Paragraph body = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(12);

            /** Generate Document **/
            // Add title
            document.Add(title);
            document.Add(subtitle);

            // Section 1 - Attenuator
            header.Add("1. Attenuator Design");

            // Gather ideal values
            body.Add(string.Format("Atten = {0} dB\n", Math.Round(tee_atten.Attenuation, digits)));
            body.Add(string.Format("Z0 = {0} Ohm\n", Math.Round(tee_atten.Impedance, digits)));
            body.Add("\n");
            body.Add(string.Format("The resistors have been calculated using {0}% standard resistor values.\n", tee_atten.Tolerance));
            body.Add(string.Format("R1 = {0} Ohm\n", tee_atten.Res1));
            body.Add(string.Format("R2 = {0} Ohm\n", tee_atten.Res2));
            body.Add(string.Format("R1 = {0} Ohm\n", tee_atten.Res1));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 2 - Attenuator
            header.Add("2. Attenuator Performance");

            // Gather ideal values
            body.Add(string.Format("Attenuation\n"));
            body.Add(string.Format("min = {0} dB\n", Math.Round(Atten_TEE_Eval_GetMinAttenuation(), digits)));
            body.Add(string.Format("nom = {0} dB\n", Math.Round(Atten_TEE_Eval_GetAttenuation(), digits)));
            body.Add(string.Format("max = {0} dB\n", Math.Round(Atten_TEE_Eval_GetMaxAttenuation(), digits)));
            body.Add("\n");
            body.Add(string.Format("Characteristic Impedance\n"));
            body.Add(string.Format("nom = {0} Ohm\n", Math.Round(Atten_TEE_Eval_GetImpedance(), digits)));
            body.Add(string.Format("worst = {0} Ohm\n", Math.Round(Atten_TEE_Eval_GetWorstImpedance(), digits)));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 3 - Power Handling
            header.Add("3. Power Handling");

            double pwr_mw = Atten_TEE_GetPowerRating() * 1000;
            double pwr_dbm = 10 * Math.Log10(pwr_mw);

            body.Add("Maximum:\n");
            body.Add(Math.Round(pwr_mw, digits) + " mW\n");
            body.Add(Math.Round(pwr_dbm, digits) + " dBm\n\n");
            body.Add("Note: \nThe maximum input power assumes resistors rated for 1/16 W, at room temperature, and 0% derated.\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            document.Close();
        }

        /** Tee Attenuator - Synthesis / Evalute **/
        public void Atten_TEE_Run()
        {
            if (attenRunSynthesis)
            {
                tee_atten.Res1 = Atten_TEE_Synth_GetSeriesRes();
                tee_atten.Res2 = Atten_TEE_Synth_GetShuntRes();
            }

            if (attenRunEvaluate)
            {
                tee_atten.Attenuation = Math.Round(Atten_TEE_Eval_GetAttenuation(), nDigits);
                tee_atten.Impedance = Math.Round(Atten_TEE_Eval_GetImpedance(), nDigits);
            }
        }
        #endregion

        /************************************/
        /** Bridged Tee Attenuator Section **/
        /************************************/
        #region BridgedTee
        //----------------------------//
        // Synthesis Function Section //
        //----------------------------//
        /** Calculate Series Resistor of BTEE Attenuator **/
        private double Atten_BTEE_Synth_GetSeriesRes()
        {
            // Initialize resistor values for a 0 dB attenuator
            double rser = 0;

            // Instantiate useful variables
            double a = Math.Pow(10, btee_atten.Attenuation / 20);
            double imped = btee_atten.Impedance;
            double tol = btee_atten.Tolerance;

            // Generate ideal resistor values
            if (1 < a)
            {
                rser = imped * (a - 1);
            }

            return CommonFunctions.GetStdResistor(rser, tol);
        }

        /** Calculate Shunt Resistor of BTEE Attenuator **/
        private double Atten_BTEE_Synth_GetShuntRes()
        {
            // Initialize resistor values for a 0 dB attenuator
            double rshunt = 1e6;

            // Instantiate useful variables
            double a = Math.Pow(10, btee_atten.Attenuation / 20);
            double imped = btee_atten.Impedance;
            double tol = btee_atten.Tolerance;

            // Generate ideal resistor values
            if (1 < a)
            {
                rshunt = imped / (a - 1);
            }

            return CommonFunctions.GetStdResistor(rshunt, tol);
        }

        //---------------------------//
        // Evaluate Function Section //
        //---------------------------//
        /** Calculate Attenuation of BTEE Attenuator **/
        private double Atten_BTEE_Eval_GetAttenuation()
        {
            // Instantiate variables
            double atten;
            double r1, r2, r3, r4;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = btee_atten.Res1;
            r2 = btee_atten.Res2;
            r3 = btee_atten.Res3;
            r4 = btee_atten.Res4;

            // Calculate the nominal attenuation
            atten = Atten_BTEE_GetAttenuation(r1, r2, r3, r4);

            return atten;
        }

        /** Calculate Attenuation of BTEE Attenuator **/
        private double Atten_BTEE_Eval_GetWorstAttenuation()
        {
            // Instantiate variables
            double attenLow, attenHigh, attenNom, attenWorst;
            double tol;
            double r1, r2, r3;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            // Assuming R3 = R2
            r1 = btee_atten.Res1;
            r2 = btee_atten.Res2;
            r3 = btee_atten.Res4;

            // Convert tolerance to decimal (from %)
            tol = btee_atten.Tolerance / 100.0;

            // Calculate the Min, Max, and the nominal attenuation for comparison
            attenNom = Atten_BTEE_GetAttenuation(r1, r2, r2, r3);
            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r2, r3 }, Atten_BTEE_GetAttenuation);
            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r2, r3 }, Atten_BTEE_GetAttenuation);

            // Find the worst-case value by comparing the results
            attenWorst = ((attenHigh - attenNom) > (attenNom - attenLow)) ? attenHigh : attenLow;

            return attenWorst;
        }

        /** Calculate Minimum Attenuation of BTEE Attenuator **/
        private double Atten_BTEE_Eval_GetMinAttenuation()
        {
            // Instantiate variables
            double attenLow;
            double tol;
            double r1, r2, r3, r4;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = btee_atten.Res1;
            r2 = btee_atten.Res2;
            r3 = btee_atten.Res3;
            r4 = btee_atten.Res4;

            // Convert tolerance to decimal (from %)
            tol = btee_atten.Tolerance / 100.0;

            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r3, r4 }, Atten_BTEE_GetAttenuation);

            return attenLow;
        }

        /** Calculate Maximum Attenuation of BTEE Attenuator **/
        private double Atten_BTEE_Eval_GetMaxAttenuation()
        {
            // Instantiate variables
            double attenHigh;
            double tol;
            double r1, r2, r3, r4;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = btee_atten.Res1;
            r2 = btee_atten.Res2;
            r3 = btee_atten.Res3;
            r4 = btee_atten.Res4;

            // Convert tolerance to decimal (from %)
            tol = btee_atten.Tolerance / 100.0;

            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r3, r4 }, Atten_BTEE_GetAttenuation);

            return attenHigh;
        }

        /** Calculate Input Impedanceof BTEE Attenuator **/
        private double Atten_BTEE_Eval_GetImpedance()
        {
            // Instantiate variables
            double imped;
            double r1, r2, r3, r4;

            // The impedance can be calculated using the BTEE attenuator
            // algorithm
            // Assuming R3 = R2
            r1 = btee_atten.Res1;
            r2 = btee_atten.Res2;
            r3 = btee_atten.Res3;
            r4 = btee_atten.Res4;

            imped = Atten_BTEE_GetImpedance(r1, r2, r3, r4);

            return imped;
        }

        /** Calculate Input Impedance of BTEE Attenuator **/
        private double Atten_BTEE_Eval_GetWorstImpedance()
        {
            double impedLow, impedHigh, impedNom, impedWorst;
            double tol;
            double r1, r2, r3, r4;

            // The attenuation can be calculated using the BTEE attenuator
            // algorithm
            r1 = btee_atten.Res1;
            r2 = btee_atten.Res2;
            r3 = btee_atten.Res3;
            r4 = btee_atten.Res4;

            // Convert tolerance to decimal (from %)
            tol = btee_atten.Tolerance / 100.0;

            // Calculate the Min, Max, and the nominal attenuation for comparison
            impedNom = Atten_BTEE_GetAttenuation(r1, r2, r3, r4);
            impedLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2, r3, r4 }, Atten_BTEE_GetImpedance);
            impedHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2, r3, r4 }, Atten_BTEE_GetImpedance);

            // Find the worst-case value by comparing the results
            impedWorst = ((impedHigh - impedNom) > (impedNom - impedLow)) ? impedHigh : impedLow;

            return impedWorst;
        }

        /** -------------------------- **/
        /** Auxiliary Function Section **/
        /** -------------------------- **/

        /** Calculate Attenuation of BTee Attenuator **/
        // private double Atten_BTEE_GetAttenuation(double r1, double r2, double r3, double r4)
        private double Atten_BTEE_GetAttenuation(params double[] rList)
        {
            // instantiate variables
            double imped;
            double a;
            double atten;
            double[] ABCD = new double[4];

            // Separate values in the passed list / array
            double r1 = rList[0];
            double r2 = rList[1];
            double r3 = rList[1];
            double r4 = rList[2];

            // If all 4 arguments have been provided
            if (4 == rList.Length)
            {
                r3 = rList[2];
                r4 = rList[3];
            }

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
        // private double Atten_BTEE_GetImpedance(double r1, double r2, double r3, double r4)
        private double Atten_BTEE_GetImpedance(params double[] rList)
        {
            // instantiate variables
            double imped;
            double[] ABCD = new double[4];

            // Separate values in the passed list / array
            double r1 = rList[0];
            double r2 = rList[1];
            double r3 = rList[1];
            double r4 = rList[2];

            // If all 4 arguments have been provided
            if (4 == rList.Length)
            {
                r3 = rList[2];
                r4 = rList[3];
            }

            // Calculate supplemental equations for determining ABCD matrix
            double x0 = r1 + r3 + r2 * (1 + (r3 / r4));
            double x1 = r4 * (r1 + r2) + r3 * (r2 + r4);
            double x2 = x0 - r1;

            // Find the ABCD matrix parameters
            ABCD[0] = 1 + r1 * (r2 / x1);   // A
            ABCD[1] = r1 * (x2 / x0);       // B
            ABCD[2] = (r1 + r2 + r3) / x1;  // C
            ABCD[3] = 1 + r1 * (r3 / x1);   // D

            imped = Math.Sqrt(ABCD[0] * ABCD[1] / (ABCD[2] * ABCD[3]));

            return imped;
        }

        /** Calculate Power Rating of BTee Attenuator **/
        private double Atten_BTEE_GetPowerRating()
        {
            // Instantiate required variables
            double a;
            double pwrRating;

            // Calculate the linear attenuation
            a = Math.Pow(10, btee_atten.Attenuation / 20);
            
            // Calculate the maximum rated power depending on attenuation
            if(a > 2)
            {
                pwrRating = 0.0625 * (a - 1) * (a - 1) / (a * a);
            } else {
                pwrRating = 0.0625 * (a - 1) / (a * a);
            }

            return pwrRating;
        }

        /** Export LTSpice of BTEE Attenuator **/
        private void Atten_BTEE_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            // Get file path to save file
            string docPath = CommonFunctions.SaveFile(".asc", "LTSpice Schematic|.asc");

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

            File.WriteAllText(docPath, adapter.ToString());
        }

        /** Generate and Export PDF Report **/
        private void Atten_BTEE_Export_Report()
        {
            // Get file path to user documents
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // Round all values to the number of digits
            int digits = 3;

            // Must have write permissions to the path folder
            PdfWriter writer = new PdfWriter(docPath);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            /** Setup Document Formatting **/
            // Setup Title formatting
            Paragraph title = new Paragraph("Bridged TEE Attenuator Report")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(24);

            // Setup Sub-Title formatting
            Paragraph subtitle = new Paragraph("By RF Design Toolkit")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBold()
               .SetItalic()
               .SetFontSize(20);

            // Setup Header formatting
            Paragraph header = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetUnderline()
               .SetBold()
               .SetFontSize(16);

            // Setup body formatting
            Paragraph body = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(12);

            /** Generate Document **/
            // Add title
            document.Add(title);
            document.Add(subtitle);

            // Section 1 - Attenuator
            header.Add("1. Attenuator Design");

            // Gather ideal values
            body.Add(string.Format("Atten = {0} dB\n", Math.Round(btee_atten.Attenuation, digits)));
            body.Add(string.Format("Z0 = {0} Ohm\n", Math.Round(btee_atten.Impedance, digits)));
            body.Add("\n");
            body.Add(string.Format("The resistors have been calculated using {0}% standard resistor values.\n", btee_atten.Tolerance));
            body.Add(string.Format("R1 = {0} Ohm\n", btee_atten.Res1));
            body.Add(string.Format("R2 = {0} Ohm\n", btee_atten.Res2));
            body.Add(string.Format("R3 = {0} Ohm\n", btee_atten.Res3));
            body.Add(string.Format("R4 = {0} Ohm\n", btee_atten.Res4));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 2 - Attenuator
            header.Add("2. Attenuator Performance");

            // Gather ideal values
            body.Add(string.Format("Attenuation\n"));
            body.Add(string.Format("min = {0} dB\n", Math.Round(Atten_BTEE_Eval_GetMinAttenuation(), digits)));
            body.Add(string.Format("nom = {0} dB\n", Math.Round(Atten_BTEE_Eval_GetAttenuation(), digits)));
            body.Add(string.Format("max = {0} dB\n", Math.Round(Atten_BTEE_Eval_GetMaxAttenuation(), digits)));
            body.Add("\n");
            body.Add(string.Format("Characteristic Impedance\n"));
            body.Add(string.Format("nom = {0} Ohm\n", Math.Round(Atten_BTEE_Eval_GetImpedance(), digits)));
            body.Add(string.Format("worst = {0} Ohm\n", Math.Round(Atten_BTEE_Eval_GetWorstImpedance(), digits)));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 3 - Power Handling
            header.Add("3. Power Handling");

            double pwr_mw = Atten_BTEE_GetPowerRating() * 1000;
            double pwr_dbm = 10 * Math.Log10(pwr_mw);

            body.Add("Maximum:\n");
            body.Add(Math.Round(pwr_mw, digits) + " mW\n");
            body.Add(Math.Round(pwr_dbm, digits) + " dBm\n\n");
            body.Add("Note: \nThe maximum input power assumes resistors rated for 1/16 W, at room temperature, and 0% derated.\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            document.Close();
        }

        /** Bridged Tee Attenuator - Synthesis / Evalute **/
        public void Atten_BTEE_Run()
        {
            // Instantiate variables
            double imped;
            double tol;

            if (attenRunSynthesis)
            {
                imped = btee_atten.Impedance;
                tol = btee_atten.Tolerance;

                btee_atten.Res1 = Atten_BTEE_Synth_GetSeriesRes();
                btee_atten.Res2 = CommonFunctions.GetStdResistor(imped, tol);
                btee_atten.Res3 = CommonFunctions.GetStdResistor(imped, tol);
                btee_atten.Res4 = Atten_BTEE_Synth_GetShuntRes();
            }

            if (attenRunEvaluate)
            {
                // Update properties with new values
                btee_atten.Attenuation = Math.Round(Atten_BTEE_Eval_GetAttenuation(), nDigits);
                btee_atten.Impedance = Math.Round(Atten_BTEE_Eval_GetImpedance(), nDigits);
            }
        }
        #endregion

        /***********************************/
        /** Reflection Attenuator Section **/
        /***********************************/
        #region Reflective
        //----------------------------//
        // Synthesis Function Section //
        //----------------------------//
        /** Calculate Series Resistor of BTEE Attenuator **/
        private double Atten_REFL_Synth_GetLowRes()
        {
            // Initialize resistor values for a 0 dB attenuator
            double rlow = 0;

            // Instantiate useful variables
            double a = Math.Pow(10, refl_atten.Attenuation / 20);
            double imped = refl_atten.Impedance;
            double tol = refl_atten.Tolerance;

            // Generate ideal resistor values
            if (1 < a)
            {
                rlow = imped * (a - 1) / (a + 1);
            }

            return CommonFunctions.GetStdResistor(rlow, tol);
        }

        /** Calculate Series Resistor of BTEE Attenuator **/
        private double Atten_REFL_Synth_GetHighRes()
        {
            // Initialize resistor values for a 0 dB attenuator
            double rhigh = 0;

            // Instantiate useful variables
            double a = Math.Pow(10, refl_atten.Attenuation / 20);
            double imped = refl_atten.Impedance;
            double tol = refl_atten.Tolerance;

            // Generate ideal resistor values
            if (1 < a)
            {
                rhigh = imped * (a + 1) / (a - 1);
            }

            return CommonFunctions.GetStdResistor(rhigh, tol);
        }

        //---------------------------//
        // Evaluate Function Section //
        //---------------------------//
        /** Calculate Attenuation of Reflection Attenuator **/
        private double Atten_REFL_Eval_GetAttenuation()
        {
            double r1 = refl_atten.Res1;
            double r2 = refl_atten.Res2;
            
            return Atten_REFL_GetAttenuation(r1, r2);
        }

        /** Calculate Worst-Case Attenuation of Reflection Attenuator **/
        private double Atten_REFL_Eval_GetWorstAttenuation()
        {
            // Instantiate variables
            double attenHigh, attenLow, attenNom, attenWorst;
            double tol;
            double r1, r2;

            // The attenuation can be calculated using the REFL attenuator
            // algorithm
            r1 = refl_atten.Res1;
            r2 = refl_atten.Res2;

            // Convert tolerance to decimal (from %)
            tol = refl_atten.Tolerance / 100.0;

            attenNom = Atten_REFL_GetAttenuation(r1, r2);
            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2 }, Atten_REFL_GetAttenuation);
            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2 }, Atten_REFL_GetAttenuation);

            // Compare the high and low attenuation values to determine the worst-case attenuation
            attenWorst = (attenHigh - attenNom) > (attenNom - attenLow) ? attenHigh : attenLow;

            return attenWorst;
        }

        /** Calculate Maximum Attenuation of Reflection Attenuator **/
        private double Atten_REFL_Eval_GetMaxAttenuation()
        {
            // Instantiate variables
            double attenHigh;
            double tol;
            double r1, r2;

            // The attenuation can be calculated using the REFL attenuator
            // algorithm
            r1 = refl_atten.Res1;
            r2 = refl_atten.Res2;

            // Convert tolerance to decimal (from %)
            tol = refl_atten.Tolerance / 100.0;

            attenHigh = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2 }, Atten_REFL_GetAttenuation);

            return attenHigh;
        }

        /** Calculate Minimum Attenuation of Reflection Attenuator **/
        private double Atten_REFL_Eval_GetMinAttenuation()
        {
            // Instantiate variables
            double attenLow;
            double tol;
            double r1, r2;

            // The attenuation can be calculated using the REFL attenuator
            // algorithm
            r1 = refl_atten.Res1;
            r2 = refl_atten.Res2;

            // Convert tolerance to decimal (from %)
            tol = refl_atten.Tolerance / 100.0;

            attenLow = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2 }, Atten_REFL_GetAttenuation);

            return attenLow;
        }

        /** Calculate Impedance of Reflection Attenuator **/
        private double Atten_REFL_Eval_GetImpedance()
        {
            double r1 = refl_atten.Res1;
            double r2 = refl_atten.Res2;

            return Atten_REFL_GetImpedance(r1, r2);
        }

        /** Calculate Worst-Case Impedance of Reflection Attenuator **/
        private double Atten_REFL_Eval_GetWorstImpedance()
        {
            // Instantiate variables
            double high, low, nom, worst;
            double tol;
            double r1, r2;

            // The attenuation can be calculated using the REFL attenuator
            // algorithm
            r1 = refl_atten.Res1;
            r2 = refl_atten.Res2;

            // Convert tolerance to decimal (from %)
            tol = refl_atten.Tolerance / 100.0;

            nom = Atten_REFL_GetImpedance(r1, r2);
            low = CommonFunctions.FindWorstMinMax("min", tol, new double[] { r1, r2 }, Atten_REFL_GetImpedance);
            high = CommonFunctions.FindWorstMinMax("max", tol, new double[] { r1, r2 }, Atten_REFL_GetImpedance);

            // Compare the high and low attenuation values to determine the worst-case attenuation
            worst = (high - nom) > (nom - low) ? high : low;

            return worst;
        }

        //----------------------------//
        // Auxiliary Function Section //
        //----------------------------//
        /** Calculate Attenuation of Reflection Attenuator **/
        private double Atten_REFL_GetAttenuation(params double[] rList)
        {
            // Instantiate variable
            double a;
            double atten;

            double r1 = rList[0];
            double r2 = rList[1];

            double imped = Math.Sqrt(r1 * r2);

            // Calculate attenuation
            a = (imped + r1) / (imped - r1);
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Input Impedance of Reflection Attenuator **/
        private double Atten_REFL_GetImpedance(params double[] rList)
        {
            // Instantiate variables
            double r1 = rList[0];
            double r2 = rList[1];
            
            // Calculate the impedance
            double imped = Math.Sqrt(r1 * r2);

            return imped;
        }

        /** Calculate Power Rating of Reflection Attenuator **/
        private double Atten_REFL_GetPowerRating()
        {
            double pwrRating;

            // The power dissipated by the resistors is the same as the input
            pwrRating = 0.0625;

            return pwrRating;
        }

        /** Generate and Export LTSpice **/
        private void Atten_Refl_Export_LTSpice()
        {
            
        }

        /** Generate and Export PDF Report **/
        private void Atten_Refl_Export_Report()
        {
            // Get file path to user documents
            string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // Round all values to the number of digits
            int digits = 3;

            // Must have write permissions to the path folder
            PdfWriter writer = new PdfWriter(docPath);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            /** Setup Document Formatting **/
            // Setup Title formatting
            Paragraph title = new Paragraph("Reflective Attenuator Report")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetFontSize(24);

            // Setup Sub-Title formatting
            Paragraph subtitle = new Paragraph("By RF Design Toolkit")
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBold()
               .SetItalic()
               .SetFontSize(20);

            // Setup Header formatting
            Paragraph header = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetUnderline()
               .SetBold()
               .SetFontSize(16);

            // Setup body formatting
            Paragraph body = new Paragraph()
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(12);

            /** Generate Document **/
            // Add title
            document.Add(title);
            document.Add(subtitle);

            // Section 1 - Attenuator
            header.Add("1. Attenuator Design");

            // Gather ideal values
            body.Add(string.Format("Atten = {0} dB\n", Math.Round(refl_atten.Attenuation, digits)));
            body.Add(string.Format("Z0 = {0} Ohm\n", Math.Round(refl_atten.Impedance, digits)));
            body.Add("\n");
            body.Add(string.Format("The resistors have been calculated using {0}% standard resistor values.\n", refl_atten.Tolerance));
            body.Add(string.Format("Rlow = {0} Ohm\n", refl_atten.Res1));
            body.Add(string.Format("Rhigh = {0} Ohm\n", refl_atten.Res2));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 2 - Attenuator
            header.Add("2. Attenuator Performance");

            // Gather ideal values
            body.Add(string.Format("Attenuation\n"));
            body.Add(string.Format("min = {0} dB\n", Math.Round(Atten_REFL_Eval_GetMinAttenuation(), digits)));
            body.Add(string.Format("nom = {0} dB\n", Math.Round(Atten_REFL_Eval_GetAttenuation(), digits)));
            body.Add(string.Format("max = {0} dB\n", Math.Round(Atten_REFL_Eval_GetMaxAttenuation(), digits)));
            body.Add("\n");
            body.Add(string.Format("Characteristic Impedance\n"));
            body.Add(string.Format("nom = {0} Ohm\n", Math.Round(Atten_REFL_Eval_GetImpedance(), digits)));
            body.Add(string.Format("worst = {0} Ohm\n", Math.Round(Atten_REFL_Eval_GetWorstImpedance(), digits)));

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            // Remove text from formatted paragraphs
            header.GetChildren().Clear();
            body.GetChildren().Clear();

            // Section 3 - Power Handling
            header.Add("3. Power Handling");

            double pwr_mw = Atten_REFL_GetPowerRating() * 1000;
            double pwr_dbm = Math.Round(10 * Math.Log10(pwr_mw), digits);

            body.Add("Maximum: \n");
            body.Add(pwr_mw + " mW\n");
            body.Add(pwr_dbm + " dBm\n\n");
            body.Add("Note: \nThe maximum input power assumes resistors rated for 1/16 W, at room temperature, and 0% derated.\n");

            // Add section header and body to document
            document.Add(header);
            document.Add(body);

            document.Close();
        }

        /** Reflection Attenuator - Synthesis / Evalute **/
        public void Atten_Refl_Run()
        {
            if (attenRunSynthesis)
            {
                refl_atten.Res1 = Atten_REFL_Synth_GetLowRes();
                refl_atten.Res2 = Atten_REFL_Synth_GetHighRes();
            }

            if (attenRunEvaluate)
            {
                refl_atten.Attenuation = Atten_REFL_Eval_GetAttenuation();
                refl_atten.Impedance = Atten_REFL_Eval_GetImpedance();
            }
        }
        #endregion

        /****************************/
        /** GUI Interface Handlers **/
        /****************************/
        /** ---------------- **/
        /** Export Functions **/
        /** ---------------- **/

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
                    Atten_Refl_Export_LTSpice();
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

        /** --------------- **/
        /** Other Functions **/
        /** --------------- **/
        
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
