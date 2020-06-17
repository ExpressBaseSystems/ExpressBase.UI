﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Data;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.Objects.DVRelated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileControl : EbMobilePageBase
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.MultiLanguageKeySelector)]
        [UIproperty]
        [PropertyGroup("Core")]
        public virtual string Label { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
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

        [PropertyGroup("Behavior")]
        [EnableInBuilder(BuilderType.MobilePage)]
        public virtual bool Required { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public virtual string Icon { get { return string.Empty; } }

        public virtual EbControl GetWebFormCtrl(int counter) { return null; }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileTextBox : EbMobileControl
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
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

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Icon { get { return "fa-i-cursor"; } }

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
        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
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

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public NumericBoxTypes RenderType { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool IsAggragate { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Icon { get { return "fa-i-cursor"; } }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileNumericBox' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                                <input type='number' class='eb_mob_numericbox' />
                                <div class='eb_mob_numericbox-btntype'>
                                    <div class='wraper'>
                                        <button class='numeric-btn'><i class='fa fa-minus'></i></button>
                                        <div class='nemric-text'>
                                            <input type='text'/>
                                        </div>
                                        <button class='numeric-btn'><i class='fa fa-plus'></i></button>
                                    </div>
                                </div>
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbNumeric
            {
                EbSid = "Numeric" + counter,
                Name = this.Name,
                //MaxLength = this.MaxLength,
                DecimalPlaces = this.DecimalPlaces,
                MaxLimit = this.MaxLimit,
                MinLimit = this.MinLimit,
                //IsCurrency = this.IsCurrency,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileDateTime : EbMobileControl
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        public EbDateType EbDateType { get; set; } = EbDateType.DateTime;

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool IsNullable { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public TimeShowFormat ShowTimeAs_ { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public DateShowFormat ShowDateAs_ { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return (EbDbTypes)this.EbDateType; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Icon { get { return "fa-calendar"; } }

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
        public override bool Unique { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType
        {
            get
            {
                if (this.ValueMember != null)
                {
                    return this.ValueMember.Type;
                }
                else
                {
                    return EbDbTypes.String;
                }
            }
            set { }
        }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.Collection)]
        [PropertyGroup(PGConstants.DATA)]
        public List<EbMobileSSOption> Options { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iDataReader)]
        [PropertyGroup(PGConstants.DATA)]
        [OnChangeExec(@"
                if (this.DataSourceRefId !== null && this.DataSourceRefId !== ''){ 
                        pg.ShowProperty('DisplayMember');
                        pg.ShowProperty('ValueMember');
                        pg.ShowProperty('OfflineQuery');
                }
                else {
                        pg.HideProperty('DisplayMember');
                        pg.HideProperty('ValueMember');
                        pg.HideProperty('OfflineQuery');
                }
            ")]
        public string DataSourceRefId { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public List<EbMobileDataColumn> Columns { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [PropertyGroup(PGConstants.DATA)]
        public EbMobileDataColumn DisplayMember { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.CollectionFrmSrc, "Columns", 1)]
        [PropertyGroup(PGConstants.DATA)]
        public EbMobileDataColumn ValueMember { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public int MinSearchLength { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.ScriptEditorCS)]
        [HelpText("sql query to get data from offline database")]
        [PropertyGroup(PGConstants.DATA)]
        public EbScript OfflineQuery { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public List<Param> Parameters { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool MultiSelect { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Icon { get { return "fa-caret-down"; } }

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

        public override string GetJsInitFunc()
        {
            return @"
                this.Init = function(id)
                    {
                        this.MinSearchLength = 3;
                    };";
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            EbPowerSelect Ps = new EbPowerSelect
            {
                EbSid = "PowerSelect" + counter,
                Name = this.Name,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };

            foreach (EbMobileSSOption so in this.Options)
            {
                Ps.Options.Add(new EbSimpleSelectOption
                {
                    Name = so.Name,
                    Value = so.Value,
                    DisplayName = so.DisplayName
                });
            }

            Ps.DataSourceId = this.DataSourceRefId;

            if (this.DisplayMember != null)
            {
                Ps.DisplayMember = new DVBaseColumn
                {
                    Name = this.DisplayMember.Name,
                    Type = this.DisplayMember.Type,
                    sTitle = this.DisplayMember.Name,
                    Data = this.DisplayMember.ColumnIndex
                };
            }

            if (this.ValueMember != null)
            {
                Ps.ValueMember = new DVBaseColumn
                {
                    Name = this.ValueMember.Name,
                    Type = this.ValueMember.Type,
                    sTitle = this.ValueMember.Name,
                    Data = this.ValueMember.ColumnIndex
                };
            }

            return Ps;
        }

        public EbMobileSimpleSelect()
        {
            Parameters = new List<Param>();
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
    public class EbMobileFileUpload : EbMobileControl, INonPersistControl
    {
        public override bool DoNotPersist { get; set; }
        public override bool Unique { get; set; }
        public override bool ReadOnly { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public bool EnableCameraSelect { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public bool EnableFileSelect { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public bool MultiSelect { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Behavior")]
        public bool EnableEdit { set; get; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileFileUpload' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <button class='eb_mob_fupbtn filesbtn'><i class='fa fa-folder-open-o'></i></button>
                               <button class='eb_mob_fupbtn camerabtn'><i class='fa fa-camera'></i></button>
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
        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.BooleanOriginal; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Icon { get { return "fa-toggle-on"; } }

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
    public class EbMobileTableLayout : EbMobileControl, ILayoutControl
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
    public class EbMobileDataColumn : EbMobileControl, INonPersistControl
    {
        public override string Label { set; get; }

        public override bool Unique { get; set; }

        public override bool ReadOnly { get; set; }

        public override bool DoNotPersist { get; set; }

        public override bool Required { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("UI")]
        public string TextFormat { get; set; }

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
        [PropertyGroup("UI")]
        [PropertyEditor(PropertyEditorType.FontSelector)]
        public EbFont Font { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("UI")]
        public int RowSpan { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("UI")]
        public int ColumnSpan { set; get; }

        public override bool Hidden { set; get; }

        public override string GetDesignHtml()
        {
            return @"<div class='data_column mob_control dropped' title=' @ColumnName' tabindex='1' onclick='$(this).focus()' eb-type='EbMobileDataColumn' id='@id'>
                        <div class='data_column_inner'>
                            <span> @ColumnName </span>
                        </div>
                    </div>".RemoveCR().DoubleQuoted();
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileGeoLocation : EbMobileControl
    {
        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        public bool HideSearchBox { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Icon { get { return "fa-map-marker"; } }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileGeoLocation' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <div class='geoloc-ctrlwrapr'>
                                    <input type='text' style='display: @display' placeholder='Search place' class='eb_mob_textbox' />
                                    <div class='map-container'>
                                        
                                    </div>
                               </div>
                            </div>
                        </div>".Replace("@display", (this.HideSearchBox) ? "none" : "block").RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbInputGeoLocation
            {
                EbSid = "InputGeoLocation" + counter,
                Name = this.Name,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileDataGrid : EbMobileControl, ILinesEnabled
    {
        public override bool DoNotPersist { get; set; }

        public override bool Unique { get; set; }

        public override bool Required { get; set; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public List<EbMobileControl> ChildControls { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public EbMobileTableLayout DataLayout { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyGroup("Data")]
        public string TableName { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iDataReader)]
        [Alias("Data Source")]
        [PropertyGroup("Data")]
        public string DataSourceRefId { set; get; }

        [EnableInBuilder(BuilderType.MobilePage)]
        [PropertyEditor(PropertyEditorType.ScriptEditorCS)]
        [HelpText("sql query to get data from offline database")]
        [Alias("Offline Query")]
        [PropertyGroup("Data")]
        public EbScript OfflineQuery { set; get; }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileDataGrid' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml ctrl_as_container'>
                               <div class='data_layout'></div>
                               <div class='control_container'></div>
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public EbDGColumn GetGridControl(EbMobileControl ctrl, int counter)
        {
            EbDGColumn dgColumn = null;
            switch (ctrl)
            {
                case EbMobileTextBox tx:
                    dgColumn = new EbDGStringColumn();
                    break;
                case EbMobileNumericBox txn:
                    dgColumn = new EbDGNumericColumn();
                    break;
                case EbMobileBoolean bl:
                    dgColumn = new EbDGBooleanColumn();
                    break;
                case EbMobileDateTime dt:
                    dgColumn = new EbDGDateColumn
                    {
                        EbDateType = dt.EbDateType
                    };
                    break;
                case EbMobileSimpleSelect ss:
                    if (string.IsNullOrEmpty(ss.DataSourceRefId))
                    {
                        dgColumn = new EbDGSimpleSelectColumn
                        {
                            Options = ss.Options.Select(item => new EbSimpleSelectOption
                            {
                                EbSid = item.EbSid,
                                Name = item.Name,
                                Value = item.Value,
                                DisplayName = item.DisplayName
                            }).ToList()
                        };
                    }
                    else
                    {
                        dgColumn = new EbDGPowerSelectColumn
                        {
                            DataSourceId = ss.DataSourceRefId,
                            ValueMember = new DVBaseColumn
                            {
                                Name = ss.ValueMember.Name,
                                Type = ss.ValueMember.Type,
                                sTitle = ss.ValueMember.Name,
                                Data = ss.ValueMember.ColumnIndex
                            },
                            DisplayMember = new DVBaseColumn
                            {
                                Name = ss.DisplayMember.Name,
                                Type = ss.DisplayMember.Type,
                                sTitle = ss.DisplayMember.Name,
                                Data = ss.DisplayMember.ColumnIndex
                            }
                        };
                    }
                    break;
                default:
                    dgColumn = new EbDGStringColumn();
                    break;
            }
            dgColumn.Title = ctrl.Label;
            dgColumn.Name = ctrl.Name;
            dgColumn.EbSid = ctrl.GetType().Name + counter;
            return dgColumn;
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            EbDataGrid dg = new EbDataGrid
            {
                EbSid = "DataGrid" + counter,
                Name = this.Name,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label,
                TableName = this.TableName
            };

            foreach (EbMobileControl ctrl in this.ChildControls)
                dg.Controls.Add(this.GetGridControl(ctrl, counter++));

            return dg;
        }
    }

    [EnableInBuilder(BuilderType.MobilePage)]
    public class EbMobileAutoId : EbMobileControl
    {
        public override bool DoNotPersist { get; set; }

        public override bool Unique { get; set; }

        public override bool Required { get; set; }

        public override bool ReadOnly { get { return true; } }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        [EnableInBuilder(BuilderType.MobilePage)]
        [HideInPropertyGrid]
        public override string Icon { get { return "fa-key"; } }

        public override string GetDesignHtml()
        {
            return @"<div class='eb_stacklayout mob_control dropped' id='@id' eb-type='EbMobileAutoId' tabindex='1' onclick='$(this).focus()'>
                            <label class='ctrl_label'> @Label </label>
                            <div class='eb_ctrlhtml'>
                               <input type='text' class='eb_mob_autoid' />
                            </div>
                        </div>".RemoveCR().DoubleQuoted();
        }

        public override EbControl GetWebFormCtrl(int counter)
        {
            return new EbAutoId
            {
                EbSid = "AutoId" + counter,
                Name = this.Name,
                Margin = new UISides { Top = 0, Bottom = 0, Left = 0, Right = 0 },
                Label = this.Label
            };
        }
    }
}
