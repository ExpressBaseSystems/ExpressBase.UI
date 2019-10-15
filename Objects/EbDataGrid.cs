﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.Helpers;
using ExpressBase.Objects.Objects.DVRelated;
using ExpressBase.Objects.ServiceStack_Artifacts;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ExpressBase.Security;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
    public class EbDataGrid : EbControlContainer, IEbSpecialContainer
    {
        public EbDataGrid()
        {
            this.Controls = new List<EbControl>();
            this.Validators = new List<EbValidator>();
        }

        public override string UIchangeFns
        {
            get
            {
                return @"EbDataGrid = {
                title : function(elementId, props) {
                    $(`[ebsid=${elementId}]th .eb-label-editable`).text(props.Title);
                }
            }";
            }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        [DefaultPropValue("200")]
        [PropertyGroup("Identity")]
        public override int Height { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public new List<EbValidator> Validators { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        [DefaultPropValue("true")]
        [PropertyGroup("Behavior")]
        [Alias("Serial numbered")]
        public bool IsShowSerialNumber { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        public bool AscendingOrder { get; set; }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public override EbScript OnChangeFn { get; set; }

        [JsonIgnore]
        public override string OnChangeBindJSFn
        {
            get
            {
                return @"
$.each(this.Controls.$values, function (i, col) {
    if ((col.OnChangeFn && col.OnChangeFn.Code && col.OnChangeFn.Code.trim() !== '') || col.DependedValExp.$values.length > 0){
        let FnString = `/*console.log('${col.__path || col.Name}');*/` + atob(col.OnChangeFn.Code) + (col.DependedValExp.$values.length !== 0 ? ` ; form.updateDependentControls(form.__getCtrlByPath(this.__path))` : '');
        let OnChangeFn = new Function('form', 'user', `event`, FnString).bind(col, this.formObject, this.__userObject);

        col.bindOnChange({form:this.formObject, col:col, DG:this, user : this.__userObject},OnChangeFn);
    }
}.bind(this));
               ";
            }
            set { }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override bool IsSpecialContainer { get { return true; } set { } }

        [OnDeserialized]
        public new void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            foreach (EbControl contol in Controls)
            {
                if (contol is EbDGPowerSelectColumn)
                {
                    EbDGPowerSelectColumn DGPowerSelectColumn = (contol as EbDGPowerSelectColumn);
                    DGPowerSelectColumn.DBareHtml = DGPowerSelectColumn.GetBareHtml();
                }
            }
        }

        [JsonIgnore]
        public override UISides Padding { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.Collection)]
        [Alias("Columns")]
        [PropertyGroup("Behavior")]
        [ListType(typeof(EbDGColumn))]
        [PropertyPriority(99)]
        public override List<EbControl> Controls { get; set; }

        //[EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        //[PropertyGroup("Behavior")]
        //[DefaultPropValue("true")]
        //public bool IsEditable { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(98)]
        [DefaultPropValue("true")]
        public bool IsAddable { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        public override bool IsDisable { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(99)]
        [HelpText("Set true if you want to hide the control.")]
        public override bool Hidden { get; set; }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-table'></i>"; } set { } }

        //public override string GetToolHtml()
        //{
        //    return @"<div eb-type='@toolName' class='tool'><i class='fa fa-table'></i>  @toolName</div>".Replace("@toolName", this.GetType().Name.Substring(2));
        //}
        public override string GetBareHtml()
        {
            string html = @"
<div class='grid-cont'>
    <div class='Dg_head'>
        <table id='tbl_@ebsid@_head' class='table table-bordered dgtbl'>
            <thead>
              <tr>
                <th class='slno' style='width:34px'><span class='grid-col-title'>SL No</span></th>";
            foreach (EbDGColumn col in Controls)
            {
                if (!col.Hidden)
                    html += string.Concat("<th class='ppbtn-cont ebResizable' ebsid='@ebsid@' style='width: @Width@; @bg@' @type@ title='", col.Title, @"'>
                                                <span class='grid-col-title eb-label-editable'>", col.Title, @"</span>
                                                <input id='@ebsid@lbltxtb' class='eb-lbltxtb' type='text'/>
                                                @req@ @ppbtn@" +
                                            "</th>")
                        .Replace("@ppbtn@", Common.HtmlConstants.CONT_PROP_BTN)
                        .Replace("@req@", (col.Required ? "<sup style='color: red'>*</sup>" : string.Empty))
                        .Replace("@ebsid@", col.EbSid)
                        .Replace("@Width@", (col.Width <= 0) ? "auto" : col.Width.ToString() + "%")
                        .Replace("@type@", "type = '" + col.ObjType + "'")
                        .Replace("@bg@", col.IsDisable ? "background-color:#fafafa; color:#555" : string.Empty);
            }

            html += @"
                @cogs@
              </tr>
            </thead>
        </table>
    </div>".Replace("@cogs@", !this.IsDisable ? "<th style='width:50px;'><span class='fa fa-cog'></span></th>" : string.Empty);

            html += @"
    <div class='Dg_body' style='overflow-y:scroll;height:@_height@px ;'>
        <table id='tbl_@ebsid@' class='table table-bordered dgtbl'>
            <tbody>
            </tbody>
        </table>
    </div>
    <div class='Dg_footer'>
        <table id='tbl_@ebsid@_footer' class='table table-bordered dgtbl'>
            <tbody>
            </tbody>
        </table>
     </div>
</div>".Replace("@_height@", this.Height.ToString());

            return html;
        }

        public override string GetDesignHtml()
        {
            return GetHtml().RemoveCR().DoubleQuoted();
        }

        public override string GetHtml()
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
               .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
               .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }
    }

    [UsedWithTopObjectParent(typeof(EbObject))]
    [HideInPropertyGrid]
    [HideInToolBox]
    public abstract class EbDGColumn : EbControl
    {
        [JsonIgnore]
        public override string OnChangeBindJSFn { get { return @"$(`[ebsid=${p1.DG.EbSid}]`).on('change', `[colname=${this.Name}] [ui-inp]`, p2);"; } set { } }

        [JsonIgnore]
        public override string SetValueJSfn
        {
            get { return @"
$('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] [ui-inp]`).val(p1).trigger('change');
$('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}]>.tdtxt>span`).html(p1);
"; }

            set { }
        }

        [JsonIgnore]
        public override string GetValueJSfn
        {
            get { return @"let val = $('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] [ui-inp]`).val(); return val;"; }

            set { }
        }

        [JsonIgnore]
        public override string EnableJSfn { get { return @"$('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] .ctrl-cover *`).prop('disabled',false).css('pointer-events', 'inherit').find('input').css('background-color','#fff');"; } set { } }

        [JsonIgnore]
        public override string DisableJSfn { get { return @"$('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] .ctrl-cover *`).attr('disabled', 'disabled').css('pointer-events', 'none').find('input').css('background-color','#eee');"; } set { } }

        [JsonIgnore]
        public override string ClearJSfn { get { return @"$('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] [ui-inp]`).val('');"; } set { } }

        [JsonIgnore]
        public override string HideJSfn { get { return @""; } set { } }

        [JsonIgnore]
        public override string ShowJSfn { get { return @""; } set { } }


        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [UIproperty]
        [OnChangeUIFunction("EbDataGrid.title")]
        public string Title { get; set; }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        public string DBareHtml { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public virtual string InputControlType { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        public override bool IsDisable { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [DefaultPropValue("true")]
        public bool IsEditable { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyGroup("Appearance")]
        public virtual int Width { get; set; }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
    [Alias("String Column")]
    [UsedWithTopObjectParent(typeof(EbObject))]
    public class EbDGStringColumn : EbDGColumn
    {

        [JsonIgnore]
        public EbTextBox EbTextBox { get; set; }

        public EbDGStringColumn()
        {
            this.EbTextBox = new EbTextBox();
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        public TextMode TextMode
        {
            get { return this.EbTextBox.TextMode; }
            set { this.EbTextBox.TextMode = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [DefaultPropValue("3")]
        public int RowsVisible
        {
            get { return this.EbTextBox.RowsVisible; }
            set { this.EbTextBox.RowsVisible = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.UserControl)]
        public override string ToolTipText
        {
            get { return this.EbTextBox.ToolTipText; }
            set { this.EbTextBox.ToolTipText = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbTextBox"; } }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            DBareHtml = EbTextBox.GetBareHtml();
        }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
    [Alias("Numeric Column")]
    [UsedWithTopObjectParent(typeof(EbObject))]
    public class EbDGNumericColumn : EbDGColumn
    {
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbNumeric"; } }

        [EnableInBuilder(BuilderType.WebForm)]
        public bool IsAggragate { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        public bool AllowNegative { get; set; }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
    [Alias("Boolean Column")]
    [UsedWithTopObjectParent(typeof(EbObject))]
    public class EbDGBooleanColumn : EbDGColumn
    {
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.Boolean; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbCheckBox"; } }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
    [Alias("Date Column")]
    [UsedWithTopObjectParent(typeof(EbObject))]
    public class EbDGDateColumn : EbDGColumn
    {
        [JsonIgnore]
        public EbDate EbDate { get; set; }

        public EbDGDateColumn()
        {
            this.EbDate = new EbDate();
        }
        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            DBareHtml = EbDate.GetBareHtml();
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType
        {
            get { return this.EbDate.EbDbType; }
            set { this.EbDate.EbDbType = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        public EbDateType EbDateType
        {
            get { return this.EbDate.EbDateType; }
            set { this.EbDate.EbDateType = value; }
        }

        [JsonIgnore]
        public override string GetValueJSfn
        {
            get
            {
                return this.EbDate.GetValueJSfn;
            }
            set { }
        }

        [JsonIgnore]
        public override string SetValueJSfn
        {
            get
            {
                return this.EbDate.SetValueJSfn;
            }
            set { }
        }

        [JsonIgnore]
        public override string GetDisplayMemberJSfn
        {
            get
            {
                return this.EbDate.GetDisplayMemberJSfn;
            }
            set { }
        }

        [JsonIgnore]
        public override string OnChangeBindJSFn { get { return @"
$(`[ebsid=${p1.DG.EbSid}]`).on('change', `[colname=${this.Name}] [ui-inp]`, p2).siblings('.nullable-check').on('change', `input[type=checkbox]`, p2);"; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbDate"; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [OnChangeExec(@"
                if (this.DoNotPersist){
                        pg.HideProperty('IsNullable');
                }
                else {
                       pg.ShowProperty('IsNullable');
                }
            ")]
        public override bool DoNotPersist
        {
            get { return this.EbDate.DoNotPersist; }
            set { this.EbDate.DoNotPersist = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public bool IsNullable
        {
            get { return this.EbDate.IsNullable; }
            set { this.EbDate.IsNullable = value; }
        }


        public override bool ParameterizeControl(IDatabase DataDB, List<DbParameter> param, string tbl, SingleColumn cField, bool ins, ref int i, ref string _col, ref string _val, ref string _extqry, User usr, SingleColumn ocF)
        {
            return this.EbDate.ParameterizeControl(DataDB, param, tbl, cField, ins, ref i, ref _col, ref _val, ref _extqry, usr, ocF);
        }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
    [Alias("Dropdown Column")]
    [UsedWithTopObjectParent(typeof(EbObject))]
    public class EbDGSimpleSelectColumn : EbDGColumn
    {
        [JsonIgnore]
        public EbSimpleSelect EbSimpleSelect { get; set; }

        public EbDGSimpleSelectColumn()
        {
            this.EbSimpleSelect = new EbSimpleSelect();
        }

        [JsonIgnore]
        public override string DisableJSfn
        {
            get
            {
                return @"$('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] .ctrl-cover .dropdown-toggle`).attr('disabled', 'disabled').css('pointer-events', 'none').css('background-color', '#f3f3f3');";
            }
            set { }
        }

        [JsonIgnore]
        public override string EnableJSfn
        {
            get
            {
                return @"$('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] .ctrl-cover .dropdown-toggle`).prop('disabled',false).css('pointer-events', 'inherit').css('background-color', '#fff');";
            }
            set { }
        }

        public override string GetDisplayMemberJSfn { get { return @" return $('[ebsid='+this.__DG.EbSid+']').find(`tr[rowid=${this.__rowid}] [colname=${this.Name}] [ui-inp] :selected`).text(); "; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbSimpleSelect"; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType
        {
            get { return this.EbSimpleSelect.EbDbType; }
            set { this.EbSimpleSelect.EbDbType = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [OSE_ObjectTypes(EbObjectTypes.iDataReader)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        public string DataSourceId
        {
            get { return this.EbSimpleSelect.DataSourceId; }
            set { this.EbSimpleSelect.DataSourceId = value; }
        }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public DVColumnCollection Columns
        {
            get { return this.EbSimpleSelect.Columns; }
            set { this.EbSimpleSelect.Columns = value; }
        }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.WebForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [OnChangeExec(@"if (this.Columns && this.Columns.$values.length === 0 ){pg.MakeReadOnly('ValueMember');} else {pg.MakeReadWrite('ValueMember');}")]
        public DVBaseColumn ValueMember
        {
            get { return this.EbSimpleSelect.ValueMember; }
            set { this.EbSimpleSelect.ValueMember = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.Collection)]
        [Alias("Options")]
        public List<EbSimpleSelectOption> Options
        {
            get { return this.EbSimpleSelect.Options; }
            set { this.EbSimpleSelect.Options = value; }
        }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.WebForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [OnChangeExec(@"if (this.Columns && this.Columns.$values.length === 0 ){pg.MakeReadOnly('DisplayMember');} else {pg.MakeReadWrite('DisplayMember');}")]
        public DVBaseColumn DisplayMember
        {
            get { return this.EbSimpleSelect.DisplayMember; }
            set { this.EbSimpleSelect.DisplayMember = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public int Value
        {
            get { return this.EbSimpleSelect.Value; }
            set { this.EbSimpleSelect.Value = value; }
        }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.BotForm)]
        public override bool IsReadOnly
        {
            get { return this.EbSimpleSelect.IsReadOnly; }
            set { this.EbSimpleSelect.IsReadOnly = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.Boolean)]
        [OnChangeExec(@"if(this.IsDynamic === true){pg.ShowProperty('DataSourceId');pg.ShowProperty('ValueMember');pg.ShowProperty('DisplayMember');pg.HideProperty('Options');}
		else{pg.HideProperty('DataSourceId');pg.HideProperty('ValueMember');pg.HideProperty('DisplayMember');pg.ShowProperty('Options');}")]
        public bool IsDynamic
        {
            get { return this.EbSimpleSelect.IsDynamic; }
            set { this.EbSimpleSelect.IsDynamic = value; }
        }

        [JsonIgnore]
        public string OptionHtml
        {
            get { return this.EbSimpleSelect.OptionHtml; }
            set { this.EbSimpleSelect.OptionHtml = value; }
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            DBareHtml = EbSimpleSelect.GetBareHtml();
        }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
    [Alias("BooleanSelect Column")]
    [UsedWithTopObjectParent(typeof(EbObject))]
    public class EbDGBooleanSelectColumn : EbDGColumn
    {
        [JsonIgnore]
        public EbBooleanSelect EbBooleanSelect { get; set; }

        [JsonIgnore]
        private EbDGSimpleSelectColumn EbDGSimpleSelectColumn { set; get; }

        public override string SetValueJSfn
        {
            get
            {
                return @"if(p1 === true)
                            p1 = 'true'
                        else if(p1 === false)
                            p1 = 'false'
                       " + EbDGSimpleSelectColumn.SetValueJSfn;
            }
            set { }
        }

        public override string GetValueJSfn
        {
            get
            {
                return EbDGSimpleSelectColumn.GetValueJSfn.Replace("return val;", "val = (val ==='true'); return val;");
            }
            set { }
        }

        public EbDGBooleanSelectColumn()
        {
            this.EbBooleanSelect = new EbBooleanSelect();
            this.EbDGSimpleSelectColumn = new EbDGSimpleSelectColumn();
        }

        [JsonIgnore]
        public override string DisableJSfn
        {
            get
            {
                return EbDGSimpleSelectColumn.DisableJSfn;
            }
            set { }
        }

        [JsonIgnore]
        public override string EnableJSfn
        {
            get
            {
                return EbDGSimpleSelectColumn.EnableJSfn;
            }
            set { }
        }

        public override string GetDisplayMemberJSfn
        {
            get
            {
                return EbDGSimpleSelectColumn.GetDisplayMemberJSfn;
            }
            set { }
        }
        public override string IsRequiredOKJSfn
        {
            get
            {
                return EbDGSimpleSelectColumn.IsRequiredOKJSfn;
            }
            set { }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(52)]
        [DefaultPropValue("Yes")]
        public string TrueText
        {
            get { return this.EbBooleanSelect.TrueText; }
            set { this.EbBooleanSelect.TrueText = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(51)]
        [DefaultPropValue("No")]
        public string FalseText
        {
            get { return this.EbBooleanSelect.FalseText; }
            set { this.EbBooleanSelect.FalseText = value; }
        }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbBooleanSelect"; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType
        {
            get { return this.EbBooleanSelect.EbDbType; }
            set { this.EbBooleanSelect.EbDbType = value; }
        }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.BotForm)]
        public override bool IsReadOnly
        {
            get { return this.EbBooleanSelect.IsReadOnly; }
            set { this.EbBooleanSelect.IsReadOnly = value; }
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            DBareHtml = EbBooleanSelect.GetBareHtml();
        }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
    [UsedWithTopObjectParent(typeof(EbObject))]
    [Alias("UserControl Column")]
    public class EbDGUserControlColumn : EbDGColumn
    {
        [JsonIgnore]
        public EbUserControl EbUserControl { get; set; }

        public EbDGUserControlColumn()
        {
            this.EbUserControl = new EbUserControl();
        }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbUserControl"; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        [Alias("Controls")]
        public List<EbControl> Columns
        {
            get { return this.EbUserControl.Controls; }
            set { this.EbUserControl.Controls = value; }
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [OSE_ObjectTypes(EbObjectTypes.iUserControl)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        public override string RefId { get { return this.EbUserControl.RefId; } set { this.EbUserControl.RefId = value; } }


        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        public string ChildHtml { get; set; }

        public override string GetBareHtml()
        {
            return this.EbUserControl.GetBareHtml();
        }

        public void InitUserControl(EbUserControl ebUserControl)
        {
            this.Columns = ebUserControl.Controls;
            this.ObjType = this.ObjType;
            this.InitDBareHtml();
        }

        public void InitDBareHtml()
        {
            DBareHtml = (@"
<div  id='@ebsid@_wrap'>
    <div class='input-group' style='width:100%;'>            
        <input id='@ebsid@_inp' ui-inp data-toggle='tooltip' title='' type='text' tabindex='0' style='width:100%; data-original-title='' disabled>
        <span id='@ebsid@_showbtn' class='input-group-addon ucspan' data-toggle='modal' data-target='#@colebsid@_usercontrolmodal' style='padding: 0px;'> <button type='button' id='Date1TglBtn' class='fa  fa-ellipsis-h ucbtn' aria-hidden='true' style='padding: 6px 12px;'></button> </span>
    </div>
</div>
").Replace("@colebsid@", EbSid_CtxId).RemoveCR();
            ChildHtml = GetBareHtml();
        }
    }

    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog)]
    [Alias("PowerSelect Column")]
    [UsedWithTopObjectParent(typeof(EbObject))]
    public class EbDGPowerSelectColumn : EbDGColumn
    {

        [JsonIgnore]
        public override string SetDisplayMemberJSfn { get { return this.EbPowerSelect.SetDisplayMemberJSfn; } set { } }

        [JsonIgnore]
        private EbPowerSelect EbPowerSelect { get; set; }


        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public EbButton AddButton { set; get; }

        public EbDGPowerSelectColumn()
        {
            this.EbPowerSelect = new EbPowerSelect();
            this.AddButton = new EbButton();
        }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iWebForm)]
        public string FormRefId { get { return this.AddButton.FormRefId; } set { this.AddButton.FormRefId = value; } }

        public override string SetValueJSfn { get { return EbPowerSelect.SetValueJSfn; } set { } }


        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [PropertyPriority(98)]
        public bool IsInsertable { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyGroup("Appearance")]
        [DefaultPropValue("100")]
        public override int Width { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iDataReader)]
        public string DataSourceId { get { return this.EbPowerSelect.DataSourceId; } set { this.EbPowerSelect.DataSourceId = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [OnChangeExec(@"
            if (this.MultiSelect === true ){
                pg.MakeReadWrite('MaxLimit');   
                if (this.Required === true ){
                    if(this.MinLimit < 1){
                        pg.setSimpleProperty('MinLimit', 1);
                    }
                    pg.MakeReadWrite('MinLimit');
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                    pg.MakeReadOnly('MinLimit');                 
                }
                if(this.MaxLimit === 1)
                    pg.setSimpleProperty('MaxLimit', 0);
                    
            } 
            else {
                pg.setSimpleProperty('MaxLimit', 1);
                pg.MakeReadOnly(['MaxLimit','MinLimit']);
                if (this.Required === true ){
                    pg.setSimpleProperty('MinLimit', 1);
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                }
                if(this.MaxLimit !== 1)
                    pg.setSimpleProperty('MaxLimit', 1);
            }")]
        public bool MultiSelect { get { return this.EbPowerSelect.MultiSelect; } set { this.EbPowerSelect.MultiSelect = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        [OnChangeExec(@"
            if (this.MultiSelect === true ){
                pg.MakeReadWrite('MaxLimit');   
                if (this.Required === true ){
                    if(this.MinLimit < 1){
                        pg.setSimpleProperty('MinLimit', 1);
                    }
                    pg.MakeReadWrite('MinLimit');
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                    pg.MakeReadOnly('MinLimit');                 
                }
            } 
            else {
                pg.setSimpleProperty('MaxLimit', 1);
                pg.MakeReadOnly(['MaxLimit','MinLimit']);
                if (this.Required === true ){
                    pg.setSimpleProperty('MinLimit', 1);
                }
                else{
                    pg.setSimpleProperty('MinLimit', 0);
                }
            }")]
        public override bool Required { get { return this.EbPowerSelect.Required; } set { this.EbPowerSelect.Required = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("1")]
        [PropertyGroup("Behavior")]
        public int MaxLimit { get { return this.EbPowerSelect.MaxLimit; } set { this.EbPowerSelect.MaxLimit = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [PropertyGroup("Behavior")]
        public int MinLimit { get { return this.EbPowerSelect.MaxLimit; } set { this.EbPowerSelect.MinLimit = value; } }

        [EnableInBuilder(BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.CollectionProp, "Columns", "bVisible")]
        public DVColumnCollection Columns { get { return this.EbPowerSelect.Columns; } set { this.EbPowerSelect.Columns = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public override string Name { get { return this.EbPowerSelect.Name; } set { this.EbPowerSelect.Name = value; } }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public override string EbSid { get { return this.EbPowerSelect.EbSid; } set { this.EbPowerSelect.EbSid = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return this.EbPowerSelect.EbDbType; } set { this.EbPowerSelect.EbDbType = value; } }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        new public string EbSid_CtxId { get { return this.EbPowerSelect.EbSid_CtxId; } set { this.EbPowerSelect.EbSid_CtxId = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns")]
        public DVColumnCollection DisplayMembers { get { return this.EbPowerSelect.DisplayMembers; } set { this.EbPowerSelect.DisplayMembers = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        public int DropdownHeight { get { return this.EbPowerSelect.DropdownHeight; } set { this.EbPowerSelect.DropdownHeight = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [Alias("DropdownWidth(%)")]
        [DefaultPropValue("100")]
        public int DropdownWidth { get { return this.EbPowerSelect.DropdownWidth; } set { this.EbPowerSelect.DropdownWidth = value; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [OnChangeExec(@"if (
this.Columns.$values.length === 0 ){
pg.MakeReadOnly('ValueMember');} 
else {pg.MakeReadWrite('ValueMember');}")]
        public DVBaseColumn ValueMember { get { return this.EbPowerSelect.ValueMember; } set { this.EbPowerSelect.ValueMember = value; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override string InputControlType { get { return "EbPowerSelect"; } }

        public override string GetBareHtml()
        {
            return this.EbPowerSelect.GetBareHtml("@ebsid@"); // temp
        }

        public string GetSelectQuery(IDatabase DataDB, Service service, string Col, string Tbl = null, string _id = null)
        {
            return this.EbPowerSelect.GetSelectQuery(DataDB, service, Col, Tbl, _id);
        }

        public string GetDisplayMembersQuery(IDatabase DataDB, Service service, string vms)
        {
            return this.EbPowerSelect.GetDisplayMembersQuery(DataDB, service, vms);
        }
    }
}
