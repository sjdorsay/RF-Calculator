using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace rf_tools
{
    public class ImpedanceConverter : INotifyPropertyChanged
    {
        private double impedanceReal;
        private double impedanceImag;
        private double impedanceMag;
        private double impedanceAngle;

        private double gammaReal;
        private double gammaImag;
        private double gammaMag;
        private double gammaAngle;

        private double vswr;
        private double returnLoss;

        public double ImpedanceReal
        {
            get { return impedanceReal; }
            set { impedanceReal = value; NotifyPropertyChanged("ImpedanceReal"); }
        }

        public double ImpedanceImag
        {
            get { return impedanceImag; }
            set { impedanceImag = value; NotifyPropertyChanged("ImpedanceImag"); }
        }

        public double ImpedanceMag
        {
            get { return impedanceMag; }
            set { impedanceMag = value; NotifyPropertyChanged("ImpedanceMag"); }
        }

        public double ImpedanceAngle
        {
            get { return impedanceAngle; }
            set { impedanceAngle = value; NotifyPropertyChanged("ImpedanceAngle"); }
        }


        public double GammaReal
        {
            get { return gammaReal; }
            set { gammaReal = value; NotifyPropertyChanged("GammaReal"); }
        }

        public double GammaImag
        {
            get { return gammaImag; }
            set { gammaImag = value; NotifyPropertyChanged("GammaImag"); }
        }

        public double GammaMag
        {
            get { return gammaMag; }
            set { gammaMag = value; NotifyPropertyChanged("GammaMag"); }
        }

        public double GammaAngle
        {
            get { return gammaAngle; }
            set { gammaAngle = value; NotifyPropertyChanged("GammaAngle"); }
        }


        public double VSWR
        {
            get { return vswr; }
            set { vswr = value; NotifyPropertyChanged("VSWR"); }
        }


        public double ReturnLoss
        {
            get { return returnLoss; }
            set { returnLoss = value; NotifyPropertyChanged("ReturnLoss"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    /// <summary>
    /// Interaction logic for Impedance.xaml
    /// </summary>
    public partial class Impedance : Window
    {
        readonly ImpedanceConverter impConv = new ImpedanceConverter();

        internal struct Complex_obj
        {
            public Complex_obj(double r, double i)
            {
                Real = r;
                Imaginary = i;
            }
            public double Real { get; }
            public double Imaginary { get; }
        }

        internal struct Polar_obj
        {
            public Polar_obj(double m, double a)
            {
                Magnitude = m;
                Angle = a;
            }
            public double Magnitude { get; }
            public double Angle { get; }
        }

        public Impedance()
        {
            InitializeComponent();

            DataContext = impConv;
        }

        // Complex Math
        private Complex_obj Complex_Add(Complex_obj in1, Complex_obj in2)
        {
            double real, imaginary;

            real = in1.Real + in2.Real;
            imaginary = in1.Imaginary + in2.Imaginary;

            Complex_obj complexResult = new Complex_obj(real, imaginary);

            return complexResult;
        }

        private Complex_obj Complex_Subtract(Complex_obj in1, Complex_obj in2)
        {
            double real, imaginary;

            real = in1.Real - in2.Real;
            imaginary = in1.Imaginary - in2.Imaginary;

            Complex_obj complexResult = new Complex_obj(real, imaginary);

            return complexResult;
        }

        private Complex_obj Complex_Multiply(Complex_obj in1, Complex_obj in2)
        {
            double real, imaginary;

            real = in1.Real * in2.Real - in1.Imaginary * in2.Imaginary;
            imaginary = in1.Imaginary * in2.Real + in1.Real * in2.Imaginary;

            Complex_obj complexResult = new Complex_obj(real, imaginary);

            return complexResult;
        }

        private Complex_obj Complex_Divide(Complex_obj in1, Complex_obj in2)
        {
            double real, imaginary;
            double scale;

            scale = Math.Pow(in2.Real, 2) + Math.Pow(in2.Imaginary, 2);

            real = in1.Real * in2.Real + in1.Imaginary * in2.Imaginary;
            real /= scale;

            _ = Complex_Multiply(in1, in2);

            imaginary = in1.Imaginary * in2.Real - in1.Real * in2.Imaginary;
            imaginary /= scale;

            Complex_obj complexResult = new Complex_obj(real, imaginary);

            return complexResult;
        }

        private Polar_obj Complex_To_Polar(Complex_obj input)
        {
            double magnitude, angle;

            magnitude = Math.Pow(input.Real, 2) + Math.Pow(input.Imaginary, 2);
            magnitude = Math.Sqrt(magnitude);

            angle = Math.Atan2(input.Imaginary, input.Real);

            Polar_obj result = new Polar_obj(magnitude, angle);

            return result;
        }

        private Complex_obj Polar_To_Complex(Polar_obj input)
        {
            double real, imaginary;

            real = input.Magnitude * Math.Cos(input.Angle);
            imaginary = input.Magnitude * Math.Sin(input.Angle);

            Complex_obj complexResult = new Complex_obj(real, imaginary);

            return complexResult;
        }

        private void ImpConv_Impedance_RealImag_Calc()
        {
            Complex_obj loadImpedanceComplex = new Complex_obj(impConv.ImpedanceReal, impConv.ImpedanceImag);
            Complex_obj sysImpedanceComplex = new Complex_obj(50, 0);

            Polar_obj loadImpedancePolar = Complex_To_Polar(loadImpedanceComplex);

            impConv.ImpedanceMag = Math.Round(loadImpedancePolar.Magnitude, 3);
            impConv.ImpedanceAngle = Math.Round(loadImpedancePolar.Angle, 3);

            Complex_obj loadGammaComplex = Complex_Divide(
                Complex_Subtract(sysImpedanceComplex, loadImpedanceComplex),
                Complex_Add(sysImpedanceComplex, loadImpedanceComplex));

            Polar_obj loadGammaPolar = Complex_To_Polar(loadGammaComplex);

            impConv.GammaReal = Math.Round(loadGammaComplex.Real, 3);
            impConv.GammaImag = Math.Round(loadGammaComplex.Imaginary, 3);

            impConv.GammaMag = Math.Round(loadGammaPolar.Magnitude, 3);
            impConv.GammaAngle = Math.Round(loadGammaPolar.Angle, 3);

            double vswr;
            double rl;

            vswr = (1 + loadGammaPolar.Magnitude) / (1 - loadGammaPolar.Magnitude);
            rl = 20 * Math.Log10(loadGammaPolar.Magnitude);

            impConv.VSWR = Math.Round(vswr, 3);
            impConv.ReturnLoss = Math.Round(rl, 3);
        }

        private void ImpConv_Impedance_Polar_Calc()
        {
            Complex_obj sysImpedanceComplex = new Complex_obj(50, 0);

            Polar_obj loadImpedancePolar = new Polar_obj(impConv.ImpedanceMag, impConv.ImpedanceAngle);
            Complex_obj loadImpedanceComplex = Polar_To_Complex(loadImpedancePolar);

            impConv.ImpedanceReal = Math.Round(loadImpedanceComplex.Real, 3);
            impConv.ImpedanceImag = Math.Round(loadImpedanceComplex.Imaginary, 3);

            Complex_obj loadGammaComplex = Complex_Divide(
                Complex_Subtract(sysImpedanceComplex, loadImpedanceComplex),
                Complex_Add(sysImpedanceComplex, loadImpedanceComplex));

            Polar_obj loadGammaPolar = Complex_To_Polar(loadGammaComplex);

            impConv.GammaReal = Math.Round(loadGammaComplex.Real, 3);
            impConv.GammaImag = Math.Round(loadGammaComplex.Imaginary, 3);

            impConv.GammaMag = Math.Round(loadGammaPolar.Magnitude, 3);
            impConv.GammaAngle = Math.Round(loadGammaPolar.Angle, 3);

            double vswr;
            double rl;

            vswr = (1 + loadGammaPolar.Magnitude) / (1 - loadGammaPolar.Magnitude);
            rl = 20 * Math.Log10(loadGammaPolar.Magnitude);

            impConv.VSWR = Math.Round(vswr, 3);
            impConv.ReturnLoss = Math.Round(rl, 3);
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ImpConv_Impedance_RealImag_Calc();
        }

        private void StackPanel_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ImpConv_Impedance_Polar_Calc();
        }
    }
}
