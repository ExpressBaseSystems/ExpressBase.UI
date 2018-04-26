﻿using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    public enum EbRadioValueType
    {
        Boolean,
        Integer,
        Text
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
    public class EbRadioGroup : EbControlUI
    {
        public EbRadioGroup()
        {
            this.Options = new List<EbRadioOption>();
            //this.Options.Add(new EbRadioOption());
            //this.Options.Add(new EbRadioOption());
            //this.Options.CollectionChanged += Options_CollectionChanged;
            this.ValueType = EbRadioValueType.Boolean;
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
             this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        private void Options_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (this.Options.Count == 3 && this.ValueType == EbRadioValueType.Boolean)
                    this.ValueType = EbRadioValueType.Integer;
            }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [PropertyGroup("Behavior")]
        public EbRadioValueType ValueType { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        [Alias("Options")]
        public List<EbRadioOption> Options { get; set; }

        public override string GetToolHtml()
        {
            return @"<div eb-type='@toolName' class='tool'> &#9673;  @toolName</div>".Replace("@toolName", this.GetType().Name.Substring(2));
        }

        public override string GetBareHtml()
        {
            string html = "<div id='@name@' data-ebtype='@data-ebtype@' name='@name@' style='padding:5px' type='RadioGroup'>";
              foreach (EbRadioOption ec in this.Options)
            {
                ec.GName = this.Name;
                html += ec.GetHtml().Replace("@defaultcheked@",((this.Options.IndexOf(ec) == 0) ? " checked='checked' " : ""));
            }
            html += "</div>";
            return html
.Replace("@name@", (this.Name != null) ? this.Name : "@name@")
.Replace("@data-ebtype@", "3");
        }

        public override string GetDesignHtml()
        {
            //return @"<div class='btn-group' data-toggle='buttons'> <label class='btn btn-primary active'> <input type='radio' name='options' id='option1' autocomplete='off' checked> Radio 1 </label> <label class='btn btn-primary'> <input type='radio' name='options' id='option2' autocomplete='off'> Radio 2 </label> <label class='btn btn-primary'> <input type='radio' name='options' id='option3' autocomplete='off'> Radio 3 </label> </div>".RemoveCR().DoubleQuoted();
            return GetHtml().RemoveCR().DoubleQuoted();
        }

        public override string GetHtml()
        {

            string html = @"
            <div id='cont_@name@' class='Eb-ctrlContainer' Ctype='RadioGroup'>
                <div class='radiog-cont'  style='@BackColor '>
                 <div id='@name@Lbl' class='radiog-label' style='@LabelBackColor @LabelForeColor '> @Label@  </div>
                        @barehtml@
                <span class='helpText'> @HelpText@ </span></div>
            </div>"
.Replace("@barehtml@", this.GetBareHtml())
.Replace("@name@", (this.Name != null) ? this.Name : "@name@")
.Replace("@Label@", this.Label)
.Replace("@HelpText@", this.HelpText ?? "")
.Replace("@LabelForeColor ", "color:" + ((this.LabelForeColor != null) ? this.LabelForeColor : "@LabelForeColor ") + ";")
.Replace("@LabelBackColor ", "background-color:" + ((this.LabelBackColor != null) ? this.LabelBackColor : "@LabelBackColor ") + ";")
.Replace("@BackColor ", ("background-color:" + ((this.BackColor != null) ? this.BackColor : "@BackColor ") + ";"));
            return html;
        }

        public override string GetJsInitFunc()
        {
            return @"
this.Init = function(id)
{
    this.Options.$values.push(new EbObjects.EbRadioOption(id + '_Rd0'));
    this.Options.$values.push(new EbObjects.EbRadioOption(id + '_Rd1'));
};";
        }
    }

    [HideInToolBox]
    public class EbRadioOptionAbstract : EbControlUI
    {

    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
    [HideInToolBox]
    public class EbRadioOption : EbControlUI
    {
        public EbRadioOption() { }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public override string Label { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public string Value { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [HideInPropertyGrid]
        public string GName { get; set; }

        public override string GetBareHtml()
        {
            return @"<div class='radio-wrap' onclick=""event.stopPropagation();$('#@name@').prop('checked', true);"">
                        <input type ='radio' id='@name@' @defaultcheked@ value='@value@' name='@gname@'>
                        <span id='@name@Lbl' style='@LabelBackColor @LabelForeColor '> @label@  </span>
                    </div>"
.Replace("@name@", this.Name)
.Replace("@gname@", this.GName)
.Replace("@label@", this.Label)
.Replace("@value@", this.Value);
        }

        public override string GetHtml()
        {
            return this.GetBareHtml(); ;
        }
    }
}
