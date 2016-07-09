using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers.ObjectsExtentions;
using System.ComponentModel;

namespace ArduinoCommandPromt
{
    public class CNCSimulator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _Xpos;
        private double _Ypos;
        private double _Zpos;
        private int _AtoolPos;
        private int _BtoolPos;
        private int _CtoolPos;
        private int _DtoolPos;
        private int _EtoolPos;
        private int _FtoolPos;



        public double Xpos
        {
            get { return _Xpos; }
            set
            {
                _Xpos = value;
                PropertyChanged.Notify(() => Xpos, this);
            }
        }
        public double Ypos
        {
            get { return _Ypos; }
            set
            {
                _Ypos = value;
                PropertyChanged.Notify(() => Ypos, this);
            }
        }
        public double Zpos
        {
            get { return _Zpos; }
            set
            {
                _Zpos = value;
                PropertyChanged.Notify(() => Zpos, this);
            }
        }
        public int AtoolPos
        {
            get { return _AtoolPos; }
            set
            {
                _AtoolPos = value;
                PropertyChanged.Notify(() => AtoolPos, this);
            }
        }
        public int BtoolPos
        {
            get { return _BtoolPos; }
            set
            {
                _BtoolPos = value;
                PropertyChanged.Notify(() => BtoolPos, this);
            }
        }
        public int CtoolPos
        {
            get { return _CtoolPos; }
            set
            {
                _CtoolPos = value;
                PropertyChanged.Notify(() => CtoolPos, this);
            }
        }
        public int DtoolPos
        {
            get { return _DtoolPos; }
            set
            {
                _DtoolPos = value;
                PropertyChanged.Notify(() => DtoolPos, this);
            }
        }
        public int EtoolPos
        {
            get { return _EtoolPos; }
            set
            {
                _EtoolPos = value;
                PropertyChanged.Notify(() => EtoolPos, this);
            }
        }
        public int FtoolPos
        {
            get { return _FtoolPos; }
            set
            {
                _FtoolPos = value;
                PropertyChanged.Notify(() => FtoolPos, this);
            }
        }


        internal void Send(string p)
        {
            ParseCommand(p.Replace('.', ','));
        }


        void ParseCommand(string command)
        {

            command = command.Trim();
            if (command.Length == 0) return;

            switch (command[0])
            {
                case 'M':
                    parseM(command);
                    break;
                case 'G':
                    parsG(command);
                    break;
                case 'X':
                    parsG("G1 " + command);
                    break;
                case 'Y':
                    parsG("G1 " + command);
                    break;
                case 'Z':
                    parsG("G1 " + command);
                    break;

            }

        }

        private void parsG(string command)
        {
            var wordEnd = command.IndexOf(@" ");
            if (wordEnd < 0) wordEnd = command.Length;
            var gCommand = command.Substring(0, wordEnd);
            switch (gCommand)
            {
                case "G1":
                case "G2":
                    parseCoordinates(command.Substring(wordEnd).Trim());
                    break;
                case "G28":
                    Xpos = 0;
                    Ypos = 0;
                    Zpos = 0;
                    break;
            }

        }



        private void parseM(string command)
        {
            var wordEnd = command.IndexOf(@" ");
            if (wordEnd < 0) wordEnd = command.Length;
            var gCommand = command.Substring(0, wordEnd);
            switch (gCommand)
            {

                case "M1":
                    parseTool(command.Substring(wordEnd).Trim());
                    break;
            }
        }

        private void parseTool(string command)
        {
            switch (command[0])
            {
                case 'A':
                    AtoolPos = command.Substring(1).TryParse<int>(0);
                    break;
                case 'B':
                    BtoolPos = command.Substring(1).TryParse<int>(0);
                    break;
                case 'C':
                    CtoolPos = command.Substring(1).TryParse<int>(0);
                    break;
                case 'D':
                    DtoolPos = command.Substring(1).TryParse<int>(0);
                    break;
                case 'E':
                    EtoolPos = command.Substring(1).TryParse<int>(0);
                    break;
                case 'F':
                    FtoolPos = command.Substring(1).TryParse<int>(0);
                    break;


            }
        }

        private void parseCoordinates(string command)
        {

            while (command.Length > 0)
            {
                var wordEnd = command.IndexOf(@" ");
                if (wordEnd < 0) wordEnd = command.Length;
                var gCommand = command.Substring(0, wordEnd);

                var coordinate = gCommand.Substring(1).TryParse<double>(0);
                switch (gCommand[0])
                {
                    case 'X':
                        Xpos = coordinate;
                        break;
                    case 'Y':
                        Ypos = coordinate;
                        break;
                    case 'Z':
                        Zpos = coordinate;
                        break;
                }

                command = command.Substring(wordEnd).Trim();
            }


        }





    }
}
