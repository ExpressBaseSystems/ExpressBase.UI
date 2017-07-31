﻿#if !NET462
using ExpressBase.Data;
using ExpressBase.Objects.Attributes;
#endif
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [ProtoBuf.ProtoContract]
    [EnableInBuilder(BuilderType.WebFormBuilder, BuilderType.FilterDialogBuilder)]
    [HideInToolBox]
    public class EbForm : EbControlContainer
    {
        [Browsable(false)]
        public bool IsUpdate { get; set; }
       
        public EbForm() { }

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

            return html;
        }

        public string GetControlNames()
        {
            List<string> _lst = new List<string>();

            //foreach (EbControl _c in this.FlattenedControls)
            //{
            //    if (!(_c is EbControlContainer))
            //        _lst.Add(_c.Name);
            //}

            return string.Join(",", _lst.ToArray());
        }
    }
}
