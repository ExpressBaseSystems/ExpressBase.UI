﻿using ExpressBase.Common;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.ServiceStack_Artifacts;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ExpressBase.Objects
{
    public static class EbFormHelper
    {
        public static List<string> DiscoverRelatedRefids(EbControlContainer _this)
        {
            List<string> refids = new List<string>();
            EbControl[] _flatCtrls = new List<EbControl>() { _this }.FlattenAllEbControls();
            for (int i = 0; i < _flatCtrls.Length; i++)
            {
                if (_flatCtrls[i] is EbUserControl || _flatCtrls[i] is EbDGUserControlColumn)
                {
                    if (!string.IsNullOrEmpty(_flatCtrls[i].RefId))
                        refids.Add(_flatCtrls[i].RefId);
                }
                else
                {
                    PropertyInfo[] _props = _flatCtrls[i].GetType().GetProperties();
                    foreach (PropertyInfo _prop in _props)
                    {
                        if (_prop.IsDefined(typeof(OSE_ObjectTypes)))
                        {
                            object _val = _prop.GetValue(_flatCtrls[i], null);
                            if (_val != null)
                            {
                                if (_prop.PropertyType == typeof(string))
                                {
                                    if (!_val.ToString().IsEmpty())
                                        refids.Add(_val.ToString());
                                }
                                else if(_prop.PropertyType == typeof(List<ObjectBasicInfo>))
                                {
                                    foreach(ObjectBasicInfo info in _val as List<ObjectBasicInfo>)
                                    {
                                        if (!string.IsNullOrEmpty(info.ObjRefId))
                                            refids.Add(info.ObjRefId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return refids;
        }

        public static void ReplaceRefid(EbControlContainer _this, Dictionary<string, string> RefidMap)
        {
            EbControl[] _flatCtrls = new List<EbControl>() { _this }.FlattenAllEbControls();
            for (int i = 0; i < _flatCtrls.Length; i++)
            {
                if (_flatCtrls[i] is EbUserControl || _flatCtrls[i] is EbDGUserControlColumn)
                {
                    if (!string.IsNullOrEmpty(_flatCtrls[i].RefId))
                    {
                        if (RefidMap.ContainsKey(_flatCtrls[i].RefId))
                            _flatCtrls[i].RefId = RefidMap[_flatCtrls[i].RefId];
                        else
                            _flatCtrls[i].RefId = "failed-to-update-";
                    }
                }
                else
                {
                    PropertyInfo[] _props = _flatCtrls[i].GetType().GetProperties();
                    foreach (PropertyInfo _prop in _props)
                    {
                        if (_prop.IsDefined(typeof(OSE_ObjectTypes)))
                        {
                            object _val = _prop.GetValue(_flatCtrls[i], null);
                            if (_val != null)
                            {
                                if (_prop.PropertyType == typeof(string))
                                {
                                    string st_val = _val.ToString();
                                    if (!st_val.IsEmpty())
                                    {
                                        if (RefidMap.ContainsKey(st_val))
                                            _prop.SetValue(_flatCtrls[i], RefidMap[st_val], null);
                                        else
                                            _prop.SetValue(_flatCtrls[i], "failed-to-update-");
                                    }
                                }
                                else if (_prop.PropertyType == typeof(List<ObjectBasicInfo>))
                                {
                                    foreach (ObjectBasicInfo info in _val as List<ObjectBasicInfo>)
                                    {
                                        if (!string.IsNullOrEmpty(info.ObjRefId))
                                        {
                                            if (RefidMap.ContainsKey(info.ObjRefId))
                                                info.ObjRefId = info.ObjRefId;
                                            else
                                                info.ObjRefId = "failed-to-update-";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Rendering side -> service = null
        public static void AfterRedisGet(EbControlContainer _this, IRedisClient Redis, IServiceClient client, Service service)
        {
            try
            {
                for (int i = 0; i < _this.Controls.Count; i++)
                {
                    EbControl c = _this.Controls[i];
                    c.IsRenderMode = _this.IsRenderMode;
                    c.IsDynamicTabChild = _this.IsDynamicTabChild;
                    if (c is EbUserControl || c is EbDGUserControlColumn)
                    {
                        EbUserControl _temp = Redis.Get<EbUserControl>(c.RefId);
                        if (_temp == null)
                        {
                            EbObjectParticularVersionResponse result = null;
                            if (client != null)
                                result = client.Get<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = c.RefId });
                            else
                                result = service.Gateway.Send<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = c.RefId });
                            _temp = EbSerializers.Json_Deserialize(result.Data[0].Json);
                            Redis.Set<EbUserControl>(c.RefId, _temp);
                        }
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
                            (c as EbUserControl).DisplayName = _temp.DisplayName;
                            (c as EbUserControl).VersionNumber = _temp.VersionNumber;
                            foreach (EbControl Control in (c as EbUserControl).Controls)
                            {
                                RenameControlsRec(Control, c.Name);
                            }
                            c.AfterRedisGet(Redis as RedisClient, client);
                        }
                    }
                    else if (c is EbControlContainer)
                    {
                        if (c is EbTabPane && (c as EbTabPane).IsDynamic)
                            c.IsDynamicTabChild = true;
                        AfterRedisGet(c as EbControlContainer, Redis, client, null);
                    }
                    else if (c is EbProvisionLocation)//add unmapped ctrls as DoNotPersist controls
                    {
                        if (_this.IsRenderMode)
                        {
                            EbProvisionLocation prvnCtrl = c as EbProvisionLocation;
                            for (int j = 0; j < prvnCtrl.Fields.Count; j++)
                            {
                                UsrLocField prvnFld = prvnCtrl.Fields[j] as UsrLocField;
                                if (prvnFld.ControlName.IsNullOrEmpty() && prvnFld.IsRequired)
                                {
                                    if (prvnFld.Type == "image")
                                    {
                                        _this.Controls.Insert(i, new EbDisplayPicture()
                                        {
                                            Name = "namecustom" + i,
                                            EbSid = "ebsidcustom" + i,
                                            EbSid_CtxId = "ebsidcustom" + i,
                                            Label = prvnFld.DisplayName,
                                            DoNotPersist = true,
                                            MaxHeight = 100
                                        });
                                    }
                                    else
                                    {
                                        _this.Controls.Insert(i, new EbTextBox()
                                        {
                                            Name = "namecustom" + i,
                                            EbSid = "ebsidcustom" + i,
                                            EbSid_CtxId = "ebsidcustom" + i,
                                            Label = prvnFld.DisplayName,
                                            DoNotPersist = true
                                        });
                                    }
                                    prvnFld.ControlName = "namecustom" + i;
                                    i++;
                                }
                            }
                        }
                    }
                    else if (c is EbProvisionUser)
                    {
                        if (_this.IsRenderMode)
                        {
                            EbProvisionUser prvnCtrl = c as EbProvisionUser;
                            for (int j = 0; j < prvnCtrl.Fields.Count; j++)
                            {
                                UsrLocField prvnFld = prvnCtrl.Fields[j] as UsrLocField;
                                if (prvnFld.ControlName.IsNullOrEmpty() && prvnFld.IsRequired)
                                {
                                    _this.Controls.Insert(i, new EbTextBox()
                                    {
                                        Name = "namecustom" + i,
                                        EbSid = "ebsidcustom" + i,
                                        EbSid_CtxId = "ebsidcustom" + i,
                                        Label = prvnFld.DisplayName,
                                        DoNotPersist = true
                                    });
                                    prvnFld.ControlName = "namecustom" + i;
                                    i++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION : FormAfterRedisGet " + e.Message);
            }
        }

        ////Builder side - whole object get, table create, data insert
        //public static void AfterRedisGet(EbControlContainer _this, Service service)
        //{
        //    try
        //    {
        //        for (int i = 0; i < _this.Controls.Count; i++)
        //        {
        //            if (_this.Controls[i] is EbUserControl || _this.Controls[i] is EbDGUserControlColumn)
        //            {
        //                EbUserControl _temp = service.Redis.Get<EbUserControl>(_this.Controls[i].RefId);
        //                if (_temp == null)
        //                {
        //                    var result = service.Gateway.Send<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = _this.Controls[i].RefId });
        //                    _temp = EbSerializers.Json_Deserialize(result.Data[0].Json);
        //                    service.Redis.Set<EbUserControl>(_this.Controls[i].RefId, _temp);
        //                }
        //                //_temp.RefId = _this.Controls[i].RefId;
        //                if (_this.Controls[i] is EbDGUserControlColumn)
        //                {
        //                    foreach (EbControl Control in _temp.Controls)
        //                    {
        //                        RenameControlsRec(Control, _this.Controls[i].Name);
        //                    }
        //                    (_this.Controls[i] as EbDGUserControlColumn).InitUserControl(_temp);
        //                }
        //                else
        //                {
        //                    (_this.Controls[i] as EbUserControl).Controls = _temp.Controls;
        //                    //foreach (EbControl Control in (_this.Controls[i] as EbUserControl).Controls)
        //                    //{
        //                    //    RenameControlsRec(Control, _this.Controls[i].Name);
        //                    //    //Control.ChildOf = "EbUserControl";
        //                    //    //Control.Name = _this.Controls[i].Name + "_" + Control.Name;
        //                    //}
        //                    //_this.Controls[i] = _temp;
        //                    (_this.Controls[i] as EbUserControl).AfterRedisGet(service);
        //                }
        //            }
        //            else if (_this.Controls[i] is EbControlContainer)
        //            {
        //                AfterRedisGet(_this.Controls[i] as EbControlContainer, service);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("EXCEPTION : EbFormAfterRedisGet(service) " + e.Message);
        //    }
        //}

        public static void RenameControlsRec(EbControl _control, string _ucName)
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

        //public static void InitDataPushers(EbWebForm _this, Service service)
        //{
        //    if (_this.DataPushers != null)
        //    {
        //        foreach (EbDataPusher pusher in _this.DataPushers)
        //        {
        //            EbWebForm _form = service.Redis.Get<EbWebForm>(pusher.FormRefId);
        //            if (_form == null)
        //            {
        //                EbObjectParticularVersionResponse result = service.Gateway.Send<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = pusher.FormRefId });
        //                _form = EbSerializers.Json_Deserialize(result.Data[0].Json);
        //                service.Redis.Set<EbWebForm>(pusher.FormRefId, _form);
        //            }
        //            _form.AfterRedisGet(service);
        //            _form.DataPusherConfig = new EbDataPusherConfig { SourceTable = _this.FormSchema.MasterTable, MultiPushId = _this.RefId + "_" + pusher.Name };
        //            pusher.WebForm = _form;
        //            _this.ExeDataPusher = true;
        //        }
        //    }
        //}

        public static void InitDataPushers(EbWebForm _this, IRedisClient Redis, IServiceClient client, Service service)
        {
            if (_this.DataPushers != null)
            {
                foreach (EbDataPusher pusher in _this.DataPushers)
                {
                    EbWebForm _form = Redis.Get<EbWebForm>(pusher.FormRefId);
                    if (_form == null)
                    {
                        EbObjectParticularVersionResponse result;
                        if (client != null)
                            result = client.Get<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = pusher.FormRefId });
                        else
                            result = service.Gateway.Send<EbObjectParticularVersionResponse>(new EbObjectParticularVersionRequest { RefId = pusher.FormRefId });
                        _form = EbSerializers.Json_Deserialize(result.Data[0].Json);
                        Redis.Set<EbWebForm>(pusher.FormRefId, _form);
                    }
                    _form.AfterRedisGet(Redis as RedisClient, client);
                    _form.DataPusherConfig = new EbDataPusherConfig { SourceTable = _this.FormSchema.MasterTable, MultiPushId = _this.RefId + "_" + pusher.Name };
                    pusher.WebForm = _form;
                    _this.ExeDataPusher = true;
                }
            }
        }
    }

    public class EbColumnExtra
    {
        public static Dictionary<string, EbDbTypes> Params
        {
            get
            {
                return new Dictionary<string, EbDbTypes> {
                    { "eb_row_num",EbDbTypes.Decimal},
                    { "eb_created_at_device",EbDbTypes.DateTime},
                    { "eb_device_id",EbDbTypes.String},
                    { "eb_appversion",EbDbTypes.String}
                };
            }
        }
    }
}
