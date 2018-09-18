﻿using ExpressBase.Common.Data;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.Objects;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.FilterDialog)]
    [HideInToolBox]
    public class EbFilterDialog : EbControlContainer
    {
        public EbFilterDialog() { }

        private List<Param> _paramlist = new List<Param>();

        public List<Param> GetDefaultParams()
        {
            foreach (EbControl c in this.Controls)
            {
                string val = string.Empty;
                if ((c.EbDbType).ToString() == "Decimal")
                    val = "0";
                else if ((c.EbDbType).ToString() == "AnsiString")
                    val = "0";
                else if ((c.EbDbType).ToString() == "String")
                    val = "EB";
                else if ((c as EbDate).ShowDateAs_ == DateShowFormat.Year_Month)
                    val = "01/2018";
                else if ((c as EbDate).ShowDateAs_ == DateShowFormat.Year_Month_Date)
                    val = "01-01-2018";
                Param _p = new Param { Name = c.Name, Type = Convert.ToInt32(c.EbDbType).ToString(), Value = val };
                if (c is EbComboBox)
                {
                    if ((c as EbComboBox).ValueMember.Type.ToString() == "Int32")
                    {
                        _p.Type = "11";
                    }
                    //_p.Type = Convert.ToInt32((EbDbType)((c as EbComboBox).ValueMember.Type));
                }
                if(c is EbUserLocation)
                {
                    _p.Value = "0";
                    _p.Type = "11";
                }
                _paramlist.Add(_p);
            }
            return _paramlist;
        }

        public override string GetHead()
        {
            string head = string.Empty;

            foreach (EbControl c in this.Controls)
                head += c.GetHead();

            return head;
        }

        public override string GetHtml()
        {
            string html = string.Empty;

            foreach (EbControl c in this.Controls)
                html += c.GetHtml();

            html += string.Format("<input type='hidden' name='all_control_names' id='all_control_names' value='{0}' />", string.Join(",", ControlNames));

            return html;
        }

        public IEnumerable<string> ControlNames
        {
            get
            {
                foreach (EbControl _c in this.Controls.Flatten())
                {
                    if (!(_c is EbControlContainer))
                        yield return _c.Name;
                }
            }
        }

        [EnableInBuilder(BuilderType.FilterDialog)]
        public int Width { get; set; }

     
        public override void ReplaceRefid(Dictionary<string, string> RefidMap)
        {
            foreach (EbControl control in Controls)
            {
                PropertyInfo[] _props = control.GetType().GetProperties();
                foreach (PropertyInfo _prop in _props)
                {
                    if (_prop.IsDefined(typeof(OSE_ObjectTypes)))
                    {
                        string _val = _prop.GetValue(control, null).ToString();
                        if (RefidMap.ContainsKey(_val))
                            _prop.SetValue(control, RefidMap[_val], null);
                        else
                            _prop.SetValue(control, "failed-to-update-");
                    }

                }
            }
        }
        public override string DiscoverRelatedRefids()
        {
            var x = this.RefId;
            string refids = "";
            foreach (EbControl control in Controls)
            {
                PropertyInfo[] _props = control.GetType().GetProperties();
                foreach (PropertyInfo _prop in _props)
                {
                    if (_prop.IsDefined(typeof(OSE_ObjectTypes)))
                        refids += _prop.GetValue(control, null).ToString() + ",";
                }
            }
            Console.WriteLine(this.RefId +"-->" + refids);
            return refids;
        }
    }
}
