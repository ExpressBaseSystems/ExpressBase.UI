﻿using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
    public class EbNumeric : EbControlUI
    {
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } }

        public EbNumeric()
        {
        }

        [JsonIgnore]
        public override string GetValueJSfn
        {
            get
            {
                return @"
                    return parseFloat($('#' + this.EbSid_CtxId).val()) || 0;
                ";
            }
            set { }
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public int MaxLength { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("2")]
        public int DecimalPlaces { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public decimal Value { get; set; }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.BotForm)]
        public override bool IsReadOnly { get => this.ReadOnly; }

        [EnableInBuilder(BuilderType.BotForm)]
        public bool AutoIncrement { get; set; }

        //[ProtoBuf.ProtoMember(4)]
        private string PlaceHolder
        {
            get
            {
                //for ( int i=0; i< this.DecimalPlaces; i++)
                //    res += "0";
                return (this.DecimalPlaces == 0) ? "0" : "0." + new String('0', this.DecimalPlaces);
            }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public bool AllowNegative { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public int MaxLimit { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public int MinLimit { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public bool IsCurrency { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public bool AutoCompleteOff { get; set; }

        //private string MaxLengthString
        //{
        //    get { return ((this.MaxLength > 0) ? "$('#{0}').focus(function() {$(this).select();});".Replace("{0}", this.Name).Replace("{1}", this.Value.ToString()) : string.Empty); }
        //}

        public override VendorDbType GetvDbType(IVendorDbTypes vDbTypes)
        {
            return vDbTypes.Decimal;
        }

        public override string GetToolHtml()
        {
            return @"<div eb-type='@toolName' class='tool'><b>0-9 </b></i>  @toolName</div>".Replace("@toolName", this.GetType().Name.Substring(2));
        }

        public override string GetDesignHtml()
        {
            return GetHtmlHelper(RenderMode.Developer).RemoveCR().DoubleQuoted();
        }

        public override string GetHtml()
        {
            return GetHtmlHelper(RenderMode.User);
        }

        //        public override string GetBareHtml()
        //        {
        //            return @" 
        //        <div class='input-group' style='width:100%;'>
        //                <span style='font-size: @fontSize@' class='input-group-addon'><i class='fa fa-sort-numeric-asc' aria-hidden='true'></i></span>   
        //                <input type='text' class='numinput' ui-inp data-ebtype='@datetype@' id='@name@' name='@name@' data-toggle='tooltip' style=' width:100%; display:inline-block;'/>
        //        </div>"
        //.Replace("@name@", this.Name)
        //.Replace("@datetype@", "11");
        //        }

        public override string GetBareHtml()
        {
            return @" 
                <div class='input-group' style='width:100%;'>
                        <span style='font-size: @fontSize@' class='input-group-addon'><i class='fa fa-sort-numeric-asc' aria-hidden='true'></i></span>   
                        <input type='text' data-ebtype='@datetype@' class='numinput' ui-inp id='@ebsid@' name='@name@' value='@value@' @placeHolder autocomplete = '@autoComplete@' data-toggle='tooltip' title='@toolTipText@' style=' width:100%; @backColor@ @foreColor@ @fontStyle@ display:inline-block; @readOnlyString@ @required@ @tabIndex@ />
                </div>"
.Replace("@name@", this.Name)
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@autoComplete@", this.AutoCompleteOff ? "off" : "on")
.Replace("@value@", "")//"value='" + this.Value + "'")
.Replace("@tabIndex@", "tabindex='" + this.TabIndex + "'")
    .Replace("@BackColor@ ", ("background-color:" + ((this.BackColor != null) ? this.BackColor : "@BackColor@ ") + ";"))
    .Replace("@ForeColor@ ", "color:" + ((this.ForeColor != null) ? this.ForeColor : "@ForeColor@ ") + ";")
.Replace("@required@", " required")//(this.Required && !this.Hidden ? " required" : string.Empty))
.Replace("@readOnlyString@", this.ReadOnlyString)
.Replace("@placeHolder@", "placeholder='" + this.PlaceHolder + "'")
.Replace("@datetype@", "11")
//.Replace("@fontStyle@", (this.FontSerialized != null) ?
//                            (" font-family:" + this.FontSerialized.FontFamily + ";" + "font-style:" + this.FontSerialized.Style
//                            + ";" + "font-size:" + this.FontSerialized.SizeInPoints + "px;")
//                        : string.Empty)
;
        }

        private string GetHtmlHelper(RenderMode mode)
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
               .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
               .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }

        //        private string GetHtmlHelper(RenderMode mode)
        //        {
        //            return (@"
        //<div id='cont_@name@' class='Eb-ctrlContainer' Ctype='Numeric' eb-hidden='@isHidden@'>
        //    <div class='eb-ctrl-label' id='@nameLbl' style='@lblBackColor @LblForeColor'>@label@</div>
        //       @barehtml@            
        //    <span class='helpText'> @helpText </span>
        //</div>"
        //.Replace("@barehtml@", this.GetBareHtml())
        //.Replace("@name@", this.Name)
        //.Replace("@left", this.Left.ToString())
        //.Replace("@top", this.Top.ToString())
        //.Replace("@height", this.Height.ToString())
        //.Replace("@label@", this.Label)//5
        //.Replace("@isHidden@", this.Hidden.ToString())
        //.Replace("@required", (this.Required && !this.Hidden ? " required" : string.Empty))
        //.Replace("@readOnlyString", this.ReadOnlyString)
        //.Replace("@toolTipText", this.ToolTipText)
        //.Replace("@helpText", this.HelpText)//10
        //.Replace("@placeHolder", "placeholder='" + this.PlaceHolder + "'")
        //.Replace("@tabIndex", "tabindex='" + this.TabIndex + "'")
        //.Replace("@autoComplete", this.AutoCompleteOff ? "off" : "on"));
        //        }
    }
}
