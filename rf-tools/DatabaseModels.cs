using System.ComponentModel;

namespace rf_tools
{
    public class DielectricDbModel : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public float Permitivity { get; set; }
        public float TanD { get; set; }
        public float CTE { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AmplifierDbModel : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public float Frequency { get; set; }
        public float Bandwidth { get; set; }
        public float Gain { get; set; }
        public string DataFile { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
