using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCommandPromt.Models
{

    public class ControllerEvent<T> : EventArgs
    {
        private readonly T _objectToPass;

        public ControllerEvent(T objectToPass)
        {
            _objectToPass = objectToPass;
        }

        public T GetData
        {
            get { return _objectToPass; }
        }
    }
}
