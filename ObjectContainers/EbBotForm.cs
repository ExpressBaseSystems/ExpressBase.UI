﻿using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.BotForm)]
    [HideInToolBox]
    public class EbBotForm : EbControlContainer
    {
        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        public bool IsUpdate { get; set; }



        public EbBotForm() { }

        public enum Operations
        {
            Access
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
            string html = "<form id='@name@' class='eb-form'>";

            foreach (EbControl c in this.Controls)
                html += c.GetHtml();

            html += "</form>";

            return html.Replace("@name@", this.Name);
        }
    }
}
