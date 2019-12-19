﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileControl : EbMobilePageBase
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.MultiLanguageKeySelector)]
        [UIproperty]
        public virtual string Label { set; get; }

        [HideInPropertyGrid]
        public virtual EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public virtual bool Hidden { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HelpText("Set true if want unique value for this control on every form save.")]
        [PropertyGroup("Behavior")]
        public virtual bool Unique { get; set; }

        [PropertyGroup("Behavior")]
        [EnableInBuilder(BuilderType.MobilePage)]
        [HelpText("Set true if you want to make this control read only.")]
        public virtual bool ReadOnly { get; set; }

        [PropertyGroup("Behavior")]
        [EnableInBuilder(BuilderType.MobilePage)]
        [HelpText("Set true if you dont want to save value from this field.")]
        public virtual bool DoNotPersist { get; set; }

        public virtual EbControl GetWebFormCtrl(int counter) { return null; }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileTextBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        public int MaxLength { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public TextTransform TextTransform { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public TextMode TextMode { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public bool AutoCompleteOff { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public bool AutoSuggestion { get; set; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileTextBox' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <input type='text' class='eb_mob_textbox' />
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbTextBox
            {
                EbSid = "TextBox" + counter,
                Name = this.Name,
                MaxLength = this.MaxLength,
                TextTransform = this.TextTransform,
                TextMode = this.TextMode,
                AutoCompleteOff = this.AutoCompleteOff,
                AutoSuggestion = this.AutoSuggestion,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileNumericBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        public int MaxLength { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [DefaultPropValue("2")]
        [HelpText("Number of decimal places")]
        public int DecimalPlaces { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [Alias("Maximum")]
        [HelpText("Maximum value allowed")]
        public int MaxLimit { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [Alias("Minimum")]
        [HelpText("Minimum value allowed")]
        public int MinLimit { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool IsCurrency { get; set; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileNumericBox' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <input type='number' class='eb_mob_textbox' />
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbNumeric
            {
                EbSid = "Numeric" + counter,
                Name = this.Name,
                MaxLength = this.MaxLength,
                DecimalPlaces = this.DecimalPlaces,
                MaxLimit = this.MaxLimit,
                MinLimit = this.MinLimit,
                IsCurrency = this.IsCurrency,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileDateTime : EbMobileControl
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        public EbDateType EbDateType { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool IsNullable { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public TimeShowFormat ShowTimeAs_ { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public DateShowFormat ShowDateAs_ { get; set; }

        public override EbDbTypes EbDbType { get { return (EbDbTypes)this.EbDateType; } set { } }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileDateTime' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <input type='text' class='eb_mob_textbox' placeholder='YYYY-MM-DD'/>
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbDate
            {
                EbSid = "Date" + counter,
                Name = this.Name,
                IsNullable = this.IsNullable,
                EbDateType = this.EbDateType,
                ShowTimeAs_ = this.ShowTimeAs_,
                ShowDateAs_ = this.ShowDateAs_,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileSimpleSelect : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbMobileSSOption> Options { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool IsMultiSelect { get; set; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileSimpleSelect' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <select class='eb_mob_select'>
                                    <option>--select--</option>
                                </select>
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbSimpleSelect
            {
                EbSid = "SimpleSelect" + counter,
                Name = this.Name,
                IsMultiSelect = this.IsMultiSelect,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileSSOption : EbMobilePageBase
    {
        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.MobilePage)]
        public string EbSid { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public override string DisplayName { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public string Value { set; get; }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileFileUpload : EbMobileControl
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        public bool EnableCameraSelect { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool EnableFileSelect { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool MultiSelect { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool EnableEdit { set; get; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileFileUpload' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <button class='eb_mob_fupbtn filesbtn'> Files </button>
                               <button class='eb_mob_fupbtn camerabtn'> Camera </button>
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override string GetJsInitFunc()
        {
            return @"
                this.Init = function(id)
                {
                    this.EnableCameraSelect = true;
                    this.EnableFileSelect= true;
                    this.MultiSelect= true;
                    this.EnableEdit= true;
                };";
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbFileUploader
            {
                EbSid = "FileUploader" + counter,
                Name = this.Name,
                IsMultipleUpload = this.MultiSelect,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileBoolean : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.BooleanOriginal; } set { } }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileBoolean' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <input type='checkbox' class='eb_mob_checkbox'/>
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbBooleanSelect
            {
                EbSid = "BooleanSelect" + counter,
                Name = this.Name,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileTableLayout : EbMobileControl
    {
        public EbMobileTableLayout()
        {
            this.CellCollection = new List<EbMobileTableCell>();
        }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public int RowCount { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public int ColumCount { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public List<EbMobileTableCell> CellCollection { set; get; }

        public override bool Hidden { set; get; }

        public override bool Unique { get; set; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_mob_tablelayout mob_control dropped' eb-type='EbMobileTableLayout' id='@id'>
                        <div class='eb_mob_tablelayout_inner'>
                            
                        </div>
                    </div>".RemoveCR().DoubleQuoted();
        }

        public override string GetJsInitFunc()
        {
            return @"
                this.Init = function(id)
                {
                    this.RowCount = 2;
                    this.ColumCount= 2;
                };";
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbTableLayout
            {
                EbSid = "TableLayout" + counter,
                Name = this.Name,
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileTableCell : EbMobilePageBase
    {
        public EbMobileTableCell()
        {
            this.ControlCollection = new List<EbMobileControl>();
        }

        [EnableInBuilder(BuilderType.MobilePage)]
        public int RowIndex { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public int ColIndex { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public int Width { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public List<EbMobileControl> ControlCollection { set; get; }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileDataColumn : EbMobileControl
    {
        public override string Label { set; get; }

        public override bool Unique { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public int TableIndex { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public int ColumnIndex { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [UIproperty]
        [MetaOnly]
        public string ColumnName { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public EbDbTypes Type { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [UIproperty]
        [PropertyEditor(PropertyEditorType.FontSelector)]
        public virtual EbFont Font { get; set; }

        public override bool Hidden { set; get; }

        public override string GetDesignHtml()
        {
            return @"<div class='data_column mob_control dropped' tabindex='1' onclick='$(this).focus()' eb-type='EbMobileDataColumn' id='@id'>
                        <div class='data_column_inner'>
                            <span> @ColumnName </span>
                        </div>
                    </div>".RemoveCR().DoubleQuoted();
        }
    }
}
