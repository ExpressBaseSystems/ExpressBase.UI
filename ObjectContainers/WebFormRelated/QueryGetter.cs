﻿using ExpressBase.Common;
using ExpressBase.Common.Constants;
using ExpressBase.Common.LocationNSolution;
using ExpressBase.Common.Objects;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Objects.WebFormRelated
{
    static class QueryGetter
    {
        public static string GetSelectQuery(EbWebForm _this, IDatabase DataDB, Service _service, out string _queryPs, out int _qryCount)
        {
            string query = string.Empty;
            string extquery = string.Empty;
            _queryPs = string.Empty;
            _qryCount = 0;
            EbSystemColumns ebs = _this.SolutionObj.SolutionSettings.SystemColumns;
            foreach (TableSchema _table in _this.FormSchema.Tables)
            {
                if (_table.IsDynamic)
                    continue;
                string _cols = $"{ebs[SystemColumns.eb_loc_id]}, id";
                string _id = "id";

                if (_table.TableName == _this.FormSchema.MasterTable)
                    _cols = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, id",
                        ebs[SystemColumns.eb_loc_id],
                        ebs[SystemColumns.eb_ver_id],
                        ebs[SystemColumns.eb_lock],
                        ebs[SystemColumns.eb_push_id],
                        ebs[SystemColumns.eb_src_id],
                        ebs[SystemColumns.eb_created_by],
                        ebs[SystemColumns.eb_void],
                        ebs[SystemColumns.eb_created_at]);
                else if (_table.TableType == WebFormTableTypes.Review)
                    _id = $"eb_ver_id = @{_this.FormSchema.MasterTable}_eb_ver_id AND eb_src_id";
                else
                    _id = _this.FormSchema.MasterTable + "_id";

                if (_table.TableType == WebFormTableTypes.Grid)
                    _cols = $"{ebs[SystemColumns.eb_loc_id]}, id, {ebs[SystemColumns.eb_row_num]}";

                IEnumerable<ColumnSchema> _columns = _table.Columns.Where(x => (!x.Control.DoNotPersist || x.Control.IsSysControl));
                if (_columns.Count() > 0)
                {
                    _cols += ", " + String.Join(", ", _columns.Select(x => x.ColumnName));
                    IEnumerable<ColumnSchema> _ph_cols = _columns.Where(x => x.Control is EbPhone && (x.Control as EbPhone).Sendotp);
                    if (_ph_cols.Count() > 0)
                        _cols += ", " + String.Join(", ", _ph_cols.Select(x => x.ColumnName + FormConstants._verified));
                }

                if (_this.DataPusherConfig == null)// master form
                {
                    query += string.Format("SELECT {0} FROM {1} WHERE {2} = @{3}_id AND COALESCE({5}, {6}) = {6} {4};",
                        _cols,
                        _table.TableName,
                        _id,
                        _this.FormSchema.MasterTable,
                        _table.TableType == WebFormTableTypes.Grid ? ("ORDER BY " + ebs[SystemColumns.eb_row_num] + (_table.DescOdr ? " DESC" : string.Empty)) : "ORDER BY id",
                        _table.TableType == WebFormTableTypes.Review ? SystemColumns.eb_del : ebs[SystemColumns.eb_del],
                        _table.TableType == WebFormTableTypes.Review ? "'F'" : ebs.GetBoolFalse(SystemColumns.eb_del));

                    foreach (ColumnSchema Col in _table.Columns.FindAll(e => !e.Control.DoNotPersist && e.Control is IEbPowerSelect && !(e.Control as IEbPowerSelect).IsDataFromApi))
                        _queryPs += (Col.Control as IEbPowerSelect).GetSelectQuery(DataDB, _service, Col.ColumnName, _table.TableName, _id, _this.FormSchema.MasterTable);
                }
                else
                {
                    if (_table.TableName == _this.FormSchema.MasterTable)
                        query += string.Format("SELECT {0} FROM {1} WHERE {2}_id = @{2}_id AND {4} = '{3}' AND COALESCE({5}, {6}) = {6};",
                            _cols,
                            _table.TableName,
                            _this.DataPusherConfig.SourceTable,
                            _this.DataPusherConfig.MultiPushId,
                            ebs[SystemColumns.eb_push_id],
                            ebs[SystemColumns.eb_del],
                            ebs.GetBoolFalse(SystemColumns.eb_del));
                    else
                        query += string.Format("SELECT {0} FROM {1} WHERE {2}_id = (SELECT id FROM {2} WHERE {3}_id = @{3}_id AND {6} = '{4}' AND COALESCE({7}, {8}) = {8} LIMIT 1) AND COALESCE({9}, {10}) = {10} {5};",
                            _cols,//0
                            _table.TableName,//1
                            _this.FormSchema.MasterTable,//2
                            _this.DataPusherConfig.SourceTable,//3
                            _this.DataPusherConfig.MultiPushId,//4
                            _table.TableType == WebFormTableTypes.Grid ? ("ORDER BY " + ebs[SystemColumns.eb_row_num] + (_table.DescOdr ? " DESC" : string.Empty)) : "ORDER BY id",//5
                            ebs[SystemColumns.eb_push_id],//6
                            ebs[SystemColumns.eb_del],//7
                            ebs.GetBoolFalse(SystemColumns.eb_del),//8
                            _table.TableType == WebFormTableTypes.Review ? SystemColumns.eb_del : ebs[SystemColumns.eb_del],//9
                            _table.TableType == WebFormTableTypes.Review ? "'F'" : ebs.GetBoolFalse(SystemColumns.eb_del));//10
                }
                _qryCount++;
            }
            bool MuCtrlFound = false;
            foreach (EbControl Ctrl in _this.FormSchema.ExtendedControls)
            {
                if (Ctrl is EbProvisionUser)
                {
                    if (!MuCtrlFound)
                    {
                        extquery += (Ctrl as EbProvisionUser).GetSelectQuery(_this.FormSchema.MasterTable);
                        MuCtrlFound = true;
                        _qryCount++;
                    }
                    extquery += (Ctrl as EbProvisionUser).GetMappedUserQuery(_this.FormSchema.MasterTable, ebs[SystemColumns.eb_del], ebs.GetBoolFalse(SystemColumns.eb_del));
                    _qryCount++;
                }
                else if (Ctrl is EbProvisionLocation)
                {
                    extquery += (Ctrl as EbProvisionLocation).GetSelectQuery(_this.FormSchema.MasterTable);
                    _qryCount++;
                }
                else if (Ctrl is EbReview)
                {
                    extquery += (Ctrl as EbReview).GetSelectQuery(_this.RefId, _this.FormSchema.MasterTable);
                    _qryCount++;
                }
                else if (Ctrl is EbDisplayPicture)
                {
                    extquery += (Ctrl as EbDisplayPicture).GetSelectQuery(DataDB, _this.FormSchema.MasterTable);
                    _qryCount++;
                }
                else if (Ctrl is EbSimpleFileUploader)
                {
                    extquery += (Ctrl as EbSimpleFileUploader).GetSelectQuery(DataDB, _this.FormSchema.MasterTable);
                    _qryCount++;
                }
                else if (Ctrl is EbMeetingPicker)
                {
                    extquery += (Ctrl as EbMeetingPicker).GetSelectQuery(DataDB, _this.FormSchema.MasterTable);
                    //_qryCount++;
                }
            }
            return query + extquery;
        }

        public static string GetDynamicGridSelectQuery(EbWebForm _this, IDatabase DataDB, Service _service, string _prntTbl, string[] _targetTbls, out string _queryPs, out int _qryCount)
        {
            string query = string.Empty;
            _queryPs = string.Empty;
            _qryCount = 0;
            EbSystemColumns ebs = _this.SolutionObj.SolutionSettings.SystemColumns;

            for (int i = 0; i < _targetTbls.Length; i++)
            {
                TableSchema _table = _this.FormSchema.Tables.Find(e => e.TableName == _targetTbls[i] && e.IsDynamic && e.TableType == WebFormTableTypes.Grid);
                string _cols = string.Format("{0}, id, {1}",
                    ebs[SystemColumns.eb_loc_id],
                    ebs[SystemColumns.eb_row_num]);

                IEnumerable<ColumnSchema> _columns = _table.Columns.Where(x => !x.Control.DoNotPersist || x.Control.IsSysControl);
                if (_columns.Count() > 0)
                    _cols += ", " + string.Join(", ", _columns.Select(x => x.ColumnName));

                query += string.Format("SELECT {0} FROM {1} WHERE {2}_id = @{2}_id AND {3}_id = @{3}_id AND COALESCE({4}, {6}) = {6} {5}; ",
                    _cols,
                    _table.TableName,
                    _this.FormSchema.MasterTable,
                    _prntTbl,
                    ebs[SystemColumns.eb_del],
                    "ORDER BY " + ebs[SystemColumns.eb_row_num] + (_table.DescOdr ? " DESC" : string.Empty),
                    ebs.GetBoolFalse(SystemColumns.eb_del));

                _qryCount++;

                foreach (ColumnSchema Col in _table.Columns.FindAll(e => !e.Control.DoNotPersist && e.Control is EbDGPowerSelectColumn && !(e.Control as EbDGPowerSelectColumn).IsDataFromApi))
                {
                    _queryPs += (Col.Control as EbDGPowerSelectColumn).GetSelectQuery123(DataDB, _service, _table.TableName, Col.ColumnName, _prntTbl, _this.FormSchema.MasterTable);
                }
            }
            return query;
        }

        public static string GetDeleteQuery(EbWebForm _this, IDatabase DataDB)
        {
            string FullQry = string.Empty;
            EbSystemColumns ebs = _this.SolutionObj.SolutionSettings.SystemColumns;

            foreach (EbWebForm ebWebForm in _this.FormCollection)
            {
                WebFormSchema _schema = ebWebForm.FormSchema;
                foreach (TableSchema _table in _schema.Tables)
                {
                    string autoIdBckUp = string.Empty;
                    //if (ebWebForm.AutoId != null && ebWebForm.AutoId.TableName == _table.TableName)
                    //    autoIdBckUp = string.Format(", {0}_ebbkup = {0}, {0} = CONCAT({0}, '_ebbkup')", ebWebForm.AutoId.Name);

                    string Qry = string.Format("UPDATE {0} SET {1} = {6}, {2} = @eb_lastmodified_by, {3} = {4} {5} ",
                        _table.TableName,
                        ebs[SystemColumns.eb_del],
                        ebs[SystemColumns.eb_lastmodified_by],
                        ebs[SystemColumns.eb_lastmodified_at],
                        DataDB.EB_CURRENT_TIMESTAMP,
                        autoIdBckUp,
                        ebs.GetBoolTrue(SystemColumns.eb_del));

                    if (ebWebForm.DataPusherConfig == null)
                        Qry += string.Format("WHERE {0} = @{1}_id AND COALESCE({2}, {3}) = {3};",
                            _table.TableName == _schema.MasterTable ? "id" : (_schema.MasterTable + "_id"),
                            _schema.MasterTable,
                            ebs[SystemColumns.eb_del],
                            ebs.GetBoolFalse(SystemColumns.eb_del));
                    else
                    {
                        EbDataPusherConfig _conf = ebWebForm.DataPusherConfig;
                        if (_table.TableName == _schema.MasterTable)
                            Qry += string.Format("WHERE {0}_id = @{0}_id AND {1} = '{2}' AND COALESCE({3}, {4}) = {4};",
                                _conf.SourceTable,
                                ebs[SystemColumns.eb_push_id],
                                _conf.MultiPushId,
                                ebs[SystemColumns.eb_del],
                                ebs.GetBoolFalse(SystemColumns.eb_del));
                        else
                            Qry += string.Format("WHERE {0}_id = (SELECT id FROM {0} WHERE {1}_id = @{1}_id AND {2} = '{3}' AND COALESCE({4}, {5}) = {5} LIMIT 1) AND COALESCE({4}, {5}) = {5};",
                                _schema.MasterTable,
                                _conf.SourceTable,
                                ebs[SystemColumns.eb_push_id],
                                _conf.MultiPushId,
                                ebs[SystemColumns.eb_del],
                                ebs.GetBoolFalse(SystemColumns.eb_del));
                    }
                    FullQry = Qry + FullQry;
                }
            }
            return FullQry;
        }

        //public static string GetDeleteQuery(EbWebForm _this, IDatabase DataDB)
        //{
        //    string query = string.Empty;
        //    foreach (TableSchema _table in _this.FormSchema.Tables)
        //    {
        //        string _id = "id";
        //        string _dupcols = string.Empty;
        //        if (_table.TableName != _this.FormSchema.MasterTable)
        //            _id = _this.FormSchema.MasterTable + "_id";
        //        foreach (ColumnSchema _column in _table.Columns)
        //        {
        //            if (_column.Control is EbAutoId)
        //            {
        //                _dupcols += string.Format(", {0}_ebbkup = {0}, {0} = CONCAT({0}, '_ebbkup')", _column.ColumnName);
        //            }
        //        }
        //        query += string.Format("UPDATE {0} SET eb_del='T',eb_lastmodified_by = @eb_lastmodified_by, eb_lastmodified_at = " + DataDB.EB_CURRENT_TIMESTAMP + " {1} WHERE {2} = @id AND COALESCE(eb_del, 'F') = 'F';", _table.TableName, _dupcols, _id);
        //    }
        //    return query;
        //}

        public static string GetCancelQuery(EbWebForm _this, IDatabase DataDB, bool Cancel)
        {
            string FullQry = string.Empty;
            EbSystemColumns ebs = _this.SolutionObj.SolutionSettings.SystemColumns;

            foreach (EbWebForm ebWebForm in _this.FormCollection)
            {
                WebFormSchema _schema = ebWebForm.FormSchema;
                foreach (TableSchema _table in _schema.Tables)
                {
                    string Qry = string.Format("UPDATE {0} SET {1} = {5}, {2} = @eb_lastmodified_by, {3} = {4} ",
                        _table.TableName,//0
                        ebs[SystemColumns.eb_void],//1
                        ebs[SystemColumns.eb_lastmodified_by],//2
                        ebs[SystemColumns.eb_lastmodified_at],//3
                        DataDB.EB_CURRENT_TIMESTAMP,//4
                        Cancel ? ebs.GetBoolTrue(SystemColumns.eb_void) : ebs.GetBoolFalse(SystemColumns.eb_void));//5

                    if (ebWebForm.DataPusherConfig == null)
                        Qry += string.Format("WHERE {0} = @{1}_id AND COALESCE({2}, {4}) = {4} AND COALESCE({3}, {5}) = {6};",
                            _table.TableName == _schema.MasterTable ? "id" : (_schema.MasterTable + "_id"),//0
                            _schema.MasterTable,//1
                            ebs[SystemColumns.eb_del],//2
                            ebs[SystemColumns.eb_void],//3
                            ebs.GetBoolFalse(SystemColumns.eb_del),//4
                            ebs.GetBoolFalse(SystemColumns.eb_void),//5
                            Cancel ? ebs.GetBoolFalse(SystemColumns.eb_void) : ebs.GetBoolTrue(SystemColumns.eb_void));//6
                    else
                    {
                        EbDataPusherConfig _conf = ebWebForm.DataPusherConfig;
                        if (_table.TableName == _schema.MasterTable)
                            Qry += string.Format("WHERE {0}_id = @{0}_id AND {1} = '{2}' AND COALESCE({3}, {5}) = {5} AND COALESCE({4}, {6}) = {7};",
                                _conf.SourceTable,//0
                                ebs[SystemColumns.eb_push_id],//1
                                _conf.MultiPushId,//2
                                ebs[SystemColumns.eb_del],//3
                                ebs[SystemColumns.eb_void],//4
                                ebs.GetBoolFalse(SystemColumns.eb_del),//5
                                ebs.GetBoolFalse(SystemColumns.eb_void),//6
                                Cancel ? ebs.GetBoolFalse(SystemColumns.eb_void) : ebs.GetBoolTrue(SystemColumns.eb_void));//7
                        else
                            Qry += string.Format("WHERE {0}_id = (SELECT id FROM {0} WHERE {1}_id = @{1}_id AND {2} = '{3}' AND COALESCE({4}, {6}) = {6} AND COALESCE({5}, {7}) = {8} LIMIT 1) AND COALESCE({4}, {6}) = {6} AND COALESCE({5}, {7}) = {8};",
                                _schema.MasterTable,//0
                                _conf.SourceTable,//1
                                ebs[SystemColumns.eb_push_id],//2
                                _conf.MultiPushId,//3
                                ebs[SystemColumns.eb_del],//4
                                ebs[SystemColumns.eb_void],//5
                                ebs.GetBoolFalse(SystemColumns.eb_del),//6
                                ebs.GetBoolFalse(SystemColumns.eb_void),//7
                                Cancel ? ebs.GetBoolFalse(SystemColumns.eb_void) : ebs.GetBoolTrue(SystemColumns.eb_void));//8
                    }
                    FullQry = Qry + FullQry;
                }
            }
            return FullQry;
        }

        //public static string GetCancelQuery(EbWebForm _this, IDatabase DataDB)
        //{
        //    string query = string.Empty;
        //    foreach (TableSchema _table in _this.FormSchema.Tables)
        //    {
        //        string _id = "id";
        //        if (_table.TableName != _this.FormSchema.MasterTable)
        //            _id = _this.FormSchema.MasterTable + "_id";
        //        query += string.Format("UPDATE {0} SET eb_void='T',eb_lastmodified_by = @eb_lastmodified_by, eb_lastmodified_at = " + DataDB.EB_CURRENT_TIMESTAMP + " WHERE {1} = @id AND COALESCE(eb_void, 'F') = 'F' AND COALESCE(eb_del, 'F') = 'F';", _table.TableName, _id);
        //    }
        //    return query;
        //}

        public static string GetLockOrUnlockQuery(EbWebForm _this, IDatabase DataDB, bool Lock)
        {
            EbSystemColumns ebs = _this.SolutionObj.SolutionSettings.SystemColumns;

            string Qry = string.Format("UPDATE {0} SET {1} = {6}, {2} = @eb_lastmodified_by, {3} = {4} WHERE id = @{0}_id AND COALESCE({1}, {5}) = {7};",
                _this.TableName,
                ebs[SystemColumns.eb_lock],//1
                ebs[SystemColumns.eb_lastmodified_by],//2
                ebs[SystemColumns.eb_lastmodified_at],//3
                DataDB.EB_CURRENT_TIMESTAMP,//4
                ebs.GetBoolFalse(SystemColumns.eb_lock),//5
                Lock ? ebs.GetBoolTrue(SystemColumns.eb_lock) : ebs.GetBoolFalse(SystemColumns.eb_lock),//6
                Lock ? ebs.GetBoolFalse(SystemColumns.eb_lock) : ebs.GetBoolTrue(SystemColumns.eb_lock));//7

            return Qry;
        }

        public static string GetInsertQuery(EbWebForm _this, IDatabase DataDB, string tblName, bool isIns)
        {
            string _qry;
            EbSystemColumns ebs = _this.SolutionObj.SolutionSettings.SystemColumns;

            if (tblName.Equals(_this.TableName))
            {
                if (_this.DataPusherConfig == null)
                {
                    _qry = string.Format("INSERT INTO {0} ({8} {1}, {2}, {3}, {4}, {5}) VALUES ({9} @eb_createdby, {6}, @eb_loc_id, @{7}_eb_ver_id, @eb_signin_log_id); ",
                        tblName,
                        ebs[SystemColumns.eb_created_by],
                        ebs[SystemColumns.eb_created_at],
                        ebs[SystemColumns.eb_loc_id],
                        ebs[SystemColumns.eb_ver_id],
                        ebs[SystemColumns.eb_signin_log_id],
                        DataDB.EB_CURRENT_TIMESTAMP,
                        _this.TableName,
                        "{0}",
                        "{1}");
                }
                else
                {
                    string srcRef = _this.DataPusherConfig.SourceRecId <= 0 ? $"(SELECT eb_currval('{_this.DataPusherConfig.SourceTable}_id_seq'))" : $"@{_this.DataPusherConfig.SourceTable}_id";

                    _qry = string.Format(@"INSERT INTO {0} ({15} {1}, {2}, {3}, {4}, {9}_id, {5}, {6}, {7}, {8}) 
                                    VALUES ({16} @eb_createdby, {10}, @eb_loc_id, @{11}_eb_ver_id, {12}, {12}, '{13}', {14}, @eb_signin_log_id); ",
                        tblName,//0
                        ebs[SystemColumns.eb_created_by],//1
                        ebs[SystemColumns.eb_created_at],//2
                        ebs[SystemColumns.eb_loc_id],//3
                        ebs[SystemColumns.eb_ver_id],//4
                        ebs[SystemColumns.eb_src_id],//5
                        ebs[SystemColumns.eb_push_id],//6
                        ebs[SystemColumns.eb_lock],//7
                        ebs[SystemColumns.eb_signin_log_id],//8
                        _this.DataPusherConfig.SourceTable,//9
                        DataDB.EB_CURRENT_TIMESTAMP,//10
                        _this.TableName,//11
                        srcRef,//12
                        _this.DataPusherConfig.MultiPushId,//13
                        ebs.GetBoolTrue(SystemColumns.eb_lock),//14
                        "{0}",//15
                        "{1}");//16
                }

                if (DataDB.Vendor == DatabaseVendors.MYSQL)
                    _qry += $"SELECT eb_persist_currval('{tblName}_id_seq'); ";
                if (_this.IsLocEditable)
                    _qry = _qry.Replace($", {ebs[SystemColumns.eb_loc_id]}", string.Empty).Replace(", @eb_loc_id", string.Empty);
            }
            else if (tblName.Equals("eb_approval_lines"))
            {
                _qry = $@"INSERT INTO eb_approval_lines ({{0}} eb_created_by, eb_created_at, eb_loc_id, eb_src_id, eb_ver_id, eb_signin_log_id) 
                            VALUES ({{1}} @eb_createdby, {DataDB.EB_CURRENT_TIMESTAMP}, @eb_loc_id, @{_this.TableName}_id, @{_this.TableName}_eb_ver_id, @eb_signin_log_id); ";
                if (DataDB.Vendor == DatabaseVendors.MYSQL)
                    _qry += "SELECT eb_persist_currval('eb_approval_lines_id_seq'); ";
                // eb_approval - update eb_approval_lines_id
                _qry += $@"UPDATE eb_approval SET eb_approval_lines_id = (SELECT eb_currval('eb_approval_lines_id_seq')), eb_lastmodified_by = @eb_modified_by, eb_lastmodified_at = {DataDB.EB_CURRENT_TIMESTAMP} 
                               WHERE eb_src_id = @{_this.TableName}_id AND eb_ver_id =  @{_this.TableName}_eb_ver_id AND COALESCE(eb_del, 'F') = 'F'; ";
            }
            else
            {
                string srcRef = isIns ? $"(SELECT eb_currval('{_this.TableName}_id_seq'))" : $"@{_this.TableName}_id";
                _qry = $@"INSERT INTO {tblName} ({{0}} {ebs[SystemColumns.eb_created_by]}, {ebs[SystemColumns.eb_created_at]}, {ebs[SystemColumns.eb_loc_id]}, {_this.TableName}_id, {ebs[SystemColumns.eb_signin_log_id]}) 
                            VALUES ({{1}} @eb_createdby, {DataDB.EB_CURRENT_TIMESTAMP}, @eb_loc_id , {srcRef}, @eb_signin_log_id); ";
                if (isIns && DataDB.Vendor == DatabaseVendors.MYSQL)
                    _qry += $"SELECT eb_persist_currval('{tblName}_id_seq'); ";
            }

            return _qry;
        }

        public static string GetUpdateQuery(EbWebForm _this, IDatabase DataDB, string tblName, bool isDel)
        {
            string _qry;
            EbSystemColumns ebs = _this.SolutionObj.SolutionSettings.SystemColumns;
            if (_this.DataPusherConfig == null)
            {
                if (tblName.Equals(_this.TableName))
                    _qry = string.Format("UPDATE {0} SET {6} {1} = @eb_modified_by, {2} = {3} WHERE id = {7} AND COALESCE({4}, {5}) = {5}; ",
                        tblName,//0
                        ebs[SystemColumns.eb_lastmodified_by],//1
                        ebs[SystemColumns.eb_lastmodified_at],//2
                        DataDB.EB_CURRENT_TIMESTAMP,//3
                        ebs[SystemColumns.eb_del],//4
                        ebs.GetBoolFalse(SystemColumns.eb_del),//5
                        "{0}",
                        "{1}");
                else
                    _qry = string.Format("UPDATE {0} SET {7} {1} = @eb_modified_by, {2} = {3} WHERE id = {8} AND {4}_id = @{4}_id AND COALESCE({5}, {6}) = {6}; ",
                        tblName,//0
                        ebs[SystemColumns.eb_lastmodified_by],//1
                        ebs[SystemColumns.eb_lastmodified_at],//2
                        DataDB.EB_CURRENT_TIMESTAMP,//3
                        _this.TableName,//4
                        ebs[SystemColumns.eb_del],//5
                        ebs.GetBoolFalse(SystemColumns.eb_del),//6
                        isDel ? $"{ebs[SystemColumns.eb_del]} = {ebs.GetBoolTrue(SystemColumns.eb_del)}, " : "{0}",
                        "{1}");
            }
            else
            {
                // if isDel is true then consider lines table also
                string parentTbl = _this.TableName;
                string pushIdChk = string.Empty;
                if (tblName.Equals(_this.TableName))
                {
                    parentTbl = _this.DataPusherConfig.SourceTable;
                    pushIdChk = $"AND {ebs[SystemColumns.eb_push_id]} = '{_this.DataPusherConfig.MultiPushId}'";
                }
                _qry = string.Format("UPDATE {0} SET {8} {1} = @eb_modified_by, {2} = {3} WHERE id = {9} AND {4}_id = @{4}_id AND COALESCE({5}, {6}) = {6} {7}; ",
                    tblName,//0
                    ebs[SystemColumns.eb_lastmodified_by],//1
                    ebs[SystemColumns.eb_lastmodified_at],//2
                    DataDB.EB_CURRENT_TIMESTAMP,//3
                    parentTbl,//4
                    ebs[SystemColumns.eb_del],//5
                    ebs.GetBoolFalse(SystemColumns.eb_del),//6
                    pushIdChk,//7
                    isDel ? $"{ebs[SystemColumns.eb_del]} = {ebs.GetBoolTrue(SystemColumns.eb_del)}, " : "{0}",
                    "{1}");
            }
            return _qry;
        }

    }
}
