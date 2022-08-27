using Microsoft.Win32;
using System;

namespace rf_tools
{
    internal class CommonFunctions
    {
        /** Calculate Standard Resistance Value **/
        internal static double GetStdResistor(double resistance, double tolerance)
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

        /** Convert to Internal Units **/
        internal static double ConvertToInternalUnits(double val, string unitOut)
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

        /** Convert to External Units **/
        internal static double ConvertToExternalUnits(double val, string unitOut)
        {
            double scale = 1;

            /**Dimensional Conversions**/
            // Metric
            if ("mm" == unitOut) scale = 1000;
            if ("cm" == unitOut) scale = 100;

            // Imperial
            if ("mil" == unitOut) scale = 39370.08;
            if ("inch" == unitOut) scale = 39.37;

            /**Frequency Conversions**/
            if ("GHz" == unitOut) scale = 0.000000001;
            if ("MHz" == unitOut) scale = 0.000001;
            if ("kHz" == unitOut) scale = 0.001;

            /**Angle Conversion**/
            if ("deg" == unitOut) scale = 180 / Math.PI;

            return scale * val;
        }

        /** Convert to Engineering Units **/
        internal static string ConvertToEngineeringUnits(double val)
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

        // Find maximum or minimum of a function based on tolerance
        internal static double FindWorstMinMax(string type, double tol, double[] nomVals, Func<double[], double> function)
        {
            // Instantiate all the required variables
            int Ntot, Nvals;
            double tempVal, worstVal;
            double[] tempVals = new double[nomVals.Length];

            type.ToLower();

            // Setup the initial case of the variables
            worstVal = function(nomVals);

            // Gather the number of values
            Nvals = nomVals.Length;
            Ntot = (int)Math.Floor(Math.Pow(2, Nvals));

            // Find the minimum value
            for (int i = 0; i < Ntot; i++)
            {
                for (int j = 0; j < Nvals; j++)
                {
                    if (0 == ((1 << j) & i))
                    {
                        tempVals[j] = (1 - tol) * nomVals[j];
                    }
                    else
                    {
                        tempVals[j] = (1 + tol) * nomVals[j];
                    }
                }

                // Calculate the current value
                tempVal = function(tempVals);

                //Find minimum value
                if ("min" == type)
                    if (tempVal < worstVal)
                        worstVal = tempVal;

                // Find maximum value
                if ("max" == type)
                    if (tempVal > worstVal)
                        worstVal = tempVal;
            }

            // Return the worst-case value recorded
            return worstVal;
        }

        // Load file dialog
        internal static string OpenFile(string extension, string filter)
        {
            string location = "hello";

            OpenFileDialog openFile = new OpenFileDialog
            {
                DefaultExt = extension,
                Filter = filter
            };

            var browsefile = openFile.ShowDialog();

            if (browsefile == true)
            {
                location = openFile.FileName;
            }

            return location;
        }

        // Bring up save file dialog
        internal static string SaveFile(string extension, string filter)
        {
            string location = "hello";

            SaveFileDialog openFile = new SaveFileDialog
            {
                DefaultExt = extension,
                Filter = filter
            };

            var browsefile = openFile.ShowDialog();

            if (browsefile == true)
            {
                location = openFile.FileName;
            }

            return location;
        }
    }
}
