﻿using ExpressBase.Common;
using ExpressBase.Common.Data;
using ExpressBase.Common.Enums;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.LocationNSolution;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Singletons;
using ExpressBase.Common.Structures;
using ExpressBase.Data;
using ExpressBase.Objects.Objects;
using ExpressBase.Objects.ServiceStack_Artifacts;
using ExpressBase.Security;
using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [EnableInBuilder(BuilderType.WebForm)]
    [HideInToolBox]
    [BuilderTypeEnum(BuilderType.WebForm)]
    public class EbWebForm : EbForm
    {
        [HideInPropertyGrid]
        public bool IsUpdate { get; set; }

        public bool IsRenderMode { get; set; }

        public EbWebForm()
        {
            this.DisableDelete = new List<EbSQLValidator>();
            this.DisableCancel = new List<EbSQLValidator>();
            this.BeforeSaveRoutines = new List<EbRoutines>();
            this.AfterSaveRoutines = new List<EbRoutines>();
        }

        public override int TableRowId { get; set; }

        public WebformData FormData { get; set; }

        public WebformData FormDataBackup { get; set; }

        public WebFormSchema FormSchema { get; set; }

        public User UserObj { get; set; }

        public int LocationId { get; set; }

        public Eb_Solution SolutionObj { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbSQLValidator> DisableDelete { get; set; }

        [PropertyGroup("Behavior")]
        [EnableInBuilder(BuilderType.WebForm)]
        public WebFormAfterSaveModes FormModeAfterSave { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbSQLValidator> DisableCancel { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbRoutines> BeforeSaveRoutines { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbRoutines> AfterSaveRoutines { get; set; }

        public static EbOperations Operations = WFOperations.Instance;

        public override string GetHead()
        {
            string head = string.Empty;

            foreach (EbControl c in this.Controls)
                head += c.GetHead();

            return head;
        }

        public override string GetHtml()
        {
            string html = "<form id='@ebsid@' isrendermode='@rmode@' ebsid='@ebsid@' class='formB-box form-buider-form ebcont-ctrl' eb-form='true' ui-inp eb-type='WebForm' @tabindex@>";

            foreach (EbControl c in this.Controls)
                html += c.GetHtml();

            html += "</form>";

            return html
                .Replace("@name@", this.Name)
                .Replace("@ebsid@", this.EbSid)
                .Replace("@rmode@", IsRenderMode.ToString().ToLower())
                .Replace("@tabindex@", IsRenderMode ? string.Empty : " tabindex='1'");
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

        public override void BeforeSave()
        {
            EbControl[] Allctrls = this.Controls.FlattenAllEbControls();
            for (int i = 0; i < Allctrls.Length; i++)
            {
                if (Allctrls[i] is EbDataGrid)
                {
                    for (int j = 0; j < (Allctrls[i] as EbDataGrid).Controls.Count; j++)
                    {
                        if ((Allctrls[i] as EbDataGrid).Controls[j] is EbDGUserControlColumn)
                        {
                            EbDGColumn DGColumn = (Allctrls[i] as EbDataGrid).Controls[j] as EbDGColumn;

                            ((Allctrls[i] as EbDataGrid).Controls[j] as EbDGUserControlColumn).Columns = new List<EbControl>();

                        }
                    }
                }
            }

            //BeforeSaveRec(this);

            CalcValueExprDependency();
        }

        private void BeforeSaveRec(EbControlContainer _container)
        {
            foreach (EbControl c in _container.Controls)
            {
                if (c is EbControlContainer)
                {
                    if ((c as EbControlContainer).TableName.IsNullOrEmpty())
                        (c as EbControlContainer).TableName = _container.TableName;
                    BeforeSaveRec(c as EbControlContainer);
                }
            }
        }

        //Populate Property DependedValExp
        private void CalcValueExprDependency()
        {
            Dictionary<int, EbControlWrapper> _dict = new Dictionary<int, EbControlWrapper>();
            GetControlsAsDict(this, "form", _dict);
            List<int> CalcFlds = new List<int>();
            List<KeyValuePair<int, int>> dpndcy = new List<KeyValuePair<int, int>>();
            List<int> ExeOrd = new List<int>();

            for (int i = 0; i < _dict.Count; i++)
            {
                if (_dict[i].Control.ValueExpr != null && !string.IsNullOrEmpty(_dict[i].Control.ValueExpr.Code))
                {
                    CalcFlds.Add(i);
                    ExeOrd.Add(i);
                }
            }

            for (int i = 0; i < CalcFlds.Count; i++)
            {
                string code = _dict[CalcFlds[i]].Control.ValueExpr.Code.ToLower();
                if (code.Contains("form"))
                {
                    for (int j = 0; j < _dict.Count; j++)
                    {
                        string[] stringArr = new string[] {
                            _dict[j].Path,
                            _dict[j].Root + ".currentrow." + _dict[j].Control.Name,
                            _dict[j].Root + ".currentrow['" + _dict[j].Control.Name + "']",
                            _dict[j].Root + ".currentrow[\"" + _dict[j].Control.Name + "\"]"
                        };
                        if (stringArr.Any(code.Contains))
                        {
                            if (CalcFlds[i] == j)
                                throw new FormException("Avoid circular reference by the following control in 'ValueExpression' : " + _dict[CalcFlds[i]].Control.Name);
                            dpndcy.Add(new KeyValuePair<int, int>(CalcFlds[i], j));//<depended, dominant>
                        }
                    }
                }
            }

            int stopCounter = 0;
            while (dpndcy.Count > 0 && stopCounter < CalcFlds.Count)
            {
                for (int i = 0; i < CalcFlds.Count; i++)
                {
                    if (dpndcy.FindIndex(x => x.Value == CalcFlds[i]) == -1)
                    {
                        bool isProcessed = false;
                        foreach (KeyValuePair<int, int> item in dpndcy.Where(e => e.Key == CalcFlds[i]))
                        {
                            _dict[item.Value].Control.DependedValExp.Remove(_dict[item.Key].Path);
                            _dict[item.Value].Control.DependedValExp.Insert(0, _dict[item.Key].Path);
                            ExeOrd.Remove(item.Value);
                            ExeOrd.Insert(0, item.Value);
                            isProcessed = true;
                        }
                        if (isProcessed)
                            dpndcy.RemoveAll(x => x.Key == CalcFlds[i]);
                    }
                }
                stopCounter++;
            }
            if (dpndcy.Count > 0)
            {
                throw new FormException("Avoid circular reference by the following controls in 'ValueExpression' : " + string.Join(',', dpndcy.Select(e => _dict[e.Key].Control.Name).Distinct()));
            }
            else
            {
                FillDependedCtrlRec(_dict, ExeOrd);
            }
        }

        //To populate multilevel DependedValExp property
        private void FillDependedCtrlRec(Dictionary<int, EbControlWrapper> _dict, List<int> ExeOrd)
        {
            for (int i = ExeOrd.Count - 1; i >= 0; i--)
            {
                List<string> extList = new List<string>();
                foreach (string item in _dict[ExeOrd[i]].Control.DependedValExp)
                {
                    EbControlWrapper ctrlWrap = _dict.Values.FirstOrDefault(e => e.Path.Equals(item));
                    foreach (var path in ctrlWrap.Control.DependedValExp)
                    {
                        if (!_dict[ExeOrd[i]].Control.DependedValExp.Contains(path) && !extList.Contains(path))
                            extList.Add(path);
                    }
                }
                _dict[ExeOrd[i]].Control.DependedValExp.AddRange(extList);
            }
        }

        private string GetSelectQuery(WebFormSchema _schema, Service _service, out string _queryExt)
        {
            string query = string.Empty;
            string fupquery = string.Empty;
            _queryExt = string.Empty;
            if (_schema == null)
                _schema = this.FormSchema;//this.GetWebFormSchema();
            foreach (TableSchema _table in _schema.Tables)
            {
                string _cols = "id, eb_loc_id";
                string _id = "id";

                if (_table.Columns.Count > 0)
                {
                    if (_table.TableType == WebFormTableTypes.Grid)
                        _cols = "id, eb_loc_id, eb_row_num, " + String.Join(", ", _table.Columns.Select(x => x.ColumnName));
                    else
                        _cols = "id, eb_loc_id, " + String.Join(", ", _table.Columns.Select(x => x.ColumnName));
                }
                if (_table.TableName != _schema.MasterTable)
                    _id = _schema.MasterTable + "_id";

                query += string.Format("SELECT {0} FROM {1} WHERE {2} = :id AND eb_del='F' {3};", _cols, _table.TableName, _id, _table.TableType == WebFormTableTypes.Grid ? "ORDER BY eb_row_num" : string.Empty);

                foreach (ColumnSchema Col in _table.Columns)
                {
                    if (Col.Control is EbPowerSelect)
                        _queryExt += (Col.Control as EbPowerSelect).GetSelectQuery(_service, Col.ColumnName, _table.TableName, _id);
                    else if (Col.Control is EbDGPowerSelectColumn)
                        _queryExt += (Col.Control as EbDGPowerSelectColumn).GetSelectQuery(_service, Col.ColumnName, _table.TableName, _id);
                }
            }
            foreach (Object Ctrl in _schema.ExtendedControls)
            {
                if (Ctrl is EbFileUploader)
                    fupquery += (Ctrl as EbFileUploader).GetSelectQuery();
            }
            return query + fupquery;
        }

        public string GetDeleteQuery(IDatabase DataDB, WebFormSchema _schema = null)
        {
            string query = string.Empty;
            if (_schema == null)
                _schema = this.FormSchema;//this.GetWebFormSchema();
            foreach (TableSchema _table in _schema.Tables)
            {
                string _id = "id";
                string _dupcols = string.Empty;
                if (_table.TableName != _schema.MasterTable)
                    _id = _schema.MasterTable + "_id";
                foreach (ColumnSchema _column in _table.Columns)
                {
                    if (_column.Control is EbAutoId)
                    {
                        _dupcols += string.Format(", {0}_ebbkup = {0}, {0} = CONCAT({0}, '_ebbkup')", _column.ColumnName);
                    }
                }
                query += string.Format("UPDATE {0} SET eb_del='T',eb_lastmodified_by = :eb_modified_by, eb_lastmodified_at = " + DataDB.EB_CURRENT_TIMESTAMP + " {1} WHERE {2} = :id AND eb_del='F';", _table.TableName, _dupcols, _id);
            }
            return query;
        }

        public string GetCancelQuery(IDatabase DataDB, WebFormSchema _schema = null)
        {
            string query = string.Empty;
            if (_schema == null)
                _schema = this.FormSchema;//this.GetWebFormSchema();
            foreach (TableSchema _table in _schema.Tables)
            {
                string _id = "id";
                if (_table.TableName != _schema.MasterTable)
                    _id = _schema.MasterTable + "_id";
                query += string.Format("UPDATE {0} SET eb_void='T',eb_lastmodified_by = :eb_modified_by, eb_lastmodified_at = " + DataDB.EB_CURRENT_TIMESTAMP + " WHERE {1} = :id AND eb_void='F' AND eb_del='F';", _table.TableName, _id);
            }
            return query;
        }

        //get formdata as globals for c# script engine
        public FormAsGlobal GetFormAsGlobal(WebformData _formData, EbControlContainer _container = null, FormAsGlobal _globals = null)
        {
            if (_container == null)
                _container = this;
            if (_globals == null)
                _globals = new FormAsGlobal { Name = this.Name };

            ListNTV listNTV = new ListNTV();

            if (_formData.MultipleTables.ContainsKey(_container.TableName))
            {
                for (int i = 0; i < _formData.MultipleTables[_container.TableName].Count; i++)
                {
                    foreach (EbControl control in _container.Controls)
                    {
                        if (control is EbControlContainer)
                        {
                            FormAsGlobal g = new FormAsGlobal();
                            g.Name = (control as EbControlContainer).Name;
                            _globals.AddContainer(g);
                            g = GetFormAsGlobal(_formData, control as EbControlContainer, g);
                        }
                        else
                        {
                            NTV n = GetNtvFromFormData(_formData, _container.TableName, i, control.Name);
                            if (n != null)
                                listNTV.Columns.Add(n);
                        }
                    }
                }
                _globals.Add(listNTV);
            }
            return _globals;
        }

        private NTV GetNtvFromFormData(WebformData _formData, string _table, int _row, string _column)
        {
            NTV ntv = null;
            if (_formData.MultipleTables.ContainsKey(_table))
            {
                foreach (SingleColumn col in _formData.MultipleTables[_table][_row].Columns)
                {
                    if (col.Name.Equals(_column))
                    {
                        ntv = new NTV()
                        {
                            Name = _column,
                            Type = (EbDbTypes)col.Type,
                            Value = col.Value
                        };
                        break;
                    }
                }
            }
            return ntv;
        }

        //get controls in webform as a single dimensional structure 
        public static void GetControlsAsDict(EbControlContainer _container, string _path, Dictionary<int, EbControlWrapper> _dict)
        {
            int _counter = _dict.Count;
            IEnumerable<EbControl> FlatCtrls = _container.Controls.Get1stLvlControls();
            foreach (EbControl control in FlatCtrls)
            {
                control.DependedValExp.Clear();
                string path = _path == "" ? control.Name : _path + "." + control.Name;
                control.__path = path;
                _dict.Add(_counter++, new EbControlWrapper
                {
                    TableName = _container.TableName,
                    Path = path,
                    Control = control,
                    Root = _path
                });
            }
            foreach (EbControl control in _container.Controls)
            {
                if (control is EbControlContainer)
                {
                    string path = _path;
                    if (control is EbDataGrid)
                        path = _path + "." + (control as EbControlContainer).Name;
                    GetControlsAsDict(control as EbControlContainer, path, _dict);
                }
            }
        }

        //get all control container as flat structure
        public List<EbControlContainer> GetAllContainers(EbControlContainer _container, List<EbControlContainer> _list = null)
        {
            if (_list == null)
                _list = new List<EbControlContainer>();
            _list.Add(_container);
            foreach (EbControl c in this.Controls)
            {
                if (c is EbControlContainer)
                {
                    _list = GetAllContainers(_container, _list);
                }
            }
            return _list;
        }

        //merge formdata and webform object
        public void MergeFormData()
        {
            MergeFormDataInner(this);
        }

        private void MergeFormDataInner(EbControlContainer _container)
        {
            if (!FormData.MultipleTables.ContainsKey(_container.TableName))
            {
                return;
            }
            foreach (EbControl c in _container.Controls)
            {
                if (c is EbDataGrid)
                {
                    foreach (EbControl control in (c as EbDataGrid).Controls)
                    {
                        if (!control.DoNotPersist)
                        {
                            List<object> val = new List<object>();
                            for (int i = 0; i < FormData.MultipleTables[(c as EbDataGrid).TableName].Count; i++)
                            {
                                if (FormData.MultipleTables[(c as EbDataGrid).TableName][i][control.Name] != null)
                                {
                                    val.Add(FormData.MultipleTables[(c as EbDataGrid).TableName][i][control.Name]);
                                    FormData.MultipleTables[(c as EbDataGrid).TableName][i].SetEbDbType(control.Name, control.EbDbType);
                                    FormData.MultipleTables[(c as EbDataGrid).TableName][i].SetControl(control.Name, control);
                                }
                            }
                            control.ValueFE = val;
                        }
                    }
                }
                else if (c is EbControlContainer)
                {
                    if (string.IsNullOrEmpty((c as EbControlContainer).TableName))
                        (c as EbControlContainer).TableName = _container.TableName;
                    MergeFormDataInner(c as EbControlContainer);
                }
                else if (c is EbAutoId)
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("{currentlocation.id}", this.LocationId.ToString());
                    dict.Add("{user.id}", this.UserObj.UserId.ToString());
                    dict.Add("{currentlocation.shortname}", this.SolutionObj.Locations[this.LocationId].ShortName);

                    MatchCollection mc = Regex.Matches((c as EbAutoId).Pattern.sPattern, @"{(.*?)}");
                    foreach (Match m in mc)
                    {
                        if (dict.ContainsKey(m.Value))
                            (c as EbAutoId).Pattern.sPattern = (c as EbAutoId).Pattern.sPattern.Replace(m.Value, dict[m.Value]);
                    }
                    FormData.MultipleTables[_container.TableName][0].SetEbDbType(c.Name, c.EbDbType);
                    FormData.MultipleTables[_container.TableName][0].SetControl(c.Name, c);
                    FormData.MultipleTables[_container.TableName][0][c.Name] = (c as EbAutoId).Pattern.sPattern;
                    c.ValueFE = FormData.MultipleTables[_container.TableName][0][c.Name];
                }
                else if (!(c is EbFileUploader))
                {
                    if (!c.DoNotPersist)
                    {
                        if (FormData.MultipleTables[_container.TableName][0][c.Name] != null)
                        {
                            c.ValueFE = FormData.MultipleTables[_container.TableName][0][c.Name];
                            FormData.MultipleTables[_container.TableName][0].SetEbDbType(c.Name, c.EbDbType);
                            FormData.MultipleTables[_container.TableName][0].SetControl(c.Name, c);
                        }
                    }
                }
            }
        }

        private void GetFormattedDataApproval(EbDataTable dataTable, SingleTable Table)
        {
            foreach(EbDataRow dataRow in dataTable.Rows)
            {
                DateTime dt = Convert.ToDateTime(dataRow["eb_created_at"]);
                Table.Add(new SingleRow { Columns = new List<SingleColumn>
                {
                    new SingleColumn { Name = "id", Type = (int)EbDbTypes.Decimal, Value = Convert.ToInt32(dataRow["id"])},
                    new SingleColumn { Name = "stage", Type = (int)EbDbTypes.String, Value = dataRow["stage"].ToString()},
                    new SingleColumn { Name = "approver_role", Type = (int)EbDbTypes.String, Value = dataRow["approver_role"].ToString()},
                    new SingleColumn { Name = "status", Type = (int)EbDbTypes.Decimal, Value = Convert.ToInt32(dataRow["status"])},
                    new SingleColumn { Name = "remarks", Type = (int)EbDbTypes.String, Value = dataRow["remarks"].ToString()},
                    new SingleColumn { Name = "eb_created_by_id", Type = (int)EbDbTypes.Decimal, Value = Convert.ToInt32(dataRow["eb_created_by"])},
                    new SingleColumn { Name = "eb_created_by_name", Type = (int)EbDbTypes.String, Value = this.SolutionObj.Users[Convert.ToInt32(dataRow["eb_created_by"])]},
                    new SingleColumn { Name = "eb_created_at", Type = (int)EbDbTypes.String, Value = dt.ConvertFromUtc(this.UserObj.TimeZone).ToString("dd-MM-yyyy hh:mm tt")}
                }, RowId = dataRow["id"].ToString(), LocId = Convert.ToInt32(dataRow["eb_loc_id"])});
            }
        }

        private void GetFormattedData(EbDataTable dataTable, SingleTable Table, TableSchema _table = null)
        {
            foreach (EbDataRow dataRow in dataTable.Rows)
            {
                string _rowId = dataRow[dataTable.Columns[0].ColumnIndex].ToString();
                bool _rowFound = false;
                foreach (SingleRow r in Table)
                {
                    if (r.RowId.Equals(_rowId))
                    {
                        _rowFound = true;
                        break;
                    }
                }
                if (_rowFound)// skipping duplicate rows in dataTable
                    continue;

                SingleRow Row = new SingleRow();
                bool skipFst = true;
                foreach (EbDataColumn dataColumn in dataTable.Columns)
                {
                    if (dataColumn.ColumnName == "eb_loc_id" && skipFst)
                    {
                        Row.LocId = Convert.ToInt32(dataRow[dataColumn.ColumnIndex]);
                        skipFst = false;
                    }
                    else if (dataRow.IsDBNull(dataColumn.ColumnIndex))
                    {
                        Row.Columns.Add(new SingleColumn()
                        {
                            Name = dataColumn.ColumnName,
                            Type = (int)dataColumn.Type,
                            Value = null
                        });
                    }
                    else
                    {
                        object _unformattedData = dataRow[dataColumn.ColumnIndex];
                        object _formattedData = _unformattedData;

                        if (_table != null)
                        {
                            ColumnSchema _column = _table.Columns.Find(c => c.ColumnName.Equals(dataColumn.ColumnName));
                            if (_column != null)
                            {
                                if (_column.Control is EbDate || _column.Control is EbDGDateColumn || _column.Control is EbSysCreatedAt || _column.Control is EbSysModifiedAt)
                                {
                                    EbDateType _type = _column.Control is EbDate ? (_column.Control as EbDate).EbDateType :
                                        _column.Control is EbDGDateColumn ? (_column.Control as EbDGDateColumn).EbDateType :
                                        _column.Control is EbSysCreatedAt ? (_column.Control as EbSysCreatedAt).EbDateType : (_column.Control as EbSysModifiedAt).EbDateType;
                                    DateTime dt = Convert.ToDateTime(_unformattedData);
                                    if (_type == EbDateType.Date)
                                    {
                                        if (_column.Control is EbSysCreatedAt || _column.Control is EbSysModifiedAt)
                                            _formattedData = dt.ConvertFromUtc(this.UserObj.Preference.TimeZone).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                                        else
                                            _formattedData = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    }
                                    else if (_type == EbDateType.DateTime)
                                    {
                                        _formattedData = dt.ConvertFromUtc(this.UserObj.Preference.TimeZone).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    }
                                    else
                                    {
                                        _formattedData = dt.ConvertFromUtc(this.UserObj.Preference.TimeZone).ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                                    }
                                }
                                else if (_column.Control is EbSysLocation)
                                {
                                    int loc_id = Convert.ToInt32(_unformattedData);
                                    EbSysLocDM dm = (_column.Control as EbSysLocation).DisplayMember;
                                    if (this.SolutionObj.Locations.ContainsKey(loc_id))
                                    {
                                        if (dm == EbSysLocDM.LongName)
                                            _formattedData = loc_id + "$$" + this.SolutionObj.Locations[loc_id].LongName;
                                        else if (dm == EbSysLocDM.ShortName)
                                            _formattedData = loc_id + "$$" + this.SolutionObj.Locations[loc_id].ShortName;
                                    }
                                }
                                else if (_column.Control is EbSysCreatedBy || _column.Control is EbSysModifiedBy)
                                {
                                    int user_id = Convert.ToInt32(_unformattedData);
                                    EbSysCreatedByDM dm = (_column.Control is EbSysCreatedBy) ? (_column.Control as EbSysCreatedBy).DisplayMember : (_column.Control as EbSysModifiedBy).DisplayMember;
                                    if (this.SolutionObj.Users != null && this.SolutionObj.Users.ContainsKey(user_id))
                                    {
                                        if (dm == EbSysCreatedByDM.FullName)
                                            _formattedData = user_id + "$$" + this.SolutionObj.Users[user_id];
                                    }
                                }
                            }
                        }
                        else if (dataColumn.Type == EbDbTypes.Date)
                        {
                            _unformattedData = (_unformattedData == DBNull.Value) ? DateTime.MinValue : _unformattedData;
                            _formattedData = ((DateTime)_unformattedData).Date != DateTime.MinValue ? Convert.ToDateTime(_unformattedData).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
                        }
                        //else if (dataColumn.Type == EbDbTypes.DateTime)
                        //{
                        //    _unformattedData = (_unformattedData == DBNull.Value) ? DateTime.MinValue : _unformattedData;
                        //    _formattedData = ((DateTime)_unformattedData).Date != DateTime.MinValue ? Convert.ToDateTime(_unformattedData).ConvertFromUtc(this.UserObj.Preference.TimeZone).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty;
                        //}
                        Row.Columns.Add(new SingleColumn()
                        {
                            Name = dataColumn.ColumnName,
                            Type = (int)dataColumn.Type,
                            Value = _formattedData
                        });
                    }
                }
                Row.RowId = _rowId;
                Table.Add(Row);
            }
        }

        //For Normal Mode
        public void RefreshFormData(IDatabase DataDB, Service service, bool backup = false)
        {
            WebFormSchema _schema = this.FormSchema;//this.GetWebFormSchema();
            string psquery = null;
            string query = this.GetSelectQuery(_schema, service, out psquery);
            string context = this.RefId.Split("-")[3] + "_" + this.TableRowId.ToString();//context format = objectId_rowId_ControlId

            EbDataSet dataset = DataDB.DoQueries(query, new DbParameter[]
            {
                DataDB.GetNewParameter("id", EbDbTypes.Int32, this.TableRowId),
                DataDB.GetNewParameter("context", EbDbTypes.String, context)
            });

            Console.WriteLine("From RefreshFormData : Schema table count = " + _schema.Tables.Count + " Dataset count = " + dataset.Tables.Count);

            WebformData _FormData = new WebformData();

            for (int i = 0; i < _schema.Tables.Count && dataset.Tables.Count >= _schema.Tables.Count; i++)
            {
                EbDataTable dataTable = dataset.Tables[i];////
                SingleTable Table = new SingleTable();

                if (_schema.Tables[i].TableType == WebFormTableTypes.Approval)
                    GetFormattedDataApproval(dataTable, Table);
                else
                    GetFormattedData(dataTable, Table, _schema.Tables[i]);

                if (!_FormData.MultipleTables.ContainsKey(_schema.Tables[i].TableName) && Table.Count > 0)
                    _FormData.MultipleTables.Add(_schema.Tables[i].TableName, Table);
            }
            if (_FormData.MultipleTables.Count > 0)
                _FormData.MasterTable = _schema.Tables[0].TableName;

            if (dataset.Tables.Count > _schema.Tables.Count)
            {
                int tableIndex = _schema.Tables.Count;

                //foreach (TableSchema Tbl in _schema.Tables)//PowerSelect
                //{
                //    foreach (ColumnSchema Col in Tbl.Columns)
                //    {
                //        if (Col.Control is EbPowerSelect || Col.Control is EbDGPowerSelectColumn)
                //        {
                //            SingleTable Table = new SingleTable();
                //            GetFormattedData(dataset.Tables[tableIndex], Table);
                //            _FormData.ExtendedTables.Add((Col.Control as EbControl).EbSid, Table);
                //            tableIndex++;
                //        }
                //    }
                //}

                foreach (Object Ctrl in _schema.ExtendedControls)//FileUploader Controls
                {
                    SingleTable Table = new SingleTable();
                    GetFormattedData(dataset.Tables[tableIndex], Table);
                    //--------------
                    List<FileMetaInfo> _list = new List<FileMetaInfo>();
                    foreach (SingleRow dr in Table)
                    {
                        FileMetaInfo info = new FileMetaInfo
                        {
                            FileRefId = dr["id"],
                            FileName = dr["filename"],
                            Meta = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(dr["tags"] as string),
                            UploadTime = dr["uploadts"],
                            FileCategory = (EbFileCategory)Convert.ToInt32(dr["filecategory"])
                        };

                        if (!_list.Contains(info))
                            _list.Add(info);
                    }
                    SingleTable _Table = new SingleTable {
                        new SingleRow() {
                            Columns = new List<SingleColumn> {
                                new SingleColumn { Name = "Files", Type = (int)EbDbTypes.Json, Value = JsonConvert.SerializeObject(_list) }
                            }
                        }
                    };
                    //--------------
                    _FormData.ExtendedTables.Add((Ctrl as EbControl).EbSid, _Table);
                    tableIndex++;
                }
            }

            if (!psquery.IsNullOrEmpty())
            {
                List<DbParameter> param = new List<DbParameter>();
                DataDB.GetNewParameter("id", EbDbTypes.Int32, this.TableRowId);
                this.LocationId = _FormData.MultipleTables[_FormData.MasterTable][0].LocId;

                for (int i = 0; i < _schema.Tables.Count && dataset.Tables.Count >= _schema.Tables.Count; i++)
                {
                    if (dataset.Tables[i].Rows.Count > 0)
                    {
                        EbDataRow dataRow = dataset.Tables[i].Rows[0];
                        foreach (EbDataColumn dataColumn in dataset.Tables[i].Columns)
                        {
                            DbParameter t = param.Find(e => e.ParameterName == dataColumn.ColumnName);
                            if (t == null)
                            {
                                if (dataRow.IsDBNull(dataColumn.ColumnIndex))
                                {
                                    var p = DataDB.GetNewParameter(dataColumn.ColumnName, dataColumn.Type);
                                    p.Value = DBNull.Value;
                                    param.Add(p);
                                }
                                else
                                    param.Add(DataDB.GetNewParameter(dataColumn.ColumnName, dataColumn.Type, dataRow[dataColumn.ColumnIndex]));
                            }
                        }
                    }
                }
                //foreach (KeyValuePair<string, SingleTable> entry in _FormData.MultipleTables)
                //{
                //    if (entry.Key == _FormData.MasterTable)
                //        this.LocationId = entry.Value[0].LocId;

                //    foreach (SingleColumn column in entry.Value[0].Columns)
                //    {
                //        DbParameter t = param.Find(e => e.ParameterName == column.Name);
                //        if (t == null)
                //        {
                //            if (column.Value == null)
                //            {
                //                var p = DataDB.GetNewParameter(column.Name, (EbDbTypes)column.Type);
                //                p.Value = DBNull.Value;
                //                param.Add(p);
                //            }
                //            else
                //                param.Add(DataDB.GetNewParameter(column.Name, (EbDbTypes)column.Type, column.Value));
                //        }
                //    }
                //}
                DbParameter tt = param.Find(e => e.ParameterName == "eb_loc_id");
                if (tt == null)
                    param.Add(DataDB.GetNewParameter("eb_loc_id", EbDbTypes.Decimal, this.LocationId));

                EbDataSet ds = DataDB.DoQueries(psquery, param.ToArray());

                if (ds.Tables.Count > 0)
                {
                    int tblIdx = 0;
                    foreach (TableSchema Tbl in _schema.Tables)//PowerSelect
                    {
                        foreach (ColumnSchema Col in Tbl.Columns)
                        {
                            if (Col.Control is EbPowerSelect || Col.Control is EbDGPowerSelectColumn)
                            {
                                SingleTable Table = new SingleTable();
                                GetFormattedData(ds.Tables[tblIdx], Table);
                                _FormData.ExtendedTables.Add((Col.Control as EbControl).EbSid, Table);
                                tblIdx++;
                            }
                        }
                    }
                }
            }

            if (backup)
                this.FormDataBackup = _FormData;
            else
            {
                this.FormData = _FormData;
                this.ExeDeleteCancelScript(DataDB);
            }
            Console.WriteLine("No Exception in RefreshFormData");
        }

        //For Prefill Mode
        public void RefreshFormData(IDatabase DataDB, Service service, List<Param> _params)
        {
            WebFormSchema _schema = this.FormSchema;//this.GetWebFormSchema();
            this.FormData = new WebformData
            {
                MasterTable = _schema.MasterTable
            };
            Dictionary<string, string> QrsDict = new Dictionary<string, string>();
            List<DbParameter> param = new List<DbParameter>();
            for (int i = 0; i < _params.Count; i++)
            {
                for (int j = 0; j < _schema.Tables.Count; j++)
                {
                    for (int k = 0; k < _schema.Tables[j].Columns.Count; k++)
                    {
                        if (_schema.Tables[j].Columns[k].ColumnName.Equals(_params[i].Name))
                        {
                            if (_schema.Tables[j].Columns[k].Control is EbPowerSelect)
                            {
                                string t = (_schema.Tables[j].Columns[k].Control as EbPowerSelect).GetSelectQuery(service, _params[i].Value);
                                QrsDict.Add((_schema.Tables[j].Columns[k].Control as EbPowerSelect).EbSid, t);
                            }
                            if (!this.FormData.MultipleTables.ContainsKey(_schema.Tables[j].TableName))
                            {
                                SingleTable tbl = new SingleTable();
                                tbl.Add(new SingleRow());
                                this.FormData.MultipleTables.Add(_schema.Tables[j].TableName, tbl);
                            }
                            SingleColumn col = new SingleColumn()
                            {
                                Name = _params[i].Name,
                                Type = _schema.Tables[j].Columns[k].EbDbType,
                                Value = _params[i].ValueTo
                            };
                            param.Add(DataDB.GetNewParameter(col.Name, (EbDbTypes)col.Type, col.Value));
                            this.FormData.MultipleTables[_schema.Tables[j].TableName][0].Columns.Add(col);
                        }
                    }
                }
            }
            if (QrsDict.Count > 0)
            {
                EbDataSet dataset = DataDB.DoQueries(string.Join(" ", QrsDict.Select(d => d.Value)), param.ToArray());
                int i = 0;
                foreach (KeyValuePair<string, string> item in QrsDict)
                {
                    SingleTable Table = new SingleTable();
                    GetFormattedData(dataset.Tables[i++], Table);
                    this.FormData.ExtendedTables.Add(item.Key, Table);
                }
            }
        }

        public int Save(IDatabase DataDB, Service service)
        {
            int r = 0;
            if (this.TableRowId > 0)
            {
                this.RefreshFormData(DataDB, service, true);
                r = this.Update(DataDB);
            }
            else
            {
                this.TableRowId = this.Insert(DataDB);
                r = 1;
            }
            this.RefreshFormData(DataDB, service);

            try
            {
                this.UpdateAuditTrail(DataDB);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception!!! UpdateAuditTrail : " + e.Message);
            }

            return r;
        }

        public int Update(IDatabase DataDB)
        {
            string fullqry = string.Empty;
            List<DbParameter> param = new List<DbParameter>();
            int i = 0;
            foreach (KeyValuePair<string, SingleTable> entry in this.FormData.MultipleTables)
            {
                foreach (SingleRow row in entry.Value)
                {
                    string _tblname = entry.Key;
                    if (Convert.ToInt32(row.RowId) > 0)
                    {
                        string _qry = "UPDATE {0} SET {1} eb_lastmodified_by = :eb_modified_by, eb_lastmodified_at = " + DataDB.EB_CURRENT_TIMESTAMP + " WHERE id={2};";
                        string _colvals = string.Empty;
                        if (row.IsDelete && !_tblname.Equals(this.FormData.MasterTable))
                        {
                            _qry = "UPDATE {0} SET {1}, eb_lastmodified_by = :eb_modified_by, eb_lastmodified_at = " + DataDB.EB_CURRENT_TIMESTAMP + " WHERE id={2} AND eb_del='F';";
                            _colvals = "eb_del='T'";
                        }
                        else
                        {
                            foreach (SingleColumn rField in row.Columns)
                            {
                                if (!(rField.Control is EbAutoId))
                                {
                                    _colvals += string.Concat(rField.Name, "=:", rField.Name, "_", i, ",");
                                    if (rField.Value == null)
                                    {
                                        var p = DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type);
                                        p.Value = DBNull.Value;
                                        param.Add(p);
                                    }
                                    else if (rField.Control is EbDate || rField.Control is EbDGDateColumn)
                                    {
                                        EbDateType _type = rField.Control is EbDate ? (rField.Control as EbDate).EbDateType : (rField.Control as EbDGDateColumn).EbDateType;
                                        if (_type == EbDateType.Date)
                                        {
                                            rField.Value = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            DateTime dt = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                            rField.Value = dt.ConvertToUtc(this.UserObj.Preference.TimeZone);
                                        }
                                        param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, EbDbTypes.DateTime, rField.Value));
                                    }
                                    //else if ((EbDbTypes)rField.Type == EbDbTypes.Date)
                                    //{
                                    //    rField.Value = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    //    param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                                    //}
                                    //else if ((EbDbTypes)rField.Type == EbDbTypes.DateTime)
                                    //{
                                    //    DateTime dt = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    //    rField.Value = dt.ConvertToUtc(this.UserObj.Preference.TimeZone);
                                    //    param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                                    //}
                                    else
                                        param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                                }
                            }
                        }

                        fullqry += string.Format(_qry, _tblname, _colvals, row.RowId);
                    }
                    else
                    {
                        string _qry = "INSERT INTO {0} ({1} eb_created_by, eb_created_at, eb_loc_id, {3}_id ) VALUES ({2} :eb_createdby, " + DataDB.EB_CURRENT_TIMESTAMP + ", :eb_loc_id, :{4}_id);";
                        string _cols = string.Empty, _vals = string.Empty;
                        foreach (SingleColumn rField in row.Columns)
                        {
                            _cols += string.Concat(rField.Name, ",");
                            _vals += string.Concat(":", rField.Name, "_", i, ",");
                            if (rField.Value == null)
                            {
                                var p = DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type);
                                p.Value = DBNull.Value;
                                param.Add(p);
                            }
                            else if (rField.Control is EbDate || rField.Control is EbDGDateColumn)
                            {
                                EbDateType _type = rField.Control is EbDate ? (rField.Control as EbDate).EbDateType : (rField.Control as EbDGDateColumn).EbDateType;
                                if (_type == EbDateType.Date)
                                {
                                    rField.Value = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    DateTime dt = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    rField.Value = dt.ConvertToUtc(this.UserObj.Preference.TimeZone);
                                }
                                param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, EbDbTypes.DateTime, rField.Value));
                            }
                            //else if ((EbDbTypes)rField.Type == EbDbTypes.Date)
                            //{
                            //    rField.Value = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            //    param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                            //}
                            //else if ((EbDbTypes)rField.Type == EbDbTypes.DateTime)
                            //{
                            //    DateTime dt = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            //    rField.Value = dt.ConvertToUtc(this.UserObj.Preference.TimeZone);
                            //    param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                            //}
                            else
                                param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                        }
                        fullqry += string.Format(_qry, _tblname, _cols, _vals, this.FormData.MasterTable, this.FormData.MasterTable);
                    }
                    i++;
                }
            }

            //------------------
            string EbObId = this.RefId.Split("-")[3];
            List<string> InnerVals = new List<string>();
            List<string> Innercxt = new List<string>();
            List<string> InnerIds = new List<string>();
            foreach (KeyValuePair<string, SingleTable> entry in this.FormData.ExtendedTables)
            {
                foreach (SingleRow row in entry.Value)
                {
                    string cn = entry.Key + "_" + i.ToString();
                    i++;
                    InnerVals.Add(string.Format("('{0}_{1}_{2}')", EbObId, this.TableRowId, entry.Key));
                    param.Add(DataDB.GetNewParameter(cn, EbDbTypes.Decimal, row.Columns[0].Value));
                    InnerIds.Add(":" + cn);
                }
                Innercxt.Add("context = '" + EbObId + "_" + this.TableRowId + "_" + entry.Key + "'");
            }
            if (InnerVals.Count > 0)
            {
                for (int k = 0; k < InnerVals.Count; k++)
                {
                    fullqry += string.Format(@"UPDATE 
                                            eb_files_ref AS t
                                        SET
                                            context = {0}                                        
                                        WHERE
                                           t.id = {1} AND t.eb_del = 'F';", InnerVals[k], InnerIds[k]);
                }

                //fullqry += string.Format(@"UPDATE 
                //                            eb_files_ref AS t
                //                        SET
                //                            context = c.context
                //                        FROM
                //                            (VALUES{0}) AS c(id, context)
                //                        WHERE
                //                            c.id = t.id AND t.eb_del = 'F';", InnerVals.Join(","));
                fullqry += string.Format(@"UPDATE eb_files_ref 
                                        SET eb_del='T' 
                                        WHERE ({0}) AND eb_del='F' AND id NOT IN ({1});", Innercxt.Join(" OR "), InnerIds.Join(","));
            }

            //-------------------------

            param.Add(DataDB.GetNewParameter(this.FormData.MasterTable + "_id", EbDbTypes.Int32, this.FormData.MultipleTables[this.FormData.MasterTable][0].RowId));
            param.Add(DataDB.GetNewParameter("eb_loc_id", EbDbTypes.Int32, this.LocationId));
            param.Add(DataDB.GetNewParameter("eb_createdby", EbDbTypes.Int32, this.UserObj.UserId));
            param.Add(DataDB.GetNewParameter("eb_modified_by", EbDbTypes.Int32, this.UserObj.UserId));

            //param.Add(DataDB.GetNewParameter("eb_loc_s", EbDbTypes.String, this.SolutionObj.Locations.ContainsKey(this.LocationId) ? this.SolutionObj.Locations[this.LocationId].ShortName : string.Empty));
            //param.Add(DataDB.GetNewParameter("eb_createdby_s", EbDbTypes.String, this.UserObj.FullName));
            //param.Add(DataDB.GetNewParameter("eb_modified_by_s", EbDbTypes.String, this.UserObj.FullName));

            return DataDB.InsertTable(fullqry, param.ToArray());
        }

        public int Insert(IDatabase DataDB)
        {
            string fullqry = string.Empty;
            List<DbParameter> param = new List<DbParameter>();
            int count = 0;
            int i = 0;
            foreach (KeyValuePair<string, SingleTable> entry in FormData.MultipleTables)
            {
                foreach (SingleRow row in entry.Value)
                {
                    string _qry = "INSERT INTO {0} ({1} eb_created_by, eb_created_at, eb_loc_id {3} ) VALUES ({2} :eb_createdby, " + DataDB.EB_CURRENT_TIMESTAMP + ", :eb_loc_id {4});";
                    if (DataDB.Vendor == DatabaseVendors.MYSQL && entry.Key == this.FormSchema.MasterTable)
                    {
                        _qry += "SELECT eb_persist_currval('" + entry.Key + "_id_seq');";
                    }
                    string _tblname = entry.Key;
                    string _cols = string.Empty;
                    string _values = string.Empty;
                    //_cols = FormObj.GetCtrlNamesOfTable(entry.Key);

                    foreach (SingleColumn rField in row.Columns)
                    {
                        if (rField.Control is EbAutoId)
                        {
                            _cols += string.Concat(rField.Name, ", ");
                            _values += string.Format("CONCAT(:{0}_{1}, (SELECT LPAD(CAST((COUNT(*) + 1) AS CHAR(12)), {2}, '0') FROM {3} WHERE {0} LIKE '{4}%')),", rField.Name, i, (rField.Control as EbAutoId).Pattern.SerialLength, entry.Key, rField.Value);
                            param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                        }
                        else if (rField.Control != null)
                        {
                            _cols += string.Concat(rField.Name, ", ");
                            _values += string.Concat(":", rField.Name, "_", i, ", ");
                            if (rField.Value == null)
                            {
                                var p = DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type);
                                p.Value = DBNull.Value;
                                param.Add(p);
                            }
                            else if (rField.Control is EbDate || rField.Control is EbDGDateColumn)
                            {
                                EbDateType _type = rField.Control is EbDate ? (rField.Control as EbDate).EbDateType : (rField.Control as EbDGDateColumn).EbDateType;
                                if (_type == EbDateType.Date)
                                {
                                    rField.Value = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    DateTime dt = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    rField.Value = dt.ConvertToUtc(this.UserObj.Preference.TimeZone);
                                }
                                param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, EbDbTypes.DateTime, rField.Value));
                            }
                            //else if ((EbDbTypes)rField.Type == EbDbTypes.Date)
                            //{
                            //    rField.Value = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            //    param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                            //}
                            //else if ((EbDbTypes)rField.Type == EbDbTypes.DateTime)
                            //{
                            //    DateTime dt = DateTime.ParseExact(rField.Value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            //    rField.Value = dt.ConvertToUtc(this.UserObj.Preference.TimeZone);
                            //    param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                            //}
                            else
                                param.Add(DataDB.GetNewParameter(rField.Name + "_" + i, (EbDbTypes)rField.Type, rField.Value));
                        }
                    }
                    i++;

                    if (count == 0)
                        _qry = _qry.Replace("{3}", "").Replace("{4}", "");
                    else
                        _qry = _qry.Replace("{3}", string.Concat(",", this.TableName, "_id")).Replace("{4}", string.Concat(", (SELECT eb_currval('", this.TableName, "_id_seq'" + "))"));
                    fullqry += string.Format(_qry, _tblname, _cols, _values);
                }
                count++;

            }

            //------------------File Uploader entries------------------------------------
            string EbObId = this.RefId.Split("-")[3];
            List<string> InnerVals = new List<string>();
            List<string> Innercxt = new List<string>();
            List<string> InnerIds = new List<string>();
            foreach (KeyValuePair<string, SingleTable> entry in FormData.ExtendedTables)
            {
                foreach (SingleRow row in entry.Value)
                {
                    string cn = entry.Key + "_" + i.ToString();
                    i++;
                    InnerVals.Add(string.Format("( CONCAT('{0}_', TRIM(CAST(eb_currval('{1}_id_seq') AS CHAR(32))), '_{2}'))", EbObId, this.TableName, entry.Key));
                    param.Add(DataDB.GetNewParameter(cn, EbDbTypes.Decimal, row.Columns[0].Value));
                    InnerIds.Add(":" + cn);
                }
                Innercxt.Add("context = CONCAT('" + EbObId + "_', TRIM(CAST(eb_currval('" + this.TableName + "_id_seq') AS CHAR(32))), '_" + entry.Key + "')");
            }

            if (InnerVals.Count > 0)
            {

                for (int k = 0; k < InnerVals.Count; k++)
                {
                    fullqry += string.Format(@"UPDATE 
                                            eb_files_ref AS t
                                        SET
                                            context = {0}                                        
                                        WHERE
                                           t.id = {1} AND t.eb_del = 'F';", InnerVals[k], InnerIds[k]);
                }

                //fullqry += string.Format(@"UPDATE 
                //                            eb_files_ref AS t
                //                        SET
                //                            context = c.context
                //                        FROM
                //                            (VALUES{0}) AS c(id, context)
                //                        WHERE
                //                            c.id = t.id AND t.eb_del = 'F';", InnerVals.Join(","));
                fullqry += string.Format(@"UPDATE eb_files_ref 
                                        SET eb_del='T' 
                                        WHERE ({0}) AND eb_del='F' AND id NOT IN ({1});", Innercxt.Join(" OR "), InnerIds.Join(","));
            }
            //-----------------------------------------------------------------------------

            param.Add(DataDB.GetNewParameter("eb_createdby", EbDbTypes.Int32, this.UserObj.UserId));
            //param.Add(DataDB.GetNewParameter("eb_createdby_s", EbDbTypes.String, this.UserObj.FullName));
            param.Add(DataDB.GetNewParameter("eb_loc_id", EbDbTypes.Int32, this.LocationId));
            //param.Add(DataDB.GetNewParameter("eb_loc_s", EbDbTypes.String, this.SolutionObj.Locations.ContainsKey(this.LocationId) ? this.SolutionObj.Locations[this.LocationId].ShortName : string.Empty));
            //param.Add(DataDB.GetNewParameter("eb_auto_id", EbDbTypes.String, FormData.AutoIdText ?? string.Empty));
            //fullqry += string.Format("UPDATE {0} SET eb_auto_id = :eb_auto_id || cur_val('{0}_id_seq')::text WHERE id = cur_val('{0}_id_seq');", this.TableName);
            fullqry += string.Concat("SELECT eb_currval('", this.TableName, "_id_seq');");

            EbDataTable temp = DataDB.DoQuery(fullqry, param.ToArray());
            int _rowid = temp.Rows.Count > 0 ? Convert.ToInt32(temp.Rows[0][0]) : 0;
            //int _rowid = temp.Tables[temp.Tables.Count - 1].Rows.Count > 0 ? Convert.ToInt32(temp.Tables[temp.Tables.Count - 1].Rows[0][0]) : 0;
            return _rowid;
        }

        public int AfterSave(IDatabase DataDB, bool IsUpdate)
        {
            string q = string.Empty;
            if (this.AfterSaveRoutines != null && this.AfterSaveRoutines.Count > 0)
            {
                foreach (EbRoutines e in this.AfterSaveRoutines)
                {
                    if (IsUpdate && !e.IsDisabledOnEdit)
                        q += e.Script.Code + ";";
                    else if (!IsUpdate && !e.IsDisabledOnNew)
                        q += e.Script.Code + ";";
                }
            }
            if (!q.Equals(string.Empty))
            {
                List<DbParameter> param = new List<DbParameter>();
                foreach (KeyValuePair<string, SingleTable> item in this.FormData.MultipleTables)
                {
                    foreach (SingleColumn rField in item.Value[0].Columns)
                    {
                        if (q.Contains(":" + item.Key + "_" + rField.Name))
                        {
                            if (rField.Value == null)
                            {
                                var p = DataDB.GetNewParameter(item.Key + "_" + rField.Name, (EbDbTypes)rField.Type);
                                p.Value = DBNull.Value;
                                param.Add(p);
                            }
                            else
                                param.Add(DataDB.GetNewParameter(item.Key + "_" + rField.Name, (EbDbTypes)rField.Type, rField.Value));
                        }
                    }
                }
                return DataDB.InsertTable(q, param.ToArray());
            }
            return -1;
        }

        private bool CanDelete(IDatabase DataDB)
        {
            if (this.DisableDelete != null && this.DisableDelete.Count > 0)
            {
                string q = string.Join(";", this.DisableDelete.Select(e => e.Script.Code));
                DbParameter[] p = new DbParameter[] {
                    DataDB.GetNewParameter("id", EbDbTypes.Int32, this.TableRowId)
                };
                EbDataSet ds = DataDB.DoQueries(q, p);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0].Count > 0)
                    {
                        if (!this.DisableDelete[i].IsDisabled && Convert.ToInt32(ds.Tables[0].Rows[0][0]) > 0 && !this.DisableDelete[i].IsWarningOnly)
                            return false;
                    }
                }
            }
            return true;
        }

        public int Delete(IDatabase DataDB)
        {
            if (this.CanDelete(DataDB))
            {
                string query = this.GetDeleteQuery(DataDB);
                DbParameter[] param = new DbParameter[] {
                    DataDB.GetNewParameter("eb_lastmodified_by", EbDbTypes.Int32, this.UserObj.UserId),
                    DataDB.GetNewParameter("id", EbDbTypes.Int32, this.TableRowId)
                };
                return DataDB.UpdateTable(query, param);
            }
            return -1;
        }

        private bool CanCancel(IDatabase DataDB)
        {
            if (this.DisableCancel != null && this.DisableCancel.Count > 0)
            {
                string q = string.Join(";", this.DisableCancel.Select(e => e.Script.Code));
                DbParameter[] p = new DbParameter[] {
                    DataDB.GetNewParameter("id", EbDbTypes.Int32, this.TableRowId)
                };
                EbDataSet ds = DataDB.DoQueries(q, p);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    if (ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0].Count > 0)
                    {
                        if (!this.DisableCancel[i].IsDisabled && Convert.ToInt32(ds.Tables[0].Rows[0][0]) > 0 && !this.DisableCancel[i].IsWarningOnly)
                            return false;
                    }
                }
            }
            return true;
        }

        public int Cancel(IDatabase DataDB)
        {
            if (this.CanCancel(DataDB))
            {
                string query = this.GetCancelQuery(DataDB);
                DbParameter[] param = new DbParameter[] {
                    DataDB.GetNewParameter("eb_lastmodified_by", EbDbTypes.Int32, this.UserObj.UserId),
                    DataDB.GetNewParameter("id", EbDbTypes.Int32, this.TableRowId)
                };
                return DataDB.UpdateTable(query, param);
            }
            return -1;
        }

        private void ExeDeleteCancelScript(IDatabase DataDB)
        {
            string q = string.Empty;
            if (this.DisableDelete != null && this.DisableDelete.Count > 0)
            {
                q = string.Join(";", this.DisableDelete.Select(e => e.Script.Code));
            }
            if (this.DisableCancel != null && this.DisableCancel.Count > 0)
            {
                q += string.Join(";", this.DisableCancel.Select(e => e.Script.Code));
            }
            if (!q.Equals(string.Empty))
            {
                DbParameter[] p = new DbParameter[] {
                    DataDB.GetNewParameter("id", EbDbTypes.Int32, this.TableRowId)
                };
                EbDataSet ds = DataDB.DoQueries(q, p);
                int i = 0;
                for (; i < this.DisableDelete.Count; i++)
                {
                    if (ds.Tables[i].Rows.Count > 0 && ds.Tables[i].Rows[0].Count > 0)
                    {
                        if (this.DisableDelete[i].IsDisabled || Convert.ToInt32(ds.Tables[i].Rows[0][0]) == 0)
                        {
                            this.FormData.DisableDelete.Add(this.DisableDelete[i].Name, false);
                        }
                        else
                        {
                            this.FormData.DisableDelete.Add(this.DisableDelete[i].Name, true);
                        }
                    }
                }

                for (int j = 0; j < this.DisableCancel.Count; i++, j++)
                {
                    if (ds.Tables[i].Rows.Count > 0 && ds.Tables[i].Rows[0].Count > 0)
                    {
                        if (this.DisableCancel[j].IsDisabled || Convert.ToInt32(ds.Tables[i].Rows[0][0]) == 0)
                        {
                            this.FormData.DisableCancel.Add(this.DisableCancel[j].Name, false);
                        }
                        else
                        {
                            this.FormData.DisableCancel.Add(this.DisableCancel[j].Name, true);
                        }
                    }
                }
            }
        }

        private void UpdateAuditTrail(IDatabase DataDB)
        {
            List<AuditTrailEntry> FormFields = new List<AuditTrailEntry>();
            foreach (KeyValuePair<string, SingleTable> entry in this.FormData.MultipleTables)
            {
                bool IsGridTable = false;
                TableSchema _table = this.FormSchema.Tables.FirstOrDefault(tbl => tbl.TableName.Equals(entry.Key));
                if (_table != null)
                    IsGridTable = _table.TableType == WebFormTableTypes.Grid;

                if (this.FormDataBackup == null || !this.FormDataBackup.MultipleTables.ContainsKey(entry.Key))//insert mode
                {
                    foreach (SingleRow rField in entry.Value)
                    {
                        this.PushAuditTrailEntry(entry.Key, rField, FormFields, true, IsGridTable, _table);
                    }
                }
                else//update mode
                {
                    List<string> rids = new List<string>();
                    foreach (SingleRow rField in entry.Value)
                    {
                        rids.Add(rField.RowId);
                        SingleRow orF = this.FormDataBackup.MultipleTables[entry.Key].Find(e => e.RowId == rField.RowId);
                        if (orF == null)//if it is new row
                        {
                            this.PushAuditTrailEntry(entry.Key, rField, FormFields, true, IsGridTable, _table);
                        }
                        else//row edited
                        {
                            string relation = string.Concat(this.TableRowId, "-", rField.RowId);

                            if (this.FormSchema.MasterTable.Equals(entry.Key))
                                relation = this.TableRowId.ToString();

                            bool IsRowEdited = false;
                            Dictionary<string, string> dic1 = null;
                            Dictionary<string, string> dic2 = null;
                            if (IsGridTable)
                            {
                                dic1 = new Dictionary<string, string>();
                                dic2 = new Dictionary<string, string>();
                            }
                            foreach (SingleColumn cField in rField.Columns)
                            {
                                if (cField.Name.Equals("id"))//skipping 'id' field
                                    continue;
                                ColumnSchema _column = _table.Columns.Find(c => c.ColumnName.Equals(cField.Name));
                                if (_column != null)
                                {
                                    if ((_column.Control as EbControl).DoNotPersist)//skip DoNotPersist field from audit entry// written for EbSystemControls
                                        continue;
                                }
                                SingleColumn ocf = orF.Columns.Find(e => e.Name == cField.Name);

                                if (ocf == null)
                                {
                                    ocf = new SingleColumn() { Name = cField.Name, Value = "[null]" };
                                }
                                if (IsGridTable)
                                {
                                    dic1.Add(cField.Name, cField.Value == null ? "[null]" : cField.Value.ToString());
                                    dic2.Add(ocf.Name, ocf.Value == null ? "[null]" : ocf.Value.ToString());
                                }
                                if (ocf.Value != cField.Value)//checking for changes /////// modifications required
                                {
                                    IsRowEdited = true;
                                    if (IsGridTable)
                                        continue;

                                    FormFields.Add(new AuditTrailEntry
                                    {
                                        Name = cField.Name,
                                        NewVal = cField.Value == null ? "[null]" : cField.Value.ToString(),
                                        OldVal = ocf.Value == null ? "[null]" : ocf.Value.ToString(),
                                        DataRel = relation,
                                        TableName = entry.Key
                                    });
                                }
                            }
                            if (IsGridTable && IsRowEdited)
                            {
                                FormFields.Add(new AuditTrailEntry
                                {
                                    Name = "dgrow",
                                    NewVal = JsonConvert.SerializeObject(dic1),
                                    OldVal = JsonConvert.SerializeObject(dic2),
                                    DataRel = relation,
                                    TableName = entry.Key
                                });
                            }
                        }
                    }
                    foreach (SingleRow Row in this.FormDataBackup.MultipleTables[entry.Key])//looking for deleted rows
                    {
                        if (!rids.Contains(Row.RowId))
                        {
                            this.PushAuditTrailEntry(entry.Key, Row, FormFields, false, IsGridTable, _table);
                        }
                    }
                }
            }
            if (FormFields.Count > 0)
            {
                if (this.FormDataBackup == null)
                    UpdateAuditTrail(DataDB, 1, FormFields);
                else
                    UpdateAuditTrail(DataDB, 2, FormFields);
            }

        }

        //managing new or deleted row
        private void PushAuditTrailEntry(string Table, SingleRow Row, List<AuditTrailEntry> FormFields, bool IsIns, bool IsGridRow, TableSchema _table)
        {
            string relation = string.Concat(this.TableRowId, "-", Row.RowId);

            if (this.FormSchema.MasterTable.Equals(Table))
                relation = this.TableRowId.ToString();

            if (IsGridRow)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (SingleColumn cField in Row.Columns)
                {
                    if (cField.Name.Equals("id"))//skipping 'id' field
                        continue;
                    dic.Add(cField.Name, cField.Value == null ? "[null]" : cField.Value.ToString());
                }
                string val = JsonConvert.SerializeObject(dic);
                FormFields.Add(new AuditTrailEntry
                {
                    Name = "dgrow",
                    NewVal = IsIns ? val : "[null]",
                    OldVal = IsIns ? "[null]" : val,
                    DataRel = relation,
                    TableName = Table
                });
            }
            else
            {
                foreach (SingleColumn cField in Row.Columns)
                {
                    if (cField.Name.Equals("id"))//skipping 'id' field
                        continue;
                    ColumnSchema _column = _table.Columns.Find(c => c.ColumnName.Equals(cField.Name));
                    if (_column != null)
                    {
                        if ((_column.Control as EbControl).DoNotPersist)//skip DoNotPersist field from audit entry// written for EbSystemControls
                            continue;
                    }

                    FormFields.Add(new AuditTrailEntry
                    {
                        Name = cField.Name,
                        NewVal = IsIns && cField.Value != null ? cField.Value.ToString() : "[null]",
                        OldVal = !IsIns && cField.Value != null ? cField.Value.ToString() : "[null]",
                        DataRel = relation,
                        TableName = Table
                    });
                }
            }
        }

        private int UpdateAuditTrail(IDatabase DataDB, int Action, List<AuditTrailEntry> _Fields)
        {
            List<DbParameter> parameters = new List<DbParameter>();
            parameters.Add(DataDB.GetNewParameter("formid", EbDbTypes.String, this.RefId));
            parameters.Add(DataDB.GetNewParameter("dataid", EbDbTypes.Int32, this.TableRowId));
            parameters.Add(DataDB.GetNewParameter("actiontype", EbDbTypes.Int32, Action));
            parameters.Add(DataDB.GetNewParameter("eb_createdby", EbDbTypes.Int32, this.UserObj.UserId));
            string Qry = DataDB.EB_UPDATEAUDITTRAIL;
            EbDataTable dt = DataDB.DoQuery(Qry, parameters.ToArray());
            var id = Convert.ToInt32(dt.Rows[0][0]);

            string lineQry = "INSERT INTO eb_audit_lines(masterid, fieldname, oldvalue, newvalue, idrelation, tablename) VALUES ";
            List<DbParameter> parameters1 = new List<DbParameter>();
            parameters1.Add(DataDB.GetNewParameter("masterid", EbDbTypes.Int32, id));
            for (int i = 0; i < _Fields.Count; i++)
            {
                lineQry += string.Format("(:masterid, :{0}_{1}, :old{0}_{1}, :new{0}_{1}, :idrel{0}_{1}, :tblname{0}_{1}),", _Fields[i].Name, i);
                parameters1.Add(DataDB.GetNewParameter(_Fields[i].Name + "_" + i, EbDbTypes.String, _Fields[i].Name));
                parameters1.Add(DataDB.GetNewParameter("new" + _Fields[i].Name + "_" + i, EbDbTypes.String, _Fields[i].NewVal));
                parameters1.Add(DataDB.GetNewParameter("old" + _Fields[i].Name + "_" + i, EbDbTypes.String, _Fields[i].OldVal));
                parameters1.Add(DataDB.GetNewParameter("idrel" + _Fields[i].Name + "_" + i, EbDbTypes.String, _Fields[i].DataRel));
                parameters1.Add(DataDB.GetNewParameter("tblname" + _Fields[i].Name + "_" + i, EbDbTypes.String, _Fields[i].TableName));
            }
            return DataDB.DoNonQuery(lineQry.Substring(0, lineQry.Length - 1), parameters1.ToArray());
        }

        //Get the latest data as a transaction
        //private FormTransaction GetCurrTransationAll()
        //{
        //    FormTransaction curTransAll = new FormTransaction();
        //    foreach (TableSchema Table in this.FormSchema.Tables)
        //    {
        //        if (Table.IsGridTable)
        //        {
        //            curTransAll.GridTables.Add(Table.TableName, new FormTransactionTable());
        //            foreach (SingleRow Row in this.FormData.MultipleTables[Table.TableName])
        //            {
        //                int _rowid = Convert.ToInt32(Row.RowId);
        //                curTransAll.GridTables[Table.TableName].Rows.Add(_rowid, new FormTransactionRow());
        //                foreach (SingleColumn Column in Row.Columns)
        //                {
        //                    if (!Column.Name.Equals("id"))//skipping id field
        //                        curTransAll.GridTables[Table.TableName].Rows[_rowid].Columns.Add(Column.Name, new FormTransactionEntry { NewValue = Column.Value.ToString(), OldValue = Column.Value.ToString() });
        //                }
        //            }
        //        }
        //        else
        //        {
        //            curTransAll.Tables.Add(Table.TableName, new FormTransactionRow());
        //            foreach (SingleColumn Column in this.FormData.MultipleTables[Table.TableName][0].Columns)
        //            {
        //                if (!Column.Name.Equals("id"))//skipping id
        //                    curTransAll.Tables[Table.TableName].Columns.Add(Column.Name, new FormTransactionEntry { NewValue = Column.Value.ToString(), OldValue = Column.Value.ToString() });
        //            }
        //        }
        //    }
        //    return curTransAll;
        //}

        public string GetAuditTrail(IDatabase DataDB, Service Service)
        {
            this.RefreshFormData(DataDB, Service);
            //FormTransaction curTransAll = this.GetCurrTransationAll();
            Dictionary<string, string> DictVmAll = new Dictionary<string, string>();

            string qry = @"	SELECT 
            	m.id, u.fullname, m.eb_createdby, m.eb_createdat, m.actiontype, l.tablename, l.fieldname, l.idrelation, l.oldvalue, l.newvalue
            FROM 
            	eb_audit_master m, eb_audit_lines l, eb_users u
            WHERE
            	m.id = l.masterid AND m.eb_createdby = u.id AND m.formid = :formid AND m.dataid = :dataid
            ORDER BY
            	m.id DESC, l.tablename, l.idrelation;";
            DbParameter[] parameters = new DbParameter[] {
                     DataDB.GetNewParameter("formid", EbDbTypes.String, this.RefId),
                     DataDB.GetNewParameter("dataid", EbDbTypes.Int32, this.TableRowId)
                 };
            EbDataTable dt = DataDB.DoQuery(qry, parameters);

            Dictionary<int, FormTransaction> Trans = new Dictionary<int, FormTransaction>();
            TableSchema _table = null;
            ColumnSchema _column = null;

            foreach (EbDataRow dr in dt.Rows)
            {
                int m_id = Convert.ToInt32(dr["id"]);
                string new_val = dr["newvalue"].ToString();
                string old_val = dr["oldvalue"].ToString();

                if (_table == null || !_table.TableName.Equals(dr["tablename"].ToString()))
                {
                    _table = this.FormSchema.Tables.FirstOrDefault(tbl => tbl.TableName == dr["tablename"].ToString());
                    if (_table == null)//skipping invalid Audit Trail entry
                        continue;
                }

                if (_table.TableType != WebFormTableTypes.Grid)
                {
                    _column = _table.Columns.FirstOrDefault(col => col.ColumnName == dr["fieldname"].ToString());
                    if (_column == null)//skipping invalid Audit Trail entry
                        continue;
                }

                if (!Trans.ContainsKey(m_id))
                {
                    Trans.Add(m_id, new FormTransaction()
                    {
                        ActionType = Convert.ToInt32(dr["actiontype"]) == 1 ? "Insert" : "Update",
                        CreatedBy = dr["fullname"].ToString(),
                        CreatedById = dr["eb_createdby"].ToString(),
                        CreatedAt = Convert.ToDateTime(dr["eb_createdat"]).ConvertFromUtc(this.UserObj.Preference.TimeZone).ToString(this.UserObj.Preference.GetShortDatePattern() + " " + this.UserObj.Preference.GetShortTimePattern(), CultureInfo.InvariantCulture)
                    });
                }

                string[] ids = dr["idrelation"].ToString().Split('-');

                if (_table.TableType == WebFormTableTypes.Grid)
                {
                    Dictionary<string, string> new_val_dict = new_val == "[null]" ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(new_val);
                    Dictionary<string, string> old_val_dict = old_val == "[null]" ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(old_val);
                    if (new_val_dict == null)
                    {
                        new_val_dict = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, string> entry in old_val_dict)
                        {
                            new_val_dict.Add(entry.Key, "[null]");
                        }
                    }
                    else if (old_val_dict == null)
                    {
                        old_val_dict = new Dictionary<string, string>();
                        foreach (KeyValuePair<string, string> entry in new_val_dict)
                        {
                            old_val_dict.Add(entry.Key, "[null]");
                        }
                    }

                    foreach (ColumnSchema __column in _table.Columns)
                    {
                        if (!Trans[m_id].GridTables.ContainsKey(_table.TableName))
                        {
                            Trans[m_id].GridTables.Add(_table.TableName, new FormTransactionTable() { Title = _table.Title });
                            for (int i = 0; i < _table.Columns.Count; i++)
                            {
                                if (_table.Columns.ElementAt(i).Control is EbDGColumn)
                                {
                                    if (_table.Columns.ElementAt(i).Control is EbDGUserControlColumn)
                                        continue;
                                    else
                                        Trans[m_id].GridTables[_table.TableName].ColumnMeta.Add(i, (_table.Columns.ElementAt(i).Control as EbDGColumn).Title);
                                }
                                else
                                    Trans[m_id].GridTables[_table.TableName].ColumnMeta.Add(i, (_table.Columns.ElementAt(i).Control as EbControl).Label);
                            }
                        }
                        int curid = Convert.ToInt32(ids[1]);
                        FormTransactionTable TblRef = Trans[m_id].GridTables[_table.TableName];
                        if (!TblRef.Rows.ContainsKey(curid))
                        {
                            TblRef.Rows.Add(curid, new FormTransactionRow() { });
                        }
                        bool IsModified = false;
                        if (!new_val_dict.ContainsKey(__column.ColumnName))
                            new_val_dict.Add(__column.ColumnName, "[null]");
                        if (!old_val_dict.ContainsKey(__column.ColumnName))
                            old_val_dict.Add(__column.ColumnName, "[null]");

                        if (new_val_dict[__column.ColumnName] != old_val_dict[__column.ColumnName])
                            IsModified = true;
                        string a = old_val_dict[__column.ColumnName];
                        string b = new_val_dict[__column.ColumnName];
                        PreProcessTransationData(DictVmAll, _table, __column, ref a, ref b);
                        TblRef.Rows[curid].Columns.Add(__column.ColumnName, new FormTransactionEntry() { OldValue = a, NewValue = b, IsModified = IsModified });
                    }
                }
                else
                {
                    if (!Trans[m_id].Tables.ContainsKey(_table.TableName))
                        Trans[m_id].Tables.Add(_table.TableName, new FormTransactionRow() { });

                    PreProcessTransationData(DictVmAll, _table, _column, ref old_val, ref new_val);

                    FormTransactionEntry curtrans = new FormTransactionEntry()
                    {
                        OldValue = old_val,
                        NewValue = new_val,
                        IsModified = true,
                        Title = (_column.Control as EbControl).Label
                    };
                    Trans[m_id].Tables[_table.TableName].Columns.Add(_column.ColumnName, curtrans);
                }
            }
            PostProcessTransationData(DataDB, Service, Trans, DictVmAll);

            return JsonConvert.SerializeObject(Trans);
        }

        private void PreProcessTransationData(Dictionary<string, string> DictVmAll, TableSchema _table, ColumnSchema _column, ref string old_val, ref string new_val)
        {
            if (_column.Control is EbPowerSelect || _column.Control is EbDGPowerSelectColumn)//copy vm for dm
            {
                string key = string.Concat(_table.TableName, "_", _column.ColumnName);
                string temp = string.Empty;
                if (!(new_val.Equals(string.Empty) || new_val.Equals("[null]")))/////
                    temp = string.Concat(new_val, ",");
                if (!(old_val.Equals(string.Empty) || old_val.Equals("[null]")))/////
                    temp += string.Concat(old_val, ",");

                if (!temp.Equals(string.Empty))
                {
                    if (!DictVmAll.ContainsKey(key))
                        DictVmAll.Add(key, temp);
                    else
                        DictVmAll[key] = string.Concat(DictVmAll[key], temp);
                }
            }
            else if (_column.Control is EbDate || _column.Control is EbDGDateColumn)
            {
                if (!old_val.Equals("[null]"))
                {
                    old_val = DateTime.ParseExact(old_val, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString(this.UserObj.Preference.GetShortDatePattern(), CultureInfo.InvariantCulture);
                }
                if (!new_val.Equals("[null]"))
                {
                    new_val = DateTime.ParseExact(new_val, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString(this.UserObj.Preference.GetShortDatePattern(), CultureInfo.InvariantCulture);
                }
            }
        }

        private void PostProcessTransationData(IDatabase DataDB, Service Service, Dictionary<int, FormTransaction> Trans, Dictionary<string, string> DictVmAll)
        {
            string Qry = string.Empty;
            foreach (TableSchema _table in this.FormSchema.Tables)
            {
                foreach (ColumnSchema _column in _table.Columns)
                {
                    if (_column.Control is EbPowerSelect || _column.Control is EbDGPowerSelectColumn)
                    {
                        string key = string.Concat(_table.TableName, "_", _column.ColumnName);
                        if (DictVmAll.ContainsKey(key))
                        {
                            if (_column.Control is EbPowerSelect)
                                Qry += (_column.Control as EbPowerSelect).GetDisplayMembersQuery(Service, DictVmAll[key].Substring(0, DictVmAll[key].Length - 1));
                            else
                                Qry += (_column.Control as EbDGPowerSelectColumn).GetDisplayMembersQuery(Service, DictVmAll[key].Substring(0, DictVmAll[key].Length - 1));
                        }
                    }
                }
            }

            EbDataSet ds = DataDB.DoQueries(Qry);

            Dictionary<string, Dictionary<string, List<string>>> DictDm = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach (string key in DictVmAll.Keys)
                DictDm.Add(key, new Dictionary<string, List<string>>());

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                foreach (EbDataRow row in ds.Tables[i].Rows)
                {
                    List<string> list = new List<string>();
                    for (int j = 1; j < row.Count; j++)
                    {
                        list.Add(row[j].ToString());
                    }
                    if (!DictDm.ElementAt(i).Value.ContainsKey(row[0].ToString()))
                        DictDm.ElementAt(i).Value.Add(row[0].ToString(), list);
                }
            }

            foreach (KeyValuePair<int, FormTransaction> trans in Trans)
            {
                foreach (KeyValuePair<string, FormTransactionRow> table in trans.Value.Tables)
                {
                    ReplaceVmWithDm(table.Value.Columns, DictDm, table.Key);
                }

                foreach (KeyValuePair<string, FormTransactionTable> table in trans.Value.GridTables)
                {
                    foreach (KeyValuePair<int, FormTransactionRow> row in table.Value.Rows)
                    {
                        ReplaceVmWithDm(row.Value.Columns, DictDm, table.Key);
                    }
                }
            }
        }

        private void ReplaceVmWithDm(Dictionary<string, FormTransactionEntry> Columns, Dictionary<string, Dictionary<string, List<string>>> DictDm, string tablename)
        {
            foreach (KeyValuePair<string, FormTransactionEntry> column in Columns)
            {
                if (DictDm.ContainsKey(tablename + "_" + column.Key))
                {
                    if (column.Value.OldValue != "[null]")
                    {
                        string[] vm_arr = column.Value.OldValue.Split(',');
                        string dm = string.Empty;
                        for (int i = 0; i < vm_arr.Length; i++)
                        {
                            List<string> dmlist = DictDm[tablename + "_" + column.Key][vm_arr[i]];
                            foreach (string d in dmlist)
                            {
                                dm += " " + d;
                            }
                            if (i < vm_arr.Length - 1)
                                dm += "<br>";
                        }
                        column.Value.OldValue = dm;
                    }
                    if (column.Value.NewValue != "[null]")
                    {
                        string[] vm_arr = column.Value.NewValue.Split(',');
                        string dm = string.Empty;
                        for (int i = 0; i < vm_arr.Length; i++)
                        {
                            List<string> dmlist = DictDm[tablename + "_" + column.Key][vm_arr[i]];
                            foreach (string d in dmlist)
                            {
                                dm += " " + d;
                            }
                            if (i < vm_arr.Length - 1)
                                dm += "<br>";
                        }
                        column.Value.NewValue = dm;
                    }
                }
            }
        }

        public Dictionary<int, List<string>> GetLocBasedPermissions()
        {
            Dictionary<int, List<string>> _perm = new Dictionary<int, List<string>>();
            //New View Edit Delete Cancel Print AuditTrail

            foreach (int locid in this.SolutionObj.Locations.Keys)
            {
                List<string> _temp = new List<string>();
                foreach (EbOperation op in Operations.Enumerator)
                {
                    if (this.HasPermission(op.Name, locid))
                        _temp.Add(op.Name);
                }
                _perm.Add(locid, _temp);
            }
            return _perm;
        }

        private bool HasPermission(string ForWhat, int LocId)
        {
            if (this.UserObj.Roles.Contains(SystemRoles.SolutionOwner.ToString()) ||
                this.UserObj.Roles.Contains(SystemRoles.SolutionAdmin.ToString()) ||
                this.UserObj.Roles.Contains(SystemRoles.SolutionPM.ToString()))
                return true;

            EbOperation Op = EbWebForm.Operations.Get(ForWhat);
            if (!Op.IsAvailableInWeb)
                return false;

            try
            {
                string Ps = string.Concat(this.RefId.Split("-")[2].PadLeft(2, '0'), '-', this.RefId.Split("-")[3].PadLeft(5, '0'), '-', Op.IntCode.ToString().PadLeft(2, '0'));
                string t = this.UserObj.Permissions.FirstOrDefault(p => p.Substring(p.IndexOf("-") + 1).Equals(Ps + ":" + LocId) ||
                            (p.Substring(p.IndexOf("-") + 1, 11).Equals(Ps) && p.Substring(p.LastIndexOf(":") + 1).Equals("-1")));
                if (!t.IsNullOrEmpty())
                    return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when checking user permission(EbWebForm -> HasPermission): " + e.Message);
            }

            return false;
        }

        private WebFormSchema GetWebFormSchema()
        {
            WebFormSchema _formSchema = new WebFormSchema();
            _formSchema.FormName = this.Name;
            _formSchema.MasterTable = this.TableName.ToLower();
            _formSchema = GetWebFormSchemaRec(_formSchema, this, this.TableName.ToLower());
            this.FormSchema = _formSchema;
            return _formSchema;
        }

        private WebFormSchema GetWebFormSchemaRec(WebFormSchema _schema, EbControlContainer _container, string _parentTable)
        {
            IEnumerable<EbControl> _flatControls = _container.Controls.Get1stLvlControls();
            TableSchema _table = _schema.Tables.FirstOrDefault(tbl => tbl.TableName == _container.TableName);
            if (_table == null)
            {
                if(_container is EbApproval)
                    _table = new TableSchema { TableName = _container.TableName.ToLower(), ParentTable = _parentTable, TableType = WebFormTableTypes.Approval, Title = _container.Label };
                else if (_container is EbDataGrid)
                    _table = new TableSchema { TableName = _container.TableName.ToLower(), ParentTable = _parentTable, TableType = WebFormTableTypes.Grid, Title = _container.Label };
                else
                    _table = new TableSchema { TableName = _container.TableName.ToLower(), ParentTable = _parentTable, TableType = WebFormTableTypes.Normal };
                _schema.Tables.Add(_table);
            }
            foreach (EbControl control in _flatControls)
            {
                if (!control.DoNotPersist || control.IsSysControl)
                {
                    if (control is EbFileUploader)
                        _schema.ExtendedControls.Add(control);
                    else if (control is EbDGUserControlColumn)
                    {
                        foreach (EbControl _ctrl in (control as EbDGUserControlColumn).Columns)
                        {
                            _table.Columns.Add(new ColumnSchema { ColumnName = _ctrl.Name, EbDbType = (int)_ctrl.EbDbType, Control = _ctrl });
                        }
                    }
                    else
                        _table.Columns.Add(new ColumnSchema { ColumnName = control.Name, EbDbType = (int)control.EbDbType, Control = control });
                }
            }

            foreach (EbControl _control in _container.Controls)
            {
                if (_control is EbControlContainer)
                {
                    EbControlContainer Container = _control as EbControlContainer;
                    string __parentTbl = _parentTable;
                    if (Container.TableName.IsNullOrEmpty())
                        Container.TableName = _container.TableName;
                    else
                        __parentTbl = _container.TableName;
                    _schema = GetWebFormSchemaRec(_schema, Container, __parentTbl);
                }
            }
            return _schema;
        }

        public void AfterRedisGet(Service service)
        {
            EbFormHelper.AfterRedisGet(this, service);
            this.GetWebFormSchema();
        }

        public override void AfterRedisGet(RedisClient Redis, IServiceClient client)
        {
            EbFormHelper.AfterRedisGet(this, Redis, client);
        }

        public override string DiscoverRelatedRefids()
        {
            return EbFormHelper.DiscoverRelatedRefids(this);
        }
    }

    public static class EbFormHelper
    {
        public static string DiscoverRelatedRefids(EbControlContainer _this)
        {
            string refids = string.Empty;
            for (int i = 0; i < _this.Controls.Count; i++)
            {
                if (_this.Controls[i] is EbUserControl)
                {
                    refids += _this.Controls[i].RefId + ",";
                }
                else
                {
                    PropertyInfo[] _props = _this.Controls[i].GetType().GetProperties();
                    foreach (PropertyInfo _prop in _props)
                    {
                        if (_prop.IsDefined(typeof(OSE_ObjectTypes)))
                            refids += _prop.GetValue(_this.Controls[i], null).ToString() + ",";
                    }
                }
            }
            return refids;
        }

        public static void AfterRedisGet(EbControlContainer _this, RedisClient Redis, IServiceClient client)
        {
            try
            {
                for (int i = 0; i < _this.Controls.Count; i++)
                {
                    EbControl c = _this.Controls[i];
                    if (c is EbUserControl || c is EbDGUserControlColumn)
                    {
                        EbUserControl _temp = Redis.Get<EbUserControl>(c.RefId);
                        if (_temp == null)
                        {
                            var result = client.Get<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = c.RefId });
                            _temp = EbSerializers.Json_Deserialize(result.Data[0].Json);
                            Redis.Set<EbUserControl>(c.RefId, _temp);
                        }
                        //_temp.RefId = _this.Controls[i].RefId;
                        if (c is EbDGUserControlColumn)
                        {
                            foreach (EbControl Control in _temp.Controls)
                            {
                                RenameControlsRec(Control, c.Name);
                            }
                            (c as EbDGUserControlColumn).InitUserControl(_temp);
                        }
                        else
                        {
                            (c as EbUserControl).Controls = _temp.Controls;
                            foreach (EbControl Control in (c as EbUserControl).Controls)
                            {
                                RenameControlsRec(Control, c.Name);
                                //Control.ChildOf = "EbUserControl";
                                //Control.Name = c.Name + "_" + Control.Name;
                            }
                            //c = _temp;
                            c.AfterRedisGet(Redis, client);
                        }
                    }
                    else if (c is EbControlContainer)
                    {
                        AfterRedisGet(c as EbControlContainer, Redis, client);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION : FormAfterRedisGet " + e.Message);
            }
        }

        public static void AfterRedisGet(EbControlContainer _this, Service service)
        {
            try
            {
                for (int i = 0; i < _this.Controls.Count; i++)
                {
                    if (_this.Controls[i] is EbUserControl || _this.Controls[i] is EbDGUserControlColumn)
                    {
                        EbUserControl _temp = service.Redis.Get<EbUserControl>(_this.Controls[i].RefId);
                        if (_temp == null)
                        {
                            var result = service.Gateway.Send<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = _this.Controls[i].RefId });
                            _temp = EbSerializers.Json_Deserialize(result.Data[0].Json);
                            service.Redis.Set<EbUserControl>(_this.Controls[i].RefId, _temp);
                        }
                        //_temp.RefId = _this.Controls[i].RefId;
                        if (_this.Controls[i] is EbDGUserControlColumn)
                        {
                            foreach (EbControl Control in _temp.Controls)
                            {
                                RenameControlsRec(Control, _this.Controls[i].Name);
                            }
                            (_this.Controls[i] as EbDGUserControlColumn).InitUserControl(_temp);
                        }
                        else
                        {
                            (_this.Controls[i] as EbUserControl).Controls = _temp.Controls;
                            foreach (EbControl Control in (_this.Controls[i] as EbUserControl).Controls)
                            {
                                RenameControlsRec(Control, _this.Controls[i].Name);
                                //Control.ChildOf = "EbUserControl";
                                //Control.Name = _this.Controls[i].Name + "_" + Control.Name;
                            }
                        //_this.Controls[i] = _temp;
                        (_this.Controls[i] as EbUserControl).AfterRedisGet(service);
                        }
                    }
                    else if (_this.Controls[i] is EbControlContainer)
                    {
                        AfterRedisGet(_this.Controls[i] as EbControlContainer, service);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION : EbFormAfterRedisGet(service) " + e.Message);
            }
        }

        private static void RenameControlsRec(EbControl _control, string _ucName)
        {
            if (_control is EbControlContainer)
            {
                if (!(_control is EbUserControl))
                {
                    foreach (EbControl _ctrl in (_control as EbControlContainer).Controls)
                    {
                        RenameControlsRec(_ctrl, _ucName);
                    }
                }
            }
            else
            {
                _control.ChildOf = "EbUserControl";
                _control.Name = _ucName + "_" + _control.Name;
                _control.EbSid = _ucName + "_" + _control.EbSid;
            }
        }
    }

    public class EbControlWrapper
    {
        public string TableName { get; set; }

        public string Path { get; set; }

        public string Root { get; set; }

        public EbControl Control { get; set; }
    }

    public class EbSQLValidator : EbValidator
    {
        public EbSQLValidator() { }

        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.ScriptEditorCS)]
        public override EbScript Script { get; set; }
    }

    public class EbRoutines : EbValidator
    {
        public EbRoutines() { }

        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.ScriptEditorJS, PropertyEditorType.ScriptEditorCS)]
        public override EbScript Script { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        public bool IsDisabledOnNew { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        public bool IsDisabledOnEdit { get; set; }

        public override bool IsWarningOnly { get; set; }

        public override string FailureMSG { get; set; }

        public override bool IsDisabled { get; set; }
    }

    public class FormTransaction
    {
        public string CreatedBy { get; set; }

        public string CreatedById { get; set; }

        public string CreatedAt { get; set; }

        public string ActionType { get; set; }

        public bool MissingEntry { get; set; }

        public Dictionary<string, FormTransactionRow> Tables { get; set; }//Key = Table name

        public Dictionary<string, FormTransactionTable> GridTables { get; set; }//Key = Table name

        public FormTransaction()
        {
            this.Tables = new Dictionary<string, FormTransactionRow>();
            this.GridTables = new Dictionary<string, FormTransactionTable>();
        }
    }

    public class FormTransactionTable
    {
        public Dictionary<int, FormTransactionRow> Rows { get; set; }//Key = Row id

        public Dictionary<int, string> ColumnMeta { get; set; }

        public string Title { get; set; }

        public FormTransactionTable()
        {
            this.Rows = new Dictionary<int, FormTransactionRow>();
            this.ColumnMeta = new Dictionary<int, string>();
        }
    }

    public class FormTransactionRow
    {
        public Dictionary<string, FormTransactionEntry> Columns { get; set; }//Key = Column name

        public FormTransactionRow()
        {
            this.Columns = new Dictionary<string, FormTransactionEntry>();
        }

        public bool IsRowModified
        {
            get
            {
                foreach (KeyValuePair<string, FormTransactionEntry> col in this.Columns)
                {
                    if (col.Value.IsModified)
                        return true;
                }
                return false;
            }
        }
    }

    public class FormTransactionEntry
    {
        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public string Title { get; set; }

        public bool IsModified { get; set; }
    }
}
