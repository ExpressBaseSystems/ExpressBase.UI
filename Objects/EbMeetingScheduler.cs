﻿using ExpressBase.Common;
using ExpressBase.Common.EbServiceStack.ReqNRes;
using ExpressBase.Common.Extensions;
using ExpressBase.Common.LocationNSolution;
using ExpressBase.Common.Objects;
using ExpressBase.Common.Objects.Attributes;
using ExpressBase.Common.Structures;
using ExpressBase.Objects.ServiceStack_Artifacts;
using ExpressBase.Security;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;

namespace ExpressBase.Objects
{

    [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
    public class EbMeetingScheduler : EbControlUI
    {
        public EbMeetingScheduler() { }
        public override string ToolIconHtml { get { return "<i class='fa fa-i-cursor'></i>"; } set { } }

        public string TableName { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [HideInPropertyGrid]
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [DefaultPropValue("1")]
        public int MeetingId { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [HideInPropertyGrid]
        public Dictionary<int, string> UsersList { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [HideInPropertyGrid]
        public string ParticipantsList { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [HideInPropertyGrid]
        public MeetingOptions MeetingOpts { get; set; }

        [EnableInBuilder(BuilderType.WebForm, BuilderType.BotForm)]
        [DefaultPropValue("1")]
        public MeetingType MeetingType { get; set; }


        public override string UIchangeFns
        {
            get
            {
                return @"EbMeetingPicker = {
                }";
            }
        }

        [OnDeserialized]
        public void OnDeserializedMethod(StreamingContext context)
        {
            this.BareControlHtml = this.GetBareHtml();
            this.BareControlHtml4Bot = this.GetWrapedCtrlHtml4bot();
            this.ObjType = this.GetType().Name.Substring(2, this.GetType().Name.Length - 2);
        }

        public override string DesignHtml4Bot
        {
            get => @"<div id='@ebsid@' ebsid='@ebsid@' name='@name@' class='Eb-ctrlContainer meeting-scheduler-outer' @childOf@ ctype='@type@'>
                        
                </div>"
                .Replace("@name@", this.Name)
                .Replace("@Label@", this.Label)
                .Replace("@ebsid@", this.EbSid_CtxId);
            set => base.DesignHtml4Bot = value;
        }

        public void InitParticipantsList(JsonServiceClient ServiceClient)
        {
            ParticipantsListResponse abc = new ParticipantsListResponse();
            abc = ServiceClient.Post<ParticipantsListResponse>(new ParticipantsListRequest { });
            this.ParticipantsList = JsonConvert.SerializeObject(abc.ParticipantsList);
        }

        public override string GetHtml4Bot()
        {
            return GetWrapedCtrlHtml4bot();
        }
        public override string GetWrapedCtrlHtml4bot()
        {
            string jk = @"<div id='@ebsid@' ebsid='@ebsid@' name='@name@' class='Eb-ctrlContainer meeting-picker-outer eb-meeting-bot' @childOf@ ctype='@type@'>
                       @GetWrapedCtrlHtml4bot@
                </div>".Replace("@date_val@", DateTime.Today.ToString("yyyy-MM-dd"));
            return ReplacePropsInHTML(jk);
        }

        public override string GetBareHtml()
        {
            string EbCtrlHTML = @"<div id='cont_@ebsid@' ebsid='@ebsid@' name='@name@' class='Eb-ctrlContainer meeting-picker-outer' @childOf@ ctype='@type@'>
                       GetBareHtml
                </div>"
                .Replace("@name@", this.Name)
                .Replace("@Label@", this.Label)
                .Replace("@date_val@", DateTime.Today.ToString("yyyy-MM-dd"))
                .Replace("@ebsid@", this.EbSid_CtxId);
            return EbCtrlHTML;
        }


        public override string GetHtml()
        {
            string EbCtrlHTML = @"<div id='cont_@ebsid@' ebsid='@ebsid@' name='@name@' class='Eb-ctrlContainer meeting-scheduler-outer' @childOf@ ctype='@type@'>
                 @meetinghtml@
            <input type='text' id='@ebsid@_MeetingJson' readonly hidden/>
                </div>
                    ";
            if (this.MeetingType == MeetingType.SingleMeeting)
                EbCtrlHTML = EbCtrlHTML.Replace("@meetinghtml@", this.GetHtml4singleMeeting());
            else if (this.MeetingType == MeetingType.MultipleMeeting)
                EbCtrlHTML = EbCtrlHTML.Replace("@meetinghtml@", this.GetHtml4MultipleMeeting());
            else if (this.MeetingType == MeetingType.AdvancedMeeting)
                EbCtrlHTML = EbCtrlHTML.Replace("@meetinghtml@", this.GetHtml4MultipleMeeting());
            EbCtrlHTML.Replace("@name@", this.Name);
            EbCtrlHTML.Replace("@date_val@", DateTime.Today.ToString("yyyy-MM-dd"));
            EbCtrlHTML.Replace("@Label@", this.Label);
            return ReplacePropsInHTML(EbCtrlHTML);
        }
        public string abc()
        {
            return string.Empty;
        }

        public string GetHtml4singleMeeting()
        {
            string Html = @"Title : <input type='text' placeholder='title' id='@ebsid@_meeting-title' class='mc-input' required/>
            Description : <textarea id='@ebsid@_description' class='mc-input' required> </textarea>
            Location : <input type='text' placeholder='location' id='@ebsid@_meeting-title' class='mc-input' required/>
            Integration : <input type='text' placeholder='integration' id='@ebsid@_meeting-title' class='mc-input' required/>
            Date :<input type='text' id='@ebsid@_meeting-date' val='@date_val@' class='mc-input' required/>
            <div class='slots-table' id='@ebsid@_slots'>
            <table> <thead><tr><th>Sl.No</th><th>Start Time</th><th>End Time</th><th>Host</th><th>Attendee</th></tr>
            <tbody>
            <tr><th>1</th>
            <th><input type='time' id='@ebsid@_time-from' data-id='0' class='mc-input time-from' required/></th>
            <th><input type='time' id='@ebsid@_time-to' data-id='0'  class='mc-input time-to' required/></th>
            <th><input type='text' id='@ebsid@_host_0' data-id='0' class='meeting-participants tb-host'/></th>
            <th><input type='text' id='@ebsid@_attendee_0' data-id='0' class='meeting-participants tb-attendee'/></th></tr>
            </tbody>
            </table>
            </div>
        ";
            return Html;
        }

        public string GetHtml4MultipleMeeting()
        {
            string Html = @"
            Title : <input type='text' placeholder='title' id='@ebsid@_meeting-title' class='mc-input' required/>
            Description : <textarea id='@ebsid@_description' class='mc-input' required> </textarea>
            Location : <input type='text' placeholder='location' id='@ebsid@_meeting-title' class='mc-input' required/>
            Integration : <input type='text' placeholder='integration' id='@ebsid@_meeting-title' class='mc-input' required/>
            Date :<input type='text' id='@ebsid@_meeting-date' val='@date_val@' class='mc-input' required/>
            <div class='slots-table' id='@ebsid@_slots'>
            <table> <thead><tr><th>Sl.No</th><th>Start Time</th><th>End Time</th><th>Host</th><th>Attendee</th></tr>
            <tbody>
            <tr><th>1</th>
            <th><input type='time' id='@ebsid@_time-from' class='mc-input' required/></th>
            <th><input type='time' id='@ebsid@_time-to' class='mc-input' required/></th>
            <th><input type='text' id='@ebsid@_host_0' class='meeting-participants tb-host'/></th>
            <th><input type='text' id='@ebsid@_attendee_0' class='meeting-participants tb-attendee'/></th></tr>
            </tbody>
            </table>
            </div>

        ";
            return Html;
        }
        public string AdvancedMeeting()
        {
            string Html = @"<div class='meeting-info'>
                Title : <input type='text' placeholder='Title' id='@ebsid@_meeting-title' class='mc-input' required/>
                Description : <textarea id='@ebsid@_description' class='mc-input' required> </textarea> </div>
                <div class='time' style='display: flex;'>
                <div class='meeting-date'>
                <div style='margin-right:15px;'><h5>Date</h5> <input type='text' id='@ebsid@_meeting-date' val='@date_val@' class='mc-input' required/></div>
                <div class='meeting-duration'>
                <h5>Duration</h5> <input type='text' id='@ebsid@_duration' data-format='HH:mm' data-template='HH : mm' name='datetime' class='mc-input'>
                </div></div></div>
                <div class='meeting-count'>
                Max-host : <input type='number' id='@ebsid@_max-host' class='meeting-spinner mc-input' min='1' max='5' value='1' >
                Min-host : <input type='number' id='@ebsid@_min-host' class='meeting-spinner mc-input' min='1' max='5' value='1'>
                Max-Attendee<input type='number' id='@ebsid@_max-attendee' class='meeting-spinner mc-input' min='1' max='5' value='1'>
                Min-Attendee<input type='number' id='@ebsid@_min-attendee' class='meeting-spinner mc-input' min='1' max='5' value='1'>
                </div>
                <div class='eligible-participant'><div style='width:100%'>
                <h5>Eligible Host</h5> <input type='text' id='@ebsid@_eligible_host_list' class='mc-input eligible-userids'/> </div>
                <div  style='width:100%'><h5>Eligible Attendee</h5><input type='text' id='@ebsid@_eligible_attendee_list' class='mc-input eligible-userids'/></div>
                </div>
                <div class='slots-table' id='@ebsid@_slots'>
                <table> <thead><tr><th>Sl.No</th><th>Start Time</th><th>End Time</th><th>Host</th><th>Attendee</th></tr>
                <tbody></tbody>
                </table>
                </div>
                <div class='meeting-recurance'>
                <div class='is-recuring'><input type='checkbox' id='@ebsid@_recuring_meeting' name='recuring' value='recuring'>
                <label for='@ebsid@_recuring_meeting'> Is recuring</label></div>    
                <div class='recuring-days' id='@ebsid@_recuring-days'>
                  <label class='checkbox-inline' ><input type='checkbox' data-label='Sun' data-code='0' checked>Sunday</label>
                  <label class='checkbox-inline' ><input type='checkbox' data-label='Mon' data-code='1' checked>Monday</label>
                  <label class='checkbox-inline' ><input type='checkbox' data-label='Tue' data-code='2' checked>Tuesday</label>
                  <label class='checkbox-inline' ><input type='checkbox' data-label='Wed' data-code='3' checked>Wednesday</label>
                  <label class='checkbox-inline' ><input type='checkbox' data-label='Thu' data-code='4' checked>Thursday</label>
                  <label class='checkbox-inline' ><input type='checkbox' data-label='Fri' data-code='5' checked>Friday</label>
                  <label class='checkbox-inline' ><input type='checkbox' data-label='Sat' data-code='6' checked>Saturday</label>
                </div>
                <div>
                Cron Exp : <input type='text' id='@ebsid@_crone-exp'>
                Cron Exp End Date :<input type='text' id='@ebsid@_crone-exp-end'></div>
                </div>";
            return Html;
        }
        public override string GetDesignHtml()
        {
            return GetDesignHtmlHelper().RemoveCR().DoubleQuoted();
        }

        public string GetDesignHtmlHelper()
        {
            string EbCtrlHTML = @"<div id='@ebsid@' ebsid='@ebsid@' name='@name@' class='Eb-ctrlContainer meeting-picker-outer' @childOf@ ctype='@type@'>
                        GetDesignHtmlHelper
                </div>"
               .Replace("@name@", this.Name)
               .Replace("@date_val@", DateTime.Today.ToString("yyyy-MM-dd"))
               .Replace("@Label@", this.Label);
            return ReplacePropsInHTML(EbCtrlHTML);
        }

        public override string GetValueFromDOMJSfn { get { return @"return $('#' + this.EbSid_CtxId +'_MeetingJson').val();"; } set { } }

        public override string OnChangeBindJSFn { get { return @" $('#' +  this.EbSid_CtxId +'_MeetingJson').on('change', p1);"; } set { } }

        public override string SetValueJSfn { get { return @"  $('#' + this.EbSid_CtxId +'_MeetingJson').val(p1).trigger('change');"; } set { } }

        public override string SetDisplayMemberJSfn { get { return @"$('#' + this.EbSid_CtxId +'_MeetingJson').val(p1)"; } set { } }

        public override SingleColumn GetSingleColumn(User UserObj, Eb_Solution SoluObj, object Value)
        {
            return new SingleColumn()
            {
                Name = this.Name,
                Type = (int)this.EbDbType,
                Value = Value,
                Control = this,
                ObjType = this.ObjType,
                F = "[]"
            };
        }

        public string GetSelectQuery(IDatabase DataDB, string MasterTable)
        {
            return string.Empty;
        }
        public int UserIdCount(List<Participants> Participants, UsersType Type)
        {
            var count = Participants.Where(item => item.Type == Type).Count();
            return count;
        }
        public override bool ParameterizeControl(IDatabase DataDB, List<DbParameter> param, string tbl, SingleColumn cField, bool ins, ref int i, ref string _col, ref string _val, ref string _extqry, User usr, SingleColumn ocF)
        {
            if (!ins)
                return false;
            MeetingSchedule Mobj = new MeetingSchedule();
            Mobj = JsonConvert.DeserializeObject<MeetingSchedule>(cField.Value.ToString());
            string[] Host = Mobj.Host.Split(',').Select(sValue => sValue.Trim()).ToArray();

            string query = "";
            if (Mobj.MeetingType == MeetingType.SingleMeeting)
            {
                query += AddMeetingSchedule(Mobj, usr);
                for (i = 0; i < Mobj.SlotList.Count; i++)
                {
                    query += $@"
                insert into eb_meeting_slots (eb_meeting_schedule_id,meeting_date,time_from,time_to,eb_created_by,eb_created_at) values 
                (eb_currval('eb_meeting_schedule_id_seq'),'{Mobj.Date}','{Mobj.SlotList[i].TimeFrom}','{Mobj.SlotList[i].TimeTo}', {usr.UserId},now() );";

                    int HostUserIdsCount = Mobj.SlotList[i].Hosts.Where(Item => Item.Type == UsersType.UserIds).Count();
                    int AttendeeUserIdsCount = Mobj.SlotList[i].Attendees.Where(Item => Item.Type == UsersType.UserIds).Count();
                    if (HostUserIdsCount == Mobj.MaxHost && HostUserIdsCount == Mobj.SlotList[i].Hosts.Count)
                    {
                        query += MeetingSlotParticipantsQry(Mobj.SlotList[i].Hosts, usr, ParticipantOpt.Fixed, ParticipantType.Host, tbl);
                    }
                    if (AttendeeUserIdsCount == Mobj.MaxAttendee && AttendeeUserIdsCount == Mobj.SlotList[i].Attendees.Count)
                    {
                        query += MeetingSlotParticipantsQry(Mobj.SlotList[i].Attendees, usr, ParticipantOpt.Fixed, ParticipantType.Attendee, tbl);
                    }
                }
            }
            //else if (Mobj.MeetingOpts == MeetingOptions.F_H_F_A && Mobj.IsMultipleMeeting == "F")
            //{
            //    query += AddMeetingSchedule(Mobj, usr);
            //    query += $@"
            //insert into eb_meeting_slots (eb_meeting_schedule_id,meeting_date,time_from,time_to,eb_created_by,eb_created_at) values 
            //(eb_currval('eb_meeting_schedule_id_seq'),'{Mobj.Date}','{Mobj.TimeFrom}','{Mobj.TimeTo}', {usr.UserId},now() );
            //insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            //is_completed,eb_del) values('{Mobj.Host}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //'{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');  
            //insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            //is_completed,eb_del) values('{Mobj.Attendee}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //'{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');
            // ";
            //    for (i = 0; i < Host.Length; i++)
            //    {
            //        query += $@"
            //         insert into eb_meeting_slot_participants(user_id, confirmation, eb_meeting_schedule_id, approved_slot_id, name, email, type_of_user, participant_type) 
            //         values ({Host[i]}, 2, eb_currval('eb_meeting_schedule_id_seq'), eb_currval('eb_meeting_slots_id_seq'), '', '', 1, 1);
            //        ";
            //    }
            //    for (i = 0; i < Attendee.Length; i++)
            //    {
            //        query += $@"
            //         insert into eb_meeting_slot_participants(user_id, confirmation, eb_meeting_schedule_id, approved_slot_id, name, email, type_of_user, participant_type) 
            //         values ({Attendee[i]}, 2, eb_currval('eb_meeting_schedule_id_seq'), eb_currval('eb_meeting_slots_id_seq'), '', '', 1, 2);
            //        ";
            //    }
            //}
            //else if (Mobj.MeetingOpts == MeetingOptions.F_H_F_A)
            //{
            //    query += AddMeetingSchedule(Mobj, usr);
            //    for (i = 0; i < Mobj.SlotList.Count; i++)
            //    {
            //        string[] Hosts = Mobj.SlotList[i].FixedHost.Split(',').Select(sValue => sValue.Trim()).ToArray();
            //        string[] Attendees = Mobj.SlotList[i].EligibleAttendees.Split(',').Select(sValue => sValue.Trim()).ToArray();
            //        query += $@"insert into eb_meeting_slots (eb_meeting_schedule_id,meeting_date,time_from,time_to,eb_created_by,eb_created_at) values 
            //        (eb_currval('eb_meeting_schedule_id_seq'),'{Mobj.Date}','{Mobj.SlotList[i].TimeFrom}:00','{Mobj.SlotList[i].TimeTo}:00', {usr.UserId},now() );";
            //        for (int j = 0; j < Hosts.Length; j++)
            //        {
            //            query += $@"
            //         insert into eb_meeting_slot_participants(user_id, confirmation, eb_meeting_schedule_id, approved_slot_id, name, email, type_of_user, participant_type,
            //           eb_created_at,eb_created_by) 
            //         values ({Hosts[j]}, 2, eb_currval('eb_meeting_schedule_id_seq'), eb_currval('eb_meeting_slots_id_seq'), '', '', 1, 1,now(),{usr.UserId});
            //        ";
            //        }
            //        for (i = 0; i < Attendees.Length; i++)
            //        {
            //            query += $@"insert into eb_meeting_slot_participants(user_id, confirmation, eb_meeting_schedule_id, approved_slot_id, name, email, type_of_user, participant_type,
            //        eb_created_at,eb_created_by) 
            //         values ({Attendees[i]}, 2, eb_currval('eb_meeting_schedule_id_seq'), eb_currval('eb_meeting_slots_id_seq'), '', '', 1, 2,now(),{usr.UserId});
            //        ";
            //        }
            //        query += $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            //        is_completed,eb_del) values('{Mobj.SlotList[i].FixedHost}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //        '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');
            //        insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            //        is_completed,eb_del) values('{Mobj.SlotList[i].FixedAttendee}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //        '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');";
            //    }
            //}
            //else if (Mobj.MeetingOpts == MeetingOptions.F_H_E_A)
            //{
            //    query += AddMeetingSchedule(Mobj, usr);
            //    for (i = 0; i < Mobj.SlotList.Count; i++)
            //    {
            //        string[] Hosts = Mobj.SlotList[i].FixedHost.Split(',').Select(sValue => sValue.Trim()).ToArray();
            //        string[] Attendees = Mobj.SlotList[i].EligibleAttendees.Split(',').Select(sValue => sValue.Trim()).ToArray();
            //        query += $@"insert into eb_meeting_slots (eb_meeting_schedule_id,meeting_date,time_from,time_to,eb_created_by,eb_created_at) values 
            //        (eb_currval('eb_meeting_schedule_id_seq'),'{Mobj.Date}','{Mobj.SlotList[i].TimeFrom}:00','{Mobj.SlotList[i].TimeTo}:00', {usr.UserId},now() );";
            //        for (int j = 0; j < Hosts.Length; j++)
            //        {
            //            query += $@"
            //         insert into eb_meeting_slot_participants(user_id, confirmation, eb_meeting_schedule_id, approved_slot_id, name, email, type_of_user, participant_type,
            //           eb_created_at,eb_created_by) 
            //         values ({Hosts[j]}, 2, eb_currval('eb_meeting_schedule_id_seq'), eb_currval('eb_meeting_slots_id_seq'), '', '', 1, 1,now(),{usr.UserId});
            //        ";
            //        }
            //        query += $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            //        is_completed,eb_del) values('{Mobj.SlotList[i].FixedHost}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //        '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');";
            //    }
            //    query += $@"
            //        insert into eb_meeting_scheduled_participants (user_ids,role_ids,eb_meeting_schedule_id,participant_type,type_of_user,
            //  eb_created_at,eb_created_by)values('{ Mobj.EligibleAttendees}','',eb_currval('eb_meeting_schedule_id_seq') , 2 ,1,now(),{usr.UserId});
            //        ";
            //    query += $@"insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_schedule_id, 
            //    is_completed,eb_del) values('{Mobj.EligibleAttendees}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //    '{MyActionTypes.Meeting}',eb_currval('eb_meeting_schedule_id_seq') , 'F','F');";
            //}
            //else if (Mobj.MeetingOpts == MeetingOptions.E_H_F_A)
            //{
            //    query += AddMeetingSchedule(Mobj, usr);
            //    for (i = 0; i < Mobj.SlotList.Count; i++)
            //    {
            //        string[] Hosts = Mobj.SlotList[i].FixedHost.Split(',').Select(sValue => sValue.Trim()).ToArray();
            //        string[] Attendees = Mobj.SlotList[i].EligibleAttendees.Split(',').Select(sValue => sValue.Trim()).ToArray();
            //        query += $@"insert into eb_meeting_slots (eb_meeting_schedule_id,meeting_date,time_from,time_to,eb_created_by,eb_created_at) values 
            //        (eb_currval('eb_meeting_schedule_id_seq'),'{Mobj.Date}','{Mobj.SlotList[i].TimeFrom}:00','{Mobj.SlotList[i].TimeTo}:00', {usr.UserId},now() );";
            //        for (int j = 0; j < Attendees.Length; j++)
            //        {
            //            query += $@"
            //         insert into eb_meeting_slot_participants(user_id, confirmation, eb_meeting_schedule_id, approved_slot_id, name, email, type_of_user, participant_type,
            //           eb_created_at,eb_created_by) 
            //         values ({Attendees[j]}, 2, eb_currval('eb_meeting_schedule_id_seq'), eb_currval('eb_meeting_slots_id_seq'), '', '', 1, 2,now(),{usr.UserId});
            //        ";
            //        }
            //        query += $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            //        is_completed,eb_del) values('{Mobj.SlotList[i].FixedAttendee}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //        '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');";
            //    }
            //    query += $@"
            //        insert into eb_meeting_scheduled_participants (user_ids,role_ids,eb_meeting_schedule_id,participant_type,type_of_user,
            //  eb_created_at,eb_created_by)values('{ Mobj.EligibleHosts}','',eb_currval('eb_meeting_schedule_id_seq') , 1 ,1,now(),{usr.UserId});
            //        ";
            //    query += $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_schedule_id, 
            //        is_completed,eb_del) values('{Mobj.EligibleHosts}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //        '{MyActionTypes.Meeting}',eb_currval('eb_meeting_schedule_id_seq') , 'F','F');";

            //}
            //else if (Mobj.MeetingOpts == MeetingOptions.E_H_E_A)
            //{
            //    query += AddMeetingSchedule(Mobj, usr);
            //    for (i = 0; i < Mobj.SlotList.Count; i++)
            //    {
            //        query += $@"insert into eb_meeting_slots (eb_meeting_schedule_id,meeting_date,time_from,time_to,eb_created_by,eb_created_at) values 
            //        (eb_currval('eb_meeting_schedule_id_seq'),'{Mobj.Date}','{Mobj.SlotList[i].TimeFrom}:00','{Mobj.SlotList[i].TimeTo}:00', {usr.UserId},now() );";
            //    }
            //    query += $@"
            //        insert into eb_meeting_scheduled_participants (user_ids,role_ids,eb_meeting_schedule_id,participant_type,type_of_user,
            //  eb_created_at,eb_created_by)values('{ Mobj.EligibleHosts}','',eb_currval('eb_meeting_schedule_id_seq') , 1 ,1,now(),{usr.UserId});
            //        insert into eb_meeting_scheduled_participants (user_ids,role_ids,eb_meeting_schedule_id,participant_type,type_of_user,
            //  eb_created_at,eb_created_by)values('{ Mobj.EligibleAttendees}','',eb_currval('eb_meeting_schedule_id_seq') , 2 ,1,now(),{usr.UserId});
            //        ";
            //    query += $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_schedule_id, 
            //        is_completed,eb_del) values('{Mobj.SlotList[0].EligibleHosts}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //        '{MyActionTypes.Meeting}',eb_currval('eb_meeting_schedule_id_seq') , 'F','F');
            //        insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_schedule_id, 
            //        is_completed,eb_del) values('{Mobj.EligibleAttendees}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            //        '{MyActionTypes.Meeting}',eb_currval('eb_meeting_schedule_id_seq') , 'F','F');";
            //}

            _extqry += query;
            return true;
        }

        public string MeetingSlotParticipantsQry(List<Participants> Participants, User usr , ParticipantOpt Opt, ParticipantType ParticipantType,string tbl)
        {
            string query = "";
            if (Opt == ParticipantOpt.Fixed)
            {
                string userids = "";
                string Roles = "";
                int UserGroup;
                for (int j = 0; j < Participants.Count; j++)
                {
                    
                    if (Participants[j].Type == UsersType.UserIds)
                        userids +=  Participants[j].Id + "," ;
                    else if (Participants[j].Type == UsersType.RoleId)
                        Roles += Participants[j].Id + "," ;
                    else
                        UserGroup = Participants[j].Id;
                    query += $@"
                insert into eb_meeting_slot_participants(user_id, confirmation, eb_meeting_schedule_id, approved_slot_id, name, email, type_of_user, participant_type,
                eb_created_at,eb_created_by) 
                values ({Participants[j].Id}, 2, eb_currval('eb_meeting_schedule_id_seq'), eb_currval('eb_meeting_slots_id_seq'), '', '', 1, {(int)ParticipantType},now(),{usr.UserId});
               ";
                }
                query += $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            is_completed,eb_del) values('{userids}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');
            insert into eb_my_actions (role_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            is_completed,eb_del) values('{Roles}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');";
            }

            if (Opt == ParticipantOpt.Eligible)
            {
                string userids ="";
                string Roles ="";
                int UserGroup;
                for (int j = 0; j < Participants.Count; j++)
                {
                    if(Participants[j].Type == UsersType.UserIds)
                    {
                        userids += Participants[j].Id;
                    } 
                    else if(Participants[j].Type == UsersType.RoleId)
                    {
                        Roles += Participants[j].Id;
                    }
                    else
                    {
                        UserGroup = Participants[j].Id;
                    }
                    query += $@"
            insert into eb_meeting_scheduled_participants (user_ids,role_ids,eb_meeting_schedule_id,participant_type,type_of_user,
            eb_created_at,eb_created_by)values('{userids}','{Roles}',eb_currval('eb_meeting_schedule_id_seq') , 1 ,1,now(),{usr.UserId});
             insert into eb_meeting_scheduled_participants (user_ids,role_ids,eb_meeting_schedule_id,{(int)ParticipantType},type_of_user,
                  ";
            query += $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_schedule_id, 
            is_completed,eb_del) values('{userids}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            '{MyActionTypes.Meeting}',eb_currval('eb_meeting_schedule_id_seq') , 'F','F');
            insert into eb_my_actions (role_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_schedule_id, 
            is_completed,eb_del) values('{Roles}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            '{MyActionTypes.Meeting}',eb_currval('eb_meeting_schedule_id_seq') , 'F','F');";
                }
            }
            return query;
        }

        public string SingleMeeting()
        {

            return string.Empty;
        }
        public string AddMeetingSchedule(MeetingSchedule Mobj, User usr)
        {
            string qry = $@"insert into eb_meeting_schedule (title,description,meeting_date,time_from,time_to,duration,is_recuring,is_multiple,venue,
			integration,max_hosts,max_attendees,no_of_attendee,no_of_hosts,eb_created_by,eb_created_at,meeting_opts)
			values('{Mobj.Title}','{Mobj.Description}','{Mobj.Date}','{Mobj.TimeFrom}:00','{Mobj.TimeTo}:00', 0,'{Mobj.IsRecuring}',
            '{Mobj.IsMultipleMeeting}','{Mobj.Location}','',{Mobj.MaxHost},{Mobj.MaxAttendee},{Mobj.MinAttendee},{Mobj.MinHost},{usr.UserId},now(),{(int)Mobj.MeetingType});";
            return qry;
        }
        public string AddMeetingSlots(MeetingSchedule Mobj, User usr)
        {
            string qry = $@" 
            insert into eb_meeting_slots (eb_meeting_schedule_id,meeting_date,time_from,time_to,eb_created_by,eb_created_at) values 
            (eb_currval('eb_meeting_schedule_id_seq'),'{Mobj.Date}','{Mobj.TimeFrom}','{Mobj.TimeTo}', {usr.UserId},now() );
            ";
            return qry;
        }
        public string AddMyAction(MeetingSchedule Mobj, User usr, string tbl)
        {
            string qry = $@" insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            is_completed,eb_del) values('{Mobj.EligibleHosts}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');  
            insert into eb_my_actions (user_ids,from_datetime,form_ref_id,form_data_id,description,my_action_type , eb_meeting_slots_id, 
            is_completed,eb_del) values('{Mobj.EligibleAttendees}',NOW(),@refid, eb_currval('{tbl}_id_seq'), 'Meeting Request',
            '{MyActionTypes.Meeting}',eb_currval('eb_meeting_slots_id_seq') , 'F','F');";
            return qry;
        }
        public string AddEligibleParticipants(MeetingSchedule Mobj, User usr)
        {
            string qry = $@"
            ";
            return qry;
        }
    }
    public class MeetingSchedule
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string IsSingleMeeting { get; set; }
        public string IsMultipleMeeting { get; set; }
        public string IsRecuring { get; set; }
        public int DayCode { get; set; }
        public string Date { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string Duration { get; set; }
        public int MaxHost { get; set; }
        public int MinHost { get; set; }
        public int MaxAttendee { get; set; }
        public int MinAttendee { get; set; }
        public string EligibleHosts { get; set; }
        public string EligibleAttendees { get; set; }
        public string Host { get; set; }
        public string Attendee { get; set; }
        public MeetingType MeetingType { get; set; }
        public List<Slots> SlotList { get; set; }
        public MeetingSchedule()
        {
            this.SlotList = new List<Slots>();
        }
        public int UserIdCount(List<Participants> Participants, UsersType Type)
        {
            var count = Participants.Where(item => item.Type == Type).Count();
            return count;
        }
    }
    public class Slots
    {
        public int Position { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string EligibleHosts { get; set; }
        public string EligibleAttendees { get; set; }
        public string FixedHost { get; set; }
        public string FixedAttendee { get; set; }
        public List<Participants> Hosts { get; set; }
        public List<Participants> Attendees { get; set; }
    }
    public class Participants
    {
        public UsersType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public enum UsersType
    {
        UserIds = 1,
        UserGroupId = 2,
        RoleId = 3,
    }
    public enum MeetingOptions
    {
        F_H_F_A = 1, /*fixed host and fixed attendee*/
        F_H_E_A = 2, /*fixed host and eligible attendee*/
        E_H_F_A = 3, /*eligible host and fixed attendee*/
        E_H_E_A = 4, /*eligible host and eligible attendee*/
    }
    public enum ParticipantOpt
    {
        Fixed = 1,
        Eligible = 2,
    }
    public enum QueryOpts
    {
        insert = 1,
        update = 2
    }
    public enum MeetingType
    {
        SingleMeeting = 1,
        MultipleMeeting = 2,
        AdvancedMeeting = 3
    }

    public class ParticipantsListResponse
    {
        public List<Participants> ParticipantsList { get; set; }

        public ParticipantsListResponse()
        {
            this.ParticipantsList = new List<Participants>();
        }
    }
    public class ParticipantsListRequest
    {

    }
}

