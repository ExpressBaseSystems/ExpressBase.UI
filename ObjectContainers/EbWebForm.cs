﻿using ExpressBase.Common;
using ExpressBase.Common.Data;
using ExpressBase.Common.Enums;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.LocationNSolution;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.WebFormRelated;
using ExpressBase.Objects.Objects;
using ExpressBase.Objects.Objects.DVRelated;
using ExpressBase.Objects.ServiceStack_Artifacts;
using ExpressBase.Security;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack;
using ServiceStack.RabbitMq;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressBase.Common.Constants;
using ExpressBase.CoreBase.Globals;
using System.Net;
using System.Threading;
using ExpressBase.Common.Security;
using System.Threading.Tasks;

namespace ExpressBase.Objects
{
    [HideInToolBox]
    [EnableInBuilder(BuilderType.WebForm)]
    [BuilderTypeEnum(BuilderType.WebForm)]
    public class EbWebForm : EbForm
    {
        public EbWebForm()
        {
            //this.Validators = new List<EbValidator>();
            this.InfoVideoURLs = new List<EbURL>();
            this.DisableDelete = new List<EbSQLValidator>();
            this.DisableCancel = new List<EbSQLValidator>();
            this.BeforeSaveRoutines = new List<EbRoutines>();
            this.AfterSaveRoutines = new List<EbRoutines>();
            this.DataPushers = new List<EbDataPusher>();
            this.TitleExpression = new EbScript();
            this.PrintDocs = new List<ObjectBasicInfo>();
            this.Notifications = new List<EbFormNotification>();
        }

        public override int TableRowId { get; set; }

        public WebformData FormData { get; set; }

        public WebformData FormDataBackup { get; set; }

        public WebFormSchema FormSchema { get; set; }

        public User UserObj { get; set; }

        public int LocationId { get; set; }

        public Eb_Solution SolutionObj { get; set; }

        public FG_Root FormGlobals { get; set; }

        public bool IsLocEditable { get; set; }

        public int FormDataPusherCount { get; set; }

        public EbDataPusherConfig DataPusherConfig { get; set; }

        public EbWebFormCollection FormCollection { get; set; }

        public string __mode { get; set; }

        public EbAutoId AutoId { get; set; }

        public int DraftId { get; set; }

        public string CrudContext { get; set; }

        public MyActionNotification MyActNotification { get; set; }

        [JsonIgnore]
        internal DbConnection DbConnection { get; set; }

        [JsonIgnore]
        private DbTransaction DbTransaction { get; set; }

        [JsonIgnore]
        internal IRedisClient RedisClient { get; set; }

        public static EbOperations Operations = WFOperations.Instance;

        [PropertyGroup(PGConstants.HELP)]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyPriority(98)]
        [PropertyEditor(PropertyEditorType.FileUploader)]
        [Alias("Info Document")]
        [HelpText("Help information.")]
        [OnChangeExec(@"
        if((this.Info && this.Info.trim()) !== '' || (this.InfoVideoURLs && this.InfoVideoURLs.$values.length > 0)){
            pg.ShowProperty('InfoIcon');
        }
        else{
            pg.HideProperty('InfoIcon');
        }")]
        public override string Info { get; set; }

        [PropertyGroup(PGConstants.HELP)]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyPriority(98)]
        [Alias("Help Video URL")]
        [HelpText("Help video.")]
        public string InfoVideoURL { get; set; }

        [PropertyGroup(PGConstants.HELP)]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.UserControl)]
        [PropertyPriority(98)]
        [Alias("Help Videos URLs")]
        [HelpText("Help videos.")]
        [PropertyEditor(PropertyEditorType.Collection)]
        public virtual List<EbURL> InfoVideoURLs { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbSQLValidator> DisableDelete { get; set; }

        [PropertyGroup("Behavior")]
        [EnableInBuilder(BuilderType.WebForm)]
        public WebFormAfterSaveModes FormModeAfterSave { get; set; }

        [PropertyGroup("Behavior")]
        [EnableInBuilder(BuilderType.WebForm)]
        public PopupWebFormSize PopupFormSize { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbSQLValidator> DisableCancel { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbRoutine> Disable_Edit { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbRoutines> BeforeSaveRoutines { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbRoutines> AfterSaveRoutines { get; set; }

        [PropertyGroup("Miscellaneous")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.ObjectSelector)]
        [OSE_ObjectTypes(EbObjectTypes.iReport)]
        [HideInPropertyGrid]
        public string PrintDoc { get; set; }//deprecated 

        [PropertyGroup("Behavior")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.ObjectSelectorCollection)]
        [OSE_ObjectTypes(EbObjectTypes.iReport)]
        public List<ObjectBasicInfo> PrintDocs { get; set; }

        [EnableInBuilder(BuilderType.WebForm)]
        [OSE_ObjectTypes(EbObjectTypes.iTableVisualization)]
        [HideInPropertyGrid]
        public string AutoGeneratedVizRefId { get; set; }

        [PropertyGroup(PGConstants.EXTENDED)]
        [Alias("Auto deploy table view")]
        [EnableInBuilder(BuilderType.WebForm)]
        public bool AutoDeployTV { get; set; }

        [PropertyGroup(PGConstants.EXTENDED)]
        [EnableInBuilder(BuilderType.WebForm)]
        public bool CanSaveAsDraft { get; set; }

        [PropertyGroup(PGConstants.EXTENDED)]
        [EnableInBuilder(BuilderType.WebForm)]
        [DefaultPropValue("true")]
        public bool MakeEbSidUnique { get; set; }

        [PropertyGroup(PGConstants.EXTENDED)]
        [EnableInBuilder(BuilderType.WebForm)]
        public bool EnableExcelImport { get; set; }

        [PropertyGroup(PGConstants.EXTENDED)]
        [Alias("Is location independent")]
        [EnableInBuilder(BuilderType.WebForm)]
        public bool IsLocIndependent { get; set; }

        [PropertyGroup(PGConstants.DATA)]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbDataPusher> DataPushers { get; set; }

        [PropertyGroup(PGConstants.EXTENDED)]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.ScriptEditorJS)]
        [HelpText("Define Title Expression")]
        public EbScript TitleExpression { get; set; }

        [PropertyGroup("Events")]
        [EnableInBuilder(BuilderType.WebForm)]
        [PropertyEditor(PropertyEditorType.Collection)]
        public List<EbFormNotification> Notifications { get; set; }

        [PropertyGroup(PGConstants.EXTENDED)]
        [EnableInBuilder(BuilderType.WebForm)]
        public override bool IsDisable { get; set; }

        public EbWebForm ShallowCopy()
        {
            return (EbWebForm)this.MemberwiseClone();
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
            string html = "<form id='@ebsid@' isrendermode='@rmode@' ebsid='@ebsid@' class='formB-box form-buider-form ebcont-ctrl ebcont-inner' eb-form='true'  eb-root-obj-container ui-inp eb-type='WebForm' @tabindex@>";

            foreach (EbControl c in this.Controls)
                html += c.GetHtml();

            html += "</form>";

            html = html
                .Replace("@name@", this.Name)
                .Replace("@ebsid@", this.EbSid_CtxId)
                .Replace("@rmode@", IsRenderMode.ToString().ToLower())
                .Replace("@tabindex@", IsRenderMode ? string.Empty : " tabindex='1'");
            return Regex.Replace(html, @"( |\r?\n)\1+", "$1");
        }

        public void SetRedisClient(IRedisClient RedisClient)
        {
            this.RedisClient = RedisClient;
        }

        //Operations to be performed before form object save - table name required, table name repetition, calculate dependency
        public override void BeforeSave(IServiceClient serviceClient, IRedisClient redis)
        {
            BeforeSaveHelper.BeforeSave(this, serviceClient, redis);
        }

        public void BeforeSave(Service service)
        {
            BeforeSaveHelper.BeforeSave(this, null, null);
        }

        //import data from another form or api linked in power select
        public void PsImportData(IDatabase DataDB, Service Service, string Trigger /*int rowId*/)
        {
            EbControl[] Allctrls = this.Controls.FlattenAllEbControls();
            EbControl TriggerCtrl = Array.Find(Allctrls, c => c.Name == Trigger);

            if (TriggerCtrl == null)
                throw new FormException("Bad request", (int)HttpStatusCode.BadRequest, "Trigger control not found: " + Trigger, "EbWebForm -> PsImportData");
            if (!(TriggerCtrl is EbPowerSelect))
                throw new FormException("Bad request", (int)HttpStatusCode.BadRequest, $"{Trigger} is not power select. Type: {TriggerCtrl.GetType().Name}", "EbWebForm -> PsImportData");

            EbPowerSelect psCtrl = TriggerCtrl as EbPowerSelect;

            //combining normal tables for simple search
            Dictionary<string, SingleTable> Bkup_Mt = this.FormDataBackup.MultipleTables;
            foreach (TableSchema _t in this.FormSchema.Tables)
            {
                SingleTable Table = Bkup_Mt.ContainsKey(_t.TableName) && Bkup_Mt[_t.TableName].Count > 0 ? Bkup_Mt[_t.TableName] : null;
                if (Table == null || _t.TableName == this.FormSchema.MasterTable)
                    continue;
                if (_t.TableType == WebFormTableTypes.Normal && Table.Count > 0)
                    Bkup_Mt[this.FormSchema.MasterTable][0].Columns.AddRange(Table[0].Columns);
            }

            if (psCtrl.IsImportFromApi)
            {
                Dictionary<string, SingleTable> Tables = EbFormHelper.GetDataFormApi(Service, psCtrl, this, this.FormDataBackup, true);
                this.FormatImportData(DataDB, Service, this, Tables);
            }
            else// form import
            {
                if (string.IsNullOrEmpty(psCtrl.DataImportId))
                    throw new FormException("Something went wrong in our end.", (int)HttpStatusCode.InternalServerError, "DataImportId is null or empty in PowerSelect: " + Trigger, "EbWebForm -> PsImportData");

                SingleColumn psColumn = Bkup_Mt[this.FormSchema.MasterTable][0].Columns.Find(e => e.Name == psCtrl.Name);
                if (psColumn == null)
                    throw new FormException("Bad request", (int)HttpStatusCode.BadRequest, "Trigger control value not found in WebFormData: " + Trigger, "EbWebForm -> PsImportData");

                EbWebForm _form;

                if (this.RefId == psCtrl.DataImportId)
                {
                    _form = this;
                    _form.TableRowId = Convert.ToInt32(psColumn.Value);
                    _form.RefreshFormData(DataDB, Service);
                    this.FormData = EbFormHelper.GetFilledNewFormData(_form);
                }
                else
                {
                    _form = EbFormHelper.GetEbObject<EbWebForm>(psCtrl.DataImportId, null, Service.Redis, Service);
                    _form.RefId = psCtrl.DataImportId;
                    _form.UserObj = this.UserObj;
                    _form.SolutionObj = this.SolutionObj;
                    _form.AfterRedisGet_All(Service);
                    _form.TableRowId = Convert.ToInt32(psColumn.Value);
                    _form.RefreshFormData(DataDB, Service);
                    _form.FormatImportData(DataDB, Service, this);
                }
            }
        }

        //import data - using data reader in dg - from another form linked in ps 
        public void ImportData(IDatabase DataDB, Service Service, List<Param> Param, string Trigger, int RowId)
        {
            EbControl[] Allctrls = this.Controls.FlattenAllEbControls();
            EbControl TriggerCtrl = Array.Find(Allctrls, c => c.Name == Trigger);
            EbControl[] DGs = Array.FindAll(Allctrls, c => c is EbDataGrid);

            if (TriggerCtrl == null)
                throw new FormException("Bad request", (int)HttpStatusCode.BadRequest, "Trigger control not found: " + Trigger, "EbWebForm -> ImportData");

            this.FormData = new WebformData();

            if (TriggerCtrl.DependedDG != null && TriggerCtrl.DependedDG.Count > 0)// dg dr
            {
                //this.GetDGsEmptyModel();
                foreach (string dgName in TriggerCtrl.DependedDG)
                {
                    if (!(Array.Find(DGs, e => e.Name == dgName) is EbDataGrid _dg))
                    {
                        Console.WriteLine("DG not found:" + dgName);
                        break;
                    }

                    TableSchema _sc = this.FormSchema.Tables.Find(tbl => tbl.TableName == _dg.TableName);
                    if (_sc == null)
                    {
                        Console.WriteLine("Schema table not found:" + _dg.TableName);
                        break;
                    }

                    EbDataReader dataReader = EbFormHelper.GetEbObject<EbDataReader>(_dg.DataSourceId, null, Service.Redis, Service);
                    foreach (Param item in dataReader.GetParams(Service.Redis as RedisClient))
                    {
                        Param _p = Param.Find(p => p.Name == item.Name);
                        if (_p != null)
                            _p.Type = item.Type;
                        else
                            Console.WriteLine("Param not found in request: " + item.Name);//or throw Exception
                    }
                    DataSourceDataSetResponse response = Service.Gateway.Send<DataSourceDataSetResponse>(new DataSourceDataSetRequest { RefId = _dg.DataSourceId, Params = Param });

                    SingleTable Table = new SingleTable();
                    Dictionary<EbDGPowerSelectColumn, string> psDict = new Dictionary<EbDGPowerSelectColumn, string>();

                    int rowCounter = -501;
                    foreach (EbDataRow _row in response.DataSet.Tables[0].Rows)
                    {
                        SingleRow Row = new SingleRow();
                        if (_dg.IsLoadDataSourceInEditMode && RowId > 0 && _row[FormConstants.id] != null)
                            Row.RowId = Convert.ToInt32(_row[FormConstants.id]);// assuming id is RowId /////
                        else
                            Row.RowId = rowCounter--;
                        foreach (ColumnSchema _column in _sc.Columns)
                        {
                            EbDataColumn dc = response.DataSet.Tables[0].Columns[_column.ColumnName];
                            if (dc != null && !_row.IsDBNull(dc.ColumnIndex))
                            {
                                string _formattedData = Convert.ToString(_row[dc.ColumnIndex]);
                                if (_column.Control is EbDGPowerSelectColumn)
                                {
                                    if (!string.IsNullOrEmpty(_formattedData))
                                    {
                                        if (!psDict.ContainsKey(_column.Control as EbDGPowerSelectColumn))
                                            psDict.Add(_column.Control as EbDGPowerSelectColumn, _formattedData);
                                        else
                                            psDict[_column.Control as EbDGPowerSelectColumn] += CharConstants.COMMA + _formattedData;
                                    }
                                }
                                Row.Columns.Add(_column.Control.GetSingleColumn(this.UserObj, this.SolutionObj, _formattedData, false));
                            }
                            else
                                Row.Columns.Add(_column.Control.GetSingleColumn(this.UserObj, this.SolutionObj, null, true));
                        }

                        Table.Add(Row);
                    }
                    this.FormData.MultipleTables.Add(_dg.TableName, Table);

                    Dictionary<string, string> QrsDict = new Dictionary<string, string>();
                    List<DbParameter> param = new List<DbParameter>();
                    foreach (Param _p in Param)
                        param.Add(DataDB.GetNewParameter(_p.Name, (EbDbTypes)Convert.ToInt16(_p.Type), _p.Value));

                    foreach (KeyValuePair<EbDGPowerSelectColumn, string> psItem in psDict)
                    {
                        string t = psItem.Key.GetSelectQuery(DataDB, Service, psItem.Value);
                        QrsDict.Add(psItem.Key.EbSid, t);
                        foreach (Param _p in psItem.Key.ParamsList)
                        {
                            if (!param.Exists(e => e.ParameterName == _p.Name))
                                param.Add(DataDB.GetNewParameter(_p.Name, (EbDbTypes)Convert.ToInt16(_p.Type), _p.Value));
                        }
                    }
                    if (QrsDict.Count > 0)
                    {
                        EbFormHelper.AddExtraSqlParams(param, DataDB, this.TableName, RowId, this.LocationId, this.UserObj.UserId);

                        EbDataSet dataset = DataDB.DoQueries(string.Join(CharConstants.SPACE, QrsDict.Select(d => d.Value)), param.ToArray());
                        int i = 0;
                        foreach (KeyValuePair<string, string> item in QrsDict)
                        {
                            SingleTable Tbl = new SingleTable();
                            this.GetFormattedData(dataset.Tables[i++], Tbl);
                            this.FormData.PsDm_Tables.Add(item.Key, Tbl);
                        }
                        this.PostFormatFormData();
                    }
                }
            }

            else if (TriggerCtrl is EbPowerSelect && !string.IsNullOrEmpty((TriggerCtrl as EbPowerSelect).DataImportId))// ps import
            {
                Param[0].Type = ((int)EbDbTypes.Int32).ToString();
                EbWebForm _form = EbFormHelper.GetEbObject<EbWebForm>((TriggerCtrl as EbPowerSelect).DataImportId, null, Service.Redis, Service);
                _form.RefId = (TriggerCtrl as EbPowerSelect).DataImportId;
                _form.UserObj = this.UserObj;
                _form.SolutionObj = this.SolutionObj;
                _form.AfterRedisGet_All(Service);
                _form.TableRowId = Param[0].ValueTo;
                _form.FormatImportData(DataDB, Service, this);
                //this.FormData = _form.FormData;
            }
        }

        public void FormatImportData(IDatabase DataDB, Service Service, EbWebForm FormDes, Dictionary<string, SingleTable> _PsApiTables = null)//COPY this TO FormDes(Destination)
        {
            //mapping is based on ctrl name //different form
            //normal table columns are copying to master entry for easy search(not for Api import)
            if (this.FormData != null)
            {
                this.FormData.MultipleTables[this.TableName][0].Columns.Add(new SingleColumn()
                {
                    Name = this.TableName + FormConstants._id,
                    Type = (int)EbDbTypes.Int32,
                    Value = this.TableRowId
                });
                foreach (TableSchema _t in this.FormSchema.Tables)
                {
                    SingleTable Table = this.FormData.MultipleTables[_t.TableName];
                    if (_t.TableName == this.FormSchema.MasterTable)
                        continue;
                    if (_t.TableType == WebFormTableTypes.Normal && Table.Count > 0)
                        this.FormData.MultipleTables[this.FormSchema.MasterTable][0].Columns.AddRange(Table[0].Columns);
                }
            }

            FormDes.FormData = FormDes.GetEmptyModel();

            //Merge FormDes.FormData with Current DataMODEL in front end; hint: grid tables are not considered
            if (FormDes.FormDataBackup != null)
            {
                foreach (TableSchema _table in FormDes.FormSchema.Tables.FindAll(t => t.TableType == WebFormTableTypes.Normal))
                {
                    SingleTable Table = FormDes.FormData.MultipleTables.ContainsKey(_table.TableName) ? FormDes.FormData.MultipleTables[_table.TableName] : null;
                    if (Table == null) continue;
                    for (int i = 0; i < Table[0].Columns.Count; i++)
                    {
                        SingleColumn ColumnSrc = FormDes.FormDataBackup.MultipleTables[FormDes.FormSchema.MasterTable][0].Columns.Find(e => e.Name == Table[0].Columns[i].Name);
                        if (ColumnSrc != null)
                            Table[0].Columns[i] = Table[0].Columns[i].Control.GetSingleColumn(this.UserObj, this.SolutionObj, ColumnSrc.Value, false);
                    }
                }
            }

            Dictionary<EbControl, string> psDict = new Dictionary<EbControl, string>();
            List<DbParameter> psParams = new List<DbParameter>();

            if (_PsApiTables == null)
                EbFormHelper.CopyFormDataToFormData(DataDB, this, FormDes, psDict, psParams);
            else
                EbFormHelper.CopyApiDataToFormData(DataDB, _PsApiTables, FormDes, psDict, psParams);

            Dictionary<string, string> psQrsDict = new Dictionary<string, string>();
            List<EbControl> apiPsList = new List<EbControl>();
            foreach (KeyValuePair<EbControl, string> psItem in psDict)
            {
                IEbPowerSelect IPwrSel = psItem.Key as IEbPowerSelect;
                if (IPwrSel.IsDataFromApi)
                    apiPsList.Add(psItem.Key);
                else
                {
                    string t = IPwrSel.GetSelectQuery(DataDB, Service, psItem.Value);
                    psQrsDict.Add(psItem.Key.EbSid, t);
                }
            }
            if (psDict.Count > 0)
            {
                if (psQrsDict.Count > 0)
                {
                    EbFormHelper.AddExtraSqlParams(psParams, DataDB, FormDes.TableName, 0, this.LocationId, this.UserObj.UserId);
                    EbDataSet dataset = DataDB.DoQueries(string.Join(CharConstants.SPACE, psQrsDict.Select(d => d.Value)), psParams.ToArray());
                    int i = 0;
                    foreach (KeyValuePair<string, string> item in psQrsDict)
                    {
                        SingleTable Tbl = new SingleTable();
                        FormDes.GetFormattedData(dataset.Tables[i++], Tbl);
                        FormDes.FormData.PsDm_Tables.Add(item.Key, Tbl);
                    }
                }
                foreach (EbControl Ctrl in apiPsList)
                {
                    Dictionary<string, SingleTable> Tables = EbFormHelper.GetDataFormApi(Service, Ctrl, this, FormDes.FormData, false);
                    if (Tables.Count > 0)
                        FormDes.FormData.PsDm_Tables.Add(Ctrl.EbSid, Tables.ElementAt(0).Value);
                    else
                        FormDes.FormData.PsDm_Tables.Add(Ctrl.EbSid, new SingleTable());
                }
                FormDes.PostFormatFormData();
            }
        }

        public WebformData GetDynamicGridData(IDatabase DataDB, Service Service, string SrcId, string[] Target)
        {
            int pid = Convert.ToInt32(SrcId.Substring(0, SrcId.IndexOf(CharConstants.UNDERSCORE)));
            string ptbl = SrcId.Substring(SrcId.IndexOf(CharConstants.UNDERSCORE) + 1);

            string query = QueryGetter.GetDynamicGridSelectQuery(this, DataDB, Service, ptbl, Target, out string psQry, out int qryCnt);
            //psQry => /////// parameterization required to execute this

            EbDataSet dataset = DataDB.DoQueries(query + psQry, new DbParameter[]
            {
                DataDB.GetNewParameter(this.FormSchema.MasterTable + FormConstants._id, EbDbTypes.Int32, this.TableRowId),
                DataDB.GetNewParameter(ptbl + FormConstants._id, EbDbTypes.Int32, pid)
            });

            this.FormData = new WebformData() { MasterTable = this.FormSchema.MasterTable };

            for (int i = 0; i < Target.Length; i++)
            {
                TableSchema _table = this.FormSchema.Tables.Find(e => e.TableName == Target[i] && e.IsDynamic && e.TableType == WebFormTableTypes.Grid);

                SingleTable Table = new SingleTable();
                EbDataTable dataTable = dataset.Tables[i];//
                this.GetFormattedData(dataTable, Table, _table);
                if (!this.FormData.MultipleTables.ContainsKey(_table.TableName))
                    this.FormData.MultipleTables.Add(_table.TableName, Table);
            }

            if (!psQry.IsNullOrEmpty())
            {
                for (int i = 0, j = Target.Length; i < Target.Length && j < dataset.Tables.Count; i++)
                {
                    TableSchema _table = this.FormSchema.Tables.Find(e => e.TableName == Target[i] && e.IsDynamic && e.TableType == WebFormTableTypes.Grid);
                    foreach (ColumnSchema Col in _table.Columns.FindAll(e => !e.Control.DoNotPersist && e.Control is EbDGPowerSelectColumn && !(e.Control as EbDGPowerSelectColumn).IsDataFromApi))
                    {
                        SingleTable Tbl = new SingleTable();
                        this.GetFormattedData(dataset.Tables[j++], Tbl);
                        if (!this.FormData.PsDm_Tables.ContainsKey(Col.Control.EbSid))
                            this.FormData.PsDm_Tables.Add(Col.Control.EbSid, Tbl);
                    }
                }
                this.PostFormatFormData();
            }

            return this.FormData;
        }

        public object ExecuteSqlValueExpression(IDatabase DataDB, Service Service, List<Param> Param, string Trigger, int ExprType)
        {
            List<EbControl> Allctrls = this.Controls.GetAllControlsRecursively();
            EbControl TriggerCtrl = Allctrls.Find(e => e.Name == Trigger);
            if (TriggerCtrl == null)
                return null;
            object val = null;
            EbScript ebScript = ExprType == 1 ? TriggerCtrl.DefaultValueExpression : TriggerCtrl.ValueExpr;
            if (!string.IsNullOrWhiteSpace(ebScript?.Code) && ebScript.Lang == ScriptingLanguage.SQL)
            {
                try
                {
                    DbParameter[] parameters = new DbParameter[Param.Count];
                    for (int i = 0; i < Param.Count; i++)
                        parameters[i] = DataDB.GetNewParameter(Param[i].Name, (EbDbTypes)Convert.ToInt32(Param[i].Type), Param[i].ValueTo);

                    EbDataTable table = DataDB.DoQuery(ebScript.Code, parameters);
                    if (table.Rows.Count > 0)
                        val = table.Rows[0][0];
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception in ExecuteSqlValueExpression: {e.Message}\n{e.StackTrace}");
                }
            }
            return val;
        }

        public string GetDataPusherJson()
        {
            JObject Obj = new JObject();

            foreach (TableSchema _table in this.FormSchema.Tables)
            {
                JObject o = new JObject();
                foreach (ColumnSchema _column in _table.Columns)
                {
                    o[_column.ColumnName] = Convert.ToString((EbDbTypes)_column.EbDbType);
                }
                JArray array = new JArray();
                array.Add(o);
                Obj[_table.TableName] = array;
            }
            return Obj.ToString().Replace("\"", "'");
        }

        //merge formdata and webform object
        public void MergeFormData()
        {
            this.__mode = this.TableRowId > 0 ? "edit" : "new";

            MergeFormDataInner(this);

            foreach (TableSchema _table in this.FormSchema.Tables)
            {
                if (!_table.IsDynamic)
                    continue;
                if (!this.FormData.MultipleTables.ContainsKey(_table.TableName))
                    continue;
                foreach (SingleRow Row in this.FormData.MultipleTables[_table.TableName])
                {
                    if (string.IsNullOrEmpty(Row.pId))
                        throw new FormException("Parent id missing in dynamic entry", (int)HttpStatusCode.BadRequest, "Table : " + _table.TableName + ", Row Id : " + Row.RowId, "From EbWebForm.MergeFormData()");

                    int id = Convert.ToInt32(Row.pId.Substring(0, Row.pId.IndexOf(CharConstants.UNDERSCORE)));
                    string tbl = Row.pId.Substring(Row.pId.IndexOf(CharConstants.UNDERSCORE) + 1);

                    //if (tbl == this.FormSchema.MasterTable)
                    //    throw new FormException("Invalid table. Master table is not allowed for dynamic entry.", (int)HttpStatusCode.BadRequest, "Table : " + _table.TableName + ", Row Id : " + Row.RowId, "From EbWebForm.MergeFormData()");

                    SingleRow _row = this.FormData.MultipleTables[tbl].Find(e => e.RowId == id);

                    if (_row == null)
                        throw new FormException("Invalid data found in dynamic entry", (int)HttpStatusCode.BadRequest, "Table : " + _table.TableName + ", Row Id : " + Row.RowId, "From EbWebForm.MergeFormData()");
                    if (_row.IsDelete)
                        continue;

                    if (_row.LinesTable.Key == null)
                        _row.LinesTable = new KeyValuePair<string, SingleTable>(_table.TableName, new SingleTable());

                    _row.LinesTable.Value.Add(Row);
                }
                this.FormData.MultipleTables.Remove(_table.TableName);
            }
        }

        private void MergeFormDataInner(EbControlContainer _container)
        {
            foreach (EbControl c in _container.Controls)
            {
                if (c is EbDataGrid)
                {
                    EbDataGrid dgCtrl = c as EbDataGrid;
                    if (!FormData.MultipleTables.ContainsKey(dgCtrl.TableName))
                        continue;
                    foreach (EbControl control in dgCtrl.Controls)
                    {
                        if (!control.DoNotPersist)
                        {
                            List<object> val = new List<object>();
                            for (int i = 0; i < FormData.MultipleTables[dgCtrl.TableName].Count; i++)
                            {
                                if (FormData.MultipleTables[dgCtrl.TableName][i].GetColumn(control.Name) != null)
                                {
                                    val.Add(FormData.MultipleTables[dgCtrl.TableName][i][control.Name]);
                                    FormData.MultipleTables[dgCtrl.TableName][i].SetEbDbType(control.Name, control.EbDbType);
                                    FormData.MultipleTables[dgCtrl.TableName][i].SetControl(control.Name, control);
                                }
                            }
                            control.ValueFE = val;
                        }
                    }
                    int count = FormData.MultipleTables[dgCtrl.TableName].Count;
                    string str_row_num = this.SolutionObj.SolutionSettings.SystemColumns[SystemColumns.eb_row_num];
                    for (int i = 0, j = count; i < count; i++, j--)
                    {

                        if (FormData.MultipleTables[dgCtrl.TableName][i].GetColumn(str_row_num) == null)
                            FormData.MultipleTables[dgCtrl.TableName][i].Columns.Add(new SingleColumn
                            {
                                Name = str_row_num,
                                Type = (int)EbDbTypes.Int32,
                                Value = 0
                            });
                        if (dgCtrl.AscendingOrder)
                            FormData.MultipleTables[dgCtrl.TableName][i][str_row_num] = i + 1;
                        else
                            FormData.MultipleTables[dgCtrl.TableName][i][str_row_num] = j;
                    }
                }
                else if (c is EbReview)
                {
                    if (!c.DoNotPersist || this.TableRowId <= 0)// merge in edit mode only
                    {
                        EbReview ebReview = (c as EbReview);
                        if (FormData.MultipleTables.ContainsKey(ebReview.TableName) && FormData.MultipleTables[ebReview.TableName].Count > 0)
                        {
                            SingleTable rows = FormData.MultipleTables[ebReview.TableName];
                            for (int i = 0; i < rows.Count; i++)
                            {
                                if (rows[i].RowId > 0)
                                {
                                    rows.RemoveAt(i--);
                                }
                            }
                            if (rows.Count == 1)//one new entry// need to write code for 'AfterSaveRoutines'
                            {
                                foreach (TableSchema t in this.FormSchema.Tables)
                                {
                                    if (t.TableName != ebReview.TableName)
                                        FormData.MultipleTables.Remove(t.TableName);// approval execution, hence removing other data if present
                                }
                                string[] str_t = { "stage_unique_id", "action_unique_id", "eb_my_actions_id", "comments" };
                                for (int i = 0; i < str_t.Length; i++)
                                {
                                    EbControl con = ebReview.Controls.Find(e => e.Name == str_t[i]);
                                    FormData.MultipleTables[ebReview.TableName][0].SetEbDbType(con.Name, con.EbDbType);
                                    FormData.MultipleTables[ebReview.TableName][0].SetControl(con.Name, con);
                                }
                            }
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
                    if (!(this.FormData.MultipleTables.ContainsKey(_container.TableName) && this.FormData.MultipleTables[_container.TableName].Count > 0))
                        continue;
                    string patternVal = string.Empty;
                    EbAutoId ctrl = c as EbAutoId;
                    SingleRow Row = this.FormData.MultipleTables[_container.TableName][0];
                    if (ctrl.BypassParameterization)
                        patternVal = Convert.ToString(Row[ctrl.Name]);
                    else if (this.TableRowId == 0)// if new mode and not data pusher slave
                    {
                        Dictionary<string, string> dict = new Dictionary<string, string>
                        {
                            { "{currentlocation.id}", this.LocationId.ToString() },
                            { "{user.id}", this.UserObj.UserId.ToString() },
                            { "{currentlocation.shortname}", this.SolutionObj.Locations[this.LocationId].ShortName }
                        };

                        MatchCollection mc = Regex.Matches(ctrl.Pattern.sPattern, @"{(.*?)}");
                        foreach (Match m in mc)
                        {
                            if (dict.ContainsKey(m.Value))
                                ctrl.Pattern.sPattern = ctrl.Pattern.sPattern.Replace(m.Value, dict[m.Value]);
                        }
                        patternVal = ctrl.Pattern.sPattern;
                    }
                    if (Row.GetColumn(c.Name) == null)
                        Row.Columns.Add(new SingleColumn { Name = c.Name });
                    Row.SetEbDbType(c.Name, c.EbDbType);
                    Row.SetControl(c.Name, c);
                    Row[c.Name] = patternVal;
                    c.ValueFE = Row[c.Name];
                }
                else if (c is EbProvisionUser)
                {
                    if (!(this.FormData.MultipleTables.ContainsKey(_container.TableName) && this.FormData.MultipleTables[_container.TableName].Count > 0))
                        continue;
                    EbProvisionUser provUsrCtrl = c as EbProvisionUser;
                    Dictionary<string, string> _d = new Dictionary<string, string>();
                    bool EmailOrPhFound = false;

                    foreach (UsrLocField obj in provUsrCtrl.Fields)
                    {
                        if (!string.IsNullOrEmpty(obj.ControlName))
                        {
                            foreach (KeyValuePair<string, SingleTable> entry in this.FormData.MultipleTables)
                            {
                                TableSchema _table = this.FormSchema.Tables.Find(e => e.TableType == WebFormTableTypes.Normal && e.TableName == entry.Key);
                                if (_table != null && entry.Value.Count > 0)
                                {
                                    SingleColumn Col = entry.Value[0].GetColumn(obj.ControlName);
                                    if (Col != null)
                                    {
                                        _d.Add(obj.Name, Convert.ToString(Col.Value).Trim());///////////////
                                        if (obj.Name == FormConstants.email || obj.Name == FormConstants.phprimary)
                                            EmailOrPhFound = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    SingleRow Row = this.FormData.MultipleTables[_container.TableName][0];
                    SingleColumn Column = Row.GetColumn(provUsrCtrl.Name);
                    if (!EmailOrPhFound)
                    {
                        if (Column != null)
                            Row.Columns.Remove(Column);
                        Console.WriteLine("EbProvisionUser: Control skipped...");
                    }
                    else
                    {
                        if (Column == null)
                        {
                            Column = c.GetSingleColumn(this.UserObj, this.SolutionObj, null, true);
                            Row.Columns.Add(Column);
                        }
                        Column.F = JsonConvert.SerializeObject(_d);
                        c.ValueFE = Column.Value;
                        Row.SetEbDbType(c.Name, c.EbDbType);
                        Row.SetControl(c.Name, c);
                    }
                }
                else if (c is EbProvisionLocation)
                {
                    if (!(this.FormData.MultipleTables.ContainsKey(_container.TableName) && this.FormData.MultipleTables[_container.TableName].Count > 0))
                        continue;
                    EbProvisionLocation provLocCtrl = c as EbProvisionLocation;
                    Dictionary<string, string> _d = new Dictionary<string, string>();
                    Dictionary<string, string> _metaDict = new Dictionary<string, string>();
                    bool skipCtrl = false;

                    foreach (UsrLocField obj in provLocCtrl.Fields)
                    {
                        if (!string.IsNullOrEmpty(obj.ControlName))
                        {
                            bool ctrlFound = false;
                            foreach (KeyValuePair<string, SingleTable> entry in this.FormData.MultipleTables)
                            {
                                TableSchema _table = this.FormSchema.Tables.Find(e => e.TableType == WebFormTableTypes.Normal && e.TableName == entry.Key);
                                if (_table != null && entry.Value.Count > 0)
                                {
                                    SingleColumn Col = entry.Value[0].GetColumn(obj.ControlName);
                                    if (Col != null)
                                    {
                                        string _val = Convert.ToString(Col.Value).Trim();
                                        if (EbProvisionLocation.IsSystemField(obj.Name))
                                            _d.Add(obj.Name, _val);///////////////
                                        else
                                            _metaDict.Add(obj.DisplayName, _val);
                                        ctrlFound = true;
                                        break;
                                    }
                                }
                            }
                            if (obj.IsRequired && !ctrlFound)
                                skipCtrl = true;
                        }
                    }
                    SingleRow Row = this.FormData.MultipleTables[_container.TableName][0];
                    SingleColumn Column = Row.GetColumn(provLocCtrl.Name);
                    if (skipCtrl)
                    {
                        if (Column != null)
                            Row.Columns.Remove(Column);
                        Console.WriteLine("EbProvisionLocation: Control skipped...");
                    }
                    else
                    {
                        if (Column == null)
                        {
                            Column = c.GetSingleColumn(this.UserObj, this.SolutionObj, null, true);
                            Row.Columns.Add(Column);
                        }
                        _d.Add(FormConstants.meta_json, JsonConvert.SerializeObject(_metaDict));
                        Column.F = JsonConvert.SerializeObject(_d);
                        c.ValueFE = Column.Value;
                        Row.SetEbDbType(c.Name, c.EbDbType);
                        Row.SetControl(c.Name, c);
                    }
                }
                else if ((!(c is EbFileUploader) && !c.DoNotPersist))
                {
                    if (FormData.MultipleTables.ContainsKey(_container.TableName) && FormData.MultipleTables[_container.TableName].Count > 0)
                    {
                        if (FormData.MultipleTables[_container.TableName][0].GetColumn(c.Name) != null)
                        {
                            c.ValueFE = FormData.MultipleTables[_container.TableName][0][c.Name];
                            FormData.MultipleTables[_container.TableName][0].SetEbDbType(c.Name, c.EbDbType);
                            FormData.MultipleTables[_container.TableName][0].SetControl(c.Name, c);
                        }
                    }
                }
            }
        }

        public WebformData GetEmptyModel()
        {
            WebformData _FormData = new WebformData() { MasterTable = this.FormSchema.MasterTable };
            foreach (TableSchema _table in this.FormSchema.Tables)
            {
                if (_table.TableType == WebFormTableTypes.Normal)
                {
                    SingleRow Row = new SingleRow() { LocId = this.LocationId };
                    SingleTable Table = new SingleTable();
                    foreach (ColumnSchema _column in _table.Columns)
                        Row.Columns.Add(_column.Control.GetSingleColumn(this.UserObj, this.SolutionObj, null, true));
                    Table.Add(Row);
                    _FormData.MultipleTables.Add(_table.TableName, Table);
                }
                else if (_table.TableType == WebFormTableTypes.Grid)
                {
                    string str_row_num = this.SolutionObj.SolutionSettings.SystemColumns[SystemColumns.eb_row_num];
                    SingleRow Row = new SingleRow() { LocId = this.LocationId };
                    Row.Columns.Add(new SingleColumn()
                    {
                        Name = str_row_num,
                        Type = (int)EbDbTypes.Int32,
                        Value = 0
                    });
                    foreach (ColumnSchema _column in _table.Columns)
                        Row.Columns.Add(_column.Control.GetSingleColumn(this.UserObj, this.SolutionObj, null, true));

                    _FormData.MultipleTables.Add(_table.TableName, new SingleTable());
                    _FormData.DGsRowDataModel.Add(_table.TableName, Row);
                }
                else if (_table.TableType == WebFormTableTypes.Review)
                    _FormData.MultipleTables.Add(_table.TableName, new SingleTable());
            }
            return _FormData;
        }

        public void GetFormattedData(EbDataTable dataTable, SingleTable Table, TableSchema _table = null)
        {
            //master table eb columns : eb_loc_id, eb_ver_id, eb_lock, eb_push_id, eb_src_id, eb_created_by, eb_void, eb_created_at, eb_src_ver_id, eb_ro, id
            //normal table eb columns : eb_loc_id, id
            //grid table eb columns   : eb_loc_id, id, eb_row_num
            EbSystemColumns ebs = this.SolutionObj.SolutionSettings.SystemColumns;

            foreach (EbDataRow dataRow in dataTable.Rows)
            {
                int _locId = 0, i = 0, j = 0;
                int _rowId = 0;
                if (_table != null)
                {
                    _locId = Convert.ToInt32(dataRow[i++]);
                    if (_table.TableName.Equals(this.FormSchema.MasterTable))
                    {
                        if (this.FormData != null)
                        {
                            this.FormData.FormVersionId = Convert.ToInt32(dataRow[i++]);
                            this.FormData.IsLocked = ebs.GetBooleanValue(SystemColumns.eb_lock, dataRow[i++]);
                            string[] pushIdParts = dataRow[i++].ToString().Split("_");
                            this.FormData.SrcRefId = pushIdParts.Length == 2 ? pushIdParts[0] : string.Empty;
                            this.FormData.SrcDataId = Convert.ToInt32(dataRow[i++]);
                            this.FormData.CreatedBy = Convert.ToInt32(dataRow[i++]);
                            this.FormData.IsCancelled = ebs.GetBooleanValue(SystemColumns.eb_void, dataRow[i++]);
                            DateTime dt = Convert.ToDateTime(dataRow[i++]).ConvertFromUtc(this.UserObj.Preference.TimeZone);
                            this.FormData.CreatedAt = dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            this.FormData.SrcVerId = Convert.ToInt32(dataRow[i++]);
                            this.FormData.IsReadOnly = ebs.GetBooleanValue(SystemColumns.eb_ro, dataRow[i++]);
                        }
                        else
                            i += 9;// 9 => Count of above properties
                    }
                    _rowId = Convert.ToInt32(dataRow[i]);
                    if (_rowId <= 0)
                        throw new FormException("Something went wrong in our end.", (int)HttpStatusCode.InternalServerError, $"Invalid data found. TableName: {_table.TableName}, RowId: {_rowId}, LocId: {_locId}", "EbWebForm -> GetFormattedData");
                    for (; j < Table.Count; j++)
                    {
                        if (Table[j].RowId == _rowId)
                            break;
                    }
                    if (j < Table.Count)// skipping duplicate rows in dataTable
                        continue;
                }

                SingleRow Row = new SingleRow() { RowId = _rowId, LocId = _locId };
                if (_table != null)
                {
                    this.GetFormattedColumn(dataTable.Columns[FormConstants.id], dataRow, Row, null);
                    if (_table.TableType == WebFormTableTypes.Grid)
                        this.GetFormattedColumn(dataTable.Columns[ebs[SystemColumns.eb_row_num]], dataRow, Row, null);
                    for (int k = 0; k < _table.Columns.Count; k++)
                    {
                        EbControl _control = _table.Columns[k].Control;
                        this.GetFormattedColumn(dataTable.Columns[_control.Name.ToLower()], dataRow, Row, _control);// card field has uppercase name, but datatable contains lower case column name
                        if (_control is EbPhone && (_control as EbPhone).Sendotp)
                            (_control as EbPhone).GetVerificationStatus(dataTable.Columns[_control.Name.ToLower() + FormConstants._verified], dataRow, Row);
                    }
                }
                else
                {
                    for (int k = 0; k < dataTable.Columns.Count; k++)
                    {
                        this.GetFormattedColumn(dataTable.Columns[k], dataRow, Row, null);
                    }
                }
                Table.Add(Row);
            }
        }

        private void GetFormattedColumn(EbDataColumn dataColumn, EbDataRow dataRow, SingleRow Row, EbControl _control)
        {
            if (dataColumn == null && _control == null)
                throw new FormException("Something went wrong in our end", (int)HttpStatusCode.InternalServerError, "EbWebForm.GetFormattedColumn: dataColumn and _control is null", "RowId: " + Row.RowId);

            if (_control != null)
            {
                if ((_control.DoNotPersist && !_control.IsSysControl) || dataColumn == null)
                    Row.Columns.Add(_control.GetSingleColumn(this.UserObj, this.SolutionObj, null, true));
                else
                {
                    object val = dataRow[dataColumn.ColumnIndex];
                    if (dataRow.IsDBNull(dataColumn.ColumnIndex))
                        val = null;
                    Row.Columns.Add(_control.GetSingleColumn(this.UserObj, this.SolutionObj, val, false));
                }
                return;
            }

            object _formattedData;
            string _displayMember = null;

            if (dataColumn.Type == EbDbTypes.Date)
            {
                DateTime dt = Convert.ToDateTime(dataRow[dataColumn.ColumnIndex]);
                _formattedData = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                _displayMember = dt.ToString(this.UserObj.Preference.GetShortDatePattern(), CultureInfo.InvariantCulture);
            }
            else if (dataColumn.Type == EbDbTypes.Decimal || dataColumn.Type == EbDbTypes.Double)
            {
                _formattedData = Convert.ToDouble(dataRow[dataColumn.ColumnIndex]);
                _displayMember = string.Format("{0:0.00}", _formattedData);
            }
            else if (dataColumn.Type == EbDbTypes.Int32 || dataColumn.Type == EbDbTypes.Int64)
            {
                _formattedData = Convert.ToInt64(dataRow[dataColumn.ColumnIndex]);
                _displayMember = "0";
            }
            else
                _formattedData = dataRow[dataColumn.ColumnIndex];

            Row.Columns.Add(new SingleColumn()
            {
                Name = dataColumn.ColumnName,
                Type = (int)dataColumn.Type,
                Value = _formattedData,
                Control = null,
                F = _displayMember ?? (_formattedData == null ? string.Empty : _formattedData.ToString()),
                ObjType = string.Empty
            });
        }

        public void PostFormatFormData()// fill ps displaymembers, columns in FormData
        {
            foreach (KeyValuePair<string, SingleTable> Table in this.FormData.MultipleTables)
            {
                foreach (SingleRow Row in Table.Value)
                {
                    foreach (SingleColumn Column in Row.Columns)
                    {
                        if (Column.Control != null && (Column.Control is EbPowerSelect || Column.Control is EbDGPowerSelectColumn))
                        {
                            string EbSid, VmName, DmName = string.Empty;
                            DVColumnCollection DmsColl;
                            bool RenderAsSS = false;
                            DVColumnCollection ColColl;

                            if (Column.Control is EbPowerSelect)
                            {
                                EbPowerSelect psCtrl = Column.Control as EbPowerSelect;
                                EbSid = psCtrl.EbSid;
                                VmName = psCtrl.ValueMember.Name;
                                RenderAsSS = psCtrl.RenderAsSimpleSelect;
                                DmName = RenderAsSS ? psCtrl.DisplayMember.Name : string.Empty;
                                DmsColl = psCtrl.DisplayMembers;
                                ColColl = psCtrl.Columns;
                            }
                            else
                            {
                                EbDGPowerSelectColumn psColCtrl = Column.Control as EbDGPowerSelectColumn;
                                EbSid = psColCtrl.EbSid;
                                VmName = psColCtrl.ValueMember.Name;
                                DmsColl = psColCtrl.DisplayMembers;
                                ColColl = psColCtrl.Columns;
                            }

                            if (Column.Value == null || string.IsNullOrEmpty(Convert.ToString(Column.Value)) || !this.FormData.PsDm_Tables.ContainsKey(EbSid))
                                continue;

                            //List<SingleRow> Cols = new List<SingleRow>();
                            Dictionary<string, List<object>> Rows = new Dictionary<string, List<object>>();
                            //Dictionary<int, string[]> Disp = new Dictionary<int, string[]>();//original
                            Dictionary<int, Dictionary<string, string>> DispM_dup = new Dictionary<int, Dictionary<string, string>>();//duplicate
                            string[] temp = Convert.ToString(Column.Value).Split(",");
                            int[] vms = Array.ConvertAll<string, int>(temp, int.Parse);
                            SingleTable tbl = this.FormData.PsDm_Tables[EbSid];

                            for (int i = 0; i < vms.Length; i++)
                            {
                                SingleRow _row = tbl.FirstOrDefault(e => Convert.ToInt32(e[VmName]) == vms[i]);
                                if (_row != null)
                                {
                                    foreach (SingleColumn _col in _row.Columns)
                                    {
                                        if (!Rows.ContainsKey(_col.Name))
                                            Rows.Add(_col.Name, new List<object>());
                                        Rows[_col.Name].Add(_col.Value);
                                    }

                                    //Cols.Add(_row);
                                    if (RenderAsSS)
                                    {
                                        //Disp.Add(vms[i], _row[DmName]);
                                        DispM_dup.Add(vms[i], new Dictionary<string, string> { { VmName, Convert.ToString(_row[DmName]) ?? string.Empty } });
                                    }
                                    else
                                    {
                                        string[] _dm = new string[DmsColl.Count];
                                        Dictionary<string, string> __d = new Dictionary<string, string>();
                                        for (int j = 0; j < DmsColl.Count; j++)
                                        {
                                            _dm[j] = Convert.ToString(_row[DmsColl[j].Name]) ?? string.Empty;
                                            __d.Add(DmsColl[j].Name, Convert.ToString(_row[DmsColl[j].Name]) ?? string.Empty);
                                        }
                                        //Disp.Add(vms[i], _dm);
                                        DispM_dup.Add(vms[i], __d);
                                    }
                                }
                                else
                                {
                                    foreach (DVBaseColumn _dvCol in ColColl)
                                    {
                                        if (!Rows.ContainsKey(_dvCol.Name))
                                            Rows.Add(_dvCol.Name, new List<object>());
                                        Rows[_dvCol.Name].Add(string.Empty);
                                    }
                                    if (RenderAsSS)
                                    {
                                        DispM_dup.Add(vms[i], new Dictionary<string, string> { { VmName, string.Empty } });
                                    }
                                    else
                                    {
                                        Dictionary<string, string> __d = new Dictionary<string, string>();
                                        for (int j = 0; j < DmsColl.Count; j++)
                                            __d.Add(DmsColl[j].Name, string.Empty);
                                        DispM_dup.Add(vms[i], __d);
                                    }
                                }
                            }
                            //Column.D = Disp;//original
                            Column.D = DispM_dup;//duplicate
                            Column.R = Rows;
                        }
                    }
                }
            }
        }

        //For Normal Mode
        public void RefreshFormData(IDatabase DataDB, Service service, bool backup = false, bool includePushData = false)
        {
            int formCount = (this.FormDataPusherCount > 0 && includePushData) ? this.FormDataPusherCount + 1 : 1;
            string[] psquery = new string[formCount];
            int[] qrycount = new int[formCount];
            EbWebForm[] _FormCollection = new EbWebForm[formCount];
            string query = QueryGetter.GetSelectQuery(this, DataDB, service, out psquery[0], out qrycount[0]);
            _FormCollection[0] = this;

            if (this.FormDataPusherCount > 0 && includePushData)
            {
                for (int i = 0, j = 1; i < this.DataPushers.Count; i++)
                {
                    if (!(this.DataPushers[i] is EbFormDataPusher))
                        continue;
                    query += QueryGetter.GetSelectQuery(this.DataPushers[i].WebForm, DataDB, service, out psquery[j], out qrycount[j]);
                    this.DataPushers[i].WebForm.UserObj = this.UserObj;
                    this.DataPushers[i].WebForm.SolutionObj = this.SolutionObj;
                    _FormCollection[j++] = this.DataPushers[i].WebForm;
                }
            }

            DbParameter[] param = new DbParameter[]
            {
                DataDB.GetNewParameter(this.FormSchema.MasterTable + FormConstants._id, EbDbTypes.Int32, this.TableRowId),
                DataDB.GetNewParameter(this.FormSchema.MasterTable + FormConstants._eb_ver_id, EbDbTypes.Int32, this.RefId.Split(CharConstants.DASH)[4]),
                DataDB.GetNewParameter(this.FormSchema.MasterTable + FormConstants._refid, EbDbTypes.String, this.RefId)
            };
            EbDataSet dataset;
            if (this.DbConnection == null)
                dataset = DataDB.DoQueries(query, param);
            else
                dataset = DataDB.DoQueries(this.DbConnection, query, param);

            Console.WriteLine("From RefreshFormData : Query count = " + qrycount.Join(",") + " DataTable count = " + dataset.Tables.Count);

            for (int i = 0, start = 0; i < formCount; start += qrycount[i], i++)
            {
                EbDataSet ds = new EbDataSet();
                ds.Tables.AddRange(dataset.Tables.GetRange(start, qrycount[i]));
                _FormCollection[i].RefreshFormDataInner(ds, DataDB, psquery[i], backup, service);
            }
            Console.WriteLine("No Exception in RefreshFormData");
        }

        public void RefreshFormDataInner(EbDataSet dataset, IDatabase DataDB, string psquery, bool backup, Service service)
        {
            WebFormSchema _schema = this.FormSchema;
            WebformData _FormData;
            if (backup)
            {
                this.FormDataBackup = new WebformData() { MasterTable = _schema.MasterTable };
                _FormData = this.FormDataBackup;
            }
            else
            {
                this.FormData = new WebformData() { MasterTable = _schema.MasterTable };
                //this.FormData = this.GetEmptyModel();
                _FormData = this.FormData;
            }

            int count = 0;
            foreach (TableSchema _table in _schema.Tables)
            {
                SingleTable Table = new SingleTable();
                if (!_table.IsDynamic)
                {
                    EbDataTable dataTable = dataset.Tables[count++];////                
                    this.GetFormattedData(dataTable, Table, _table);
                }
                //if (!_FormData.MultipleTables.ContainsKey(_table.TableName))
                _FormData.MultipleTables.Add(_table.TableName, Table);
                //else
                //    ;

                //if (!(_table.TableType == WebFormTableTypes.Normal && Table.Count == 0) && _FormData.MultipleTables.ContainsKey(_table.TableName))
                //    _FormData.MultipleTables[_table.TableName] = Table;
                //else if (backup)
                //    _FormData.MultipleTables.Add(_table.TableName, Table);

            }

            if (!_FormData.MultipleTables.ContainsKey(_FormData.MasterTable) || _FormData.MultipleTables[_FormData.MasterTable].Count == 0)
            {
                if (this.DataPusherConfig != null)
                    return;
                string t = "From RefreshFormData - TABLE : " + _FormData.MasterTable + "   ID : " + this.TableRowId + "\nData Not Found";
                Console.WriteLine(t);
                throw new FormException("Error in loading data", (int)HttpStatusCode.NotFound, t, string.Empty);
            }
            else
            {
                if (!backup)
                {
                    WebformData tempWFD = this.GetEmptyModel();
                    _FormData.DGsRowDataModel = tempWFD.DGsRowDataModel;
                    foreach (TableSchema _table in _schema.Tables.FindAll(e => e.TableType == WebFormTableTypes.Normal))
                    {
                        if (_FormData.MultipleTables[_table.TableName].Count == 0)
                            _FormData.MultipleTables[_table.TableName] = tempWFD.MultipleTables[_table.TableName];
                    }
                }
                this.TableRowId = _FormData.MultipleTables[_FormData.MasterTable][0].RowId;
                this.LocationId = _FormData.MultipleTables[_FormData.MasterTable][0].LocId;
            }

            if (dataset.Tables.Count > _schema.Tables.Count)
            {
                int tableIndex = _schema.Tables.Count;
                SingleTable UserTable = null;
                foreach (EbControl Ctrl in _schema.ExtendedControls)
                {
                    if (Ctrl is IEbExtraQryCtrl)
                    {
                        SingleTable Table = new SingleTable();
                        if (!(UserTable != null && Ctrl is EbProvisionUser))
                            this.GetFormattedData(dataset.Tables[tableIndex], Table);

                        if (Ctrl is EbProvisionUser)
                        {
                            EbProvisionUser provUser = Ctrl as EbProvisionUser;
                            SingleColumn Column = _FormData.MultipleTables[provUser.TableName][0].GetColumn(Ctrl.Name);
                            if (UserTable == null)
                            {
                                UserTable = Table;
                                tableIndex++; //one query is used to select required user records
                            }
                            SingleRow Row_U = null;
                            foreach (SingleRow R in UserTable)
                            {
                                SingleColumn C = R.Columns.Find(e => e.Name == FormConstants.id);
                                if (C != null && (Convert.ToInt32(C.Value) == Convert.ToInt32(Column.Value)))
                                {
                                    Row_U = R;
                                    break;
                                }
                            }
                            Dictionary<string, object> _d = new Dictionary<string, object>();
                            if (Row_U != null)
                            {
                                NTV[] pArr = provUser.FuncParam;
                                for (int k = 0; k < pArr.Length; k++)
                                {
                                    if (Row_U[pArr[k].Name] != null)
                                        _d.Add(pArr[k].Name, Row_U[pArr[k].Name]);
                                }
                            }
                            SingleTable map_Table = new SingleTable();
                            this.GetFormattedData(dataset.Tables[tableIndex], map_Table);
                            if (map_Table.Count > 0)
                            {
                                _d.Add("map_" + FormConstants.id, map_Table[0][FormConstants.id]);
                                _d.Add("map_" + FormConstants.fullname, map_Table[0][FormConstants.fullname]);
                                _d.Add("map_" + FormConstants.email, map_Table[0][FormConstants.email]);
                                _d.Add("map_" + FormConstants.phprimary, map_Table[0][FormConstants.phprimary]);
                            }
                            Column.F = JsonConvert.SerializeObject(_d);
                        }
                        else if (Ctrl is EbProvisionLocation)
                        {
                            Dictionary<string, object> _d = new Dictionary<string, object>();
                            if (Table.Count == 1)
                            {
                                _d.Add(FormConstants.id, Table[0][FormConstants.id]);
                                _d.Add(FormConstants.longname, Table[0][FormConstants.longname]);
                                _d.Add(FormConstants.shortname, Table[0][FormConstants.shortname]);
                                _d.Add(FormConstants.image, Table[0][FormConstants.image]);
                                _d.Add(FormConstants.meta_json, Table[0][FormConstants.meta_json]);
                                _d.Add(FormConstants.is_group, Table[0][FormConstants.is_group]);
                                _d.Add(FormConstants.parent_id, Table[0][FormConstants.parent_id]);
                                _d.Add(FormConstants.eb_location_types_id, Table[0][FormConstants.eb_location_types_id]);
                            }
                            _FormData.MultipleTables[(Ctrl as EbProvisionLocation).TableName][0].GetColumn(Ctrl.Name).F = JsonConvert.SerializeObject(_d);
                        }
                        else if (Ctrl is EbReview)
                        {
                            if (Table.Count == 1)
                            {
                                string stageEbSid = Convert.ToString(Table[0]["stage_unique_id"]);
                                EbReviewStage activeStage = (EbReviewStage)(Ctrl as EbReview).FormStages.Find(e => (e as EbReviewStage).EbSid == stageEbSid);

                                if (activeStage != null)
                                {
                                    List<int> user_ids = new List<int>();
                                    List<int> role_ids = new List<int>();
                                    string sUserIds = Convert.ToString(Table[0]["user_ids"]);
                                    string sRoleIds = Convert.ToString(Table[0]["role_ids"]);
                                    int.TryParse(Convert.ToString(Table[0]["usergroup_id"]), out int ugId);
                                    if (!sUserIds.IsNullOrEmpty())
                                        user_ids = Array.ConvertAll(sUserIds.Split(','), int.Parse).ToList();
                                    if (!sRoleIds.IsNullOrEmpty())
                                        role_ids = Array.ConvertAll(sRoleIds.Split(','), int.Parse).ToList();

                                    bool hasRoleMatch = this.UserObj.RoleIds.Select(x => x).Intersect(role_ids).Any();
                                    bool hasPerm = false;
                                    if (hasRoleMatch || user_ids.Contains(this.UserObj.UserId) || this.UserObj.UserGroupIds.Contains(ugId))
                                        hasPerm = true;
                                    else if (this.UserObj.Roles.Contains(SystemRoles.SolutionOwner.ToString()) || this.UserObj.Roles.Contains(SystemRoles.SolutionAdmin.ToString()))
                                    {
                                        hasPerm = true;
                                        Console.WriteLine("Permission granted to Solu Owner/Admin to execute Review");
                                    }
                                    DateTime dt_con = DateTime.UtcNow.ConvertFromUtc(this.UserObj.Preference.TimeZone);
                                    string dt = dt_con.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                    string f_dt = dt_con.ToString(this.UserObj.Preference.GetShortDatePattern() + " " + this.UserObj.Preference.GetShortTimePattern(), CultureInfo.InvariantCulture);
                                    string stAction = activeStage.StageActions.Count > 0 ? (activeStage.StageActions[0] as EbReviewAction).EbSid : string.Empty;
                                    _FormData.MultipleTables["eb_approval_lines"].Add(new SingleRow()
                                    {
                                        RowId = 0,
                                        Columns = new List<SingleColumn>
                                        {
                                            new SingleColumn{ Name = "stage_unique_id", Type = (int)EbDbTypes.String, Value = activeStage.EbSid},
                                            new SingleColumn{ Name = "action_unique_id", Type = (int)EbDbTypes.String, Value = stAction},
                                            new SingleColumn{ Name = "eb_my_actions_id", Type = (int)EbDbTypes.Decimal, Value = (hasPerm || backup) ? Table[0]["id"] : 0},
                                            new SingleColumn{ Name = "comments", Type = (int)EbDbTypes.String, Value = ""},
                                            new SingleColumn{ Name = "eb_created_at", Type = (int)EbDbTypes.DateTime, Value = hasPerm ? dt : null, F = hasPerm ? f_dt : null},
                                            new SingleColumn{ Name = "eb_created_by", Type = (int)EbDbTypes.Decimal, Value = hasPerm ? this.UserObj.UserId : 0, F = hasPerm ? this.UserObj.FullName : string.Empty},
                                            new SingleColumn{ Name = "is_form_data_editable", Type = (int)EbDbTypes.String, Value = Convert.ToString(Table[0]["is_form_data_editable"])},
                                            new SingleColumn{ Name = "has_permission", Type = (int)EbDbTypes.String, Value = hasPerm ? "T" : "F"}
                                        }
                                    });
                                    if (this.MyActNotification != null)
                                    {
                                        this.MyActNotification.MyActionId = Convert.ToInt32(Table[0]["id"]);
                                        this.MyActNotification.Description = Convert.ToString(Table[0]["description"]);
                                    }
                                }

                            }
                        }
                        else if (Ctrl is EbDisplayPicture || Ctrl is EbSimpleFileUploader)
                        {
                            List<FileMetaInfo> _list = new List<FileMetaInfo>();
                            foreach (SingleRow dr in Table)
                            {
                                FileMetaInfo info = new FileMetaInfo
                                {
                                    FileRefId = Convert.ToInt32(dr[FormConstants.id]),
                                    FileName = Convert.ToString(dr[FormConstants.filename]),
                                    Meta = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(dr[FormConstants.tags] as string),
                                    UploadTime = Convert.ToString(dr[FormConstants.uploadts]),
                                    FileCategory = (EbFileCategory)Convert.ToInt32(dr[FormConstants.filecategory])
                                };

                                if (!_list.Contains(info))
                                    _list.Add(info);
                            }
                            _FormData.MultipleTables[(Ctrl as IEbExtraQryCtrl).TableName][0].GetColumn(Ctrl.Name).F = JsonConvert.SerializeObject(_list);
                        }
                        else if (Ctrl is EbQuestionnaireConfigurator)
                        {
                            List<Ques_Confi> Ques = new List<Ques_Confi>();
                            foreach (SingleRow dr in Table)
                            {
                                Ques_Confi Que = new Ques_Confi()
                                {
                                    id = Convert.ToInt32(dr[FormConstants.id]),
                                    ques_id = Convert.ToInt32(dr["ques_id"]),
                                    ext_props = JsonConvert.DeserializeObject<Ques_ext_props>(Convert.ToString(dr["ext_props"]))
                                };
                                Ques.Add(Que);
                            }
                            _FormData.MultipleTables[(Ctrl as IEbExtraQryCtrl).TableName][0][Ctrl.Name] = JsonConvert.SerializeObject(Ques);
                        }

                        tableIndex++;
                    }
                }
            }

            if (!backup && this.DataPusherConfig == null)//isMasterFormNormalRefresh
            {
                foreach (EbControl Ctrl in _schema.ExtendedControls)// EbFileUploader
                {
                    if (Ctrl is EbFileUploader)
                    {
                        string context = this.RefId.Split(CharConstants.DASH)[3] + CharConstants.UNDERSCORE + this.TableRowId.ToString();//context format = objectId_rowId_ControlId
                        EbFileUploader _ctrl = Ctrl as EbFileUploader;
                        string cxt2 = null;
                        if (_ctrl.ContextGetExpr != null && !_ctrl.ContextGetExpr.Code.IsNullOrEmpty())
                        {
                            if (this.FormGlobals == null)
                                this.FormGlobals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, _FormData, null, DataDB, null, false);
                            cxt2 = Convert.ToString(this.ExecuteCSharpScriptNew(_ctrl.ContextGetExpr.Code, this.FormGlobals));
                        }

                        string qry = (Ctrl as EbFileUploader).GetSelectQuery(DataDB, string.IsNullOrEmpty(cxt2));

                        DbParameter[] param = new DbParameter[]
                        {
                        DataDB.GetNewParameter(FormConstants.id, EbDbTypes.Int32, this.TableRowId),
                        DataDB.GetNewParameter(FormConstants.context, EbDbTypes.String, context),
                        DataDB.GetNewParameter(FormConstants.context_sec, EbDbTypes.String, cxt2 ?? string.Empty),
                        DataDB.GetNewParameter(FormConstants.eb_ver_id, EbDbTypes.Int32, this.RefId.Split(CharConstants.DASH)[4])
                        };

                        EbDataTable dt;
                        if (this.DbConnection == null)
                            dt = DataDB.DoQuery(qry, param);
                        else
                            dt = DataDB.DoQuery(this.DbConnection, qry, param);

                        SingleTable Table = new SingleTable();
                        this.GetFormattedData(dt, Table);

                        List<FileMetaInfo> _list = new List<FileMetaInfo>();
                        foreach (SingleRow dr in Table)
                        {
                            FileMetaInfo info = new FileMetaInfo
                            {
                                FileRefId = Convert.ToInt32(dr[FormConstants.id]),
                                FileName = Convert.ToString(dr[FormConstants.filename]),
                                Meta = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(dr[FormConstants.tags] as string),
                                UploadTime = Convert.ToString(dr[FormConstants.uploadts]),
                                FileCategory = (EbFileCategory)Convert.ToInt32(dr[FormConstants.filecategory])
                            };

                            if (!_list.Contains(info))
                                _list.Add(info);
                        }
                        SingleTable _Table = new SingleTable {
                            new SingleRow() {
                                Columns = new List<SingleColumn> {
                                    new SingleColumn { Name = FormConstants.Files, Type = (int)EbDbTypes.Json, Value = JsonConvert.SerializeObject(_list) }
                                }
                            }
                        };
                        _FormData.ExtendedTables.Add(Ctrl.Name ?? Ctrl.EbSid, _Table);//fup
                    }
                }

                List<EbControl> drPsList = new List<EbControl>();
                List<EbControl> apiPsList = new List<EbControl>();
                foreach (TableSchema Tbl in _schema.Tables)
                {
                    drPsList.AddRange(Tbl.Columns.FindAll(e => !e.Control.DoNotPersist && e.Control is IEbPowerSelect && !(e.Control as IEbPowerSelect).IsDataFromApi).Select(e => e.Control));
                    apiPsList.AddRange(Tbl.Columns.FindAll(e => !e.Control.DoNotPersist && e.Control is IEbPowerSelect && (e.Control as IEbPowerSelect).IsDataFromApi).Select(e => e.Control));
                }

                if (drPsList.Count > 0)
                {
                    this.LocationId = _FormData.MultipleTables[_FormData.MasterTable][0].LocId;
                    List<DbParameter> param = this.GetPsParams(drPsList, _FormData, DataDB, service);
                    EbDataSet ds;
                    if (this.DbConnection == null)
                        ds = DataDB.DoQueries(psquery, param.ToArray());
                    else
                        ds = DataDB.DoQueries(this.DbConnection, psquery, param.ToArray());

                    if (ds.Tables.Count > 0)
                    {
                        int tblIdx = 0;
                        foreach (EbControl ctrl in drPsList)
                        {
                            SingleTable Table = new SingleTable();
                            this.GetFormattedData(ds.Tables[tblIdx], Table);
                            _FormData.PsDm_Tables.Add(ctrl.EbSid, Table);
                            tblIdx++;
                        }
                    }
                }

                foreach (EbControl Ctrl in apiPsList)
                {
                    Dictionary<string, SingleTable> Tables = EbFormHelper.GetDataFormApi(service, Ctrl, this, _FormData, false);
                    if (Tables.Count > 0)
                        _FormData.PsDm_Tables.Add(Ctrl.EbSid, Tables.ElementAt(0).Value);
                    else
                        _FormData.PsDm_Tables.Add(Ctrl.EbSid, new SingleTable());
                }

                this.PostFormatFormData();
                this.FormGlobals = null;// FormGlobals is a reusing Object, so clear when a data change happens
                this.ExeDeleteCancelEditScript(DataDB, _FormData);
            }
        }

        private List<DbParameter> GetPsParams(List<EbControl> drPsList, WebformData _FormData, IDatabase DataDB, Service service)
        {
            TableSchema _table = null;
            ColumnSchema _column = null;
            List<DbParameter> param = new List<DbParameter>();
            EbFormHelper.AddExtraSqlParams(param, DataDB, this.TableName, this.TableRowId, this.LocationId, this.UserObj.UserId);

            foreach (EbControl psCtrl in drPsList)
            {
                List<Param> ParamsList = (psCtrl as IEbDataReaderControl).ParamsList;
                if (ParamsList == null)
                {
                    Console.WriteLine($"ParamsList in PowerSelect {psCtrl.Name} is null.");
                    (psCtrl as IEbPowerSelect).UpdateParamsMeta(service, service.Redis);
                    ParamsList = (psCtrl as IEbDataReaderControl).ParamsList;
                }

                foreach (Param _psParam in ParamsList)
                {
                    if (param.Exists(e => e.ParameterName == _psParam.Name))
                        continue;

                    _table = null;
                    _column = null;
                    foreach (TableSchema __table in this.FormSchema.Tables)
                    {
                        _column = __table.Columns.Find(e => e.Control.Name == _psParam.Name);
                        if (_column != null)
                        {
                            _table = __table;
                            break;
                        }
                    }

                    if (_table != null && _FormData.MultipleTables.TryGetValue(_table.TableName, out SingleTable Table) && Table.Count > 0)
                    {
                        if (_table.TableType == WebFormTableTypes.Grid)
                        {
                            if (psCtrl.Name == _psParam.Name)
                            {
                                List<string> vms = new List<string>();
                                foreach (SingleRow Row in Table)
                                {
                                    if (Row[_column.ColumnName] != null)
                                        vms.Add(Convert.ToString(Row[_column.ColumnName]));
                                }
                                param.Add(DataDB.GetNewParameter(_psParam.Name + FormConstants.__ebedt, EbDbTypes.String, vms.Join(",")));
                                continue;
                            }
                        }
                        else
                        {
                            param.Add(DataDB.GetNewParameter(_psParam.Name, (EbDbTypes)Convert.ToInt32(_psParam.Type), Table[0][_column.ColumnName]));
                            continue;
                        }
                    }
                    param.Add(DataDB.GetNewParameter(_psParam.Name, (EbDbTypes)Convert.ToInt32(_psParam.Type), _psParam.ValueTo));
                }
            }
            return param;
        }

        //For Prefill Mode
        public void RefreshFormData(IDatabase DataDB, Service service, List<Param> _params)
        {
            this.FormData = this.GetEmptyModel();
            Dictionary<string, string> QrsDict = new Dictionary<string, string>();
            List<DbParameter> param = new List<DbParameter>();
            string DataImportPS = null;
            foreach (KeyValuePair<string, SingleTable> Table in this.FormData.MultipleTables)
            {
                foreach (SingleRow Row in Table.Value)
                {
                    foreach (SingleColumn Column in Row.Columns)
                    {
                        Param p = _params.Find(e => e.Name == Column.Name);
                        if (p != null)
                        {
                            SingleColumn NwCol = Column.Control.GetSingleColumn(this.UserObj, this.SolutionObj, p.ValueTo, false);
                            Column.Value = NwCol.Value;
                            Column.F = NwCol.F;
                            param.Add(DataDB.GetNewParameter(Column.Name, (EbDbTypes)Column.Type, Column.Value));
                            if (Column.Control is EbPowerSelect)
                            {
                                EbPowerSelect psCtrl = Column.Control as EbPowerSelect;
                                if ((psCtrl.IsImportFromApi || !string.IsNullOrWhiteSpace(psCtrl.DataImportId)) && !psCtrl.MultiSelect && p.ValueTo > 0)
                                    DataImportPS = psCtrl.Name;

                                if (!psCtrl.IsDataFromApi && DataImportPS == null)
                                {
                                    string t = psCtrl.GetSelectQuery(DataDB, service, p.Value);
                                    QrsDict.Add(psCtrl.EbSid, t);
                                }
                            }
                        }
                    }
                }
            }

            if (DataImportPS != null)
            {
                this.FormDataBackup = this.FormData;
                this.PsImportData(DataDB, service, DataImportPS);
            }
            else if (QrsDict.Count > 0)
            {
                EbFormHelper.AddExtraSqlParams(param, DataDB, this.TableName, this.TableRowId, this.LocationId, this.UserObj.UserId);

                EbDataSet dataset = DataDB.DoQueries(string.Join(CharConstants.SPACE, QrsDict.Select(d => d.Value)), param.ToArray());
                int i = 0;
                foreach (KeyValuePair<string, string> item in QrsDict)
                {
                    SingleTable Table = new SingleTable();
                    this.GetFormattedData(dataset.Tables[i++], Table);
                    this.FormData.PsDm_Tables.Add(item.Key, Table);
                }
                this.PostFormatFormData();
            }
        }

        public List<Param> GetFormData4Mobile(IDatabase DataDB, Service service)
        {
            List<Param> data = new List<Param>();
            this.RefreshFormData(DataDB, service);
            foreach (TableSchema _table in this.FormSchema.Tables.FindAll(e => e.TableType == WebFormTableTypes.Normal))
            {
                if (this.FormData.MultipleTables.ContainsKey(_table.TableName) && this.FormData.MultipleTables[_table.TableName].Count > 0)
                {
                    foreach (SingleColumn Column in this.FormData.MultipleTables[_table.TableName][0].Columns)
                    {
                        if (Column.Control != null && !Column.Control.DoNotPersist && !Column.Control.Hidden)
                        {
                            EbControl _c = Column.Control;
                            if (!(_c is EbTextBox || _c is EbBooleanSelect || _c is EbCheckBoxGroup || _c is EbDate || _c is EbNumeric || _c is EbPowerSelect || _c is EbRadioButton || _c is EbSimpleSelect || _c is EbRichText || _c is EbAutoId || _c is EbUserSelect || _c is EbTagInput))
                                continue;
                            string _value = string.IsNullOrEmpty(Column.F) ? Convert.ToString(Column.Value) : Column.F;
                            if (Column.Control is EbPowerSelect)
                            {
                                string dm = string.Empty;
                                foreach (KeyValuePair<int, Dictionary<string, string>> dp in Column.D)
                                {
                                    foreach (KeyValuePair<string, string> dc in dp.Value)
                                        dm += dc.Value + CharConstants.SPACE;
                                }
                                _value = dm;
                            }
                            else if (Column.Control is EbRichText)
                            {
                                _value = Regex.Replace(_value, "<[^>]*>", "");
                            }
                            data.Add(new Param
                            {
                                Name = string.IsNullOrEmpty(Column.Control.Label) ? Column.Control.Name : Column.Control.Label,
                                Type = ((int)EbDbTypes.String).ToString(),
                                Value = _value
                            });
                        }
                    }
                }
            }
            return data;
        }

        //Normal save
        public string Save(EbConnectionFactory EbConFactory, Service service, string wc)
        {
            IDatabase DataDB = EbConFactory.DataDB;
            this.DbConnection = DataDB.GetNewConnection();
            string resp;
            try
            {
                WebformData in_data = JsonConvert.DeserializeObject<WebformData>(JsonConvert.SerializeObject(this.FormData));
                this.DbConnection.Open();
                this.DbTransaction = this.DbConnection.BeginTransaction();

                this.ExecProvUserCreateOnlyIfScript();
                bool IsUpdate = this.TableRowId > 0;
                if (IsUpdate)
                {
                    this.RefreshFormData(DataDB, service, true, true);
                    if (this.FormData.IsReadOnly)
                        throw new FormException("This form submission is READONLY!", (int)HttpStatusCode.Forbidden, "ReadOnly record", "EbWebForm -> Save");
                    if (this.FormData.IsLocked)
                        throw new FormException("This form submission is LOCKED!", (int)HttpStatusCode.Forbidden, "Locked record", "EbWebForm -> Save");
                    if (this.FormData.FormVersionId == 0)
                        throw new FormException("Edit is blocked - Invalid Form RefId!", (int)HttpStatusCode.Forbidden, "Invalid FormVersionId", "EbWebForm -> Save");

                    if (wc == TokenConstants.UC && !(EbFormHelper.HasPermission(this.UserObj, this.RefId, OperationConstants.EDIT, this.LocationId, this.IsLocIndependent) ||
                        (this.UserObj.UserId == this.FormData.CreatedBy && EbFormHelper.HasPermission(this.UserObj, this.RefId, OperationConstants.OWN_DATA, this.LocationId, this.IsLocIndependent))))
                        throw new FormException("Access denied!", (int)HttpStatusCode.Forbidden, "Access denied", "EbWebForm -> Save");

                    resp = "Updated: " + this.Update(DataDB, service);
                }
                else
                {
                    //if (wc == TokenConstants.UC && !EbFormHelper.HasPermission(this.UserObj, this.RefId, OperationConstants.NEW, this.LocationId, this.IsLocIndependent))
                    //    throw new FormException("Access denied to save this data entry!", (int)HttpStatusCode.Forbidden, "Access denied", "EbWebForm -> Save");

                    if (this.IsDisable)
                        throw new FormException("This form is READONLY!", (int)HttpStatusCode.Forbidden, "ReadOnly form", "EbWebForm -> Save");

                    this.TableRowId = this.Insert(DataDB, service);
                    resp = "Inserted: " + this.TableRowId;
                    Console.WriteLine("New record inserted. Table :" + this.TableName + ", Id : " + this.TableRowId);
                }
                this.RefreshFormData(DataDB, service, false, true);
                Console.WriteLine("EbWebForm.Save.UpdateAuditTrail start");
                EbAuditTrail ebAuditTrail = new EbAuditTrail(this, DataDB);
                resp += " - AuditTrail: " + ebAuditTrail.UpdateAuditTrail();
                Console.WriteLine("EbWebForm.Save.AfterSave start");
                resp += " - AfterSave: " + this.AfterSave(DataDB, IsUpdate);
                List<ApiRequest> ApiRqsts = new List<ApiRequest>();
                resp += " - ApiDataPushers: " + EbDataPushHelper.ProcessApiDataPushers(this, service, DataDB, this.DbConnection, ApiRqsts);
                resp += " - BatchFormDataPushers: " + EbDataPushHelper.ProcessBatchFormDataPushers(this, service, DataDB, this.DbConnection, in_data);
                this.DbTransaction.Commit();
                Console.WriteLine("EbWebForm.Save.DbTransaction Committed");
                resp += " - ApiDataPushers Response: " + EbDataPushHelper.CallInternalApis(ApiRqsts, service);
                Console.WriteLine("EbWebForm.Save.SendNotifications start");
                resp += " - Notifications: " + EbFnGateway.SendNotifications(this, DataDB, service);
                Console.WriteLine("EbWebForm.Save.SendMobileNotification start");
                EbFnGateway.SendMobileNotification(this, EbConFactory);
                Console.WriteLine("EbWebForm.Save.InsertOrUpdate Global Search start");
                SearchHelper.InsertOrUpdate(DataDB, this);
                Console.WriteLine("EbWebForm.Save.resp = " + resp);
            }
            catch (FormException ex1)
            {
                try
                {
                    this.DbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Rollback Exception Type: {ex2.GetType()}\nMessage: {ex2.Message}");
                }
                throw ex1;
            }
            catch (Exception ex1)
            {
                try
                {
                    this.DbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Rollback Exception Type: {ex2.GetType()}\nMessage: {ex2.Message}");
                }
                throw new FormException("Exception in Form data save", (int)HttpStatusCode.InternalServerError, ex1.Message, ex1.StackTrace);
            }
            this.DbConnection.Close();
            return resp;
        }

        public int Insert(IDatabase DataDB, Service service)
        {
            string fullqry = string.Empty;
            string _extqry = string.Empty;
            List<DbParameter> param = new List<DbParameter>();
            int i = 0;
            if (this.FormDataPusherCount > 0)
                this.PrepareWebFormData();

            this.FormCollection.ExecUniqueCheck(DataDB);
            this.FormCollection.Insert(DataDB, param, ref fullqry, ref _extqry, ref i);

            fullqry += _extqry;
            fullqry += this.GetFileUploaderUpdateQuery(DataDB, param, ref i);
            fullqry += this.GetMyActionInsertUpdateQuery(DataDB, param, ref i, true, service);
            fullqry += this.GetDraftTableUpdateQuery(DataDB, param, ref i);

            param.Add(DataDB.GetNewParameter(FormConstants.eb_createdby, EbDbTypes.Int32, this.UserObj.UserId));
            param.Add(DataDB.GetNewParameter(FormConstants.eb_currentuser_id, EbDbTypes.Int32, this.UserObj.UserId));
            param.Add(DataDB.GetNewParameter(FormConstants.eb_loc_id, EbDbTypes.Int32, this.LocationId));
            param.Add(DataDB.GetNewParameter(FormConstants.eb_signin_log_id, EbDbTypes.Int32, this.UserObj.SignInLogId));
            fullqry += $"SELECT eb_currval('{this.TableName}_id_seq');";

            EbDataSet tem = DataDB.DoQueries(this.DbConnection, fullqry, param.ToArray());
            EbDataTable temp = tem.Tables[tem.Tables.Count - 1];
            int _rowid = temp.Rows.Count > 0 ? Convert.ToInt32(temp.Rows[0][0]) : 0;
            if (_rowid <= 0)
                throw new FormException("Something went wrong in our end.", (int)HttpStatusCode.InternalServerError, $"SELECT eb_currval('{this.TableName}_id_seq') returned an invalid data: {_rowid}", "EbWebForm -> Insert");
            return _rowid;
        }

        private string GetDraftTableUpdateQuery(IDatabase DataDB, List<DbParameter> param, ref int i)
        {
            string Qry = string.Empty;
            if (this.DraftId > 0 && this.TableRowId == 0)
            {
                Qry = $@"UPDATE eb_form_drafts SET is_submitted = 'T', form_data_id = eb_currval('{this.TableName}_id_seq'), eb_lastmodified_at = {DataDB.EB_CURRENT_TIMESTAMP}
                        WHERE id = @draft_id_{i} AND form_ref_id = @{this.TableName}_form_ref_id AND eb_created_by = @eb_createdby AND is_submitted = 'F' AND eb_del = 'F'; ";

                param.Add(DataDB.GetNewParameter($"draft_id_{i++}", EbDbTypes.Int32, this.DraftId));
                param.Add(DataDB.GetNewParameter($"{this.TableName}_form_ref_id", EbDbTypes.String, this.RefId));
            }
            return Qry;
        }

        //pTable => Parent Table, pRow => Parent Row
        public string InsertUpdateLines(string pTable, SingleRow parentRow, ParameterizeCtrl_Params args)
        {
            string fullqry = string.Empty;
            string tempStr = string.Empty;////////////////////warning
            ParameterizeCtrl_Params inArgs = new ParameterizeCtrl_Params(args.DataDB, args.param, args.i, tempStr);
            inArgs.SetFormRelated(this.TableName, this.UserObj, this);

            if (parentRow.LinesTable.Key != null)// Lines table of the grid
            {
                foreach (SingleRow _lineRow in parentRow.LinesTable.Value)
                {
                    inArgs.ResetColVals();
                    inArgs.ResetColsAndVals();

                    if (!_lineRow.IsDelete)
                    {
                        foreach (SingleColumn cField in _lineRow.Columns)
                        {
                            if (_lineRow.RowId <= 0)
                                inArgs.InsertSet(cField);
                            else
                                inArgs.UpdateSet(cField);

                            if (cField.Control != null)
                                cField.Control.ParameterizeControl(inArgs, this.CrudContext);
                            else
                                this.ParameterizeUnknown(inArgs);
                        }
                    }

                    if (this.TableRowId <= 0 && parentRow.RowId <= 0 && _lineRow.RowId <= 0)//III
                    {
                        string _qry = QueryGetter.GetInsertQuery(this, inArgs.DataDB, parentRow.LinesTable.Key, true);
                        _qry = string.Format(_qry, $"{{0}} {pTable}_id,", $"{{1}} (SELECT eb_currval('{pTable}_id_seq')),");
                        fullqry += string.Format(_qry, inArgs._cols, inArgs._vals);
                    }
                    else if (this.TableRowId > 0 && parentRow.RowId <= 0 && _lineRow.RowId <= 0)//EII
                    {
                        string _qry = QueryGetter.GetInsertQuery(this, inArgs.DataDB, parentRow.LinesTable.Key, false);
                        _qry = string.Format(_qry, $"{{0}} {pTable}_id,", $"{{1}} (SELECT eb_currval('{pTable}_id_seq')),");
                        fullqry += string.Format(_qry, inArgs._cols, inArgs._vals);
                    }
                    else if (this.TableRowId > 0 && parentRow.RowId > 0 && _lineRow.RowId <= 0)//EEI
                    {
                        string _qry = QueryGetter.GetInsertQuery(this, inArgs.DataDB, parentRow.LinesTable.Key, false);
                        _qry = string.Format(_qry, $"{{0}} {pTable}_id,", $"{{1}} {parentRow.RowId},");
                        fullqry += string.Format(_qry, inArgs._cols, inArgs._vals);
                    }
                    else if (this.TableRowId > 0 && parentRow.RowId > 0 && _lineRow.RowId > 0)//EEE
                    {
                        string _qry = QueryGetter.GetUpdateQuery(this, inArgs.DataDB, parentRow.LinesTable.Key, _lineRow.IsDelete);
                        _qry = string.Format(_qry, "{0}", $"{{1}} AND {pTable}_id = {parentRow.RowId} ");
                        fullqry += string.Format(_qry, inArgs._colvals, _lineRow.RowId);
                    }
                }
            }
            //args.CopyBack(ref tempStr, ref i);
            args.i = inArgs.i;
            return fullqry;
        }

        public int Update(IDatabase DataDB, Service service)
        {
            string fullqry = string.Empty;
            string _extqry = string.Empty;
            List<DbParameter> param = new List<DbParameter>();
            int i = 0;
            if (this.FormDataPusherCount > 0)
                this.PrepareWebFormData();

            this.FormCollection.ExecUniqueCheck(DataDB);
            this.FormCollection.Update(DataDB, param, ref fullqry, ref _extqry, ref i);

            fullqry += _extqry;
            fullqry += this.GetFileUploaderUpdateQuery(DataDB, param, ref i);
            fullqry += this.GetMyActionInsertUpdateQuery(DataDB, param, ref i, false, service);
            param.Add(DataDB.GetNewParameter(FormConstants.eb_loc_id, EbDbTypes.Int32, this.LocationId));
            param.Add(DataDB.GetNewParameter(FormConstants.eb_createdby, EbDbTypes.Int32, this.UserObj.UserId));
            param.Add(DataDB.GetNewParameter(FormConstants.eb_currentuser_id, EbDbTypes.Int32, this.UserObj.UserId));
            param.Add(DataDB.GetNewParameter(FormConstants.eb_modified_by, EbDbTypes.Int32, this.UserObj.UserId));
            param.Add(DataDB.GetNewParameter(FormConstants.eb_signin_log_id, EbDbTypes.Int32, this.UserObj.SignInLogId));
            return DataDB.DoNonQuery(this.DbConnection, fullqry, param.ToArray());
        }

        private void ExecProvUserCreateOnlyIfScript()
        {
            List<EbControl> ctrls = this.FormSchema.ExtendedControls.FindAll(e => e is EbProvisionUser);
            if (ctrls.Count == 0)
                return;
            FG_Root globals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, this.FormData, this.FormDataBackup, null, null, false);
            foreach (EbProvisionUser provCtrl in ctrls)
            {
                if (!string.IsNullOrEmpty(provCtrl.CreateOnlyIf?.Code))
                {
                    object flag = this.ExecuteCSharpScriptNew(provCtrl.CreateOnlyIf.Code, globals);
                    if (flag is bool && Convert.ToBoolean(flag))
                        provCtrl.CreateOnlyIf_b = true;
                    else
                        provCtrl.CreateOnlyIf_b = false;
                }
                else
                    provCtrl.CreateOnlyIf_b = true;
            }
        }

        private string GetMyActionInsertUpdateQuery(IDatabase DataDB, List<DbParameter> param, ref int i, bool isInsert, Service service)
        {
            EbReview ebReview = (EbReview)this.FormSchema.ExtendedControls.FirstOrDefault(e => e is EbReview);
            if (ebReview == null || ebReview.FormStages.Count == 0)
                return string.Empty;
            string insUpQ = string.Empty;
            string masterId = $"@{this.TableName}_id";
            EbReviewStage nextStage = null;
            bool insMyActRequired = false;
            bool insInEdit = false;
            bool entryCriteriaRslt = true;
            FG_Root globals = null;
            if (!string.IsNullOrWhiteSpace(ebReview.EntryCriteria?.Code))
            {
                globals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, this.FormData, this.FormDataBackup, DataDB, null, false);
                object status = this.ExecuteCSharpScriptNew(ebReview.EntryCriteria.Code, globals);
                bool.TryParse(Convert.ToString(status), out entryCriteriaRslt);
            }
            if (isInsert)
            {
                if (entryCriteriaRslt)
                {
                    masterId = $"(SELECT eb_currval('{this.TableName}_id_seq'))";
                    nextStage = ebReview.FormStages[0];
                }
                else
                    return string.Empty;
            }
            else
            {
                if (!entryCriteriaRslt)
                {
                    if (this.FormDataBackup != null && this.FormDataBackup.MultipleTables.ContainsKey(ebReview.TableName))
                    {
                        SingleRow RowBkUp = this.FormDataBackup.MultipleTables[ebReview.TableName].Find(e => e.RowId <= 0);
                        if (RowBkUp != null)
                        {
                            insUpQ += $@"
UPDATE 
    eb_my_actions 
SET 
    completed_at = {DataDB.EB_CURRENT_TIMESTAMP},
    completed_by = @eb_createdby, 
    is_completed = 'F', 
    eb_del = 'T'
WHERE 
    id = @eb_my_actions_id_{i} AND 
    eb_del = 'F'; ";
                            param.Add(DataDB.GetNewParameter($"@eb_my_actions_id_{i++}", EbDbTypes.Int32, RowBkUp["eb_my_actions_id"]));
                            Console.WriteLine("Will try to DELETE eb_my_actions");
                        }
                        if (this.FormDataBackup.MultipleTables[ebReview.TableName].Count > 0)
                        {
                            insUpQ += $@"
UPDATE 
    eb_approval 
SET 
    eb_lastmodified_by = @eb_modified_by, 
    eb_lastmodified_at = {DataDB.EB_CURRENT_TIMESTAMP}, 
    eb_del = 'T'
WHERE 
    eb_src_id = @{this.TableName}_id AND 
    eb_ver_id =  @{this.TableName}_eb_ver_id AND 
    COALESCE(eb_del, 'F') = 'F'; 
UPDATE 
    eb_approval_lines
SET
    eb_lastmodified_by = @eb_modified_by, 
    eb_lastmodified_at = {DataDB.EB_CURRENT_TIMESTAMP}, 
    eb_del = 'T'
WHERE
    eb_src_id = @{this.TableName}_id AND 
    eb_ver_id =  @{this.TableName}_eb_ver_id AND 
    COALESCE(eb_del, 'F') = 'F'; ";
                        }
                    }
                }
                else
                {
                    int reviewRowCount = this.FormData.MultipleTables.ContainsKey(ebReview.TableName) ? this.FormData.MultipleTables[ebReview.TableName].Count : 0;

                    if (reviewRowCount == 1)
                    {
                        bool permissionGranted = false;
                        if (this.FormDataBackup != null && this.FormDataBackup.MultipleTables.ContainsKey(ebReview.TableName))
                        {
                            SingleRow Row = this.FormDataBackup.MultipleTables[ebReview.TableName].Find(e => e.RowId <= 0);
                            if (Row != null && Convert.ToString(Row["eb_my_actions_id"]) == Convert.ToString(this.FormData.MultipleTables[ebReview.TableName][0]["eb_my_actions_id"]))
                                permissionGranted = true;
                        }
                        if (!permissionGranted)
                            throw new FormException("Access denied to execute review", (int)HttpStatusCode.Unauthorized, $"Following entry is not present in FormDataBackup. eb_my_actions_id: {this.FormData.MultipleTables[ebReview.TableName][0]["eb_my_actions_id"]} ", "From GetMyActionInsertUpdateQuery");

                        insUpQ += $@"UPDATE eb_my_actions SET completed_at = {DataDB.EB_CURRENT_TIMESTAMP}, completed_by = @eb_createdby, is_completed = 'T',
					    eb_approval_lines_id = (SELECT eb_currval('eb_approval_lines_id_seq')) WHERE id = @eb_my_actions_id_{i} AND eb_del = 'F'; ";
                        param.Add(DataDB.GetNewParameter($"@eb_my_actions_id_{i++}", EbDbTypes.Int32, this.FormData.MultipleTables[ebReview.TableName][0]["eb_my_actions_id"]));
                        Console.WriteLine("Will try to UPDATE eb_my_actions");

                        if (!(ebReview.FormStages.Find(e => e.EbSid == Convert.ToString(this.FormData.MultipleTables[ebReview.TableName][0]["stage_unique_id"])) is EbReviewStage currentStage))
                            throw new FormException("Bad Request", (int)HttpStatusCode.BadRequest, $"eb_approval_lines contains an invalid stage_unique_id: {this.FormData.MultipleTables[ebReview.TableName][0]["stage_unique_id"]} ", "From GetMyActionInsertUpdateQuery");

                        //_FG_WebForm global = GlobalsGenerator.GetCSharpFormGlobals(this, this.FormData);
                        //_FG_Root globals = new _FG_Root(global, this, service);

                        if (globals == null)
                            globals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, this.FormData, this.FormDataBackup, DataDB, null, false);

                        object stageObj = this.ExecuteCSharpScriptNew(currentStage.NextStage.Code, globals);
                        string nxtStName = string.Empty;
                        if (stageObj is FG_Review_Stage)
                            nxtStName = (stageObj as FG_Review_Stage).name;

                        GlobalsGenerator.PostProcessGlobals(this, globals, service);
                        string _reviewStatus = globals.form.review._ReviewStatus;
                        if (_reviewStatus == "Completed" || _reviewStatus == "Abandoned")
                        {
                            this.AfterSaveRoutines.AddRange(ebReview.OnApprovalRoutines);
                            insMyActRequired = false;
                            // eb_approval - update review_status
                            insUpQ += $@"UPDATE eb_approval SET review_status = '{_reviewStatus}', eb_lastmodified_by = @eb_modified_by, eb_lastmodified_at = {DataDB.EB_CURRENT_TIMESTAMP} 
                                    WHERE eb_src_id = @{this.TableName}_id AND eb_ver_id =  @{this.TableName}_eb_ver_id AND COALESCE(eb_del, 'F') = 'F'; ";
                        }
                        else
                        {
                            EbReviewStage nxtSt = currentStage;
                            if (!nxtStName.IsNullOrEmpty())
                                nxtSt = ebReview.FormStages.Find(e => e.Name == nxtStName);

                            if (nxtSt != null)
                            {
                                //backtrack to the same user - code here if needed
                                nextStage = nxtSt;
                                insMyActRequired = true;
                            }
                            else
                                throw new FormException("Unable to decide next stage", (int)HttpStatusCode.InternalServerError, "NextStage C# script returned a value that is not recognized as a stage", "Return value : " + nxtStName);
                        }
                    }
                    else if (reviewRowCount == 0)
                    {
                        if (this.FormDataBackup != null && this.FormDataBackup.MultipleTables.ContainsKey(ebReview.TableName))
                        {
                            if (this.FormDataBackup.MultipleTables[ebReview.TableName].Count == 0)
                            {
                                insInEdit = true;
                                nextStage = ebReview.FormStages[0];
                            }
                        }
                        if (!insInEdit)
                        {
                            Console.WriteLine("No items reviewed in this form data save");
                            return string.Empty;
                        }
                    }
                    else
                        throw new FormException("Bad Request for review control", (int)HttpStatusCode.BadRequest, "eb_approval_lines contains more than one rows, only one review allowed at a time", "From GetMyActionInsertUpdateQuery");
                }
            }

            if (isInsert || insMyActRequired || insInEdit)// first save or insert myaction required in edit
            {
                string _col = string.Empty, _val = string.Empty;
                this.MyActNotification = new MyActionNotification() { ApproverEntity = nextStage.ApproverEntity };
                if (nextStage.ApproverEntity == ApproverEntityTypes.Role)
                {
                    _col = "role_ids";
                    _val = $"@role_ids_{i}";
                    string roles = nextStage.ApproverRoles == null ? string.Empty : nextStage.ApproverRoles.Join(",");
                    param.Add(DataDB.GetNewParameter($"@role_ids_{i++}", EbDbTypes.String, roles));
                    this.MyActNotification.RoleIds = nextStage.ApproverRoles;
                }
                else if (nextStage.ApproverEntity == ApproverEntityTypes.UserGroup)
                {
                    _col = "usergroup_id";
                    _val = $"@usergroup_id_{i}";
                    param.Add(DataDB.GetNewParameter($"@usergroup_id_{i++}", EbDbTypes.Int32, nextStage.ApproverUserGroup));
                    this.MyActNotification.UserGroupId = nextStage.ApproverUserGroup;
                }
                else if (nextStage.ApproverEntity == ApproverEntityTypes.Users)
                {
                    string t1 = string.Empty, t2 = string.Empty, t3 = string.Empty;
                    List<DbParameter> _params = new List<DbParameter>();
                    int _idx = 0;
                    foreach (KeyValuePair<string, string> p in nextStage.QryParams)
                    {
                        if (EbFormHelper.IsExtraSqlParam(p.Key, this.TableName))
                            continue;
                        SingleTable Table = null;
                        if (this.FormData.MultipleTables.ContainsKey(p.Value))
                            Table = this.FormData.MultipleTables[p.Value];
                        else if (this.FormDataBackup != null && this.FormDataBackup.MultipleTables.ContainsKey(p.Value))
                            Table = this.FormDataBackup.MultipleTables[p.Value];
                        else
                            throw new FormException($"Bad Request", (int)HttpStatusCode.BadRequest, $"GetFirstMyActionInsertQuery: Review control parameter {p.Key} is not idetified", $"{p.Value} not found in MultipleTables");
                        TableSchema _table = this.FormSchema.Tables.Find(e => e.TableName == p.Value);
                        if (_table.TableType != WebFormTableTypes.Normal)
                            throw new FormException($"Bad Request", (int)HttpStatusCode.BadRequest, $"GetFirstMyActionInsertQuery: Review control parameter {p.Key} is not idetified", $"{p.Value} found in MultipleTables but it is not a normal table");
                        if (Table.Count != 1)
                            throw new FormException($"Bad Request", (int)HttpStatusCode.BadRequest, $"GetFirstMyActionInsertQuery: Review control parameter {p.Key} is not idetified", $"{p.Value} found in MultipleTables but table is empty");
                        SingleColumn Column = Table[0].Columns.Find(e => e.Control?.Name == p.Key);
                        if (Column == null || Column.Control == null)
                            throw new FormException($"Bad Request", (int)HttpStatusCode.BadRequest, $"GetFirstMyActionInsertQuery: Review control parameter {p.Key} is not idetified", $"{p.Value} found in MultipleTables but data not available");

                        ParameterizeCtrl_Params args = new ParameterizeCtrl_Params(DataDB, _params, Column, _idx, this.UserObj);
                        Column.Control.ParameterizeControl(args, this.CrudContext);
                        _idx = args.i;
                        _params[_idx - 1].ParameterName = p.Key;
                    }
                    List<int> uids = new List<int>();
                    EbFormHelper.AddExtraSqlParams(_params, DataDB, this.TableName, this.TableRowId, this.LocationId, this.UserObj.UserId);
                    EbDataTable dt = DataDB.DoQuery(nextStage.ApproverUsers.Code, _params.ToArray());
                    foreach (EbDataRow dr in dt.Rows)
                    {
                        int.TryParse(dr[0].ToString(), out int temp);
                        if (!uids.Contains(temp))
                            uids.Add(temp);
                    }
                    _col = "user_ids";
                    _val = $"'{uids.Join(",")}'";
                    this.MyActNotification.UserIds = uids;
                }
                else
                    throw new FormException("Unable to process review control", (int)HttpStatusCode.InternalServerError, "Invalid value for ApproverEntity : " + nextStage.ApproverEntity, "From GetMyActionInsertUpdateQuery");

                string description = null;
                string autoId = string.Empty;
                if (this.AutoId != null)
                {
                    if (isInsert)
                        autoId = $" ' || (SELECT {this.AutoId.Name} FROM {this.AutoId.TableName} WHERE {(this.AutoId.TableName == this.TableName ? string.Empty : (this.TableName + CharConstants.UNDERSCORE))}id = {masterId}) || ' ";
                    else if (this.FormDataBackup.MultipleTables.ContainsKey(this.AutoId.TableName) && this.FormDataBackup.MultipleTables[this.AutoId.TableName].Count > 0)
                        autoId = CharConstants.SPACE + Convert.ToString(this.FormDataBackup.MultipleTables[this.AutoId.TableName][0][this.AutoId.Name]) + CharConstants.SPACE;
                }
                if (!string.IsNullOrEmpty(nextStage.NotificationContent?.Code?.Trim()))
                {
                    if (globals == null)
                        globals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, this.FormData, this.FormDataBackup, DataDB, null, false);
                    object msg = this.ExecuteCSharpScriptNew(nextStage.NotificationContent.Code, globals);
                    description = Convert.ToString(msg);
                    if (!string.IsNullOrEmpty(description))
                    {
                        if (this.AutoId != null && isInsert && description.Contains(FormConstants.AutoId_PlaceHolder))
                            description = description.Replace(FormConstants.AutoId_PlaceHolder, autoId);
                    }
                }
                if (string.IsNullOrEmpty(description))
                    description = $"{this.DisplayName} {(autoId.IsEmpty() ? string.Empty : (CharConstants.SPACE + autoId))}in {nextStage.Name}";

                insUpQ += $@"INSERT INTO eb_my_actions({_col}, from_datetime, is_completed, eb_stages_id, form_ref_id, form_data_id, eb_del, description, is_form_data_editable, my_action_type)
                                VALUES ({_val}, {DataDB.EB_CURRENT_TIMESTAMP}, 'F', (SELECT id FROM eb_stages WHERE stage_unique_id = '{nextStage.EbSid}' AND form_ref_id = '{this.RefId}' AND eb_del = 'F'), 
                                '{this.RefId}', {masterId}, 'F', '{description}', '{(nextStage.IsFormEditable ? "T" : "F")}', '{MyActionTypes.Approval}'); ";
                if (DataDB.Vendor == DatabaseVendors.MYSQL)
                    insUpQ += "SELECT eb_persist_currval('eb_my_actions_id_seq'); ";

                this.MyActNotification.Title = "Review required";
                Console.WriteLine("Will try to INSERT eb_my_actions");

                if (isInsert)// eb_approval - insert entry here
                {
                    insUpQ += $@"INSERT INTO eb_approval(review_status, eb_my_actions_id, eb_src_id, eb_ver_id, eb_created_by, eb_created_at, eb_del)
                                    VALUES('In Process', (SELECT eb_currval('eb_my_actions_id_seq')), (SELECT eb_currval('{this.TableName}_id_seq')), 
                                    @{this.TableName}_eb_ver_id, @eb_createdby, {DataDB.EB_CURRENT_TIMESTAMP}, 'F'); ";
                }
                else if (insInEdit)
                {
                    insUpQ += $@"INSERT INTO eb_approval(review_status, eb_my_actions_id, eb_src_id, eb_ver_id, eb_created_by, eb_created_at, eb_del)
                                    VALUES('In Process', (SELECT eb_currval('eb_my_actions_id_seq')), @{this.TableName}_id, 
                                    @{this.TableName}_eb_ver_id, @eb_createdby, {DataDB.EB_CURRENT_TIMESTAMP}, 'F'); ";
                }
                else // eb_approval - update eb_my_actions_id
                {
                    insUpQ += $@"UPDATE eb_approval SET eb_my_actions_id = (SELECT eb_currval('eb_my_actions_id_seq')), eb_lastmodified_by = @eb_modified_by, eb_lastmodified_at = {DataDB.EB_CURRENT_TIMESTAMP} 
                                    WHERE eb_src_id = @{this.TableName}_id AND eb_ver_id =  @{this.TableName}_eb_ver_id AND COALESCE(eb_del, 'F') = 'F'; ";
                }
            }

            return insUpQ;
        }

        public string GetFileUploaderUpdateQuery(IDatabase DataDB, List<DbParameter> param, ref int i)
        {
            string _qry = string.Empty;
            foreach (EbControl control in this.FormSchema.ExtendedControls)
            {
                if (control is EbFileUploader)
                {
                    EbFileUploader _c = control as EbFileUploader;
                    if (this.FormData.ExtendedTables.ContainsKey(_c.Name) || (this.FormData.ExtendedTables.ContainsKey(_c.Name + "_add") && this.FormData.ExtendedTables.ContainsKey(_c.Name + "_del")))
                    {
                        if (this.FormGlobals == null)
                            this.FormGlobals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, this.FormData, null, DataDB, null, false);
                        string secCxtGet = null, secCxtSet = null;
                        if (_c.ContextGetExpr != null && !_c.ContextGetExpr.Code.IsNullOrEmpty())
                            secCxtGet = Convert.ToString(this.ExecuteCSharpScriptNew(_c.ContextGetExpr.Code, this.FormGlobals));
                        if (_c.ContextSetExpr != null && !_c.ContextSetExpr.Code.IsNullOrEmpty())
                            secCxtSet = Convert.ToString(this.ExecuteCSharpScriptNew(_c.ContextSetExpr.Code, this.FormGlobals));
                        _qry += _c.GetUpdateQuery2(DataDB, param, this.FormData.ExtendedTables, this.TableName, this.RefId.Split("-")[3], ref i, this.TableRowId, secCxtGet, secCxtSet);
                    }
                }
            }
            return _qry;
        }

        //IDatabase DataDB, List<DbParameter> param, SingleColumn cField, bool ins, ref int i, ref string _col, ref string _val
        public bool ParameterizeUnknown(ParameterizeCtrl_Params args)
        {
            string cFldName = this.SolutionObj.SolutionSettings.SystemColumns[SystemColumns.eb_row_num];
            if (cFldName == args.cField.Name)
                cFldName = SystemColumns.eb_row_num;
            else
                cFldName = args.cField.Name;

            if (EbColumnExtra.Params.ContainsKey(cFldName))
            {
                if (args.cField.Value == null)
                {
                    var p = args.DataDB.GetNewParameter(args.cField.Name + "_" + args.i, EbColumnExtra.Params[cFldName]);
                    p.Value = DBNull.Value;
                    args.param.Add(p);
                }
                else
                {
                    args.param.Add(args.DataDB.GetNewParameter(args.cField.Name + "_" + args.i, EbColumnExtra.Params[cFldName], args.cField.Value));
                }
                if (args.ins)
                {
                    args._cols += string.Concat(args.cField.Name, ", ");
                    args._vals += string.Concat("@", args.cField.Name, "_", args.i, ", ");
                }
                else
                    args._colvals += string.Concat(args.cField.Name, "=@", args.cField.Name, "_", args.i, ", ");
                args.i++;
                return true;
            }
            else if (!args.cField.Name.Equals("id"))
                Console.WriteLine($"Unknown parameter found in formdata... \nForm RefId : {this.RefId}\tName : {args.cField.Name}\tType : {args.cField.Type}\tValue : {args.cField.Value}");
            return false;
        }

        //form data submission using PushJson and FormGlobals - SQL Job
        public void PrepareWebFormData(IDatabase DataDB, Service service, string PushJson, FormGlobals FormGlobals)
        {
            this.FormData = new WebformData() { MasterTable = this.FormSchema.MasterTable };
            JObject JObj = JObject.Parse(PushJson);

            foreach (TableSchema _table in this.FormSchema.Tables)
            {
                if (JObj[_table.TableName] != null)
                {
                    SingleTable Table = new SingleTable();
                    foreach (JToken jRow in JObj[_table.TableName])
                    {
                        Table.Add(this.GetSingleRow(jRow, _table, FormGlobals));
                    }
                    this.FormData.MultipleTables.Add(_table.TableName, Table);
                }
            }
            this.MergeFormData();

            if (this.TableRowId > 0)//if edit mode then fill or map id by refering FormDataBackup
            {
                this.RefreshFormData(DataDB, service, true, true);

                foreach (KeyValuePair<string, SingleTable> entry in this.FormDataBackup.MultipleTables)
                {
                    if (this.FormData.MultipleTables.ContainsKey(entry.Key))
                    {
                        for (int i = 0; i < entry.Value.Count; i++)
                        {
                            if (i < this.FormData.MultipleTables[entry.Key].Count)
                                this.FormData.MultipleTables[entry.Key][i].RowId = entry.Value[i].RowId;
                            else
                            {
                                this.FormData.MultipleTables[entry.Key].Add(entry.Value[i]);
                                this.FormData.MultipleTables[entry.Key][i].IsDelete = true;
                            }
                        }
                    }
                    else
                    {
                        this.FormData.MultipleTables.Add(entry.Key, entry.Value);
                        foreach (SingleRow Row in this.FormData.MultipleTables[entry.Key])
                            Row.IsDelete = true;
                    }
                }
            }
        }

        //form data submission using PushJson and FormGlobals - SQL Job, Excel Import save
        public string Save(IDatabase DataDB, Service service, DbConnection DbCon)
        {
            if (DbCon == null)
                this.DbConnection = DataDB.GetNewConnection();
            else
                this.DbConnection = DbCon;

            string resp;
            try
            {
                if (DbCon == null)
                {
                    this.DbConnection.Open();
                    this.DbTransaction = this.DbConnection.BeginTransaction();
                }
                this.ExecProvUserCreateOnlyIfScript();
                bool IsUpdate = this.TableRowId > 0;
                if (IsUpdate)
                {
                    this.RefreshFormData(DataDB, service, true, false);
                    resp = "Updated: " + this.Update(DataDB, service);
                }
                else
                {
                    this.TableRowId = this.Insert(DataDB, service);
                    resp = "Inserted: " + this.TableRowId;
                    Console.WriteLine("New record inserted. Table :" + this.TableName + ", Id : " + this.TableRowId);
                }
                this.RefreshFormData(DataDB, service, false, true);
                Console.WriteLine("EbWebForm.Save.UpdateAuditTrail start");
                EbAuditTrail ebAuditTrail = new EbAuditTrail(this, DataDB);
                resp += " - AuditTrail: " + ebAuditTrail.UpdateAuditTrail();
                resp += " - AfterSave: " + this.AfterSave(DataDB, IsUpdate);

                if (DbCon == null)
                    this.DbTransaction.Commit();
            }
            catch (Exception ex1)
            {
                try
                {
                    if (DbCon == null)
                        this.DbTransaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Rollback Exception Type: {ex2.GetType()}\nMessage: {ex2.Message}");
                }
                throw new FormException("Exception in Form data save", (int)HttpStatusCode.InternalServerError, ex1.Message, ex1.StackTrace);
            }
            if (DbCon == null)
                this.DbConnection.Close();
            return resp;
        }

        //Combined CS script creation and execution// under testing
        private void PrepareWebFormData()
        {
            FG_Root globals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, this.FormData, this.FormDataBackup, null, null, true);
            globals.DestinationForms = GlobalsGenerator.GetEmptyDestinationModelGlobals(this.FormCollection.Skip(1).ToList());

            EbDataPushHelper ebDataPushHelper = new EbDataPushHelper(this);
            string code = ebDataPushHelper.GetProcessedSingleCode();
            if (code != string.Empty)
            {
                object out_dict = this.ExecuteCSharpScriptNew(code, globals);
                ebDataPushHelper.CreateWebFormData(out_dict);
            }
        }

        //Excel Import
        public List<int> ProcessBatchRequest(EbDataTable Data, IDatabase DataDB, Service service, DbConnection TransactionConnection)
        {
            EbDataPushHelper DataPushHelper = new EbDataPushHelper(this);
            string Json = DataPushHelper.GetPusherJson(Data);
            string Code = DataPushHelper.GetProcessedCode(Json);
            Script _script = CSharpScript.Create<dynamic>(
                Code,
                ScriptOptions.Default.WithReferences("Microsoft.CSharp", "System.Core").WithImports("System", "System.Collections.Generic", "System.Linq"),
                globalsType: typeof(FG_Root)
            );
            _script.Compile();

            List<int> DataIds = new List<int>();
            for (int i = 0; i < Data.Rows.Count; i++)
            {
                FG_Root fG_Root = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, Data, i);
                object out_dict = null;
                try
                {
                    out_dict = _script.RunAsync(fG_Root).Result.ReturnValue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception in C# Expression evaluation:" + Code + " \nMessage : " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    throw new FormException("Exception in C# code evaluation", (int)HttpStatusCode.InternalServerError, $"{ex.Message} \n C# code : {Code}", $"StackTrace : {ex.StackTrace}");
                }
                this.TableRowId = 0;//insert
                DataPushHelper.CreateWebFormData_Demo(out_dict, Json);
                this.Save(DataDB, service, TransactionConnection);
                DataIds.Add(this.TableRowId);
            }
            if (this.FormSchema.ExtendedControls.Find(e => e is EbProvisionUser) != null)
            {
                Console.WriteLine("ProcessBatchRequest - UpdateSolutionObjectRequest start");
                service.Gateway.Send<UpdateSolutionObjectResponse>(new UpdateSolutionObjectRequest { SolnId = this.SolutionObj.SolutionID, UserId = this.UserObj.UserId });
            }
            return DataIds;
        }

        //duplicate for SQL job - remove this fn if globals conversion is completed
        private SingleRow GetSingleRow(JToken JRow, TableSchema _table, FormGlobals globals)
        {
            SingleRow Row = new SingleRow() { RowId = 0 };
            foreach (ColumnSchema _column in _table.Columns)
            {
                object val = null;
                if (JRow[_column.ColumnName] != null)
                    val = this.ExecuteCSharpScript(JRow[_column.ColumnName].ToString(), globals);

                Row.Columns.Add(new SingleColumn
                {
                    Name = _column.ColumnName,
                    Type = _column.EbDbType,
                    Value = val
                });
            }
            return Row;
        }

        private object ExecuteCSharpScript(string code, FormGlobals globals)
        {
            try
            {
                Script valscript = CSharpScript.Create<dynamic>(
                    code,
                    ScriptOptions.Default.WithReferences("Microsoft.CSharp", "System.Core").WithImports("System.Dynamic", "System", "System.Collections.Generic",
                    "System.Diagnostics", "System.Linq"),
                    globalsType: typeof(FormGlobals)
                );
                //var compilation = valscript.GetCompilation();
                //var ilstream = new MemoryStream();
                //var pdbstream = new MemoryStream();
                //compilation.Emit(ilstream, pdbstream);
                valscript.Compile();
                return (valscript.RunAsync(globals)).Result.ReturnValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in C# Expression evaluation:" + code + " \nMessage : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new FormException("Exception in C# code evaluation", (int)HttpStatusCode.InternalServerError, $"{ex.Message} \n C# code : {code}", $"StackTrace : {ex.StackTrace}");
            }
        }

        public object ExecuteCSharpScriptNew(string code, FG_Root globals)
        {
            try
            {
                Script valscript = CSharpScript.Create<dynamic>(
                    code,
                    ScriptOptions.Default.WithReferences("Microsoft.CSharp", "System.Core").WithImports("System", "System.Collections.Generic", "System.Linq"),
                    globalsType: typeof(FG_Root)
                );
                valscript.Compile();
                return (valscript.RunAsync(globals)).Result.ReturnValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in C# Expression evaluation:" + code + " \nMessage : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new FormException("Exception in C# code evaluation", (int)HttpStatusCode.InternalServerError, $"{ex.Message} \n C# code : {code}", $"StackTrace : {ex.StackTrace}");
            }
        }

        //execute sql queries after form data save
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
                    if (item.Value.Count == 0)
                        continue;
                    foreach (SingleColumn cField in item.Value[item.Value.Count - 1].Columns)
                    {
                        if (q.Contains("@" + item.Key + "_" + cField.Name) || q.Contains(":" + item.Key + "_" + cField.Name))
                        {
                            if (cField.Value == null)
                            {
                                var p = DataDB.GetNewParameter(item.Key + "_" + cField.Name, (EbDbTypes)cField.Type);
                                p.Value = DBNull.Value;
                                param.Add(p);
                            }
                            else
                                param.Add(DataDB.GetNewParameter(item.Key + "_" + cField.Name, (EbDbTypes)cField.Type, cField.Value));
                        }
                    }
                }
                return DataDB.DoNonQuery(this.DbConnection, q, param.ToArray());
            }
            return 0;
        }

        public void AfterExecutionIfUserCreated(Service Service, Common.Connections.EbMailConCollection EmailCon, RabbitMqProducer MessageProducer3, string wc, Dictionary<string, string> MetaData)
        {
            bool UpdateSoluObj = false;
            foreach (EbControl c in this.FormSchema.ExtendedControls)
            {
                if (c is EbProvisionUser && (c as EbProvisionUser).IsUserCreated())
                {
                    UpdateSoluObj = true;

                    EbProvisionUser pc = c as EbProvisionUser;

                    if (this.FormData?.MultipleTables.ContainsKey(pc.TableName) == true && this.FormData?.MultipleTables[pc.TableName].Count > 0)
                    {
                        SingleColumn col = this.FormData.MultipleTables[pc.TableName][0].Columns.Find(e => e.Name == pc.Name);
                        if (col != null && !MetaData.ContainsKey(FormMetaDataKeys.signup_user))// && this.RefId == this.SolutionObj.SolutionSettings.SignupFormRefid
                        {
                            //pc.UserCredentials.UserId = Convert.ToInt32(col.Value);
                            Dictionary<string, string> _od = JsonConvert.DeserializeObject<Dictionary<string, string>>(col.F);
                            EbSignUpUserInfo _user = new EbSignUpUserInfo()
                            {
                                UserName = string.IsNullOrEmpty(_od[FormConstants.email]) ? _od[FormConstants.phprimary] : _od[FormConstants.email],
                                AuthId = string.Format(TokenConstants.SUB_FORMAT, this.SolutionObj.SolutionID, _od[FormConstants.id], wc),
                                UserType = Convert.ToInt32(_od[FormConstants.usertype])
                            };

                            if (pc.SendVerificationMsg)
                            {
                                string msg = string.Empty;
                                _user.VerificationRequired = true;
                                Authenticate2FAResponse resp = Service.Gateway.Send<Authenticate2FAResponse>(new SendUserVerifCodeRequest
                                {
                                    UserId = Convert.ToInt32(_od[FormConstants.id]),
                                    WC = wc,
                                    SolnId = this.SolutionObj.SolutionID
                                });
                                if (!string.IsNullOrEmpty(_od[FormConstants.email]))
                                {
                                    if (resp.EmailVerifCode.AuthStatus)
                                    {
                                        _user.VerifyEmail = _od[FormConstants.email];
                                    }
                                    msg = resp.EmailVerifCode.Message;
                                }
                                if (!string.IsNullOrEmpty(_od[FormConstants.phprimary]))
                                {
                                    if (resp.MobileVerifCode.AuthStatus)
                                    {
                                        _user.VerifyPhone = _od[FormConstants.phprimary];
                                    }
                                    msg += "; " + resp.MobileVerifCode.Message;
                                }
                                _user.Message = msg;
                            }
                            else
                            {
                                _user.VerificationRequired = false;
                                _user.Message = "Verification is not required";
                            }
                            _user.Token = EbTokenGenerator.GenerateToken(_user.AuthId);
                            MetaData.Add(FormMetaDataKeys.signup_user, JsonConvert.SerializeObject(_user));
                        }
                    }

                    //Console.WriteLine("AfterExecutionIfUserCreated - New User creation found");
                    //if (EmailCon?.Primary != null)
                    //{
                    //    Console.WriteLine("AfterExecutionIfUserCreated - SendWelcomeMail start");
                    //    (c as EbProvisionUser).SendWelcomeMail(MessageProducer3, this.UserObj, this.SolutionObj);
                    //}
                }
                else if (c is EbProvisionLocation && (c as EbProvisionLocation).IsLocationCreated)
                    UpdateSoluObj = true;
            }
            if (UpdateSoluObj)
            {
                Console.WriteLine("AfterExecutionIfUserCreated - UpdateSolutionObjectRequest start");
                var temp = Service.Gateway.Send<UpdateSolutionObjectResponse>(new UpdateSolutionObjectRequest { SolnId = this.SolutionObj.SolutionID, UserId = this.UserObj.UserId });
            }
        }

        //to check whether this form data entry can be delete by executing DisableDelete sql quries
        private bool CanDelete(IDatabase DataDB)
        {
            if (this.DisableDelete != null && this.DisableDelete.Count > 0)
            {
                string q = string.Join(";", this.DisableDelete.Select(e => e.Script.Code));
                DbParameter[] p = new DbParameter[] {
                    DataDB.GetNewParameter(FormConstants.id, EbDbTypes.Int32, this.TableRowId)
                };
                EbDataSet ds = DataDB.DoQueries(q, p);

                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    if (ds.Tables[i].Rows.Count > 0 && ds.Tables[i].Rows[0].Count > 0)
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
            int status = -1;
            if (this.CanDelete(DataDB))
            {
                try
                {
                    this.DbConnection = DataDB.GetNewConnection();
                    this.DbConnection.Open();
                    this.DbTransaction = this.DbConnection.BeginTransaction();

                    string query = QueryGetter.GetDeleteQuery(this, DataDB);
                    DbParameter[] param = new DbParameter[] {
                        DataDB.GetNewParameter(FormConstants.eb_lastmodified_by, EbDbTypes.Int32, this.UserObj.UserId),
                        DataDB.GetNewParameter(this.TableName + FormConstants._id, EbDbTypes.Int32, this.TableRowId)
                    };
                    status = DataDB.DoNonQuery(this.DbConnection, query, param);
                    if (status > 0)
                    {
                        EbAuditTrail ebAuditTrail = new EbAuditTrail(this, DataDB);
                        ebAuditTrail.UpdateAuditTrail(DataModificationAction.Deleted);
                    }
                    this.DbTransaction.Commit();
                    this.DbConnection.Close();
                }
                catch (Exception ex)
                {
                    this.DbTransaction.Rollback();
                    Console.WriteLine("Exception in Delete/RevokeDelete: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return status;
        }

        //to check whether this form data entry can be cancel by executing DisableCancel sql quries
        private bool CanCancel(IDatabase DataDB)
        {
            EbSystemColumns ebs = this.SolutionObj.SolutionSettings.SystemColumns;

            string LockCheckQry = string.Format("SELECT id FROM {0} WHERE id = @id AND COALESCE({1}, {2}) = {2} AND COALESCE({3}, {4}) = {4};",
                this.TableName,
                ebs[SystemColumns.eb_del],
                ebs.GetBoolFalse(SystemColumns.eb_del),
                ebs[SystemColumns.eb_lock],
                ebs.GetBoolFalse(SystemColumns.eb_lock));

            DbParameter[] p = new DbParameter[] {
                DataDB.GetNewParameter(FormConstants.id, EbDbTypes.Int32, this.TableRowId)
            };

            if (this.DisableCancel != null && this.DisableCancel.Count > 0)
            {
                string q = string.Join(";", this.DisableCancel.Select(e => e.Script.Code));

                EbDataSet ds = DataDB.DoQueries(q + LockCheckQry, p);
                int i = 0;
                for (; i < this.DisableCancel.Count; i++)
                {
                    if (ds.Tables[i].Rows.Count > 0 && ds.Tables[i].Rows[0].Count > 0)
                    {
                        if (!this.DisableCancel[i].IsDisabled && Convert.ToInt32(ds.Tables[0].Rows[0][0]) > 0 && !this.DisableCancel[i].IsWarningOnly)
                            return false;
                    }
                }
                if (ds.Tables[i].Rows.Count <= 0)
                    return false;
            }
            else
            {
                EbDataSet ds = DataDB.DoQueries(LockCheckQry, p);
                if (ds.Tables[0].Rows.Count <= 0)
                    return false;
            }
            return true;
        }

        public int Cancel(IDatabase DataDB, bool Cancel)
        {
            int status = -1;
            if (this.CanCancel(DataDB))
            {
                try
                {
                    this.DbConnection = DataDB.GetNewConnection();
                    this.DbConnection.Open();
                    this.DbTransaction = this.DbConnection.BeginTransaction();

                    string query = QueryGetter.GetCancelQuery(this, DataDB, Cancel);
                    DbParameter[] param = new DbParameter[] {
                        DataDB.GetNewParameter(FormConstants.eb_lastmodified_by, EbDbTypes.Int32, this.UserObj.UserId),
                        DataDB.GetNewParameter(this.TableName + FormConstants._id, EbDbTypes.Int32, this.TableRowId)
                    };
                    status = DataDB.DoNonQuery(this.DbConnection, query, param);
                    if (status > 0)
                    {
                        EbAuditTrail ebAuditTrail = new EbAuditTrail(this, DataDB);
                        ebAuditTrail.UpdateAuditTrail(Cancel ? DataModificationAction.Cancelled : DataModificationAction.CancelReverted);
                    }
                    this.DbTransaction.Commit();
                    this.DbConnection.Close();
                }
                catch (Exception ex)
                {
                    this.DbTransaction.Rollback();
                    Console.WriteLine("Exception in Cancel/RevokeCancel: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return status;
        }

        public int LockOrUnlock(IDatabase DataDB, bool Lock)
        {
            int status = -1;
            try
            {
                this.DbConnection = DataDB.GetNewConnection();
                this.DbConnection.Open();
                this.DbTransaction = this.DbConnection.BeginTransaction();

                string query = QueryGetter.GetLockOrUnlockQuery(this, DataDB, Lock);
                DbParameter[] param = new DbParameter[] {
                        DataDB.GetNewParameter(FormConstants.eb_lastmodified_by, EbDbTypes.Int32, this.UserObj.UserId),
                        DataDB.GetNewParameter(this.TableName + FormConstants._id, EbDbTypes.Int32, this.TableRowId)
                    };
                status = DataDB.DoNonQuery(this.DbConnection, query, param);
                if (status > 0)
                {
                    EbAuditTrail ebAuditTrail = new EbAuditTrail(this, DataDB);
                    ebAuditTrail.UpdateAuditTrail(Lock ? DataModificationAction.Locked : DataModificationAction.Unlocked);
                }
                this.DbTransaction.Commit();
                this.DbConnection.Close();
            }
            catch (Exception ex)
            {
                this.DbTransaction.Rollback();
                Console.WriteLine("Exception in Lock/Unlock: " + ex.Message + "\n" + ex.StackTrace);
            }
            return status;
        }

        private void ExeDeleteCancelEditScript(IDatabase DataDB, WebformData _FormData)
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
            if (this.Disable_Edit?.Count > 0)
            {
                q += string.Join(";", this.Disable_Edit.FindAll(e => e.Script.Lang == ScriptingLanguage.SQL).Select(e => e.Script.Code));
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

                List<EbRoutine> rt = this.Disable_Edit.FindAll(e => e.Script.Lang == ScriptingLanguage.SQL);
                for (int j = 0; j < rt.Count; i++, j++)
                {
                    if (ds.Tables[i].Rows.Count > 0 && ds.Tables[i].Rows[0].Count > 0 && Convert.ToInt32(ds.Tables[i].Rows[0][0]) > 0)
                    {
                        this.FormData.DisableEdit.Add(rt[j].Name, true);
                    }
                    else
                    {
                        this.FormData.DisableEdit.Add(rt[j].Name, false);
                    }
                }
            }

            if (this.Disable_Edit?.Count > 0)
            {
                foreach (EbRoutine rt in this.Disable_Edit)
                {
                    if (rt.Script.Lang == ScriptingLanguage.CSharp && !string.IsNullOrEmpty(rt.Script.Code))
                    {
                        if (this.FormGlobals == null)
                            this.FormGlobals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, _FormData, null, DataDB, null, false);
                        bool status = Convert.ToBoolean(this.ExecuteCSharpScriptNew(rt.Script.Code, this.FormGlobals));
                        this.FormData.DisableEdit.Add(rt.Name, status);
                    }
                }
            }
        }

        public string GetAuditTrail(IDatabase DataDB, Service Service)
        {
            EbAuditTrail ebAuditTrail = new EbAuditTrail(this, DataDB, Service);
            return ebAuditTrail.GetAuditTrail();
        }

        public Dictionary<int, List<int>> GetLocBasedPermissions()
        {
            Dictionary<int, List<int>> _perm = new Dictionary<int, List<int>>();
            //New View Edit Delete Cancel Print AuditTrail

            foreach (int locid in this.SolutionObj.Locations.Keys)
            {
                List<int> _temp = new List<int>();
                foreach (EbOperation op in Operations.Enumerator)
                {
                    if (EbFormHelper.HasPermission(this.UserObj, RefId, op.Name, locid))
                        _temp.Add(op.IntCode);
                }
                _perm.Add(locid, _temp);
            }
            return _perm;
        }

        public void DoRequiredCheck(bool IsMasterForm)
        {
            if (this.DataPusherConfig?.AllowPush == false)
                return;

            foreach (TableSchema _table in this.FormSchema.Tables)
            {
                if (!(this.FormData.MultipleTables.TryGetValue(_table.TableName, out SingleTable Table) && Table.Count > 0))
                    continue;

                foreach (ColumnSchema _column in _table.Columns.FindAll(e => e.Control.Required && !e.Control.BypassParameterization))
                {
                    foreach (SingleRow Row in Table)
                    {
                        if (Row.IsDelete)
                            continue;
                        SingleColumn cField = Row.GetColumn(_column.ColumnName);
                        if (string.IsNullOrWhiteSpace(Convert.ToString(cField?.Value)) || (Double.TryParse(Convert.ToString(cField.Value), out double __val) && __val == 0))
                        {
                            string msg = $"is Required {(IsMasterForm ? "" : "(DataPusher Field)")} {(cField.Control.Hidden ? "[Hidden]" : "")}";
                            if (_table.TableType == WebFormTableTypes.Grid)
                                msg = $"'{(cField.Control as EbDGColumn).Title ?? cField.Control.Name}' in {_table.Title ?? _table.ContainerName} Grid {msg}";
                            else
                                msg = $"'{cField.Control.Label ?? cField.Control.Name}' {msg}";
                            throw new FormException(msg, (int)HttpStatusCode.BadRequest, msg, "EbWebForm -> DoRequiredCheck");
                        }
                    }
                }
            }
        }

        public void AfterRedisGet_All(Service service)
        {
            EbFormHelper.AfterRedisGet(this, service.Redis, null, service);
            SchemaHelper.GetWebFormSchema(this);
            EbFormHelper.InitDataPushers(this, service.Redis, null, service);
        }

        public void AfterRedisGet_All(RedisClient Redis, IServiceClient client)
        {
            EbFormHelper.AfterRedisGet(this, Redis, client, null);
            SchemaHelper.GetWebFormSchema(this);
            EbFormHelper.InitDataPushers(this, Redis, client, null);
        }

        public override void AfterRedisGet(RedisClient Redis, IServiceClient client)
        {
            EbFormHelper.AfterRedisGet(this, Redis, client, null);
        }

        public override List<string> DiscoverRelatedRefids()
        {
            return EbFormHelper.DiscoverRelatedRefids(this);
        }
        public override void ReplaceRefid(Dictionary<string, string> RefidMap)
        {
            EbFormHelper.ReplaceRefid(this, RefidMap);
        }

        //-------------Backup------------
        //private void PrepareWebFormData()
        //{
        //    DateTime startdt = DateTime.Now;
        //    FG_Root globals = GlobalsGenerator.GetCSharpFormGlobals_NEW(this, this.FormData, this.FormDataBackup);
        //    foreach (EbDataPusher pusher in this.DataPushers)
        //    {
        //        pusher.WebForm.DataPusherConfig.SourceRecId = this.TableRowId;
        //        pusher.WebForm.RefId = pusher.FormRefId;
        //        pusher.WebForm.UserObj = this.UserObj;
        //        pusher.WebForm.LocationId = this.LocationId;
        //        pusher.WebForm.SolutionObj = this.SolutionObj;

        //        if (!pusher.PushOnlyIf.IsNullOrEmpty())
        //        {
        //            string status = Convert.ToString(pusher.WebForm.ExecuteCSharpScriptNew(pusher.PushOnlyIf, globals));
        //            if (status.Equals(true.ToString()))
        //                pusher.WebForm.DataPusherConfig.AllowPush = true;
        //        }
        //        else
        //            pusher.WebForm.DataPusherConfig.AllowPush = true;

        //        if (pusher.WebForm.DataPusherConfig.AllowPush)
        //        {
        //            pusher.WebForm.ProcessPushJson(pusher, globals);
        //            pusher.WebForm.MergeFormData();
        //        }
        //        else
        //            pusher.WebForm.FormData = new WebformData();

        //        if (this.TableRowId > 0)//if edit mode then fill or map the id by refering FormDataBackup
        //        {
        //            if (pusher.WebForm.DataPusherConfig.AllowPush)
        //            {
        //                if (pusher.WebForm.FormDataBackup != null)
        //                {
        //                    foreach (KeyValuePair<string, SingleTable> entry in pusher.WebForm.FormDataBackup.MultipleTables)
        //                    {
        //                        if (pusher.WebForm.FormData.MultipleTables.ContainsKey(entry.Key))
        //                        {
        //                            for (int i = 0; i < entry.Value.Count; i++)
        //                            {
        //                                if (i < pusher.WebForm.FormData.MultipleTables[entry.Key].Count)
        //                                    pusher.WebForm.FormData.MultipleTables[entry.Key][i].RowId = entry.Value[i].RowId;
        //                                else
        //                                {
        //                                    pusher.WebForm.FormData.MultipleTables[entry.Key].Add(entry.Value[i]);
        //                                    pusher.WebForm.FormData.MultipleTables[entry.Key][i].IsDelete = true;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            pusher.WebForm.FormData.MultipleTables.Add(entry.Key, entry.Value);
        //                            foreach (SingleRow Row in pusher.WebForm.FormData.MultipleTables[entry.Key])
        //                                Row.IsDelete = true;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                pusher.WebForm.FormData = pusher.WebForm.FormDataBackup;
        //                foreach (KeyValuePair<string, SingleTable> entry in pusher.WebForm.FormData.MultipleTables)
        //                {
        //                    foreach (SingleRow Row in entry.Value)
        //                        Row.IsDelete = true;
        //                }
        //            }
        //        }
        //    }
        //    Console.WriteLine("PrepareWebFormData for Data Pushers. Execution Time = " + (DateTime.Now - startdt).TotalMilliseconds);
        //}

        //public void ProcessPushJson(EbDataPusher pusher, FG_Root globals)
        //{
        //    this.FormData = new WebformData() { MasterTable = this.FormSchema.MasterTable };
        //    JObject JObj = JObject.Parse(pusher.Json);
        //    foreach (TableSchema _table in this.FormSchema.Tables)
        //    {
        //        if (JObj[_table.TableName] != null)
        //        {
        //            SingleTable Table = new SingleTable();
        //            foreach (JToken jRow in JObj[_table.TableName])
        //            {
        //                if (_table.TableType == WebFormTableTypes.Grid && !pusher.SkipLineItemIf.IsNullOrEmpty())
        //                {
        //                    string status = Convert.ToString(this.ExecuteCSharpScriptNew(pusher.SkipLineItemIf, globals));
        //                    if (status.Equals(true.ToString()))
        //                        continue;
        //                }
        //                Table.Add(this.GetSingleRow(jRow, _table, globals));
        //            }
        //            this.FormData.MultipleTables.Add(_table.TableName, Table);
        //        }
        //    }
        //}

        //private SingleRow GetSingleRow(JToken JRow, TableSchema _table, FG_Root globals)
        //{
        //    SingleRow Row = new SingleRow() { RowId = 0 };
        //    foreach (ColumnSchema _column in _table.Columns)
        //    {
        //        object val = null;
        //        if (JRow[_column.ColumnName] != null)
        //            val = this.ExecuteCSharpScriptNew(JRow[_column.ColumnName].ToString(), globals);
        //        Row.Columns.Add(new SingleColumn
        //        {
        //            Name = _column.ColumnName,
        //            Type = _column.EbDbType,
        //            Value = val
        //        });
        //    }
        //    return Row;
        //}

        //public void GetImportData123(IDatabase DataDB, Service Service, EbWebForm Form)
        //{
        //    this.RefreshFormData(DataDB, Service);
        //    foreach (TableSchema _table in this.FormSchema.Tables)
        //    {
        //        if (this.FormData.MultipleTables.ContainsKey(_table.TableName))
        //        {
        //            SingleTable Table = this.FormData.MultipleTables[_table.TableName];
        //            int rowCounter = -501;
        //            foreach (SingleRow Row in Table)
        //            {
        //                Row.Columns.RemoveAll(e => e.Name == "id");
        //                Row.RowId = rowCounter--;
        //            }
        //            this.FormData.MultipleTables.Remove(_table.TableName);
        //            if (_table.TableName == this.FormSchema.MasterTable)
        //            {
        //                this.FormData.MultipleTables.Add(Form.Name, Table);
        //                this.FormData.MasterTable = Form.Name;
        //            }
        //            else
        //            {
        //                if (_table.TableType == WebFormTableTypes.Normal)
        //                    this.FormData.MultipleTables[this.FormData.MasterTable][0].Columns.AddRange(Table[0].Columns);
        //                else
        //                    this.FormData.MultipleTables.Add(_table.ContainerName, Table);
        //            }
        //        }
        //    }
        //}
    }


    [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl, BuilderType.DashBoard, BuilderType.DVBuilder)]
    [HideInToolBox]
    [UsedWithTopObjectParent(typeof(EbObject), typeof(EbDashBoardWraper), typeof(EbDataVisualizationObject))]
    [Alias("URL")]
    public class EbURL
    {
        [Alias("URL")]
        public EbURL() { }

        [HideInPropertyGrid]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl, BuilderType.DashBoard, BuilderType.DVBuilder)]
        public string EbSid { get; set; }

        [PropertyGroup(PGConstants.CORE)]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl, BuilderType.DashBoard, BuilderType.DVBuilder)]
        [EbRequired]
        [Unique]
        [regexCheck]
        [InputMask("[a-z][a-z0-9]*(_[a-z0-9]+)*")]
        public string Name { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl, BuilderType.DashBoard, BuilderType.DVBuilder)]
        [PropertyGroup(PGConstants.CORE)]
        [Alias("URL")]
        [OnChangeExec(@"
            if (this.URL) {
                if (EbIsValidURL(this.URL)) {
                    pg.setSimpleProperty('URL', this.URL.replace('.youtube.com/watch?v=', '.youtube.com/embed/').replace(/\?rel=0?$/, '') + '?rel=0');
                }
            }
        ")]
        public string URL { get; set; }

        [PropertyGroup(PGConstants.CORE)]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl, BuilderType.DashBoard, BuilderType.DVBuilder)]
        [Unique]
        public string Title { get; set; }

        [PropertyGroup(PGConstants.CORE)]
        [EnableInBuilder(BuilderType.WebForm, BuilderType.FilterDialog, BuilderType.BotForm, BuilderType.UserControl, BuilderType.DashBoard, BuilderType.DVBuilder)]
        public bool Hide { get; set; }
    }
}
