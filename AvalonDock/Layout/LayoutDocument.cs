//Copyright (c) 2007-2012, Adolfo Marinucci
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the 
//following conditions are met:

//* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

//* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
//disclaimer in the documentation and/or other materials provided with the distribution.

//* Neither the name of Adolfo Marinucci nor the names of its contributors may be used to endorse or promote products
//derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
//INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
//EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
//STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AvalonDock.Layout
{
    [Serializable]
    public class LayoutDocument : LayoutContent
    {
        public bool IsVisible
        {
            get { return true; }
        }

        #region Description

        private string _description = null;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }

        #endregion

        #region LastActivationTimeStamp

        private DateTime? _lastActivationTimeStamp = null;
        public DateTime? LastActivationTimeStamp
        {
            get { return _lastActivationTimeStamp; }
            set
            {
                if (_lastActivationTimeStamp != value)
                {
                    _lastActivationTimeStamp = value;
                    RaisePropertyChanged("LastActivationTimeStamp");
                }
            }
        }

        #endregion

        protected override void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            if (IsActive)
                LastActivationTimeStamp = DateTime.Now;

            base.OnIsActiveChanged(oldValue, newValue);
        }


        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);

            if (!string.IsNullOrWhiteSpace(Description))
                writer.WriteAttributeString("Description", Description);
            if (LastActivationTimeStamp != null)
                writer.WriteAttributeString("LastActivationTimeStamp", LastActivationTimeStamp.Value.ToString(CultureInfo.InvariantCulture));

        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("Description"))
                Description = reader.Value;
            if (reader.MoveToAttribute("LastActivationTimeStamp"))
                LastActivationTimeStamp = DateTime.Parse(reader.Value, CultureInfo.InvariantCulture);

            base.ReadXml(reader);
       }

    }
}
