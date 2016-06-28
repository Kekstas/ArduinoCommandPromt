using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoCommandPromt
{
    public class StringBuilderWrapper : INotifyPropertyChanged
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private EventHandler<EventArgs> TextChanged;

        private string _text="";

        public string Text
        {
            get { return _text; }
        }

        public int Count
        {
            get { return _builder.Length; }
        }

        public void Append(string text)
        {
            _builder.Append(text);
            if (TextChanged != null)
                TextChanged(this, null);
            _text = _builder.ToString();
            RaisePropertyChanged("Text");
        }
        public void Append(string[] textLines)
        {
            foreach (var line in textLines)
            {
                _builder.Append(line);
            }

            _text = _builder.ToString();
            if (TextChanged != null)
                TextChanged(this, null);
            RaisePropertyChanged("Text");
        }
        public void AppendLines(string[] textLines)
        {
            //var _builder = new StringBuilder(_text);
            foreach (var line in textLines)
            {
                _builder.AppendLine(line);
            }
            _text = _builder.ToString();

            if (TextChanged != null)
                TextChanged(this, null);
            RaisePropertyChanged("Text");
        }


        public void AppendLine(string text)
        {

            _builder.AppendLine(text);
            _text = _builder.ToString();
            if (TextChanged != null)
                TextChanged(this, null);
            RaisePropertyChanged("Text");
        }

        public void Clear()
        {
            _builder.Clear();
            _text = _builder.ToString();
            if (TextChanged != null)
                TextChanged(this, null);
            RaisePropertyChanged("Text");
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        //public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        //{
        //    if (propertyExpression == null)
        //    {
        //        return;
        //    }

        //    var handler = PropertyChanged;

        //    if (handler != null)
        //    {
        //        var body = propertyExpression.Body as MemberExpression;
        //        if (body != null)
        //            handler(this, new PropertyChangedEventArgs(body.Member.Name));
        //    }
        //}

        #endregion


    }
}
