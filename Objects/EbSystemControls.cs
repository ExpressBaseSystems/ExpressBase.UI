﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.LocationNSolution;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace ExpressBase.Objects
{
    public enum EbSysLocDM//DisplayMember
    {
        LocationId = 1,
        ShortName,
        LongName
    }

   
    [EnableInBuilder(BuilderType.WebForm)]
    public class EbSysLocation : EbControlUI, IEbPlaceHolderControl
    {
        public EbSysLocation() { }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            //this.Name = "eb_loc_id";
        }
        
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        [DefaultPropValue("eb_loc_id")]
        public override string Name { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public override EbScript ValueExpr { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl)]
        public override EbScript DefaultValueExpression { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } }

        [EnableInBuilder(BuilderType.WebForm)]
        public EbSysLocDM DisplayMember { get; set; }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-plus'></i><i class='fa fa-map-marker'></i>"; } set { } }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolNameAlias { get { return "Created From"; } set { } }

        //public override string GetToolHtml()
        //{
        //    return @"<div eb-type='@toolName' class='tool'><i class='fa fa-desktop'></i> Created From </div>".Replace("@toolName", this.GetType().Name.Substring(2));
        //}

        public override string GetHtml()
        {
            return GetHtmlHelper();
        }

        private string GetHtmlHelper()
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
               .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
               .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }

        public override string GetDesignHtml()
        {
            string _html = null;

            if (this.Name == null) //if in new mode
                _html = GetHtml();

            else //if edit mode
                _html = Wrap4Developer(GetHtml());

            return _html.RemoveCR().GraveAccentQuoted();
        }

        public string Wrap4Developer(string EbCtrlHTML)
        {
            return @"<div class='controlTile' tabindex='1' onclick='event.stopPropagation();$(this).focus()'>
                                <div class='ctrlHead' style='display:none;'>
                                    <i class='fa fa-arrows moveBtn' aria-hidden='true'></i>
                                    <a href='#' class='close' style='cursor:default' data-dismiss='alert' aria-label='close' title='close'>×</a>
                                </div>"
                                    + EbCtrlHTML
                        + "</div>";
        }

        public override string GetBareHtml()
        {
            string htmlinput = "<input id='@ebsid@' data-ebtype='@data-ebtype@'  data-toggle='tooltip' title='@toolTipText@' class='date' type='text' name='@name@' autocomplete = 'off' @value@ @tabIndex@ style='width:100%; @BackColor@ @ForeColor@ display:inline-block; @fontStyle@' @required@ @placeHolder@ disabled />";
            string htmlselect = @"<select id='@ebsid@' ui-inp class='' title='@toolTipText@' @selOpts@ @MaxLimit@ @multiple@ @IsSearchable@ name='@ebsid@' @bootStrapStyle@ data-ebtype='@data-ebtype@' style='width: 100%;'>
                                 </select>";
            return (this.IsDisable ? htmlinput : htmlselect)
.Replace("@name@", (this.Name != null ? this.Name.Trim() : ""))
.Replace("@data-ebtype@", "16")//( (int)this.EbDateType ).ToString())
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@value@", "")//"value='" + this.Value + "'")
.Replace("@tabIndex@", "tabindex='" + this.TabIndex + "'")
.Replace("@BackColor@ ", "background-color: #eee;")
    //.Replace("@BackColor@ ", ("background-color:" + ((this.BackColor != null) ? this.BackColor : "@BackColor@ ") + ";"))
    .Replace("@ForeColor@ ", "color:" + ((this.ForeColor != null) ? this.ForeColor : "@ForeColor@ ") + ";")
.Replace("@required@", (this.Required && !this.Hidden ? " required" : string.Empty))
.Replace("@placeHolder@", "placeholder=''");
        }


        public override string EnableJSfn 
        {
            get 
            {
                if (this.IsDisable)
                {
                    return @"";
                }
                else
                {
                    return @"$('#cont_' + this.EbSid_CtxId + ' *').prop('disabled',false).css('pointer-events', 'inherit').find('[ui-inp]').css('background-color', '#fff');";
                }
            } 
            set { } 
        }

        public override string GetValueFromDOMJSfn
        { get 
            { 
                return @"
                if(this.IsDisable)
                    return $('#' + this.EbSid_CtxId).attr('data-id');
                else
                    return $('#' + this.EbSid_CtxId).val();
                "; 
            } 
            set { } 
        }

        public override string SetValueJSfn
        {
            get
            {
                return @"
                    if(this.IsDisable){
                        if(!p1)
                            return false;                        
                        let arr = p1.split('$$');
                        $('#' + this.EbSid_CtxId).attr('data-id', arr[0]);
                        $('#' + this.EbSid_CtxId).val(arr[1]).trigger('change');
                    }
                    else
                        $('#' + this.EbSid_CtxId).val(p1).trigger('change');
                    ";
            }
            set { }
        }

        public override string RefreshJSfn { get { return @""; } set { } }

        public override string ClearJSfn { get { return @""; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool IsSysControl { get { return true; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool DoNotPersist 
        {
            get 
            {
                if (this.IsDisable == true)
                    return true;
                else
                    return false;
            } 
        }

        [EnableInBuilder(BuilderType.WebForm)]
        [DefaultPropValue("true")]
        public override bool IsDisable { get; set; }


        public override bool Unique { get { return false; } }
        public override bool Required { get { return false; } }
        public override EbScript _OnChange { get => base._OnChange; set => base._OnChange = value; }
        public override EbScript OnChangeFn { get => base.OnChangeFn; set => base.OnChangeFn = value; }
        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override List<EbValidator> Validators { get => base.Validators; set => base.Validators = value; }
        public override EbScript HiddenExpr { get => base.HiddenExpr; set => base.HiddenExpr = value; }
        public override EbScript DisableExpr { get => base.DisableExpr; set => base.DisableExpr = value; }


        public override SingleColumn GetSingleColumn(User UserObj, Eb_Solution SoluObj, object Value)
        {
            object _formattedData = Value;
            string _displayMember = string.Empty;
            int loc_id = UserObj.Preference.DefaultLocation;
            if (Value != null)
                loc_id = Convert.ToInt32(Value);

            if (this.IsDisable)
            {
                EbSysLocDM dm = this.DisplayMember;
                if (SoluObj.Locations.ContainsKey(loc_id))
                {
                    if (dm == EbSysLocDM.LongName)
                    {
                        _formattedData = loc_id + "$$" + SoluObj.Locations[loc_id].LongName;
                        _displayMember = SoluObj.Locations[loc_id].LongName;
                    }
                    else
                    {
                        _formattedData = loc_id + "$$" + SoluObj.Locations[loc_id].ShortName;
                        _displayMember = SoluObj.Locations[loc_id].ShortName;
                    }
                }
            }
            else
                _formattedData = loc_id;

            return new SingleColumn()
            {
                Name = this.Name,
                Type = (int)this.EbDbType,
                Value = _formattedData,
                Control = this,
                ObjType = this.ObjType,
                F = _displayMember
            };
        }
    }

    [EnableInBuilder(BuilderType.WebForm)]
    public class EbSysCreatedBy : EbControlUI, IEbPlaceHolderControl
    {
        public EbSysCreatedBy() { }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            this.Name = "eb_created_by";
        }

        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-plus'></i><i class='fa fa-user'></i>"; } set { } }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolNameAlias { get { return "Created By"; } set { } }

        //public override string GetToolHtml()
        //{
        //    return @"<div eb-type='@toolName' class='tool'><i class='fa fa-desktop'></i>  Created By </div>".Replace("@toolName", this.GetType().Name.Substring(2));
        //}

        public override string GetHtml()
        {
            return GetHtmlHelper();
        }

        private string GetHtmlHelper()
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
               .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
               .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }

        public override string GetDesignHtml()
        {
            string _html = null;

            if (this.Name == null) //if in new mode
                _html = GetHtml();

            else //if edit mode
                _html = Wrap4Developer(GetHtml());

            return _html.RemoveCR().GraveAccentQuoted();
        }

        public string Wrap4Developer(string EbCtrlHTML)
        {
            return @"<div class='controlTile' tabindex='1' onclick='event.stopPropagation();$(this).focus()'>
                                <div class='ctrlHead' style='display:none;'>
                                    <i class='fa fa-arrows moveBtn' aria-hidden='true'></i>
                                    <a href='#' class='close' style='cursor:default' data-dismiss='alert' aria-label='close' title='close'>×</a>
                                </div>"
                                    + EbCtrlHTML
                        + "</div>";
        }
		
		public override string GetBareHtml()
        {
            return @"
						<div style='display: flex;'>
							<img id='@ebsid@_usrimg'class='sysctrl_usrimg' src='' alt='' onerror=this.onerror=null;this.src='/images/nulldp.png';>
							<div id='@ebsid@' data-ebtype='@data-ebtype@'  data-toggle='tooltip' title='@toolTipText@' class=' sysctrl_usrname'  name='@name@' autocomplete = 'off' @value@ @tabIndex@ style='width:100%; @BackColor@ @ForeColor@ display:inline-block; @fontStyle@' @required@ @placeHolder@ disabled ></div>
						
						</div>
            "
.Replace("@name@", (this.Name != null ? this.Name.Trim() : ""))
.Replace("@data-ebtype@", "16")//( (int)this.EbDateType ).ToString())
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@value@", "")//"value='" + this.Value + "'")
.Replace("@tabIndex@", "tabindex='" + this.TabIndex + "'")
.Replace("@BackColor@ ", "background-color: #eee;")
    //.Replace("@BackColor@ ", ("background-color:" + ((this.BackColor != null) ? this.BackColor : "@BackColor@ ") + ";"))
    .Replace("@ForeColor@ ", "color:" + ((this.ForeColor != null) ? this.ForeColor : "@ForeColor@ ") + ";")
.Replace("@required@", (this.Required && !this.Hidden ? " required" : string.Empty))
.Replace("@placeHolder@", "placeholder=''");
        }

        public override string EnableJSfn { get { return @""; } set { } }

        public override string GetValueFromDOMJSfn { get { return @"return $('#' + this.EbSid_CtxId).attr('data-id');"; } set { } }

        public override string SetValueJSfn
        {
            get
            {
                return @"
                        if(!p1)
                            return false;
                        let arr = p1.split('$$');
                        $('#' + this.EbSid_CtxId).attr('data-id', arr[0]);
                        $('#' + this.EbSid_CtxId).text(arr[1]).trigger('change');
						let imgsrc='/images/dp/'+ arr[0] +'.png';
						$('#' + this.EbSid_CtxId + '_usrimg').attr('src',imgsrc );";
			}
            set { }
        }

		public override string GetDisplayMemberFromDOMJSfn
        {
			get
			{
				return @"return $('#' + this.EbSid_CtxId).text();";
			}
			set { }
		}

        public override string RefreshJSfn { get { return @""; } set { } }

        public override string ClearJSfn { get { return @""; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool IsSysControl { get { return true; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool DoNotPersist { get { return true; } }

        public override bool Unique { get { return false; } }
        public override bool Required { get { return false; } }
        public override EbScript _OnChange { get => base._OnChange; set => base._OnChange = value; }
        public override EbScript OnChangeFn { get => base.OnChangeFn; set => base.OnChangeFn = value; }
        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override List<EbValidator> Validators { get => base.Validators; set => base.Validators = value; }
        public override EbScript ValueExpr { get => base.ValueExpr; set => base.ValueExpr = value; }
        public override EbScript HiddenExpr { get => base.HiddenExpr; set => base.HiddenExpr = value; }
        public override EbScript DisableExpr { get => base.DisableExpr; set => base.DisableExpr = value; }
        public override bool IsDisable { get => base.IsDisable; set => base.IsDisable = value; }

        //EbSysCreatedBy
        public override SingleColumn GetSingleColumn(User UserObj, Eb_Solution SoluObj, object Value)
        {
            return EbSysCreatedBy.GetSingleColumn(this, UserObj, SoluObj, Value);
        }

        public static SingleColumn GetSingleColumn(EbControl _this, User UserObj, Eb_Solution SoluObj, object Value)
        {
            object _formattedData = null;
            string _displayMember = string.Empty;
            int user_id = UserObj.UserId;
            if (Value != null)
            {
                int.TryParse(Value.ToString(), out user_id);
            }
            if (user_id == UserObj.UserId)
            {
                _formattedData = UserObj.UserId + "$$" + UserObj.FullName;
                _displayMember = UserObj.FullName;
            }
            else if (SoluObj.Users != null && SoluObj.Users.ContainsKey(user_id))
            {
                _formattedData = user_id + "$$" + SoluObj.Users[user_id];
                _displayMember = SoluObj.Users[user_id];
            }
            else
            {
                _formattedData = user_id + "$$No Name";
                _displayMember = "No Name";
            }

            return new SingleColumn()
            {
                Name = _this.Name,
                Type = (int)_this.EbDbType,
                Value = _formattedData,
                Control = _this,
                ObjType = _this.ObjType,
                F = _displayMember
            };
        }
    }

    [EnableInBuilder(BuilderType.WebForm)]
    public class EbSysCreatedAt : EbControlUI, IEbPlaceHolderControl
    {
        public EbSysCreatedAt()
        {
            this.EbDate = new EbDate();
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            this.Name = "eb_created_at";
        }

        public EbDate EbDate { get; set; }

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

        public override string GetValueFromDOMJSfn
        {
            get
            {
                return this.EbDate.GetValueFromDOMJSfn;
            }
            set { }
        }

        public override string SetValueJSfn
        {
            get
            {
                return this.EbDate.SetValueJSfn;
            }
            set { }
        }

        //public override string JustSetValueJSfn
        //{
        //    get
        //    {
        //        return this.EbDate.JustSetValueJSfn;
        //    }
        //    set { }
        //}

        public override string GetDisplayMemberFromDOMJSfn
        {
            get
            {
                return this.EbDate.GetDisplayMemberFromDOMJSfn;
            }
            set { }
        }

        public override string RefreshJSfn
        {
            get
            {
                return this.EbDate.RefreshJSfn;
            }
            set { }
        }

        public override string ClearJSfn
        {
            get
            {
                return this.EbDate.ClearJSfn;
            }
            set { }
        }


        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-plus'></i><i class='fa fa-clock-o'></i>"; } set { } }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolNameAlias { get { return " Created At"; } set { } }

        //public override string GetToolHtml()
        //{
        //    return @"<div eb-type='@toolName' class='tool'><i class='fa fa-desktop'></i> Created At </div>".Replace("@toolName", this.GetType().Name.Substring(2));
        //}

        public override string GetHtml()
        {
            return GetHtmlHelper();
        }

        private string GetHtmlHelper()
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
               .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
               .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }

        public override string GetDesignHtml()
        {
            string _html = null;

            if (this.Name == null) //if in new mode
                _html = GetHtml();

            else //if edit mode
                _html = Wrap4Developer(GetHtml());

            return _html.RemoveCR().GraveAccentQuoted();
        }

        public string Wrap4Developer(string EbCtrlHTML)
        {
            return @"<div class='controlTile' tabindex='1' onclick='event.stopPropagation();$(this).focus()'>
                                <div class='ctrlHead' style='display:none;'>
                                    <i class='fa fa-arrows moveBtn' aria-hidden='true'></i>
                                    <a href='#' class='close' style='cursor:default' data-dismiss='alert' aria-label='close' title='close'>×</a>
                                </div>"
                                    + EbCtrlHTML
                        + "</div>";
        }

        public override string GetBareHtml()
        {
            return @"
            <input id='@ebsid@' data-ebtype='@data-ebtype@'  data-toggle='tooltip' title='@toolTipText@' class='date' type='text' name='@name@' autocomplete = 'off' @value@ @tabIndex@ style='width:100%; @BackColor@ @ForeColor@ display:inline-block; @fontStyle@' @required@ @placeHolder@ disabled />
            "
.Replace("@name@", (this.Name != null ? this.Name.Trim() : ""))
.Replace("@data-ebtype@", "16")//( (int)this.EbDateType ).ToString())
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@value@", "")//"value='" + this.Value + "'")
.Replace("@tabIndex@", "tabindex='" + this.TabIndex + "'")
.Replace("@BackColor@ ", "background-color: #eee;")
    //.Replace("@BackColor@ ", ("background-color:" + ((this.BackColor != null) ? this.BackColor : "@BackColor@ ") + ";"))
    .Replace("@ForeColor@ ", "color:" + ((this.ForeColor != null) ? this.ForeColor : "@ForeColor@ ") + ";")
.Replace("@required@", (this.Required && !this.Hidden ? " required" : string.Empty))
.Replace("@placeHolder@", "placeholder=''");
        }

        public override string EnableJSfn { get { return @""; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool IsSysControl { get { return true; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool DoNotPersist { get { return true; } }

        public override bool Unique { get { return false; } }
        public override bool Required { get { return false; } }
        public override EbScript _OnChange { get => base._OnChange; set => base._OnChange = value; }
        public override EbScript OnChangeFn { get => base.OnChangeFn; set => base.OnChangeFn = value; }
        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override List<EbValidator> Validators { get => base.Validators; set => base.Validators = value; }
        public override EbScript ValueExpr { get => base.ValueExpr; set => base.ValueExpr = value; }
        public override EbScript HiddenExpr { get => base.HiddenExpr; set => base.HiddenExpr = value; }
        public override EbScript DisableExpr { get => base.DisableExpr; set => base.DisableExpr = value; }
        public override bool IsDisable { get => base.IsDisable; set => base.IsDisable = value; }

        //EbSysCreatedAt
        public override SingleColumn GetSingleColumn(User UserObj, Eb_Solution SoluObj, object Value)
        {
            return EbDate.GetSingleColumn(this, UserObj, SoluObj, Value);
        }
    }

    [EnableInBuilder(BuilderType.WebForm)]
    public class EbSysModifiedBy : EbControlUI, IEbPlaceHolderControl
    {
        public EbSysModifiedBy() { }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            this.Name = "eb_lastmodified_by";
        }

        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } }

        //[EnableInBuilder(BuilderType.WebForm)]
        //public EbSysCreatedByDM DisplayMember { get; set; }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-pencil'></i><i class='fa fa-user' aria-hidden='true'></i>"; } set { } }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolNameAlias { get { return "Modified By"; } set { } }


        //public override string GetToolHtml()
        //{
        //    return @"<div eb-type='@toolName' class='tool'><i class='fa fa-desktop'></i> Modified By </div>".Replace("@toolName", this.GetType().Name.Substring(2));
        //}

        public override string GetHtml()
        {
            return GetHtmlHelper();
        }

        private string GetHtmlHelper()
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
               .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
               .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }

        public override string GetDesignHtml()
        {
            string _html = null;

            if (this.Name == null) //if in new mode
                _html = GetHtml();

            else //if edit mode
                _html = Wrap4Developer(GetHtml());

            return _html.RemoveCR().GraveAccentQuoted();
        }

        public string Wrap4Developer(string EbCtrlHTML)
        {
            return @"<div class='controlTile' tabindex='1' onclick='event.stopPropagation();$(this).focus()'>
                                <div class='ctrlHead' style='display:none;'>
                                    <i class='fa fa-arrows moveBtn' aria-hidden='true'></i>
                                    <a href='#' class='close' style='cursor:default' data-dismiss='alert' aria-label='close' title='close'>×</a>
                                </div>"
                                    + EbCtrlHTML
                        + "</div>";
        }

        public override string GetBareHtml()
        {
            return @"<div style='display: flex;'>
							<img id='@ebsid@_usrimg'class='sysctrl_usrimg' src='' alt='' onerror=this.onerror=null;this.src='/images/nulldp.png';>
							<div id='@ebsid@' data-ebtype='@data-ebtype@'  data-toggle='tooltip' title='@toolTipText@' class=' sysctrl_usrname'  name='@name@' autocomplete = 'off' @value@ @tabIndex@ style='width:100%; @BackColor@ @ForeColor@ display:inline-block; @fontStyle@' @required@ @placeHolder@ disabled ></div>
						
						</div>
            "
.Replace("@name@", (this.Name != null ? this.Name.Trim() : ""))
.Replace("@data-ebtype@", "16")//( (int)this.EbDateType ).ToString())
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@value@", "")//"value='" + this.Value + "'")
.Replace("@tabIndex@", "tabindex='" + this.TabIndex + "'")
.Replace("@BackColor@ ", "background-color: #eee;")
    //.Replace("@BackColor@ ", ("background-color:" + ((this.BackColor != null) ? this.BackColor : "@BackColor@ ") + ";"))
    .Replace("@ForeColor@ ", "color:" + ((this.ForeColor != null) ? this.ForeColor : "@ForeColor@ ") + ";")
.Replace("@required@", (this.Required && !this.Hidden ? " required" : string.Empty))
.Replace("@placeHolder@", "placeholder=''");
        }

        public override string EnableJSfn { get { return @""; } set { } }

        public override string GetValueFromDOMJSfn { get { return @"return $('#' + this.EbSid_CtxId).attr('data-id');"; } set { } }

        public override string SetValueJSfn
        {
            get
            {
                return @"
                        if(!p1)
                            return false;
                        let arr = p1.split('$$');
                        $('#' + this.EbSid_CtxId).attr('data-id', arr[0]);
                        $('#' + this.EbSid_CtxId).text(arr[1]).trigger('change');
						let imgsrc='/images/dp/'+ arr[0] +'.png';
						$('#' + this.EbSid_CtxId + '_usrimg').attr('src',imgsrc );";
            }
            set { }
        }

		public override string GetDisplayMemberFromDOMJSfn
        {
			get
			{
				return @"return $('#' + this.EbSid_CtxId).text();";
			}
			set { }
		}

        public override string RefreshJSfn { get { return @""; } set { } }

        public override string ClearJSfn { get { return @""; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool IsSysControl { get { return true; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool DoNotPersist { get { return true; } }

        public override bool Unique { get { return false; } }
        public override bool Required { get { return false; } }
        public override EbScript _OnChange { get => base._OnChange; set => base._OnChange = value; }
        public override EbScript OnChangeFn { get => base.OnChangeFn; set => base.OnChangeFn = value; }
        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override List<EbValidator> Validators { get => base.Validators; set => base.Validators = value; }
        public override EbScript ValueExpr { get => base.ValueExpr; set => base.ValueExpr = value; }
        public override EbScript HiddenExpr { get => base.HiddenExpr; set => base.HiddenExpr = value; }
        public override EbScript DisableExpr { get => base.DisableExpr; set => base.DisableExpr = value; }
        public override bool IsDisable { get => base.IsDisable; set => base.IsDisable = value; }

        //EbSysModifiedBy
        public override SingleColumn GetSingleColumn(User UserObj, Eb_Solution SoluObj, object Value)
        {
            return EbSysCreatedBy.GetSingleColumn(this, UserObj, SoluObj, Value);
        }
    }

    [EnableInBuilder(BuilderType.WebForm)]
    public class EbSysModifiedAt : EbControlUI, IEbPlaceHolderControl
    {
        public EbSysModifiedAt()
        {
            this.EbDate = new EbDate();
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
            this.Name = "eb_lastmodified_at";
        }

        public EbDate EbDate { get; set; }

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

        public override string GetValueFromDOMJSfn
        {
            get
            {
                return this.EbDate.GetValueFromDOMJSfn;
            }
            set { }
        }

        public override string SetValueJSfn
        {
            get
            {
                return this.EbDate.SetValueJSfn;
            }
            set { }
        }

        public override string GetDisplayMemberFromDOMJSfn
        {
            get
            {
                return this.EbDate.GetDisplayMemberFromDOMJSfn;
            }
            set { }
        }
        
        public override string RefreshJSfn
        {
            get
            {
                return this.EbDate.RefreshJSfn;
            }
            set { }
        }

        public override string ClearJSfn
        {
            get
            {
                return this.EbDate.ClearJSfn;
            }
            set { }
        }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolIconHtml { get { return "<i class='fa fa-pencil'></i><i class='fa fa-clock-o'></i>"; } set { } }

        [HideInPropertyGrid]
        [JsonIgnore]
        public override string ToolNameAlias { get { return " Modified At"; } set { } }


        //public override string GetToolHtml()
        //{
        //    return @"<div eb-type='@toolName' class='tool'><i class='fa fa-desktop'></i>  Modified At</div>".Replace("@toolName", this.GetType().Name.Substring(2));
        //}

        public override string GetHtml()
        {
            return GetHtmlHelper();
        }

        private string GetHtmlHelper()
        {
            string EbCtrlHTML = HtmlConstants.CONTROL_WRAPER_HTML4WEB
               .Replace("@LabelForeColor ", "color:" + (LabelForeColor ?? "@LabelForeColor ") + ";")
               .Replace("@LabelBackColor ", "background-color:" + (LabelBackColor ?? "@LabelBackColor ") + ";");

            return ReplacePropsInHTML(EbCtrlHTML);
        }

        public override string GetDesignHtml()
        {
            string _html = null;

            if (this.Name == null) //if in new mode
                _html = GetHtml();

            else //if edit mode
                _html = Wrap4Developer(GetHtml());

            return _html.RemoveCR().GraveAccentQuoted();
        }

        public string Wrap4Developer(string EbCtrlHTML)
        {
            return @"<div class='controlTile' tabindex='1' onclick='event.stopPropagation();$(this).focus()'>
                                <div class='ctrlHead' style='display:none;'>
                                    <i class='fa fa-arrows moveBtn' aria-hidden='true'></i>
                                    <a href='#' class='close' style='cursor:default' data-dismiss='alert' aria-label='close' title='close'>×</a>
                                </div>"
                                    + EbCtrlHTML
                        + "</div>";
        }

        public override string GetBareHtml()
        {
            return @"
            <input id='@ebsid@' data-ebtype='@data-ebtype@'  data-toggle='tooltip' title='@toolTipText@' class='date' type='text' name='@name@' autocomplete = 'off' @value@ @tabIndex@ style='width:100%; @BackColor@ @ForeColor@ display:inline-block; @fontStyle@' @required@ @placeHolder@ disabled />
            "
.Replace("@name@", (this.Name != null ? this.Name.Trim() : ""))
.Replace("@data-ebtype@", "16")//( (int)this.EbDateType ).ToString())
.Replace("@toolTipText@", this.ToolTipText)
.Replace("@ebsid@", String.IsNullOrEmpty(this.EbSid_CtxId) ? "@ebsid@" : this.EbSid_CtxId)
.Replace("@value@", "")//"value='" + this.Value + "'")
.Replace("@tabIndex@", "tabindex='" + this.TabIndex + "'")
.Replace("@BackColor@ ", "background-color: #eee;")
    //.Replace("@BackColor@ ", ("background-color:" + ((this.BackColor != null) ? this.BackColor : "@BackColor@ ") + ";"))
    .Replace("@ForeColor@ ", "color:" + ((this.ForeColor != null) ? this.ForeColor : "@ForeColor@ ") + ";")
.Replace("@required@", (this.Required && !this.Hidden ? " required" : string.Empty))
.Replace("@placeHolder@", "placeholder=''");
        }

        public override string EnableJSfn { get { return @""; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool IsSysControl { get { return true; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool DoNotPersist { get { return true; } }

        public override bool Unique { get { return false; } }
        public override bool Required { get { return false; } }
        public override EbScript _OnChange { get => base._OnChange; set => base._OnChange = value; }
        public override EbScript OnChangeFn { get => base.OnChangeFn; set => base.OnChangeFn = value; }
        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override List<EbValidator> Validators { get => base.Validators; set => base.Validators = value; }
        public override EbScript ValueExpr { get => base.ValueExpr; set => base.ValueExpr = value; }
        public override EbScript HiddenExpr { get => base.HiddenExpr; set => base.HiddenExpr = value; }
        public override EbScript DisableExpr { get => base.DisableExpr; set => base.DisableExpr = value; }
        public override bool IsDisable { get => base.IsDisable; set => base.IsDisable = value; }

        //EbSysModifiedAt
        public override SingleColumn GetSingleColumn(User UserObj, Eb_Solution SoluObj, object Value)
        {
            return EbDate.GetSingleColumn(this, UserObj, SoluObj, Value);
        }
    }
}
