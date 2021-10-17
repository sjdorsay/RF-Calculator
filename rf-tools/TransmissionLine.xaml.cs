using iText.IO.Image;
// iText7 PDF .NET Module
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Image = iText.Layout.Element.Image;
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace rf_tools
{
    /* TO DO:
     * 1. Implement report generation
     * 2. Create help documentation
     * 3. Implement help function
     */

    /** Dielectric Properties **/
    public class DielProp : INotifyPropertyChanged
    {
        private string name;
        private double permitivity;
        private double tanD;
        private double cte;

        private bool isCustom;

        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged("Name"); }
        }

        public double Permitivity
        {
            get { return permitivity; }
            set { permitivity = value; NotifyPropertyChanged("Permitivity"); }
        }

        public double TanD
        {
            get { return tanD; }
            set { tanD = value; NotifyPropertyChanged("TanD"); }
        }

        public double CTE
        {
            get { return cte; }
            set { cte = value; NotifyPropertyChanged("CTE"); }
        }

        public bool IsCustom
        {
            get { return isCustom; }
            set { isCustom = value; NotifyPropertyChanged("IsCustom"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    /** System Properties **/
    public class SysProp : INotifyPropertyChanged
    {
        private double impedance;

        private double theta;
        private string thetaUnits;

        private double frequency;
        private string frequencyUnits;

        public double Impedance
        {
            get { return impedance; }
            set
            {
                if (value > 0)
                {
                    impedance = value;
                    NotifyPropertyChanged("Impedance");
                }
            }
        }

        public double Theta
        {
            get { return theta; }
            set { theta = value; NotifyPropertyChanged("Theta"); }
        }

        public string ThetaUnits
        {
            get { return thetaUnits; }
            set { thetaUnits = value; }
        }

        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; NotifyPropertyChanged("Frequency"); }
        }

        public string FrequencyUnits
        {
            get { return frequencyUnits; }
            set { frequencyUnits = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    /** Transmission Line Parameters **/
    public class TLineParameters : INotifyPropertyChanged
    {
        public SysProp SysProp { get; set; } = new SysProp();
        public DielProp DielProp { get; set; } = new DielProp();

        private double length;
        private string lengthUnits;

        private double width;
        private string widthUnits;

        private double height;
        private string heightUnits;

        private double thickness;
        private string thicknessUnits;

        private double gap;
        private string gapUnits;

        public double Length
        {
            get { return length; }
            set { length = value; NotifyPropertyChanged("Length"); }
        }

        public string LengthUnits
        {
            get { return lengthUnits; }
            set { lengthUnits = value; }
        }

        public double Width
        {
            get { return width; }
            set { width = value; NotifyPropertyChanged("Width"); }
        }

        public string WidthUnits
        {
            get { return widthUnits; }
            set { widthUnits = value; }
        }

        public double Height
        {
            get { return height; }
            set { height = value; NotifyPropertyChanged("Height"); }
        }

        public string HeightUnits
        {
            get { return heightUnits; }
            set { heightUnits = value; }
        }

        public double Thickness
        {
            get { return thickness; }
            set { thickness = value; NotifyPropertyChanged("Thickness"); }
        }

        public string ThicknessUnits
        {
            get { return thicknessUnits; }
            set { thicknessUnits = value; }
        }

        public double Gap
        {
            get { return gap; }
            set { gap = value; NotifyPropertyChanged("Gap"); }
        }

        public string GapUnits
        {
            get { return gapUnits; }
            set { gapUnits = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public partial class TransmissionLine : Window
    {
        bool tlineRunSynthesis = false;
        bool tlineRunEvaluate = false;

        bool genReport = false;

        readonly TLineParameters ms_params = new TLineParameters();
        readonly TLineParameters sl_params = new TLineParameters();
        readonly TLineParameters cpw_params = new TLineParameters();
        readonly TLineParameters gcpw_params = new TLineParameters();
        readonly TLineParameters siw_params = new TLineParameters();

        /** Transmission Line Initialization **/
        public TransmissionLine()
        {
            InitializeComponent();

            DatabaseService dbService = new DatabaseService();
            dbService.Set_Table("dielecitrc");

            DataTable dataTable = dbService.Get_Selected_Table();
            List<string> dielList = new List<string>();

            dielList.Add("Custom");

            foreach (DataRow row in dataTable.Rows)
            {
                dielList.Add(row.ItemArray[0].ToString());
            }

            tline_ms_diel_cb.ItemsSource = dielList;
            tline_sl_diel_cb.ItemsSource = dielList;
            tline_cpw_diel_cb.ItemsSource = dielList;
            tline_gcpw_diel_cb.ItemsSource = dielList;
            tline_siw_diel_cb.ItemsSource = dielList;

            tlineMSTab.DataContext = ms_params;
            tlineSLTab.DataContext = sl_params;
            tlineCPWTab.DataContext = cpw_params;
            tlineGCPWTab.DataContext = gcpw_params;
            tlineSIWTab.DataContext = siw_params;
        }

        /*************************************
        ******   Calculator Functions   ******
        **************************************/

        /** Microstrip - Synthesis / Evalute **/
        private void TLine_MS_Run()
        {
            // Gather physical constants
            double speedOfLight = 299792458;
            double rho_cu = 1.68 * Math.Pow(10, -8);
            double permeability = 4 * Math.PI * Math.Pow(10, -7);
            double permitivityEff = 1;

            // Initialize variables
            double propDelay = 0;
            double wavelength = 0;
            double beta = 0;

            // Unitless Values
            double permitivityRel = ms_params.DielProp.Permitivity;

            double height = Convert_To_Internal_Units(ms_params.Height, ms_params.HeightUnits);
            //double thickness = Convert_To_Internal_Units(ms_params.Thickness, ms_params.ThicknessUnits);
            double frequency = Convert_To_Internal_Units(ms_params.SysProp.Frequency, ms_params.SysProp.FrequencyUnits);

            // Initialize Input/Output Variables
            double width = 0;
            double length = 0;
            double impedance = 0;
            double theta = 0;

            if (tlineRunSynthesis)
            {
                // Gather Synthesis specific inputs
                impedance = ms_params.SysProp.Impedance;
                theta = Convert_To_Internal_Units(ms_params.SysProp.Theta, ms_params.SysProp.ThetaUnits);

                // Calculate auxiliary values
                double A = (impedance / 60) * Math.Sqrt((permitivityRel + 1) / 2) + ((permitivityRel - 1) / (permitivityRel + 1)) * (0.23 + 0.11 / permitivityRel);
                double B = 377 * Math.PI / (2 * impedance * Math.Sqrt(permitivityRel));

                // Assume W/H <2
                double w_h = 8 * Math.Exp(A) / (Math.Exp(2 * A) - 2);

                if (w_h > 2)
                {
                    w_h = B - 1 - Math.Log(2 * B - 1) + (permitivityRel - 1) * (Math.Log(B - 1) + 0.39 - (0.61 / permitivityRel)) / (2 * permitivityRel);
                    w_h = 2 * w_h / Math.PI;
                }

                // Calculate the effective permitivity for beta
                permitivityEff = ((permitivityRel + 1) / 2) + (permitivityRel - 1) / (2 * Math.Sqrt(1 + (12 / w_h)));

                propDelay = speedOfLight / Math.Sqrt(permitivityEff);
                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                // Update transmission line parameters
                width = w_h * height;
                length = theta / beta;

                // Trace Width
                ms_params.Width = Convert_To_External_Units(width, ms_params.WidthUnits);

                // Trace Length
                ms_params.Length = Convert_To_External_Units(length, ms_params.LengthUnits);
            }

            if (tlineRunEvaluate)
            {
                // Gather Evaluate specific inputs
                width = Convert_To_Internal_Units(ms_params.Width, ms_params.WidthUnits);
                length = Convert_To_Internal_Units(ms_params.Length, ms_params.LengthUnits);

                double w_h = width / height;

                // Calculate the effective permitivity
                permitivityEff = ((permitivityRel + 1) / 2) + ((permitivityRel - 1) / (2 * Math.Sqrt(1 + 12 * (height / width))));

                if (w_h < 1)
                {
                    impedance = 60 / Math.Sqrt(permitivityEff);
                    impedance *= Math.Log((8 / w_h) + (w_h / 4));
                }
                else
                {
                    impedance = 120 * Math.PI / Math.Sqrt(permitivityEff);
                    impedance /= w_h + 1.393 + 0.667 * Math.Log(w_h + 1.444);
                }

                // Calculate propagation parameters
                propDelay = speedOfLight / Math.Sqrt(permitivityEff);
                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                theta = beta * length;

                // Trace Impedance
                ms_params.SysProp.Impedance = impedance;

                // Electrical Angle
                ms_params.SysProp.Theta = Convert_To_External_Units(theta, ms_params.SysProp.ThetaUnits);
            }

            if (genReport)
            {
                double alpha_d;
                double alpha_c;

                alpha_d = beta * permitivityRel * (permitivityEff - 1) * ms_params.DielProp.TanD;
                alpha_d /= 2 * permitivityEff * (permitivityRel - 1);
                alpha_d *= 20 * Math.Log10(Math.E);

                alpha_c = Math.Sqrt(Math.PI * frequency * rho_cu * permeability);
                alpha_c /= impedance * width;
                alpha_c *= 20 * Math.Log10(Math.E);

                // Get path to user documents assuming C drive
                string UserName = Environment.UserName;
                string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

                // Must have write permissions to the path folder
                PdfWriter writer = new PdfWriter(DocuPath + "Microstrip_Report.pdf");
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                /** Setup Document Formats **/
                // Setup Title formatting
                Paragraph title = new Paragraph("Microstrip Report")
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

                ImageData imgData = ImageDataFactory.Create(new Uri("C:\\Users\\Visitor\\source\\repos\\rf-tools\\rf-tools\\Images\\TransmissionLines\\Microstrip.png"));
                Image img = new Image(imgData);

                img.Scale(0.5f, 0.5f);

                document.Add(img);

                // Section 1 - Material Properties
                header.Add("1. Material Properties");

                body.Add(ms_params.DielProp.Name + "\n");
                body.Add("eps_rel = " + permitivityRel + "\n");
                body.Add("eps_eff = " + permitivityEff + "\n");
                body.Add("tan d = " + ms_params.DielProp.TanD + "\n");
                body.Add("CTE-z = " + ms_params.DielProp.CTE + " ppm\n");

                // Add section header and body to document
                document.Add(header);
                document.Add(body);

                // Remove text from formatted paragraphs
                header.GetChildren().Clear();
                body.GetChildren().Clear();

                // Section 2 - Trace Geometry
                header.Add("2. Trace Geometry");

                body.Add("W = " +
                    Convert_To_External_Units(width, ms_params.WidthUnits) +
                    " " +
                    ms_params.WidthUnits +
                    "\n");

                body.Add("L = " +
                    Convert_To_External_Units(length, ms_params.LengthUnits) +
                    " " +
                    ms_params.LengthUnits +
                    "\n");

                body.Add("H = " +
                    Convert_To_External_Units(height, ms_params.HeightUnits) +
                    " " +
                    ms_params.HeightUnits +
                    "\n");

                // Add section header and body to document
                document.Add(header);
                document.Add(body);

                // Remove text from formatted paragraphs
                header.GetChildren().Clear();
                body.GetChildren().Clear();

                header.Add("3. Electrical Properties");

                body.Add("Z = " + impedance + " Ohms\n");

                body.Add("theta = " +
                    Convert_To_External_Units(theta, ms_params.SysProp.ThetaUnits) +
                    " " +
                    ms_params.SysProp.ThetaUnits +
                    "\n");

                body.Add("beta = " +
                    Convert_To_External_Units(beta, ms_params.LengthUnits) + // since the variable unit is 1/length
                    " rad/" +
                    ms_params.LengthUnits +
                    "\n");

                body.Add("wavelength = " +
                    Convert_To_External_Units(wavelength, ms_params.LengthUnits) +
                    " " +
                    ms_params.LengthUnits +
                    "\n");

                body.Add("propDelay = " + propDelay + " m/s\n");

                // Add section header and body to document
                document.Add(header);
                document.Add(body);

                // Remove text from formatted paragraphs
                header.GetChildren().Clear();
                body.GetChildren().Clear();

                // Section 4 - Microstrip Losses
                header.Add("4. Microstrip Losses");

                body.Add("alpha_d = " +
                    Convert_To_Internal_Units(alpha_d, ms_params.LengthUnits) +
                    " dB/" +
                    ms_params.LengthUnits +
                    "\n");

                body.Add("alpha_c = " +
                    Convert_To_Internal_Units(alpha_c, ms_params.LengthUnits) +
                    " dB/" +
                    ms_params.LengthUnits +
                    "\n");

                document.Add(header);
                document.Add(body);

                document.Close();
            }
        }

        /** Stripline - Synthesis / Evalute **/
        private void TLine_SL_Run()
        {
            // Gather physical constants
            double speedOfLight = 299792458;
            double rho_cu = 1.68 * Math.Pow(10, -8);
            double permeability = 4 * Math.PI * Math.Pow(10, -7);

            // Initialize variables
            double propDelay = 0;
            double wavelength = 0;
            double beta = 0;

            // Unitless Values
            double permitivityRel = sl_params.DielProp.Permitivity;

            double height = Convert_To_Internal_Units(sl_params.Height, sl_params.HeightUnits);
            //double thickness = Convert_To_Internal_Units(sl_params.Thickness, sl_params.ThicknessUnits);
            double frequency = Convert_To_Internal_Units(sl_params.SysProp.Frequency, sl_params.SysProp.FrequencyUnits);

            // Initialize Input/Output Variables
            double width = 0;
            double length = 0;
            double impedance = 0;
            double theta = 0;

            if (tlineRunSynthesis)
            {
                // Gather Synthesis specific inputs
                impedance = sl_params.SysProp.Impedance;
                theta = Convert_To_Internal_Units(sl_params.SysProp.Theta, sl_params.SysProp.ThetaUnits);

                // Calculate auxiliary values
                double y = Math.Sqrt(permitivityRel) * impedance;
                double x = 30 * Math.PI / (Math.Sqrt(permitivityRel) * impedance);
                x -= 2 * Math.Log(2) / Math.PI;

                // Calculate the width for the required characteristic impedance
                if (y > 120)
                {
                    width = height * (0.85 - Math.Sqrt(0.6 - x));
                }
                else
                {
                    width = height * x;
                }

                // Calculate the propagation delay, wavelength, and beta
                propDelay = speedOfLight / Math.Sqrt(permitivityRel);
                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                // Calculate the trace length for the required electrical length
                length = theta / beta;

                // Trace Width
                sl_params.Width = Convert_To_External_Units(width, sl_params.WidthUnits);

                // Trace Length
                sl_params.Length = Convert_To_External_Units(length, sl_params.LengthUnits);
            }

            if (tlineRunEvaluate)
            {
                // Gather Evaluate specific inputs
                width = Convert_To_Internal_Units(sl_params.Width, sl_params.WidthUnits);
                length = Convert_To_Internal_Units(sl_params.Length, sl_params.LengthUnits);

                double w_h = width / height;

                // Calculate the effective width
                double We;

                if (w_h > 0.35)
                {
                    We = width;
                }
                else
                {
                    We = width - height * Math.Pow(0.35 - w_h, 2);
                }

                // Calculate the impedance from the effective width
                impedance = 30 * Math.PI * height / Math.Sqrt(permitivityRel);
                impedance /= We + 0.441 * height;

                // Calculate propagation parameters
                propDelay = speedOfLight / Math.Sqrt(permitivityRel);
                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                theta = beta * length;

                // Trace Impedance
                sl_params.SysProp.Impedance = impedance;

                // Electrical Angle
                sl_params.SysProp.Theta = Convert_To_External_Units(theta, sl_params.SysProp.ThetaUnits);
            }

            if (genReport)
            {
                double alpha_d;
                double alpha_c;

                alpha_d = beta * sl_params.DielProp.TanD / 2;
                alpha_d *= 20 * Math.Log10(Math.E);

                alpha_c = Math.Sqrt(Math.PI * frequency * rho_cu * permeability);
                alpha_c /= impedance * width;
                alpha_c *= 20 * Math.Log10(Math.E);

                // Get path to user documents assuming C drive
                string UserName = Environment.UserName;
                string DocuPath = "C:\\Users\\" + UserName + "\\Documents\\";

                // Must have write permissions to the path folder
                PdfWriter writer = new PdfWriter(DocuPath + "Stripline_Report.pdf");
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                /** Setup Document Formats **/
                // Setup Title formatting
                Paragraph title = new Paragraph("Stripline Report")
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

                ImageData imgData = ImageDataFactory.Create(new Uri("C:\\Users\\Visitor\\source\\repos\\rf-tools\\rf-tools\\Images\\TransmissionLines\\Stripline.png"));
                Image img = new Image(imgData);

                document.Add(img);

                // Section 1 - Material Properties
                header.Add("1. Material Properties");

                body.Add(sl_params.DielProp.Name + "\n");
                body.Add("eps_rel = " + permitivityRel + "\n");
                body.Add("tan d = " + sl_params.DielProp.TanD + "\n");
                body.Add("CTE-z = " + sl_params.DielProp.CTE + " ppm\n");

                // Add section header and body to document
                document.Add(header);
                document.Add(body);

                // Remove text from formatted paragraphs
                header.GetChildren().Clear();
                body.GetChildren().Clear();

                // Section 2 - Trace Geometry
                header.Add("2. Trace Geometry");

                body.Add("W = " +
                    Convert_To_External_Units(width, sl_params.WidthUnits) +
                    " " +
                    sl_params.WidthUnits +
                    "\n");

                body.Add("L = " +
                    Convert_To_External_Units(length, sl_params.LengthUnits) +
                    " " +
                    sl_params.LengthUnits +
                    "\n");

                body.Add("H = " +
                    Convert_To_External_Units(height, sl_params.HeightUnits) +
                    " " +
                    sl_params.HeightUnits +
                    "\n");

                // Add section header and body to document
                document.Add(header);
                document.Add(body);

                // Remove text from formatted paragraphs
                header.GetChildren().Clear();
                body.GetChildren().Clear();

                header.Add("3. Electrical Properties");

                body.Add("Z = " + impedance + " Ohms\n");

                body.Add("theta = " +
                    Convert_To_External_Units(theta, sl_params.SysProp.ThetaUnits) +
                    " " +
                    sl_params.SysProp.ThetaUnits +
                    "\n");

                body.Add("beta = " +
                    Convert_To_Internal_Units(beta, sl_params.LengthUnits) + // since the variable unit is 1/length
                    " rad/" +
                    sl_params.LengthUnits +
                    "\n");

                body.Add("wavelength = " +
                    Convert_To_External_Units(wavelength, sl_params.LengthUnits) +
                    " " +
                    sl_params.LengthUnits +
                    "\n");

                body.Add("propDelay = " + propDelay + " m/s\n");

                // Add section header and body to document
                document.Add(header);
                document.Add(body);

                // Remove text from formatted paragraphs
                header.GetChildren().Clear();
                body.GetChildren().Clear();

                // Section 4 - Microstrip Losses
                header.Add("4. Microstrip Losses");

                body.Add("alpha_d = " +
                    Convert_To_Internal_Units(alpha_d, sl_params.LengthUnits) +
                    " dB/" +
                    sl_params.LengthUnits +
                    "\n");

                body.Add("alpha_c = " +
                    Convert_To_Internal_Units(alpha_c, sl_params.LengthUnits) +
                    " dB/" +
                    sl_params.LengthUnits +
                    "\n");

                document.Add(header);
                document.Add(body);

                document.Close();
            }
        }

        /** GCPW - Synthesis / Evalute **/
        private void TLine_GCPW_Run()
        {
            // Gather physical constants
            double speedOfLight = 299792458;
            //double rho_cu = 1.68 * Math.Pow(10, -8);
            //double permeability = 4 * Math.PI * Math.Pow(10, -7);
            double permitivityEff;

            // Initialize variables
            double propDelay;
            double wavelength;
            double beta;

            // Unitless Values
            double permitivityRel = gcpw_params.DielProp.Permitivity;

            double height = Convert_To_Internal_Units(gcpw_params.Height, gcpw_params.HeightUnits);
            //double thickness = Convert_To_Internal_Units(gcpw_params.Thickness, gcpw_params.ThicknessUnits);
            double frequency = Convert_To_Internal_Units(gcpw_params.SysProp.Frequency, gcpw_params.SysProp.FrequencyUnits);

            // Initialize Input/Output Variables
            double width;
            double length;
            double gap;
            double impedance;
            double theta;

            if (tlineRunSynthesis)
            {
                // Gather Synthesis specific inputs
                impedance = gcpw_params.SysProp.Impedance;
                theta = Convert_To_Internal_Units(gcpw_params.SysProp.Theta, gcpw_params.SysProp.ThetaUnits);

            }

            if (tlineRunEvaluate)
            {
                // Gather Evaluate specific inputs
                width = Convert_To_Internal_Units(gcpw_params.Width, gcpw_params.WidthUnits);
                length = Convert_To_Internal_Units(gcpw_params.Length, gcpw_params.LengthUnits);
                gap = Convert_To_Internal_Units(gcpw_params.Gap, gcpw_params.GapUnits);

                double k0 = width / (width + 2 * gap);
                double k0p = Math.Sqrt(1 - Math.Pow(k0, 2));

                double k1 = Math.Tanh(Math.PI * width / (4 * height));
                k1 /= Math.Tanh(Math.PI * (width + 2 * gap) / (4 * height));
                double k1p = Math.Sqrt(1 - Math.Pow(k1, 2));

                double Kr0 = CEI_First(k0) / CEI_First(k0p);
                double Kr1 = CEI_First(k1) / CEI_First(k1p);

                permitivityEff = 1 + permitivityRel * (Kr1 / Kr0);
                permitivityEff /= 1 + (Kr1 / Kr0);

                impedance = 60 * Math.PI / Math.Sqrt(permitivityEff);
                impedance /= Kr0 + Kr1;

                // Calculate propagation parameters
                propDelay = speedOfLight / Math.Sqrt(permitivityEff);
                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                theta = beta * length;

                // Trace Impedance
                gcpw_params.SysProp.Impedance = impedance;

                // Electrical Angle
                gcpw_params.SysProp.Theta = Convert_To_External_Units(theta, gcpw_params.SysProp.ThetaUnits);
            }
        }

        /** CPW - Synthesis / Evalute **/
        private void TLine_CPW_Run()
        {
            // Gather physical constants
            double speedOfLight = 299792458;
            double rho_cu = 1.68 * Math.Pow(10, -8);
            double permeability = 4 * Math.PI * Math.Pow(10, -7);
            double permitivityEff = 1;

            // Initialize variables
            double propDelay;
            double wavelength;
            double beta;

            // Unitless Values
            double permitivityRel = cpw_params.DielProp.Permitivity;

            double height = Convert_To_Internal_Units(cpw_params.Height, cpw_params.HeightUnits);
            //double thickness = Convert_To_Internal_Units(cpw_params.Thickness, cpw_params.ThicknessUnits);
            double frequency = Convert_To_Internal_Units(cpw_params.SysProp.Frequency, cpw_params.SysProp.FrequencyUnits);

            // Instantiate Input/Output Variables
            double width;
            double length;
            double gap;
            double impedance;
            double theta;

            if (tlineRunSynthesis)
            {
                // Gather Synthesis specific inputs
                impedance = cpw_params.SysProp.Impedance;
                theta = Convert_To_Internal_Units(cpw_params.SysProp.Theta, cpw_params.SysProp.ThetaUnits);

            }

            if (tlineRunEvaluate)
            {
                // Gather Evaluate specific inputs
                width = Convert_To_Internal_Units(cpw_params.Width, cpw_params.WidthUnits);
                length = Convert_To_Internal_Units(cpw_params.Length, cpw_params.LengthUnits);
                gap = Convert_To_Internal_Units(cpw_params.Gap, cpw_params.GapUnits);

                double k0 = width / (width + 2 * gap);
                double k0p = Math.Sqrt(1 - Math.Pow(k0, 2));

                double k1 = Math.Tanh(Math.PI * width / (4 * height));
                k1 /= Math.Tanh(Math.PI * (width + 2 * gap) / (4 * height));
                double k1p = Math.Sqrt(1 - Math.Pow(k1, 2));

                double Kr0 = CEI_First(k0) / CEI_First(k0p);
                double Kr1 = CEI_First(k1) / CEI_First(k1p);

                permitivityEff = 1 + ((permitivityRel - 1) / 2) * (Kr1 / Kr0);

                impedance = 30 * Math.PI;
                impedance /= Math.Sqrt(permitivityEff) * Kr0;

                // Calculate propagation parameters
                propDelay = speedOfLight / Math.Sqrt(permitivityEff);
                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                theta = beta * length;

                // Trace Impedance
                cpw_params.SysProp.Impedance = impedance;

                // Electrical Angle
                cpw_params.SysProp.Theta = Convert_To_External_Units(theta, cpw_params.SysProp.ThetaUnits);
            }
        }

        /** Substrate Integrated Waveguide - Synthesis / Evalute **/
        private void TLine_SIW_Run()
        {
            // Assign constants
            double speedOfLight = 299792458;

            // Initialize variables
            double propDelay;
            double wavelength;
            double beta;

            // Unitless Values
            double permitivityRel = siw_params.DielProp.Permitivity;

            if (tlineRunSynthesis)
            {
                // Gather Synthesis specific inputs
                double theta = Convert_To_Internal_Units(siw_params.SysProp.Theta, siw_params.SysProp.ThetaUnits);
                double frequency = Convert_To_Internal_Units(siw_params.SysProp.Frequency, siw_params.SysProp.FrequencyUnits);

                // Initialize Output Variables
                double width;
                double length;
                double gap;
                double viaDia;

                propDelay = speedOfLight / Math.Sqrt(permitivityRel);
                width = propDelay / (2 * frequency);

                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                length = theta / beta;

                viaDia = wavelength / 5;
                gap = 2 * viaDia;

                width += Math.Pow(viaDia, 2) / (0.95 * gap);

                siw_params.Width = Convert_To_External_Units(width, siw_params.WidthUnits);
                siw_params.Length = Convert_To_External_Units(length, siw_params.LengthUnits);
                siw_params.Gap = Convert_To_External_Units(gap, siw_params.GapUnits);
            }

            if (tlineRunEvaluate)
            {
                // Gather Evaluation specific inputs
                double width = Convert_To_Internal_Units(siw_params.Width, siw_params.WidthUnits);
                double length = Convert_To_Internal_Units(siw_params.Length, siw_params.LengthUnits);

                // Initialize Output Variables
                double frequency;
                double theta;

                propDelay = speedOfLight / Math.Sqrt(permitivityRel);
                frequency = propDelay / (2 * width);

                wavelength = propDelay / frequency;
                beta = 2 * Math.PI / wavelength;

                theta = beta * length;

                siw_params.SysProp.Frequency = Convert_To_External_Units(frequency, siw_params.SysProp.FrequencyUnits);
                siw_params.SysProp.Theta = Convert_To_External_Units(theta, siw_params.SysProp.ThetaUnits);
            }
        }

        /***************************************
        ******   Supplemental Functions   ******
        ****************************************/

        /** Convert to Internal Units **/
        private double Convert_To_Internal_Units(double val, string unitOut)
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

            return (scale * val);
        }

        /** Convert to External Units **/
        private double Convert_To_External_Units(double val, string unitOut)
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

            return (scale * val);
        }

        /** Extract Value from String **/
        private double Extract_Value(string inputs)
        {
            string temps;

            temps = inputs;

            temps = temps.Replace(" ", "");
            temps = temps.Replace("\\", "");

            double.TryParse(temps, out double outVal);

            return outVal;
        }

        /** Double Factorial **/
        private double Double_Factorial(double val)
        {
            double retVal;

            if (val < 2)
            {
                retVal = 1;
            }
            else
            {
                retVal = val * Double_Factorial(val - 2);
            }

            return retVal;
        }

        /** Complete Elliptic Integral of the First Kind **/
        private double CEI_First(double k)
        {
            double retVal = 0;

            double pTerm = 1;
            double cTerm;

            int count = 0;

            // Approximation converges only when k < 1
            if (1 <= k) return -1;

            while (0.001 < pTerm)
            {
                cTerm = Double_Factorial(2 * count - 1) / Double_Factorial(2 * count);
                cTerm *= Math.Pow(k, count);
                cTerm = Math.Pow(cTerm, 2);
                cTerm *= Math.PI / 2;

                retVal += cTerm;

                pTerm = cTerm;
                count++;
            }

            return retVal;
        }

        /********************************
         ******   Event Handlers   ******
         ********************************/

        /** Synthesis Click Event Handler **/
        private void TLine_Synthesis_Click(object sender, RoutedEventArgs e)
        {
            tlineRunSynthesis = true;
            tlineRunEvaluate = false;

            // Assign the permitivity value
            switch (tlineTabCtrl.SelectedIndex)
            {
                /**Tab 0: Microstrip**/
                case 0:
                    TLine_MS_Run();
                    break;

                /**Tab 1: stripline**/
                case 1:
                    TLine_SL_Run();
                    break;

                /**Tab 2: Coplanar Waveguide**/
                case 2:
                    TLine_CPW_Run();
                    break;

                /**Tab 3: Grounded Coplanar Waveguide**/
                case 3:
                    TLine_GCPW_Run();
                    break;

                /**Tab 4: Substrate Integrate Waveguide**/
                case 4:
                    TLine_SIW_Run();
                    break;

                /**Default: Microstrip**/
                default:
                    TLine_MS_Run();
                    break;
            }

            tlineRunSynthesis = false;
            tlineRunEvaluate = false;
        }

        /** Evaluate Click Event Handler **/
        private void TLine_Evaluate_Click(object sender, RoutedEventArgs e)
        {
            tlineRunSynthesis = false;
            tlineRunEvaluate = true;

            // Assign the permitivity value
            switch (tlineTabCtrl.SelectedIndex)
            {
                /**Tab 0: Microstrip**/
                case 0:
                    TLine_MS_Run();
                    break;

                /**Tab 1: stripline**/
                case 1:
                    TLine_SL_Run();
                    break;

                /**Tab 2: Coplanar Waveguide**/
                case 2:
                    TLine_CPW_Run();
                    break;

                /**Tab 3: Grounded Coplanar Waveguide**/
                case 3:
                    TLine_GCPW_Run();
                    break;

                /**Tab 4: Substrate Integrate Waveguide**/
                case 4:
                    TLine_SIW_Run();
                    break;

                /**Default: Microstrip**/
                default:
                    TLine_MS_Run();
                    break;
            }

            tlineRunSynthesis = false;
            tlineRunEvaluate = false;
        }

        /** Dielectric ComboBox Selection Change Event Handler **/
        private void DielCBSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() != typeof(ComboBox))
                return;

            string name = "Custom";
            double permitivity = 0;
            double tand = 0;
            double cte = 0;

            TLineParameters temp;

            string selectedItem = e.AddedItems[0].ToString();

            // Select the current data context based on the selected tab
            switch (tlineTabCtrl.SelectedIndex)
            {
                /**Tab 0: Microstrip**/
                case 0:
                    temp = ms_params;
                    break;

                /**Tab 1: stripline**/
                case 1:
                    temp = sl_params;
                    break;

                /** Tab 2: Coplanar Waveguide **/
                case 2:
                    temp = cpw_params;
                    break;

                /** Tab 3: Grounded Coplanar Waveguide **/
                case 3:
                    temp = gcpw_params;
                    break;

                /** Tab 4: Substrate Integrated Waveguide **/
                case 4:
                    temp = siw_params;
                    break;

                /**Default: Microstrip**/
                default:
                    temp = ms_params;
                    break;
            }

            // If custom dielectric then enable editing
            if (name == selectedItem)
            {
                // Do Nothing - Data will be updated from text box
                temp.DielProp.IsCustom = true;
                return;
            }

            DatabaseService dbService = new DatabaseService();
            dbService.Set_Table("dielecitrc");

            DataTable dataTable = dbService.Get_Selected_Table();

            // Dielectric Permitivity
            foreach (DataRow row in dataTable.Rows)
            {
                if (selectedItem == row.ItemArray[0].ToString())
                {
                    name = row.ItemArray[0].ToString();
                    permitivity = Extract_Value(row.ItemArray[1].ToString());
                    tand = Extract_Value(row.ItemArray[2].ToString());
                    cte = Extract_Value(row.ItemArray[3].ToString());
                }
            }

            // Assign the permitivity value
            temp.DielProp.Name = name;
            temp.DielProp.Permitivity = permitivity;
            temp.DielProp.TanD = tand;
            temp.DielProp.CTE = cte;
            temp.DielProp.IsCustom = false;
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.CurrentDirectory;

            System.Diagnostics.Process.Start(filePath + "\\HelpGuide\\transmissionLine.html");
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
