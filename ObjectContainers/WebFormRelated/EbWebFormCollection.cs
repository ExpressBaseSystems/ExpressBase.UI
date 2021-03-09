﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.LocationNSolution;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.ServiceStack_Artifacts;
using ExpressBase.Objects.WebFormRelated;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;

namespace ExpressBase.Objects
{
    public class EbWebFormCollection : List<EbWebForm>
    {
        private EbWebForm MasterForm { get; set; }

        public EbWebFormCollection(EbWebForm ebWebForm)
        {
            this.Add(ebWebForm);
            this.MasterForm = ebWebForm;
        }

        public void Insert(IDatabase DataDB, List<DbParameter> param, ref string fullqry, ref string _extqry, ref int i)
        {
            foreach (EbWebForm WebForm in this)
            {
                if (WebForm.DataPusherConfig?.AllowPush == false)
                    continue;
                if (!(WebForm.FormData.MultipleTables.ContainsKey(WebForm.FormSchema.MasterTable) && WebForm.FormData.MultipleTables[WebForm.FormSchema.MasterTable].Count > 0))
                {
                    string _q = QueryGetter.GetInsertQuery(WebForm, DataDB, WebForm.FormSchema.MasterTable, true);
                    fullqry += string.Format(_q, string.Empty, string.Empty);
                }
                foreach (TableSchema _table in WebForm.FormSchema.Tables)
                {
                    if (!WebForm.FormData.MultipleTables.ContainsKey(_table.TableName))
                        continue;

                    foreach (SingleRow row in WebForm.FormData.MultipleTables[_table.TableName])
                    {
                        string _cols = string.Empty;
                        string _values = string.Empty;

                        foreach (SingleColumn cField in row.Columns)
                        {
                            if (cField.Control != null)
                                cField.Control.ParameterizeControl(DataDB, param, WebForm.TableName, cField, true, ref i, ref _cols, ref _values, ref _extqry, WebForm.UserObj, null);
                            else
                                WebForm.ParameterizeUnknown(DataDB, param, cField, true, ref i, ref _cols, ref _values);
                        }

                        string _qry = QueryGetter.GetInsertQuery(WebForm, DataDB, _table.TableName, true);
                        fullqry += string.Format(_qry, _cols, _values);

                        fullqry += WebForm.InsertUpdateLines(_table.TableName, row, DataDB, param, ref i);
                    }
                }
                param.Add(DataDB.GetNewParameter(WebForm.TableName + FormConstants._eb_ver_id, EbDbTypes.Int32, WebForm.RefId.Split(CharConstants.DASH)[4]));
                param.Add(DataDB.GetNewParameter("refid", EbDbTypes.String, WebForm.RefId));
            }
        }

        public void Update(IDatabase DataDB, List<DbParameter> param, ref string fullqry, ref string _extqry, ref int i)
        {
            foreach (EbWebForm WebForm in this)
            {
                foreach (KeyValuePair<string, SingleTable> entry in WebForm.FormData.MultipleTables)
                {
                    foreach (SingleRow row in entry.Value)
                    {
                        string _colvals = string.Empty;
                        string _temp = string.Empty;
                        if (row.RowId > 0)
                        {
                            SingleRow bkup_Row = WebForm.FormDataBackup.MultipleTables[entry.Key].Find(e => e.RowId == row.RowId);
                            if (bkup_Row == null)
                            {
                                Console.WriteLine($"Row edit request ignored(Row not in backup table). \nTable name: {entry.Key}, RowId: {row.RowId}, RefId: {WebForm.RefId}");
                                continue;
                            }
                            string t = string.Empty;
                            if (!row.IsDelete)
                            {
                                foreach (SingleColumn cField in row.Columns)
                                {
                                    if (cField.Control != null)
                                    {
                                        SingleColumn ocF = bkup_Row.Columns.Find(e => e.Name.Equals(cField.Name));
                                        cField.Control.ParameterizeControl(DataDB, param, WebForm.TableName, cField, false, ref i, ref _colvals, ref _temp, ref _extqry, WebForm.UserObj, ocF);
                                    }
                                    else
                                        WebForm.ParameterizeUnknown(DataDB, param, cField, false, ref i, ref _colvals, ref _temp);
                                }
                            }
                            else if (WebForm.DataPusherConfig == null && !entry.Key.Equals(WebForm.TableName))
                            {
                                List<TableSchema> _tables = WebForm.FormSchema.Tables.FindAll(e => e.IsDynamic && e.TableType == WebFormTableTypes.Grid);
                                foreach (TableSchema _table in _tables)
                                {
                                    t += $@"UPDATE {_table.TableName} SET eb_del = 'T', eb_lastmodified_by = @eb_modified_by, eb_lastmodified_at = {DataDB.EB_CURRENT_TIMESTAMP} WHERE
                                        {entry.Key}_id = @{entry.Key}_id_{i} AND {WebForm.TableName}_id = @{WebForm.TableName}_id AND COALESCE(eb_del, 'F') = 'F'; ";
                                    param.Add(DataDB.GetNewParameter(entry.Key + "_id_" + i, EbDbTypes.Int32, row.RowId));
                                    i++;
                                }
                            }

                            string _qry = QueryGetter.GetUpdateQuery(WebForm, DataDB, entry.Key, row.IsDelete);
                            fullqry += string.Format(_qry, _colvals, row.RowId);
                            fullqry += t;
                        }
                        else
                        {
                            string _cols = string.Empty;
                            string _vals = string.Empty;

                            foreach (SingleColumn cField in row.Columns)
                            {
                                if (cField.Control != null)
                                    cField.Control.ParameterizeControl(DataDB, param, WebForm.TableName, cField, true, ref i, ref _cols, ref _vals, ref _extqry, WebForm.UserObj, null);
                                else
                                    WebForm.ParameterizeUnknown(DataDB, param, cField, true, ref i, ref _cols, ref _vals);
                            }
                            string _qry = QueryGetter.GetInsertQuery(WebForm, DataDB, entry.Key, WebForm.TableRowId == 0);
                            fullqry += string.Format(_qry, _cols, _vals);
                        }

                        fullqry += WebForm.InsertUpdateLines(entry.Key, row, DataDB, param, ref i);

                    }
                }
                param.Add(DataDB.GetNewParameter(WebForm.TableName + FormConstants._id, EbDbTypes.Int32, WebForm.TableRowId));
                param.Add(DataDB.GetNewParameter(WebForm.TableName + FormConstants._eb_ver_id, EbDbTypes.Int32, WebForm.RefId.Split(CharConstants.DASH)[4]));
            }
        }

        public void ExecUniqueCheck(IDatabase DataDB)
        {
            string fullQuery = string.Empty;
            List<DbParameter> Dbparams = new List<DbParameter>();
            List<EbControl> UniqueCtrls = new List<EbControl>();
            int paramCounter = 0, mstrFormCtrls = 0;
            EbSystemColumns SysCols = this.MasterForm.SolutionObj.SolutionSettings.SystemColumns;

            foreach (EbWebForm WebForm in this)
            {
                if (WebForm.DataPusherConfig?.AllowPush == false)
                    continue;

                foreach (TableSchema _table in WebForm.FormSchema.Tables.FindAll(e => e.TableType == WebFormTableTypes.Normal))
                {
                    if (!(WebForm.FormData.MultipleTables.TryGetValue(_table.TableName, out SingleTable Table) && Table.Count > 0))
                        continue;

                    foreach (ColumnSchema _column in _table.Columns.FindAll(e => e.Control.Unique))
                    {
                        SingleColumn cField = Table[0].GetColumn(_column.ColumnName);

                        if (cField == null || cField.Value == null || (Double.TryParse(Convert.ToString(cField.Value), out double __val) && __val == 0))
                            continue;

                        if (WebForm.FormDataBackup != null)
                        {
                            if (WebForm.FormDataBackup.MultipleTables.TryGetValue(_table.TableName, out SingleTable TableBkUp) && TableBkUp.Count > 0)
                            {
                                SingleColumn ocField = TableBkUp[0].GetColumn(_column.ColumnName);
                                if (ocField != null && Convert.ToString(cField.Value).Trim().ToLower() == Convert.ToString(ocField.Value ?? string.Empty).Trim().ToLower())
                                    continue;
                            }
                        }

                        fullQuery += string.Format("SELECT id FROM {0} WHERE {5}{1}{6} = {5}@{1}_{2}{6} AND COALESCE({3}, {4}) = {4};",
                            _table.TableName,
                            _column.ColumnName,
                            paramCounter,
                            SysCols[SystemColumns.eb_del],
                            SysCols.GetBoolFalse(SystemColumns.eb_del),
                            _column.Control.EbDbType == EbDbTypes.String ? "LOWER(TRIM(" : string.Empty,
                            _column.Control.EbDbType == EbDbTypes.String ? "))" : string.Empty);
                        Dbparams.Add(DataDB.GetNewParameter($"{_column.ColumnName}_{paramCounter++}", _column.Control.EbDbType, cField.Value));
                        UniqueCtrls.Add(_column.Control);
                        if (WebForm == MasterForm)
                            mstrFormCtrls++;
                    }
                }
            }

            if (fullQuery != string.Empty)
            {
                EbDataSet ds = DataDB.DoQueries(fullQuery, Dbparams.ToArray());
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    if (ds.Tables[i].Rows.Count > 0)
                    {
                        if (mstrFormCtrls > i)
                            throw new FormException($"{UniqueCtrls[i].Label} must be unique", (int)HttpStatusCode.BadRequest, $"Value of {UniqueCtrls[i].Label} is not unique. Control name: {UniqueCtrls[i].Name}", "EbWebFormCollection -> ExecUniqueCheck");
                        else
                            throw new FormException($"{UniqueCtrls[i].Label} in data pusher must be unique", (int)HttpStatusCode.BadRequest, $"Value of {UniqueCtrls[i].Label} in data pusher is not unique. Control name: {UniqueCtrls[i].Name}", "EbWebFormCollection -> ExecUniqueCheck");
                    }
                }
            }
        }
    }
}
