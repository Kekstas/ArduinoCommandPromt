namespace ArduinoCommandPromt
{
    public class DDeviceStatus
    {
        public DDeviceStatus(double xposPrevious, double yposPrevious, double zposPrevious)
        {
            XposPrevious = xposPrevious;
            YposPrevious = yposPrevious;
            ZposPrevious = zposPrevious;
        }

        public bool ZposChange { get; internal set; }
        public double XposPrevious { get; private set; }
        public double YposPrevious { get; private set; }
        public double ZposPrevious { get; private set; }



        private double _Xpos;
        private double _Ypos;
        private double _Zpos;
        private int _AtoolPos;
        private int _BtoolPos;
        private int _CtoolPos;
        private int _DtoolPos;
        private int _EtoolPos;
        private int _FtoolPos;
    }
}