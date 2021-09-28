using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace rf_tools
{
    public class FilterPrototype : INotifyPropertyChanged
    {
        private double[] protoVals;

        private string type;
        private string arch;
        private string firstElem;
        private int order;

        private double impedance;
        private double ripple;
        private double frequency;
        private string frequencyUnits;
        private double bandwidth;
        private string bandwidthUnits;

        private bool enableRipple;
        private bool enableBandwidth;

        private bool isLpfEn;
        private bool isHpfEn;
        private bool isBpfEn;
        private bool isBsfEn;


        public bool IsLpfEn
        {
            get { return isLpfEn; }
            set
            {
                isLpfEn = value;
                NotifyPropertyChanged("IsLpfEn");
            }
        }

        public bool IsHpfEn
        {
            get { return isHpfEn; }
            set
            {
                isHpfEn = value;
                NotifyPropertyChanged("IsHpfEn");
            }
        }

        public bool IsBpfEn
        {
            get { return isBpfEn; }
            set
            {
                isBpfEn = value;
                NotifyPropertyChanged("IsBpfEn");
            }
        }

        public bool IsBsfEn
        {
            get { return isBsfEn; }
            set
            {
                isBsfEn = value;
                NotifyPropertyChanged("IsBsfEn");
            }
        }


        public bool EnableBandwidth
        {
            get { return enableBandwidth; }
            set
            {
                enableBandwidth = value;
                NotifyPropertyChanged("EnableBandwidth");
            }
        }


        public bool EnableRipple
        {
            get { return enableRipple; }
            set
            {
                enableRipple = value;
                NotifyPropertyChanged("EnableRipple");
            }
        }


        public double[] ProtoVals
        {
            get { return protoVals; }
            set
            {
                protoVals = value;
                NotifyPropertyChanged("ProtoVals");
            }
        }



        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Arch
        {
            get { return arch; }
            set { arch = value; }
        }

        public string FirstElem
        {
            get { return firstElem; }
            set { firstElem = value; }
        }

        public int Order
        {
            get { return order; }
            set { order = value; }
        }



        public double Impedance
        {
            get { return impedance; }
            set
            {
                impedance = value;
                NotifyPropertyChanged("Impedance");
            }
        }

        public double Ripple
        {
            get { return ripple; }
            set
            {
                ripple = value;
                NotifyPropertyChanged("Ripple");
            }
        }

        public double Frequency
        {
            get { return frequency; }
            set
            {
                frequency = value;
                NotifyPropertyChanged("Frequency");
            }
        }

        public string FrequencyUnits
        {
            get { return frequencyUnits; }
            set { frequencyUnits = value; }
        }

        public double Bandwidth
        {
            get { return bandwidth; }
            set
            {
                bandwidth = value;
                NotifyPropertyChanged("Bandwidth");
            }
        }

        public string BandwidthUnits
        {
            get { return bandwidthUnits; }
            set { bandwidthUnits = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class LumpedFilterParams : INotifyPropertyChanged
    {
        private string compString;
        private BitmapImage filtImage;

        public string Ideal { get; set; }

        public double MinCap { get; set; }
        public string CapUnit { get; set; }
        public double CapTol { get; set; }

        public double MinInd { get; set; }
        public string IndUnit { get; set; }
        public double IndTol { get; set; }

        public string CompString
        {
            get { return compString; }
            set
            {
                compString = value;
                NotifyPropertyChanged("CompString");
            }
        }

        public BitmapImage FiltImage
        {
            get { return filtImage; }
            set
            {
                filtImage = value;
                NotifyPropertyChanged("FiltImage");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class CommLineFilterParams : INotifyPropertyChanged
    {
        private string tLineString;

        public double MinImpedance { get; set; }
        public double MaxImpedance { get; set; }

        public string TLineString
        {
            get { return tLineString; }
            set
            {
                tLineString = value;
                NotifyPropertyChanged("TLineString");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class SteppedFilterParams : INotifyPropertyChanged
    {
        private string tLineString;

        public double LowImpedance { get; set; }
        public double HighImpedance { get; set; }

        public string TLineString
        {
            get { return tLineString; }
            set
            {
                tLineString = value;
                NotifyPropertyChanged("TLineString");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    /// <summary>
    /// Interaction logic for Filter.xaml
    /// </summary>
    ///
    public partial class Filter : Window
    {
        readonly double[][] flatPhaseProto;

        readonly FilterPrototype prototype = new FilterPrototype();
        readonly LumpedFilterParams lumpedParams = new LumpedFilterParams();
        readonly CommLineFilterParams commParams = new CommLineFilterParams();
        readonly SteppedFilterParams steppedParams = new SteppedFilterParams();

        public Filter()
        {
            InitializeComponent();

            // Assigning linear phase prototype values to jagged array
            flatPhaseProto = new double[10][];

            flatPhaseProto[0] = new double[] { 2.0000, 1.0000 };
            flatPhaseProto[1] = new double[] { 1.5774, 0.4226, 1.0000 };
            flatPhaseProto[2] = new double[] { 1.2550, 0.5528, 0.1922, 1.0000 };

            filterTabCtrl.DataContext = prototype;
            filtLumpedTab.DataContext = lumpedParams;
            filtCommLineTab.DataContext = commParams;
            filtSteppedTab.DataContext = steppedParams;

            // Set default configuration
            prototype.IsLpfEn = true;
            prototype.IsHpfEn = false;
            prototype.IsBpfEn = false;
            prototype.IsBsfEn = false;

            if (filtCommLineTab != null)
                filtCommLineTab.IsEnabled = prototype.IsLpfEn;

            if (filtSteppedTab != null)
                filtSteppedTab.IsEnabled = prototype.IsLpfEn;

            if (filtCoupLineTab != null)
                filtCoupLineTab.IsEnabled = prototype.EnableBandwidth;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += timer_Tick;

            timer.Start();
        }

        /***************************************
        ******   Supplemental Functions   ******
        ****************************************/
        /** Calculate Standard Resistance Value **/
        private double GetStdValue(double value, double tolerance)
        {
            // Default value is 0
            double stdVal = 0;
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
            if (value != 0)
            {
                whole = Math.Log10(value);
                inte = (int)whole;
                deci = whole - inte;

                double exp = 2 + (Math.Round(esr * deci) / esr);

                stdVal = Math.Round(Math.Pow(10, exp));
                stdVal = stdVal * Math.Pow(10, inte - 2);
            }

            return stdVal;
        }

        /** Convert to Internal Units **/
        private double ConvertToInternalUnits(double val, string unitOut)
        {
            double scale = 1;

            /**Dimensional Conversions**/
            // Metric
            if ("mm" == unitOut) scale = 0.001;
            if ("cm" == unitOut) scale = 0.01;

            // Imperial
            if ("mil" == unitOut) scale = 0.0000254;
            if ("inch" == unitOut) scale = 0.0254;

            /**Frequency Conversions**/
            if ("GHz" == unitOut) scale = 1000000000;
            if ("MHz" == unitOut) scale = 1000000;
            if ("kHz" == unitOut) scale = 1000;

            /**Angle Conversion**/
            if ("deg" == unitOut) scale = Math.PI / 180;

            /**Capacitance Conversion**/
            if ("fF" == unitOut) scale = 0.000000000000001;
            if ("pF" == unitOut) scale = 0.000000000001;
            if ("nF" == unitOut) scale = 0.000000001;
            if ("uF" == unitOut) scale = 0.000001;

            /**Inductance Conversion**/
            if ("pH" == unitOut) scale = 0.000000000001;
            if ("nH" == unitOut) scale = 0.000000001;
            if ("uH" == unitOut) scale = 0.000001;

            return (scale * val);
        }

        /** Convert to Engineering Units **/
        private string ConvertToEngineeringUnits(double val)
        {
            double scale = 1;
            string unit = "";

            string output;

            double logVal = Math.Log10(val);

            if (-12 > logVal)
            {
                scale = 1e15;
                unit = "f";
            }

            if (-12 <= logVal && -9 > logVal)
            {
                scale = 1e12;
                unit = "p";
            }

            if (-9 <= logVal && -7 > logVal)
            {
                scale = 1e9;
                unit = "n";
            }

            if (-6 <= logVal && -4 > logVal)
            {
                scale = 1e6;
                unit = "u";
            }

            output = string.Format("{0} {1}", Math.Round(val * scale, 3), unit);

            return output;
        }

        /** Update Filter Images **/
        private void UpdateFilterImages()
        {
            int order = prototype.Order;
            string type = prototype.Type;
            string firstElem = prototype.FirstElem;

            // To be called after type change and order change
            if (0 == order % 2)
            {
                // Even Order
                if ("LPF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Even_LPF_Series.png", UriKind.Relative));
                    }
                    else
                    {
                        // Shunt Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Even_LPF_Shunt.png", UriKind.Relative));
                    }
                }

                if ("HPF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Even_HPF_Series.png", UriKind.Relative));
                    }
                    else
                    {
                        // Shunt Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Even_HPF_Shunt.png", UriKind.Relative));

                    }
                }

                if ("BPF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem

                    }
                    else
                    {
                        // Shunt Elem

                    }
                }

                if ("BSF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem

                    }
                    else
                    {
                        // Shunt Elem

                    }
                }
            }
            else
            {
                // Odd order
                if ("LPF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Odd_LPF_Series.png", UriKind.Relative));
                    }
                    else
                    {
                        // Shunt Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Odd_LPF_Shunt.png", UriKind.Relative));
                    }
                }

                if ("HPF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Odd_HPF_Series.png", UriKind.Relative));
                    }
                    else
                    {
                        // Shunt Elem
                        lumpedParams.FiltImage = new BitmapImage(new Uri(@"/Images/Filters/LumpedFilter_Odd_HPF_Shunt.png", UriKind.Relative));
                    }
                }

                if ("BPF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem

                    }
                    else
                    {
                        // Shunt Elem

                    }
                }

                if ("BSF" == type)
                {
                    if ("Series" == firstElem)
                    {
                        // Series Elem

                    }
                    else
                    {
                        // Shunt Elem

                    }
                }
            }
        }

        // Inverse Hyperbolic Sine 
        public static double Asinh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x + 1));
        }

        // Inverse Hyperbolic Cosine 
        public static double Acosh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x - 1));
        }

        /*******************************************
        ******   Filter Prototype Functions   ******
        ********************************************/
        /** Butterworth Prototype Value **/
        private double[] Filter_GetButterworthValues()
        {
            // Gather information from bindings
            int order = prototype.Order;

            // Instantiate arrayof prototype values to be returned
            double[] butterworthValues = new double[order + 1];

            double aR0 = 0;
            double aR1;

            double bR0 = 1;
            double bR1;

            double delta = 0;

            butterworthValues[order] = 1;

            // Calculate the normalized component values
            for (int i = 1; i < order + 1; i++)
            {
                aR1 = Math.Sin((2 * i - 1) * Math.PI / (2 * order));

                bR1 = 1 + Math.Pow(delta, 2);
                bR1 -= 2 * delta * Math.Cos(i * Math.PI / order);

                if (1 == i)
                {
                    butterworthValues[i - 1] = 2 * aR1 / (1 - delta);
                }
                else
                {
                    butterworthValues[i - 1] = 4 * aR1 * aR0 / (bR0 * butterworthValues[i - 2]);
                }


                aR0 = aR1;
                bR0 = bR1;
            }

            return butterworthValues;
        }

        /** Chebyshev Prototype Value **/
        private double[] Filter_GetChebyshevValues()
        {
            double ripple = prototype.Ripple;
            int order = prototype.Order;

            double[] chebyshevValues = new double[order + 1];

            double aR0 = 0;
            double aR1;

            double bR0 = 1;
            double bR1;

            double delta;
            double gamma;

            double eps = Math.Sqrt(Math.Pow(10, ripple / 10) - 1);
            double beta = 2 * Asinh(1 / eps);
            double T = 1;

            double multiplier = Math.Cosh(Acosh(1 / eps) / order);
            chebyshevValues[order] = 1;

            // If the order is even then the filter requires modifying
            if (0 == order % 2)
            {
                T = Math.Pow(10, ripple / 10);
                multiplier = Math.Cosh(Acosh(Math.Sqrt(Math.Pow(2.10, ripple / 10) - 1) / eps) / order);

                // Get the normalized load impedance
                if ("Series" == prototype.FirstElem)
                {
                    chebyshevValues[order] = 1 / Math.Pow(Math.Tanh(beta / 4), 2);
                }
                else
                {
                    chebyshevValues[order] = Math.Pow(Math.Tanh(beta / 4), 2);
                }
            }

            delta = Math.Sinh(Asinh(Math.Sqrt(1 - T) / eps) / order);
            gamma = Math.Sinh(beta / (2 * order));

            // Calculate the normalized component values
            for (int i = 1; i < order + 1; i++)
            {
                aR1 = Math.Sin((2 * i - 1) * Math.PI / (2 * order));

                bR1 = Math.Pow(gamma, 2) + Math.Pow(delta, 2);
                bR1 -= 2 * gamma * delta * Math.Cos(i * Math.PI / (2 * order));
                bR1 += Math.Pow(Math.Sin(i * Math.PI / order), 2);

                if (1 == i)
                {
                    chebyshevValues[i - 1] = 2 * aR1 / (gamma - delta);
                }
                else
                {
                    chebyshevValues[i - 1] = 4 * aR1 * aR0 / (bR0 * chebyshevValues[i - 2]);
                }

                aR0 = aR1;
                bR0 = bR1;
            }

            // Apply constant multiplication factor
            for (int i = 0; i < order; i++)
            {
                chebyshevValues[i] *= multiplier;
            }

            return chebyshevValues;
        }

        /** Generic Wrapper Function to get Prototype Values **/
        private void Filter_GetProtoValues()
        {
            // Filter prototype values are indexed by 1 less than the order
            int index = prototype.Order - 1;

            if ("Butterworth" == prototype.Arch)
            {
                // Calculate Butterworth Prototype Values based on order and impedance
                prototype.ProtoVals = Filter_GetButterworthValues();
            }

            if ("Chebyshev" == prototype.Arch)
            {
                // Prototype values need to be calculated based on ripple req't
                prototype.ProtoVals = Filter_GetChebyshevValues();
            }

            if ("Equiripple" == prototype.Arch)
            {
                // Prototype values need to be calculated based on ripple req't
            }

            if ("Flat Phase" == prototype.Arch)
            {
                //Flat phase prototype has only one set of values for each order
                prototype.ProtoVals = flatPhaseProto[index];
            }
        }

        /************************************************
        ******   Filter Implementation Functions   ******
        *************************************************/
        // Lumped Element Filter
        private void Filter_CalcLumped()
        {
            double[] filtVals;

            double frequency = ConvertToInternalUnits(prototype.Frequency, prototype.FrequencyUnits);
            double bandwidth = ConvertToInternalUnits(prototype.Bandwidth, prototype.BandwidthUnits);

            double delta = bandwidth / frequency;

            double impedance = prototype.Impedance;

            double minC = ConvertToInternalUnits(lumpedParams.MinCap, lumpedParams.CapUnit);
            double tempC;

            double minL = ConvertToInternalUnits(lumpedParams.MinInd, lumpedParams.IndUnit);
            double tempL;

            bool toggle = false;

            lumpedParams.CompString = "Component Values\n";

            switch (prototype.Type)
            {
                case "LPF":
                    filtVals = new double[prototype.Order];

                    if ("Series" == prototype.FirstElem) toggle = true;
                    if ("Shunt" == prototype.FirstElem) toggle = false;

                    for (int i = 0; i < prototype.Order; i++)
                    {
                        // Frequency scale the prototype values
                        filtVals[i] = prototype.ProtoVals[i] / (2 * Math.PI * frequency);

                        // Impedance scaling the prototype values
                        if (toggle)
                        {
                            // Inductor value
                            filtVals[i] *= impedance;

                            // if(true) is placeholder for the expression TBD
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempL = GetStdValue(filtVals[i], lumpedParams.IndTol);

                                if (tempL < minL) tempL = minL;

                                filtVals[i] = tempL;
                            }

                            lumpedParams.CompString += string.Format("L{0} = {1}H\n", i + 1, ConvertToEngineeringUnits(filtVals[i]));
                        }
                        else
                        {
                            // Capacitor value
                            filtVals[i] /= impedance;

                            // if(true) is placeholder for the expression TBD
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempC = GetStdValue(filtVals[i], lumpedParams.CapTol);

                                if (tempC < minC) tempC = minC;

                                filtVals[i] = tempC;
                            }

                            lumpedParams.CompString += string.Format("C{0} = {1}F\n", i + 1, ConvertToEngineeringUnits(filtVals[i]));
                        }

                        toggle = !toggle;
                    }
                    break;

                case "HPF":
                    filtVals = new double[prototype.Order];

                    if ("Series" == prototype.FirstElem) toggle = true;
                    if ("Shunt" == prototype.FirstElem) toggle = false;

                    for (int i = 0; i < prototype.Order; i++)
                    {
                        // Frequency scale the prototype values
                        filtVals[i] = 1 / (2 * Math.PI * frequency * prototype.ProtoVals[0]);

                        // Impedance scaling the prototype values
                        if (toggle)
                        {
                            filtVals[i] *= impedance;

                            // if(true) is placeholder for the expression TBD
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempC = GetStdValue(filtVals[i], lumpedParams.CapTol);

                                if (tempC < minC) tempC = minC;

                                filtVals[i] = tempC;
                            }

                            lumpedParams.CompString += string.Format("C{0} = {1}F\n", i + 1, ConvertToEngineeringUnits(filtVals[i]));
                        }
                        else
                        {
                            // Inductor value
                            filtVals[i] /= impedance;

                            // Check for standard or ideal min/max 
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempC = GetStdValue(filtVals[i], lumpedParams.IndTol);

                                if (tempC < minC) tempC = minC;

                                filtVals[i] = tempC;
                            }

                            lumpedParams.CompString += string.Format("L{0} = {1}H\n", i + 1, ConvertToEngineeringUnits(filtVals[i]));
                        }

                        toggle = !toggle;
                    }
                    break;

                case "BPF":
                    filtVals = new double[2 * prototype.Order];

                    if ("Series" == prototype.FirstElem) toggle = true;
                    if ("Shunt" == prototype.FirstElem) toggle = false;

                    for (int i = 0; i < prototype.Order; i++)
                    {
                        // Split the proto components into inductor and capacitor
                        // Frequency scale values 
                        if (toggle)
                        {
                            // Series Element (inductor proto)
                            filtVals[2 * i] = prototype.ProtoVals[i] / (2 * Math.PI * frequency * delta);
                            filtVals[2 * i + 1] = delta / (2 * Math.PI * frequency * prototype.ProtoVals[i]);

                            // Check for standard or ideal min/max 
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempL = GetStdValue(filtVals[2 * i], lumpedParams.IndTol);
                                tempC = GetStdValue(filtVals[2 * i + 1], lumpedParams.CapTol);

                                if (tempC < minC) tempC = minC;
                                if (tempL < minL) tempL = minL;

                                filtVals[2 * i] = tempC;
                                filtVals[2 * i + 1] = tempL;
                            }
                        }
                        else
                        {
                            // Shunt Element (capacitor proto)
                            filtVals[2 * i] = delta / (2 * Math.PI * frequency * prototype.ProtoVals[i]);
                            filtVals[2 * i + 1] = prototype.ProtoVals[i] / (2 * Math.PI * frequency * delta);

                            // Check for standard or ideal min/max 
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempL = GetStdValue(filtVals[2 * i], lumpedParams.IndTol);
                                tempC = GetStdValue(filtVals[2 * i + 1], lumpedParams.CapTol);

                                if (tempC < minC) tempC = minC;
                                if (tempL < minL) tempL = minL;

                                filtVals[2 * i] = tempC;
                                filtVals[2 * i + 1] = tempL;
                            }
                        }

                        // Scale the impedance
                        // - inductance first then capacitance
                        filtVals[2 * i] *= impedance;
                        filtVals[2 * i + 1] /= impedance;

                        lumpedParams.CompString += string.Format("L{0} = {1}H, ", i + 1, ConvertToEngineeringUnits(filtVals[2 * i]));
                        lumpedParams.CompString += string.Format("C{0} = {1}F\n", i + 1, ConvertToEngineeringUnits(filtVals[2 * i + 1]));

                        // Toggle between series and shunt component
                        toggle = !toggle;
                    }
                    break;

                case "BSF":
                    filtVals = new double[2 * prototype.Order];

                    if ("Series" == prototype.FirstElem) toggle = true;
                    if ("Shunt" == prototype.FirstElem) toggle = false;

                    for (int i = 0; i < prototype.Order; i++)
                    {
                        // Split the proto components into inductor and capacitor
                        // Frequency scale values 
                        if (toggle)
                        {
                            // Series Element (inductor proto)
                            filtVals[2 * i] = delta * prototype.ProtoVals[i] / (2 * Math.PI * frequency);
                            filtVals[2 * i + 1] = 1 / (2 * Math.PI * frequency * delta * prototype.ProtoVals[i]);

                            // Check for standard or ideal min/max 
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempL = GetStdValue(filtVals[2 * i], lumpedParams.IndTol);
                                tempC = GetStdValue(filtVals[2 * i + 1], lumpedParams.CapTol);

                                if (tempC < minC) tempC = minC;
                                if (tempL < minL) tempL = minL;

                                filtVals[2 * i] = tempC;
                                filtVals[2 * i + 1] = tempL;
                            }
                        }
                        else
                        {
                            // Shunt Element (capacitor proto)
                            filtVals[2 * i] = 1 / (2 * Math.PI * frequency * delta * prototype.ProtoVals[i]);
                            filtVals[2 * i + 1] = delta * prototype.ProtoVals[i] / (2 * Math.PI * frequency);

                            // Check for standard or ideal min/max 
                            if ("Standard" == lumpedParams.Ideal)
                            {
                                // Use temperary variable to find standard value and limit
                                tempL = GetStdValue(filtVals[2 * i], lumpedParams.IndTol);
                                tempC = GetStdValue(filtVals[2 * i + 1], lumpedParams.CapTol);

                                if (tempC < minC) tempC = minC;
                                if (tempL < minL) tempL = minL;

                                filtVals[2 * i] = tempC;
                                filtVals[2 * i + 1] = tempL;
                            }
                        }

                        // Scale the impedance
                        // - inductance first then capacitance
                        filtVals[2 * i] *= impedance;
                        filtVals[2 * i + 1] /= impedance;

                        lumpedParams.CompString += string.Format("L{0} = {1}H, ", i + 1, ConvertToEngineeringUnits(filtVals[2 * i]));
                        lumpedParams.CompString += string.Format("C{0} = {1}F\n", i + 1, ConvertToEngineeringUnits(filtVals[2 * i + 1]));

                        // Toggle between series and shunt component
                        toggle = !toggle;
                    }
                    break;
            }
        }

        // Commensurate Line Filter Using Richardson-Kuroda Method
        private void Filter_CalcCommLine()
        {
            if ("LPF" != prototype.Type) return;

            bool toggle = false;

            if ("Series" == prototype.FirstElem) toggle = true;
            if ("Shunt" == prototype.FirstElem) toggle = false;

            // Gather user input values
            double impedance = prototype.Impedance;
            int order = prototype.Order;

            // Extract number of inductor and capacitors
            int Nind = (int)Math.Floor(order / 2.0);
            int Ncap = (int)Math.Floor(order / 2.0);
            int NTlines = 0;
            int Ncomp = 0;

            // Check for odd order filter
            if (1 == order % 2)
            {
                if (toggle)
                {
                    // Series first = 1 additional inductor
                    Nind += 1;
                    NTlines += 2;
                }
                else
                {
                    // Shunt first = 1 additional capacitor
                    Ncap += 1;
                    NTlines += 0;
                }
            }
            else
            {
                // If the filter order is even the number of TLines goes up by one
                NTlines += 1;
            }

            NTlines += Nind;
            NTlines += Ncap;

            Ncomp = Nind + Ncap;

            double[] impedVals = new double[NTlines];
            double[] angleVals = new double[NTlines];
            double n2;

            int j = 0;

            for (int i = 0; i < Ncomp; i++)
            {
                if (toggle)
                {
                    if (0 == i)
                    {
                        n2 = 1 + 1 / prototype.ProtoVals[0];

                        impedVals[0] = n2 * impedance;
                        angleVals[0] = 45;

                        impedVals[1] = n2 * impedance * prototype.ProtoVals[0];
                        angleVals[1] = 45;

                        j += 2;
                    }

                    if (Ncomp - 1 == i)
                    {
                        n2 = 1 + 1 / prototype.ProtoVals[Ncomp - 1];

                        impedVals[NTlines - 1] = n2 * impedance;
                        angleVals[NTlines - 1] = 45;

                        impedVals[NTlines - 2] = n2 * impedance * prototype.ProtoVals[0];
                        angleVals[NTlines - 2] = 45;

                        j += 2;
                    }

                    if ((0 < i) && (Ncomp - 1 > i))
                    {
                        // Split inductor in two
                        n2 = 1 + 2 / prototype.ProtoVals[i];

                        double Z1 = n2 * prototype.ProtoVals[i] * impedance;
                        double Z2 = n2 * impedance;
                        double tempImped;

                        // Add additional capacitance to existing value
                        tempImped = 1 / impedVals[i - 1];
                        tempImped += 1 / Z2;
                        tempImped = 1 / tempImped;

                        impedVals[j - 1] = tempImped;
                        angleVals[j - 1] = 45;

                        impedVals[j] = Z1;
                        angleVals[j] = 90;

                        impedVals[j + 1] = Z2;
                        angleVals[j + 1] = 45;
                    }
                }
                else
                {
                    if (0 == impedVals[j])
                    {
                        impedVals[j] = impedance / prototype.ProtoVals[i];
                        angleVals[j] = 45;
                    }
                    else
                    {
                        double tempImped;

                        // Add additional capacitance to existing value
                        tempImped = 1 / impedVals[i];
                        tempImped += prototype.ProtoVals[i] / impedance;
                        tempImped = 1 / tempImped;

                        impedVals[j] = tempImped;
                        angleVals[j] = 45;
                    }

                    j += 1;
                }

                toggle = !toggle;
            }

            commParams.TLineString = "Transmission Line Values\n";

            // Create output report for TLine values
            for (int i = 0; i < NTlines; i++)
            {
                commParams.TLineString += string.Format("TL{0} = {1} Ohm, {2} deg\n", i, impedVals[i], angleVals[i]);
            }
        }

        // Stepped Impedance Filter (LPF only)
        private void Filter_CalcStepped()
        {
            if ("LPF" != prototype.Type) return;

            bool toggle = false;

            if ("Series" == prototype.FirstElem) toggle = true;
            if ("Shunt" == prototype.FirstElem) toggle = false;

            // Gather user input values
            double Zhigh = steppedParams.HighImpedance;
            double Zlow = steppedParams.LowImpedance;
            double impedance = prototype.Impedance;
            int order = prototype.Order;

            double[] impedVals = new double[order];
            double[] angleVals = new double[order];

            for (int i = 0; i < order; i++)
            {
                if (toggle)
                {
                    impedVals[i] = Zhigh;
                    angleVals[i] = prototype.ProtoVals[i] * impedance / Zhigh;
                }
                else
                {
                    impedVals[i] = Zlow;
                    angleVals[i] = prototype.ProtoVals[i] * Zlow / impedance;
                }

                angleVals[i] *= 180 / Math.PI;

                toggle = !toggle;
            }

            steppedParams.TLineString = "Transmission Line Values\n";

            // Create output report for TLine values
            for (int i = 0; i < order; i++)
            {
                steppedParams.TLineString += string.Format("TL{0} = {1} Ohm, {2} deg\n", i, impedVals[i], angleVals[i]);
            }
        }

        // Resonant Stub Filter (BPF or BSF)


        // Coupled Line Filter (BPF or BSF)


        // Gap-Coupled Filter (BPF only)


        /****************************************
        ******   Event Handler Functions   ******
        *****************************************/
        private void Filter_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() != typeof(ComboBox))
                return;

            ComboBoxItem comboObj = (ComboBoxItem)e.AddedItems[0];

            string selectedItem = comboObj.Content.ToString();

            if (DebugTB != null)
            {
                DebugTB.Text = selectedItem;
            }

            prototype.IsLpfEn = "LPF" == selectedItem;
            prototype.IsHpfEn = "HPF" == selectedItem;
            prototype.IsBpfEn = "BPF" == selectedItem;
            prototype.IsBsfEn = "BSF" == selectedItem;

            if ("BPF" == selectedItem || "BSF" == selectedItem)
            {
                prototype.EnableBandwidth = true;
            }
            else
            {
                prototype.EnableBandwidth = false;
            }

            if (filtCommLineTab != null)
                filtCommLineTab.IsEnabled = prototype.IsLpfEn;

            if (filtSteppedTab != null)
                filtSteppedTab.IsEnabled = prototype.IsLpfEn;

            if (filtCoupLineTab != null)
                filtCoupLineTab.IsEnabled = prototype.EnableBandwidth;
        }

        private void Filter_Arch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() != typeof(ComboBox))
                return;

            ComboBoxItem comboObj = (ComboBoxItem)e.AddedItems[0];

            string selectedItem = comboObj.Content.ToString();

            if ("Chebyshev" == selectedItem || "Equiripple" == selectedItem)
            {
                prototype.EnableRipple = true;
            }
            else
            {
                prototype.EnableRipple = false;
            }
        }

        private void Filter_FirstElem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() != typeof(ComboBox))
                return;

            ComboBoxItem comboObj = (ComboBoxItem)e.AddedItems[0];

            string selectedItem = comboObj.Content.ToString();

            if (DebugTB != null)
            {
                DebugTB.Text = selectedItem;
            }
        }

        private void Filter_Order_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() != typeof(ComboBox))
                return;

            ComboBoxItem comboObj = (ComboBoxItem)e.AddedItems[0];

            string selectedItem = comboObj.Content.ToString();

            if (DebugTB != null)
            {
                DebugTB.Text = selectedItem;
            }
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void Filter_SynthLumpedValues_Click(object sender, RoutedEventArgs e)
        {
            Filter_GetProtoValues();
            Filter_CalcLumped();
        }

        private void Filter_SynthCommLineValues_Click(object sender, RoutedEventArgs e)
        {
            Filter_GetProtoValues();
            Filter_CalcCommLine();
        }

        private void Filter_SynthSteppedValues_Click(object sender, RoutedEventArgs e)
        {
            Filter_GetProtoValues();
            Filter_CalcStepped();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateFilterImages();
        }
    }
}
