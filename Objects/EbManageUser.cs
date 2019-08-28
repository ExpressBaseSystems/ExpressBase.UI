﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using ExpressBase.Security;
using Newtonsoft.Json;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.Objects;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.WebForm)]
    [HideInToolBox]
    class EbManageUser : EbControlUI, IEbPlaceHolderControl
    {
        public EbManageUser()
        {
            Fields = new List<MngUsrLocFieldAbstract>();
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        public override string ToolIconHtml { get { return "<i class='fa fa-user'></i>"; } set { } }

        public override string ToolNameAlias { get { return "Manage User"; } set { } }

        [EnableInBuilder(BuilderType.WebForm)]
        public override bool Hidden { get { return true; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public override bool IsSysControl { get { return true; } }

        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        [ListType(typeof(MngUsrLocFieldAbstract))]
        public List<MngUsrLocFieldAbstract> Fields { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } }

        public NTV[] FuncParam = {
            //new NTV (){ Name = "userid", Type = EbDbTypes.Int32, Value = DBNull.Value},//eb_createdby
            new NTV (){ Name = "id", Type = EbDbTypes.Int32, Value = 0},
            new NTV (){ Name = "fullname", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "nickname", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "email", Type = EbDbTypes.String, Value = DBNull.Value},

            new NTV (){ Name = "pwd", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "dob", Type = EbDbTypes.Date, Value = DBNull.Value},
            new NTV (){ Name = "sex", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "alternateemail", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "phprimary", Type = EbDbTypes.String, Value = DBNull.Value},

            new NTV (){ Name = "phsecondary", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "phlandphone", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "extension", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "fbid", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "fbname", Type = EbDbTypes.String, Value = DBNull.Value},

            new NTV (){ Name = "roles", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "groups", Type = EbDbTypes.String, Value = DBNull.Value},
            new NTV (){ Name = "statusid", Type = EbDbTypes.Int32, Value = 0},
            new NTV (){ Name = "hide", Type = EbDbTypes.String, Value = "no"},
            new NTV (){ Name = "anonymoususerid", Type = EbDbTypes.Int32, Value = DBNull.Value},

            new NTV (){ Name = "preference", Type = EbDbTypes.String, Value = "{'Locale':'en-IN','TimeZone':'(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi'}"},
            new NTV (){ Name = "consadd", Type = EbDbTypes.String, Value = string.Empty},
            new NTV (){ Name = "consdel", Type = EbDbTypes.String, Value = string.Empty}
        };

        public override string GetJsInitFunc()
        {
            return @"
this.Init = function(id)
{
	this.Fields.$values.push(new EbObjects.MngUsrLocField('email'));
	this.Fields.$values.push(new EbObjects.MngUsrLocField('fullname'));
	this.Fields.$values.push(new EbObjects.MngUsrLocField('nickname'));
};";
        }

        public override string GetBareHtml()
        {
            return @"
            <input id='@ebsid@' data-ebtype='@data-ebtype@'  data-toggle='tooltip' title='@toolTipText@' class='date' type='text' name='@name@' autocomplete = 'off' @value@ @tabIndex@ style='width:100%; @BackColor@ @ForeColor@ display:inline-block; @fontStyle@ @readOnlyString@ @required@ @placeHolder@ disabled />
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
.Replace("@readOnlyString@", this.ReadOnlyString)
.Replace("@placeHolder@", "placeholder=''");
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

        public string VirtualTable { get; set; }

        public IEnumerable<MngUsrLocFieldAbstract> PersistingFields
        {
            get
            {
                return this.Fields.Where(f => !string.IsNullOrEmpty((f as MngUsrLocField).ControlName));
            }
        }

        public string GetSelectQuery()
        {
            string cols = string.Join(",", this.PersistingFields.Select(f => (f as MngUsrLocField).Name));
            return string.Format("SELECT id,{0} FROM eb_users WHERE eb_ver_id = :eb_ver_id AND eb_data_id = :id;", cols);
        }
        private string GetSaveQuery(bool ins, string param, string mtbl, string pemail)
        {
            if (ins)
                return string.Format("SELECT * FROM eb_security_user(:eb_createdby, {0}); UPDATE eb_users SET eb_ver_id = :eb_ver_id, eb_data_id = eb_currval('{1}_id_seq') WHERE email = {2};", param, mtbl, pemail);
            else
                return string.Format("SELECT * FROM eb_security_user(:eb_createdby, {0});", param);
        }

        public override bool ParameterizeControl(IDatabase DataDB, List<DbParameter> param, string tbl, SingleColumn cField, bool ins, ref int i, ref string _col, ref string _val, ref string _extqry, User usr, SingleColumn ocF)
        {
            string c = string.Empty;
            string ep = string.Empty;
            Dictionary<string, string> _d = JsonConvert.DeserializeObject<Dictionary<string, string>>(cField.Value);
            if (ins)
            {
                string sql = "SELECT id FROM eb_users WHERE LOWER(email) LIKE LOWER(:email) AND eb_del = 'F' AND (statusid = 0 OR statusid = 1 OR statusid = 2);";
                DbParameter[] parameters = new DbParameter[] { DataDB.GetNewParameter("email", EbDbTypes.String, _d["email"]) };
                EbDataTable dt = DataDB.DoQuery(sql, parameters);
                if (dt.Rows.Count > 0)
                    return false;// raise an exception to notify email already exists
            }
            else
            {
                Dictionary<string, string> _od = JsonConvert.DeserializeObject<Dictionary<string, string>>(ocF.Value);
                _d["id"] = _od["id"];
                _d["email"] = _od["email"];
            }
            for(int k = 0; k < this.FuncParam.Length; k++, i++)
            {
                if (_d.ContainsKey(this.FuncParam[k].Name))
                {
                    this.FuncParam[k].Value = _d[this.FuncParam[k].Name];
                }
                c += string.Concat(":", this.FuncParam[k].Name, "_", i, ", ");
                if(this.FuncParam[k].Value == DBNull.Value)
                {
                    var p = DataDB.GetNewParameter(this.FuncParam[k].Name + "_" + i, this.FuncParam[k].Type);
                    p.Value = this.FuncParam[k].Value;
                    param.Add(p);
                }
                else
                {
                    param.Add(DataDB.GetNewParameter(this.FuncParam[k].Name + "_" + i, this.FuncParam[k].Type, this.FuncParam[k].Value));
                }
                
                if (this.FuncParam[k].Name.Equals("email"))
                    ep = string.Concat(":", this.FuncParam[k].Name, "_", i);
            }

            _extqry += this.GetSaveQuery(ins, c.Substring(0, c.Length - 2), tbl, ep);
            
            return true;
        }
    }



    public abstract class MngUsrLocFieldAbstract { }

    [UsedWithTopObjectParent(typeof(EbObject))]
    [EnableInBuilder(BuilderType.WebForm)]
    [Alias("ControlField")]
    public class MngUsrLocField : MngUsrLocFieldAbstract
    {
        public MngUsrLocField() { }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public string Name { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        public string ControlName { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public bool IsRequired { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public EbDbTypes EbDbType { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        [HideInPropertyGrid]
        public EbControl Control { get; set; }
    }
}