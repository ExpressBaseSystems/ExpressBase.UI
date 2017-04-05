﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    public enum EbDateType
    {
        Date,
        Time,
        DateTime,
    }

    [ProtoBuf.ProtoContract]
    public class EbDate : EbControl
    {
        public EbDate() { }

        public EbDate(object parent)
        {
            this.Parent = parent;
        }

        [ProtoBuf.ProtoMember(1)]
        [System.ComponentModel.Category("Behavior")]
        public EbDateType EbDateType { get; set; }

        [ProtoBuf.ProtoMember(2)]
        [System.ComponentModel.Category("Data")]
        public DateTime Min { get; set; }

        [ProtoBuf.ProtoMember(3)]
        [System.ComponentModel.Category("Data")]
        public DateTime Max { get; set; }

        [ProtoBuf.ProtoMember(4)]
        [System.ComponentModel.Category("Data")]
        public DateTime Value { get; set; }

        [ProtoBuf.ProtoMember(5)]
        [System.ComponentModel.Category("Behavior")]
        public string PlaceHolder { get; set; }

        [ProtoBuf.ProtoMember(6)]
        [System.ComponentModel.Category("Behavior")]
        public bool AutoCompleteOff { get; set; }

        private string EbDateTypeString
        {
            get
            {
                string returnval = string.Empty;
                switch (this.EbDateType)
                {
                    case EbDateType.Time:
                        returnval = "time";
                        break;
                    case EbDateType.Date:
                        returnval = "date";
                        break;
                    case EbDateType.DateTime:
                        returnval = "datetime";
                        break;
                }
                return returnval;
            }
        }

        public override string GetHead()
        {
            return  (((!this.Hidden) ? this.UniqueString + this.RequiredString : string.Empty) + @"".Replace("{0}", this.Name)) + @"
$('#$idTglBtn').click(function(){
    //$('#$idContainer [class=date]').toggle();
        $('#$id').focus();
        $('#$id').trigger('click');

});

$('.date').mask('0000/00/00'); 
$('#$id').$$$$$$$picker({
    dateFormat: 'yy/mm/dd',
	timeFormat: 'hh:mm:ss:tt',
	stepHour: 1,
	stepMinute: 1,
	stepSecond: 1
});".Replace("$$$$$$$", this.EbDateTypeString)
.Replace("$id", this.Name);
        }

        public override string GetHtml()
        {
            return string.Format(@"
<div id='@nameContainer' style='position:absolute; left:@leftpx; top:@top; @hiddenString'>
    <span id='@nameLbl' style='@lblBackColor @LblForeColor'>@label</span>
    <div  class='input-group' style='width:1px;'>
        <input id='@name' data-EbType='@datetype'  data-toggle='tooltip' title='@toolTipText' class='date' type='text'  name='@name'  autocomplete = '@autoComplete' @value @tabIndex style='width:@widthpx; height:@heightpx; @backColor @foreColor display:inline-block; @fontStyle @readOnlyString @required @placeHolder{11} />
        <i id='@nameTglBtn' class='fa  @atchdLbl input-group-addon' aria-hidden='true'></i>
    </div>
    <span class='helpText'> @helpText </span>
</div>
",
this.Name, this.Left, this.Top, this.Width, this.Height, this.Label, //5
this.HiddenString, (this.Required && !this.Hidden ? " required" : string.Empty), this.ReadOnlyString,//8
this.ToolTipText, this.HelpText, "tabindex='" + this.TabIndex + "'",//11
 "background-color:" + this.BackColorSerialized + ";", "color:" + this.ForeColorSerialized + ";", "background-color:" + this.LabelBackColorSerialized + ";", "color:" + this.LabelForeColorSerialized + ";")

.Replace("@name", this.Name)
.Replace("@left", this.Left.ToString())
.Replace("@top", this.Top.ToString())
.Replace("@width", this.Width.ToString())
.Replace("@height", this.Height.ToString())
.Replace("@datetype", this.EbDateTypeString)
.Replace("@value", "value='" + this.Value + "'")
.Replace("@label", this.Label)
.Replace("@hiddenString", this.HiddenString)
.Replace("@required", (this.Required && !this.Hidden ? " required" : string.Empty))
.Replace("@readOnlyString", this.ReadOnlyString)
.Replace("@toolTipText", this.ToolTipText)
.Replace("@helpText", this.HelpText)
.Replace("@placeHolder", "placeholder='" + this.PlaceHolder + "'")
.Replace("@tabIndex", "tabindex='" + this.TabIndex + "'")
.Replace("@autoComplete", this.AutoCompleteOff ? "off" : "on")
.Replace("@backColor", "background-color:" + this.BackColorSerialized + ";")
.Replace("@foreColor", "color:" + this.ForeColorSerialized + ";")
.Replace("@lblBackColor", "background-color:" + this.LabelBackColorSerialized + ";")
.Replace("@LblForeColor", "color:" + this.LabelForeColorSerialized + ";")
.Replace("@fontStyle", (this.FontSerialized != null) ?
                            (" font-family:" + this.FontSerialized.FontFamily + ";" + "font-style:" + this.FontSerialized.Style
                            + ";" + "font-size:" + this.FontSerialized.SizeInPoints + "px;")
                        : string.Empty)
.Replace("@atchdLbl", (this.EbDateTypeString=="time") ? "fa-clock-o" : "fa-calendar")
;
        }
    }
}
