using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace rf_tools
{
    public class IntermodProducts : INotifyPropertyChanged
    {
        private double fundLowPwr;
        private double fundHighPwr;

        private double fundLowFreq;
        private double fundHighFreq;

        private double im3LowPwr;
        private double im3HighPwr;

        private double im3LowFreq;
        private double im3HighFreq;

        private double gain;
        private double iip3;
        private double oip3;

        public double FundLowPwr
        {
            get { return fundLowPwr; }
            set { fundLowPwr = value; NotifyPropertyChanged("FundLowPwr"); }
        }

        public double FundHighPwr
        {
            get { return fundHighPwr; }
            set { fundHighPwr = value; NotifyPropertyChanged("FundHighPwr"); }
        }

        public double FundLowFreq
        {
            get { return fundLowFreq; }
            set { fundLowFreq = value; NotifyPropertyChanged("FundLowFreq"); }
        }

        public double FundHighFreq
        {
            get { return fundHighFreq; }
            set { fundHighFreq = value; NotifyPropertyChanged("FundHighFreq"); }
        }

        public double IM3LowPwr
        {
            get { return im3LowPwr; }
            set { im3LowPwr = value; NotifyPropertyChanged("IM3LowPwr"); }
        }

        public double IM3HighPwr
        {
            get { return im3HighPwr; }
            set { im3HighPwr = value; NotifyPropertyChanged("IM3HighPwr"); }
        }

        public double IM3LowFreq
        {
            get { return im3LowFreq; }
            set { im3LowFreq = value; NotifyPropertyChanged("IM3LowFreq"); }
        }

        public double IM3HighFreq
        {
            get { return im3HighFreq; }
            set { im3HighFreq = value; NotifyPropertyChanged("IM3HighFreq"); }
        }

        public double Gain
        {
            get { return gain; }
            set { gain = value; NotifyPropertyChanged("Gain"); }
        }

        public double IIP3
        {
            get { return iip3; }
            set { iip3 = value; NotifyPropertyChanged("IIP3"); }
        }

        public double OIP3
        {
            get { return oip3; }
            set { oip3 = value; NotifyPropertyChanged("OIP3"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    /// <summary>
    /// Interaction logic for Intermods.xaml
    /// </summary>
    public partial class Intermods : Window
    {
        private readonly IntermodProducts intermod = new IntermodProducts();

        double[] dataX = new double[4];
        double[] dataY = new double[4];
        private bool updateTable = false;
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public Intermods()
        {
            InitializeComponent();

            DataContext = intermod;

            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += Timer_Tick;

            timer.Start();
        }

        private void Intermods_Calc_IM3(object sender, RoutedEventArgs e)
        {
            // Initialize require variables
            double deltaHigh;
            double deltaLow;

            double fundFreqLow;
            double fundFreqHigh;

            double iip3;
            double oip3;

            // Check if IIP3 or OIP3 was specified
            iip3 = intermod.IIP3;
            oip3 = intermod.OIP3;

            // Check the IIP3 relative value of OIP3
            if (oip3 - intermod.Gain != iip3)
            {
                // Thus the IIP3 value has not been correctly set
                iip3 = oip3 - intermod.Gain;
            }

            // Check the OIP3 relative value of IIP3
            if (iip3 + intermod.Gain != oip3)
            {
                // Thus the OIP3 value has not been correctly set
                oip3 = iip3 + intermod.Gain;
            }

            intermod.IIP3 = iip3;
            intermod.OIP3 = oip3;

            // Use OIP3 to calculate the IM3 products
            fundFreqLow = intermod.FundLowFreq;
            fundFreqHigh = intermod.FundHighFreq;

            deltaHigh = intermod.FundHighPwr - oip3;
            deltaLow = intermod.FundLowPwr - oip3;

            deltaHigh *= 2;
            deltaLow *= 2;

            intermod.IM3HighPwr = deltaHigh + intermod.FundHighPwr;
            intermod.IM3LowPwr = deltaLow + intermod.FundLowPwr;

            intermod.IM3HighFreq = 2 * fundFreqHigh - fundFreqLow;
            intermod.IM3LowFreq = 2 * fundFreqLow - fundFreqHigh;

            updateTable = true;
        }

        private void Intermods_Calc_TOI(object sender, RoutedEventArgs e)
        {
            // Initialize require variables
            double deltaHigh;
            double deltaLow;

            double oip3High;
            double oip3Low;

            double fundFreqLow;
            double fundFreqHigh;

            // Calculate the OIP3 and IIP3
            // 1. Get intermod power in dBc (IM3 - Pwr)
            // 2. Divide this by 2
            // 3. Subtract from Pwr
            deltaHigh = intermod.IM3HighPwr - intermod.FundHighPwr;
            deltaLow = intermod.IM3LowPwr - intermod.FundLowPwr;

            deltaHigh /= 2;
            deltaLow /= 2;

            oip3High = intermod.FundHighPwr - deltaHigh;
            oip3Low = intermod.FundLowPwr - deltaLow;

            intermod.OIP3 = (oip3High > oip3Low) ? oip3Low : oip3High;
            intermod.IIP3 = intermod.OIP3 - intermod.Gain;

            // Set the IM product frequencies
            // This waythe user does not need to enter these values if they don't want to
            fundFreqLow = intermod.FundLowFreq;
            fundFreqHigh = intermod.FundHighFreq;

            intermod.IM3HighFreq = 2 * fundFreqHigh - fundFreqLow;
            intermod.IM3LowFreq = 2 * fundFreqLow - fundFreqHigh;

            updateTable = true;
        }

        private void UpdateIntermodPlot()
        {
            if (updateTable)
            {
                double[] yOffsets = { -100, -100, -100, -100 };

                dataX[0] = intermod.IM3LowFreq;
                dataX[1] = intermod.FundLowFreq;
                dataX[2] = intermod.FundHighFreq;
                dataX[3] = intermod.IM3HighFreq;

                dataY[0] = intermod.IM3LowPwr - yOffsets[0];
                dataY[1] = intermod.FundLowPwr - yOffsets[1];
                dataY[2] = intermod.FundHighPwr - yOffsets[2];
                dataY[3] = intermod.IM3HighPwr - yOffsets[3];

                var lollipopPlot = GraphPlot.Plot.AddLollipop(dataY, dataX);

                lollipopPlot.ValueOffsets = yOffsets;

                GraphPlot.Plot.SetAxisLimits(yMin: -100);
                GraphPlot.Plot.RenderLegend();
                GraphPlot.Plot.AxisAuto();
                GraphPlot.Plot.YLabel("Power (dBm)");
                GraphPlot.Plot.XLabel("Frequency");
                GraphPlot.Refresh();

                updateTable = false;
            }
        }

        // Thread event for updating the Graph withthe IM information
        void Timer_Tick(object sender, EventArgs e)
        {
            UpdateIntermodPlot();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            timer.Stop();
        }
    }
}
