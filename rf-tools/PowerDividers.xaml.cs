using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace rf_tools
{
    public class PowerDivider : INotifyPropertyChanged
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

    public class ResPowerDivider : PowerDivider
    {
        private double res;
        private double nPorts;

        public double Res
        {
            get { return res; }
            set { res = value; NotifyPropertyChanged("Res"); }
        }

        public double NPorts
        {
            get { return nPorts; }
            set { nPorts = value; NotifyPropertyChanged("NPorts"); }
        }
    }

    public class WilkPowerDivider : PowerDivider
    {
        private double res;
        private double tlImpedance;

        public double Res
        {
            get { return res; }
            set { res = value; NotifyPropertyChanged("Res"); }
        }

        public double TLImpedance
        {
            get { return tlImpedance; }
            set { tlImpedance = value; NotifyPropertyChanged("TLImpedance"); }
        }
    }

    public class BailPowerDivider : PowerDivider
    {
        private double pwrRatio;
        private double impedQuartWave;

        private double theta1;
        private double theta2;
        private double theta3;

        private string theta1Units;
        private string theta2Units;
        private string theta3Units;

        public double PwrRatio
        {
            get { return pwrRatio; }
            set { pwrRatio = value; NotifyPropertyChanged("PwrRatio"); }
        }

        public double ImpedQuartWave
        {
            get { return impedQuartWave; }
            set { impedQuartWave = value; NotifyPropertyChanged("ImpedQuartWave"); }
        }

        public double Theta1
        {
            get { return theta1; }
            set { theta1 = value; NotifyPropertyChanged("Theta1"); }
        }

        public double Theta2
        {
            get { return theta2; }
            set { theta2 = value; NotifyPropertyChanged("Theta2"); }
        }

        public double Theta3
        {
            get { return theta3; }
            set { theta3 = value; NotifyPropertyChanged("Theta3"); }
        }

        public string Theta1Units
        {
            get { return theta1Units; }
            set { theta1Units = value; NotifyPropertyChanged("Theta1Units"); }
        }

        public string Theta2Units
        {
            get { return theta2Units; }
            set { theta2Units = value; NotifyPropertyChanged("Theta2Units"); }
        }

        public string Theta3Units
        {
            get { return theta3Units; }
            set { theta3Units = value; NotifyPropertyChanged("Theta3Units"); }
        }
    }

    /// <summary>
    /// Interaction logic for PowerDividers.xaml
    /// </summary>
    public partial class PowerDividers : Window
    {
        private bool powDivRunSynthesis = false;
        private bool powDivRunEvaluate = false;

        private readonly ResPowerDivider resPowDiv = new ResPowerDivider();
        private readonly WilkPowerDivider wilkPowDiv = new WilkPowerDivider();
        private readonly BailPowerDivider bailPowDiv = new BailPowerDivider();

        public PowerDividers()
        {
            InitializeComponent();

            powDivResTab.DataContext = resPowDiv;
            powDivWilkTab.DataContext = wilkPowDiv;
            powDivBailTab.DataContext = bailPowDiv;
        }

        /** Synthesis Click Handler **/
        private void Atten_Synthesis_Click(object sender, RoutedEventArgs e)
        {
            powDivRunSynthesis = true;
            powDivRunEvaluate = false;
        }

        /** Evaluate Click Handler **/
        private void Atten_Evaluate_Click(object sender, RoutedEventArgs e)
        {
            powDivRunSynthesis = false;
            powDivRunEvaluate = true;
        }

        /*************************************/
        /** Resistive Power Divider Section **/
        /*************************************/

        /** -------------------------- **/
        /** Synthesis Function Section **/
        /** -------------------------- **/

        /** Calculate Resistances of Resistive Power Divider **/
        // 1. Calculate the common resistance for the N ports
        // 2. Convert the resistance to industry standard
        private double PowDiv_Res_Synth_GetResistance()
        {
            // Initialize variable values
            double r1;
            double n;
            double imped;
            double tol;

            imped = resPowDiv.Impedance;
            tol = resPowDiv.Tolerance;
            n = resPowDiv.NPorts;

            r1 = imped * (n - 2) / n;

            return CommonFunctions.GetStdResistor(r1, tol);
        }

        /** Calculate Ideal Attenuation of Resistive Power Divider **/
        private double PowDiv_Res_Synth_GetAttenuation()
        {
            double atten;

            if (1 >= resPowDiv.NPorts)
            {
                // If the number of ports is less than 1 then the Log function cannot return a value
                // In practical cases the number of ports won't be less than 2
                return -1;
            }

            atten = 20 * Math.Log10(resPowDiv.NPorts - 1);

            return atten;
        }

        /** ------------------------- **/
        /** Evaluate Function Section **/
        /** ------------------------- **/

        /** Calculate Real Attenuation of Resistive Power Divider **/
        private double PowDiv_Res_Eval_GetAttenuation()
        {
            double atten;
            double a, r1, r2;
            double imped = PowDiv_Res_Eval_GetImpedance();

            double[] ABCD = new double[4];

            // The resistive power divider is similar to the T attenuator when looking at
            // a single port. The attenuation can be calculated using the T attenuator
            // algorithm with a few adjustments.
            // 1. We can assume r1 = r3 since it follows for all the worst cases scenarios
            // 2. We assume that r2 = resistance + Z0
            r1 = resPowDiv.Res;
            r2 = resPowDiv.Res + resPowDiv.Impedance;

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 * (1 + ABCD[0]);
            ABCD[2] = 1 / r2;
            ABCD[3] = ABCD[0];

            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            atten = 20 * Math.Log10(Math.Abs(a));

            return atten;
        }

        /** Calculate Real Attenuation of Resistive Power Divider **/
        private double PowDiv_Res_Eval_GetWorstAttenuation()
        {
            double worstAtten;
            double nomAtten = PowDiv_Res_Eval_GetAttenuation();
            double lowAtten;
            double highAtten;

            double a, r1, r2;
            double imped = PowDiv_Res_Eval_GetImpedance();

            double[] ABCD = new double[4];

            // The resistive power divider is similar to the T attenuator when looking at
            // a single port. The attenuation can be calculated using the T attenuator
            // algorithm with a few adjustments.
            // 1. We can assume r1 = r3 since it follows for all the worst cases scenarios
            // 2. We assume that r2 = resistance + Z0

            // Highest Attenuation Case
            r1 = resPowDiv.Res * (1 + resPowDiv.Tolerance / 100);
            r2 = resPowDiv.Res * (1 - resPowDiv.Tolerance / 100) + resPowDiv.Impedance;

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 * (1 + ABCD[0]);
            ABCD[2] = 1 / r2;
            ABCD[3] = ABCD[0];

            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            highAtten = 20 * Math.Log10(Math.Abs(a));

            // Lowest Attenuation Case
            r1 = resPowDiv.Res * (1 - resPowDiv.Tolerance / 100);
            r2 = resPowDiv.Res * (1 + resPowDiv.Tolerance / 100) + resPowDiv.Impedance;

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 * (1 + ABCD[0]);
            ABCD[2] = 1 / r2;
            ABCD[3] = ABCD[0];

            a = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;
            lowAtten = 20 * Math.Log10(Math.Abs(a));

            worstAtten = (highAtten - nomAtten) > (nomAtten - lowAtten) ? highAtten : lowAtten;

            return worstAtten;
        }

        /** Calculate Input Impedance of Resistive Power Divider **/
        private double PowDiv_Res_Eval_GetImpedance()
        {
            double imped;

            imped = resPowDiv.Res;
            imped += (resPowDiv.Res + resPowDiv.Impedance) / (resPowDiv.NPorts - 1);

            return imped;
        }

        /** Calculate Input Impedance of Resistive Power Divider **/
        private double PowDiv_Res_Eval_GetWorstImpedance()
        {
            double nomImped = resPowDiv.Impedance;
            double lowImped;
            double highImped;
            double worstImped;

            double res;

            res = resPowDiv.Res * (1 + resPowDiv.Tolerance / 100);
            highImped = res;
            highImped += (res + resPowDiv.Impedance) / (resPowDiv.NPorts - 1);

            res = resPowDiv.Res * (1 - resPowDiv.Tolerance / 100);
            lowImped = res;
            lowImped += (res + resPowDiv.Impedance) / (resPowDiv.NPorts - 1);

            worstImped = (highImped - nomImped) > (nomImped - lowImped) ? highImped : lowImped;

            return worstImped;
        }

        /** -------------------------- **/
        /** Auxiliary Function Section **/
        /** -------------------------- **/

        /** Calculate Maximum Power of Resistive Power Divider **/
        private double PowDiv_Res_GetPowerRating()
        {
            double pwrRating = 0.0625;

            if (1 >= resPowDiv.NPorts)
            {
                // If the number of is less than 1 then the Log function cannot return a value
                // In practical cases the number of ports won't be less than 2
                return -1;
            }

            if (powDivRunSynthesis)
            {
                pwrRating = 0.0625 * (resPowDiv.NPorts - 1) / resPowDiv.NPorts;
            }

            if (powDivRunEvaluate)
            {
                double n = resPowDiv.Impedance / resPowDiv.Res;
                pwrRating = 0.0625 * (n - 1) / n;
            }

            return pwrRating;
        }

        private double PowDiv_Res_GetImbalance()
        {
            double imbalance;
            double lowA;
            double highA;

            double r1, r2;
            double imped = PowDiv_Res_Eval_GetImpedance();

            double[] ABCD = new double[4];

            // The resistive power divider is similar to the T attenuator when looking at
            // a single port. The attenuation can be calculated using the T attenuator
            // algorithm with a few adjustments.
            // 1. We can assume r1 = r3 since it follows for all the worst cases scenarios
            // 2. We assume that r2 = resistance + Z0

            // Highest Attenuation Case
            r1 = resPowDiv.Res * (1 + resPowDiv.Tolerance / 100);
            r2 = resPowDiv.Res * (1 - resPowDiv.Tolerance / 100) + resPowDiv.Impedance;

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 * (1 + ABCD[0]);
            ABCD[2] = 1 / r2;
            ABCD[3] = ABCD[0];

            highA = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;

            // Lowest Attenuation Case
            r1 = resPowDiv.Res * (1 - resPowDiv.Tolerance / 100);
            r2 = resPowDiv.Res * (1 + resPowDiv.Tolerance / 100) + resPowDiv.Impedance;

            ABCD[0] = 1 + (r1 / r2);
            ABCD[1] = r1 * (1 + ABCD[0]);
            ABCD[2] = 1 / r2;
            ABCD[3] = ABCD[0];

            lowA = (ABCD[0] + ABCD[1] / imped + ABCD[2] * imped + ABCD[3]) / 2;

            // Because we're using attenuation and not gain highA > lowA
            // Convert the imbalance into +/- the nominal by dividing by 2
            imbalance = 0.5 * 20 * Math.Log10(highA / lowA);

            return imbalance;
        }

        /** Export LTSpice of Resistive Power Divider **/
        private void PowDiv_Res_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            string DocuPath = CommonFunctions.SaveFile(".asc", "LTSpice File|.asc");

            adapter.AddSource(new int[] { 1, 0 }, new LTSpiceCoords(-64, 16, 0));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 2, 1 }, new LTSpiceCoords(96, 0, 90),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 2, 3 }, new LTSpiceCoords(80, 32, 0),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 3, 0 }, new LTSpiceCoords(80, -32, 0),
                string.Format("{{mc({0}, tolR)}}", "50"));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 4, 2 }, new LTSpiceCoords(224, 0, 90),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation
            adapter.AddResistor(new int[] { 4, 0 }, new LTSpiceCoords(208, 16, 0), "50");

            adapter.AddNetSim(0, -128, 100000000, 4000000000, 100);
            adapter.AddParameter(0, -64, "tolR", (float)resPowDiv.Tolerance / 100);

            File.WriteAllText(DocuPath, adapter.ToString());
        }

        /** Generate and Export PDF Report **/
        private void PowDiv_Res_Export_Report()
        {
            // Keep error list happy
            // string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // 1. Ideal Parameters
            // 1a. Number of Ports
            // 1b. System Impedance
            // 1c. Resistor Values
            _ = resPowDiv.NPorts;
            _ = resPowDiv.Impedance;
            _ = resPowDiv.Res;

            // 2. Evaluated Parameters
            // 2a. Input impedance
            // 2b. Input impedance tolerance
            // 2c. Branch attenuation
            // 2d. Branch attenuation tolerance
            _ = PowDiv_Res_Eval_GetImpedance();
            _ = PowDiv_Res_Eval_GetWorstImpedance();
            _ = PowDiv_Res_Eval_GetAttenuation();
            _ = PowDiv_Res_Eval_GetWorstAttenuation();

            // 3. Additional Parameters
            // 3a. Maximum power handling
            // 3b. Worst case imbalance
            _ = PowDiv_Res_GetPowerRating();
            _ = PowDiv_Res_GetImbalance();
        }

        /** Resistive Power Divider - Synthesis / Evalute **/
        public void PowDiv_Res_Run()
        {
            // Initialize variable values
            bool genReport = resPowDiv.GenReport;

            if (powDivRunSynthesis)
            {
                _ = PowDiv_Res_Synth_GetAttenuation();
                resPowDiv.Res = PowDiv_Res_Synth_GetResistance();
            }

            if (powDivRunEvaluate)
            {
                _ = PowDiv_Res_Eval_GetAttenuation();
                resPowDiv.Impedance = PowDiv_Res_Eval_GetImpedance();
            }

            if (genReport)
            {
                // Synthesis and Evaluate toggles will be false
                // There are 3 possible states during report generation:
                // 1. No data has been entered
                // 2. Data has been entered but not processed
                // 3. Data has been entered and processed

                resPowDiv.GenReport = false;

                // Run Synthesis Commands
                powDivRunSynthesis = true;
                powDivRunEvaluate = false;

                PowDiv_Res_Run();

                // Run Evaluate Commands
                powDivRunSynthesis = false;
                powDivRunEvaluate = true;

                PowDiv_Res_Run();

                // Reset toggle states
                resPowDiv.GenReport = true;
                powDivRunSynthesis = false;
                powDivRunEvaluate = false;

                PowDiv_Res_Export_Report();
            }
        }

        /*************************************/
        /** Wilkinson Power Divider Section **/
        /*************************************/

        /** -------------------------- **/
        /** Synthesis Function Section **/
        /** -------------------------- **/

        /** Calculate Input Impedance of Wilkinson Power Divider **/
        private double PowDiv_Wilk_Synth_GetTLineImpedance()
        {
            double imped;

            imped = Math.Sqrt(2) * wilkPowDiv.Impedance;

            return imped;
        }

        /** Calculate Resistor of Wilkinson Power Divider **/
        private double PowDiv_Wilk_Synth_GetResistance()
        {
            double res;
            double tol;

            res = 2 * wilkPowDiv.Impedance;
            tol = wilkPowDiv.Tolerance;

            return CommonFunctions.GetStdResistor(res, tol);
        }

        /** ------------------------- **/
        /** Evaluate Function Section **/
        /** ------------------------- **/

        /** Calculate Input Impedance of Wilkinson Power Divider **/
        private double PowDiv_Wilk_Eval_GetSystemImpedance()
        {
            double imped;
            double r;

            r = wilkPowDiv.Res;

            imped = r / 2;

            return imped;
        }

        /** -------------------------- **/
        /** Auxiliary Function Section **/
        /** -------------------------- **/

        /** Calculate Power Rating of Wilkinson Power Divider **/
        private double PowDiv_Wilk_GetPowerRating()
        {
            // During fault scenario the resistor dissipates 1/4 of the input power
            // Pres = Pin / 4 -> Pin = 4 * Pres
            // https://www.microwaves101.com/encyclopedias/how-to-size-isolation-resistors-in-sspas
            double pwrRating = 0.0625 * 4;

            return pwrRating;
        }

        /** Calculate Power Imbalance of Wilkinson Power Divider **/
        private double PowDiv_Wilk_GetImbalance()
        {
            double imbalance = 0;

            return imbalance;
        }

        /** Export LTSpice of Wilkinson Power Divider **/
        // TO DO: NEEDS TO BE UPDATED - COPIED FROM TEE ATTENUATOR
        private void PowDiv_Wilk_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();

            string UserName = Environment.UserName;
            string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

            adapter.AddSource(new int[] { 1, 0 }, new LTSpiceCoords(-48, 16, 0));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 2, 1 }, new LTSpiceCoords(96, 0, 90),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 2, 0 }, new LTSpiceCoords(80, 16, 0),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 3, 2 }, new LTSpiceCoords(224, 0, 90),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation
            adapter.AddResistor(new int[] { 3, 0 }, new LTSpiceCoords(208, 16, 0), "50");

            adapter.AddNetSim(0, -128, 100000000, 4000000000, 100);
            adapter.AddParameter(0, -64, "tolR", (float)resPowDiv.Tolerance / 100);

            File.WriteAllText(DocuPath + "resistive_power_divider.asc", adapter.ToString());
        }

        /** Generate and Export PDF Report **/
        private void PowDiv_Wilk_Export_Report()
        {
            // Keep error list happy
            // string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // 1. Ideal Parameters
            // 1a. System Impedance
            // 1b. Resistor Values
            _ = wilkPowDiv.Impedance;
            _ = wilkPowDiv.Res;

            // 2. Evaluated Parameters

            // 3. Additional Parameters
            // 3a. Maximum power handling
            // 3b. Worst case imbalance
            _ = PowDiv_Wilk_GetPowerRating();
            _ = PowDiv_Wilk_GetImbalance();
        }

        /** Wilkinson Power Divider - Synthesis / Evalute **/
        public void PowDiv_Wilk_Run()
        {
            // Initialize variable values
            bool genReport = wilkPowDiv.GenReport;

            if (powDivRunSynthesis)
            {
                wilkPowDiv.Res = PowDiv_Wilk_Synth_GetResistance();
                wilkPowDiv.TLImpedance = PowDiv_Wilk_Synth_GetTLineImpedance();
            }

            if (powDivRunEvaluate)
            {
                wilkPowDiv.Impedance = PowDiv_Wilk_Eval_GetSystemImpedance();
            }

            if (genReport)
            {
                // Synthesis and Evaluate toggles will be false
                // There are 3 possible states during report generation:
                // 1. No data has been entered
                // 2. Data has been entered but not processed
                // 3. Data has been entered and processed

                resPowDiv.GenReport = false;

                // Run Synthesis Commands
                powDivRunSynthesis = true;
                powDivRunEvaluate = false;

                PowDiv_Wilk_Run();

                // Run Evaluate Commands
                powDivRunSynthesis = false;
                powDivRunEvaluate = true;

                PowDiv_Wilk_Run();

                // Reset toggle states
                wilkPowDiv.GenReport = true;
                powDivRunSynthesis = false;
                powDivRunEvaluate = false;

                PowDiv_Wilk_Export_Report();
            }
        }

        /**********************************/
        /** Bailey Power Divider Section **/
        /**********************************/

        /** -------------------------- **/
        /** Synthesis Function Section **/
        /** -------------------------- **/

        /** Calculate Quarter Wave transformer impedance of Bailey Power Divider **/
        private double PowDiv_Bail_Synth_GetTheta1()
        {
            double theta1;

            theta1 = Math.PI / 2;

            return CommonFunctions.ConvertToExternalUnits(theta1, bailPowDiv.Theta1Units);
        }

        /** Calculate Theta 2 of Bailey Power Divider **/
        private double PowDiv_Bail_Synth_GetTheta2()
        {
            double theta2;

            theta2 = Math.Atan(Math.Sqrt(bailPowDiv.PwrRatio));

            return CommonFunctions.ConvertToExternalUnits(theta2, bailPowDiv.Theta2Units);
        }

        /** Calculate Theta 3 of Bailey Power Divider **/
        private double PowDiv_Bail_Synth_GetTheta3()
        {
            double theta2;
            double theta3;

            theta2 = Math.Atan(Math.Sqrt(bailPowDiv.PwrRatio));
            theta3 = Math.PI / 2 - theta2;

            return CommonFunctions.ConvertToExternalUnits(theta3, bailPowDiv.Theta3Units);
        }

        /** Calculate Quarter Wave transformer impedance of Bailey Power Divider **/
        private double PowDiv_Bail_Synth_GetQuartWaveImpedance()
        {
            double imped;

            imped = bailPowDiv.Impedance / Math.Sqrt(2);

            return imped;
        }

        /** ------------------------- **/
        /** Evaluate Function Section **/
        /** ------------------------- **/
        // No evaluate functions available

        /** -------------------------- **/
        /** Auxiliary Function Section **/
        /** -------------------------- **/

        /** Export LTSpice of Resistive Power Divider **/
        private void PowDiv_Bail_Export_LTSpice()
        {
            LTSpiceAdapter adapter = new LTSpiceAdapter();
            string DocuPath = CommonFunctions.SaveFile(".asc", "LTSpice File|.asc");

            adapter.AddSource(new int[] { 1, 0 }, new LTSpiceCoords(-64, 16, 0));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 2, 1 }, new LTSpiceCoords(96, 0, 90),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 2, 3 }, new LTSpiceCoords(80, 32, 0),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 3, 0 }, new LTSpiceCoords(80, -32, 0),
                string.Format("{{mc({0}, tolR)}}", "50"));

            // Add resistor at the specified location and rotation with monte-carlo enabled
            adapter.AddResistor(new int[] { 4, 2 }, new LTSpiceCoords(224, 0, 90),
                string.Format("{{mc({0}, tolR)}}", resPowDiv.Res));

            // Add resistor at the specified location and rotation
            adapter.AddResistor(new int[] { 4, 0 }, new LTSpiceCoords(208, 16, 0), "50");

            adapter.AddNetSim(0, -128, 100000000, 4000000000, 100);
            adapter.AddParameter(0, -64, "tolR", (float)resPowDiv.Tolerance / 100);

            File.WriteAllText(DocuPath, adapter.ToString());
        }

        /** Generate and Export PDF Report **/
        private void PowDiv_Bail_Export_Report()
        {
            // Keep error list happy
            // string docPath = CommonFunctions.SaveFile(".pdf", "Portable Document Format|.pdf");

            // 1. Ideal Parameters
            _ = bailPowDiv.ImpedQuartWave;

            _ = bailPowDiv.Theta1;
            _ = bailPowDiv.Theta2;
            _ = bailPowDiv.Theta3;
        }

        /** Resistive Power Divider - Synthesis / Evalute **/
        public void PowDiv_Bail_Run()
        {
            // Initialize variable values
            bool genReport = resPowDiv.GenReport;

            if (powDivRunSynthesis)
            {
                bailPowDiv.ImpedQuartWave = PowDiv_Bail_Synth_GetQuartWaveImpedance();

                bailPowDiv.Theta1 = PowDiv_Bail_Synth_GetTheta1();
                bailPowDiv.Theta2 = PowDiv_Bail_Synth_GetTheta2();
                bailPowDiv.Theta3 = PowDiv_Bail_Synth_GetTheta3();
            }

            if (powDivRunEvaluate)
            {
            }

            if (genReport)
            {
                // Synthesis and Evaluate toggles will be false
                // There are 3 possible states during report generation:
                // 1. No data has been entered
                // 2. Data has been entered but not processed
                // 3. Data has been entered and processed

                resPowDiv.GenReport = false;

                // Run Synthesis Commands
                powDivRunSynthesis = true;
                powDivRunEvaluate = false;

                PowDiv_Bail_Run();

                // Run Evaluate Commands
                powDivRunSynthesis = false;
                powDivRunEvaluate = true;

                PowDiv_Bail_Run();

                // Reset toggle states
                resPowDiv.GenReport = true;
                powDivRunSynthesis = false;
                powDivRunEvaluate = false;

                PowDiv_Bail_Export_Report();
            }
        }

        /*****************************/
        /** Export Function Section **/
        /*****************************/

        /** Report Generation Function **/
        private void Export_Report(object sender, RoutedEventArgs e)
        {
            PowDiv_Synthesis_Click(sender, e);
            PowDiv_Evaluate_Click(sender, e);

            // Select the appropriate export tool
            switch (powDivTabCtrl.SelectedIndex)
            {
                /**Tab 0: Resistive Power Divider**/
                case 0:
                    PowDiv_Res_Export_Report();
                    break;

                /**Tab 1: Wilkinson Power Divider**/
                case 1:
                    PowDiv_Wilk_Export_Report();
                    break;

                /**Tab 2: Bailey Power Divider**/
                case 2:
                    PowDiv_Bail_Export_Report();
                    break;

                /**Default: Resistive Power Divider**/
                default:
                    PowDiv_Res_Export_Report();
                    break;
            }
        }

        /** LTSpice Export Handler **/
        private void Export_LTSpice(object sender, RoutedEventArgs e)
        {
            // Assign the permitivity value
            switch (powDivTabCtrl.SelectedIndex)
            {
                /**Tab 0: Resistive Power Divider**/
                case 0:
                    PowDiv_Res_Export_LTSpice();
                    break;

                /**Tab 1: Wilkinson Power Divider**/
                case 1:
                    PowDiv_Wilk_Export_LTSpice();
                    break;

                /**Tab 2: Bailey Power Divider**/
                case 2:
                    PowDiv_Bail_Export_LTSpice();
                    break;

                /**Default: Resistive Power Divider**/
                default:
                    PowDiv_Res_Export_LTSpice();
                    break;
            }
        }

        /***********************************/
        /** GUI Interface Handler Section **/
        /***********************************/

        /** Synthesis Click Handler **/
        private void PowDiv_Synthesis_Click(object sender, RoutedEventArgs e)
        {
            powDivRunSynthesis = true;
            powDivRunEvaluate = false;

            switch (powDivTabCtrl.SelectedIndex)
            {
                /**Tab 0: Resistive Power Divider**/
                case 0:
                    PowDiv_Res_Run();
                    break;

                /**Tab 1: Wilkinson Power Divider**/
                case 1:
                    PowDiv_Wilk_Run();
                    break;

                /**Tab 2: Bailey Power Divider**/
                case 2:
                    PowDiv_Bail_Run();
                    break;

                /**Default: Resistive Power Divider**/
                default:
                    PowDiv_Res_Run();
                    break;
            }

            powDivRunSynthesis = false;
            powDivRunEvaluate = false;
        }

        /** Evaluate Click Handler **/
        private void PowDiv_Evaluate_Click(object sender, RoutedEventArgs e)
        {
            powDivRunSynthesis = false;
            powDivRunEvaluate = true;

            switch (powDivTabCtrl.SelectedIndex)
            {
                /**Tab 0: Resistive Power Divider**/
                case 0:
                    PowDiv_Res_Run();
                    break;

                /**Tab 1: Wilkinson Power Divider**/
                case 1:
                    PowDiv_Wilk_Run();
                    break;

                /**Tab 2: Bailey Power Divider**/
                case 2:
                    PowDiv_Bail_Run();
                    break;

                /**Default: Resistive Power Divider**/
                default:
                    PowDiv_Res_Run();
                    break;
            }

            powDivRunSynthesis = false;
            powDivRunEvaluate = false;
        }

        /** Help Button Click Handler **/
        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.CurrentDirectory;

            _ = System.Diagnostics.Process.Start(filePath + "\\HelpGuide\\attenuator.html");
        }
    }
}
