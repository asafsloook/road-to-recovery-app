﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Messege
/// </summary>
public class Message
{
    public Message()
    {
        //
        // TODO: Add constructor logic here
        //
        
    }

    public int insertMsg(int parentID, string type, string title, string msgContent, int ridePatID, DateTime dateTime, int userID, string userNotes, bool isPush, bool isMail, bool isWhatsapp)
    {

        DbService db = new DbService();
        SqlCommand cmd = new SqlCommand();
        cmd.CommandType = CommandType.Text;
        SqlParameter[] cmdParams = new SqlParameter[11];
        cmdParams[0] = cmd.Parameters.AddWithValue("@ParentID", parentID);
        cmdParams[1] = cmd.Parameters.AddWithValue("@Type", type);
        cmdParams[2] = cmd.Parameters.AddWithValue("@Title", title);
        cmdParams[3] = cmd.Parameters.AddWithValue("@MsgContent", msgContent);
        cmdParams[4] = cmd.Parameters.AddWithValue("@RidePatID", ridePatID);
        cmdParams[5] = cmd.Parameters.AddWithValue("@DateTime", dateTime);
        cmdParams[6] = cmd.Parameters.AddWithValue("@UserID", userID);
        cmdParams[7] = cmd.Parameters.AddWithValue("@UserNotes", userNotes);
        cmdParams[8] = cmd.Parameters.AddWithValue("@isPush", isPush);
        cmdParams[9] = cmd.Parameters.AddWithValue("@isMail", isMail);
        cmdParams[10] = cmd.Parameters.AddWithValue("@isWhatsapp", isWhatsapp);
        string query = "insert into [Messages] OUTPUT inserted.MsgID values (@ParentID,@Type,@Title,@MsgContent,@RidePatID,@DateTime,@UserID,@UserNotes,@isPush,@isMail,@isWhatsapp)";
        
        try
        {
            return int.Parse(db.GetObjectScalarByQuery(query, cmd.CommandType, cmdParams).ToString());
        }
        catch (Exception e)
        {
            //add to log
            throw e;
        }
    }

    public void globalMessage(string message, string title)
    {
        //insert msg to db
        int msgID = insertMsg(0, "Global", title, message, 0, DateTime.Now, 0, "", true, false, false);

        //get volunteers
        Volunteer v = new Volunteer();
        List<Volunteer> volunteersList = v.getVolunteersList(true);
        
        //create push
        var x = new JObject();
        x.Add("message", message);
        x.Add("title", title);
        x.Add("msgID", msgID);
        x.Add("content-available", 1);

        //send push
        myPushNot push = new myPushNot();
        push.RunPushNotificationAll(volunteersList, x);
    }

    public void cancelRide(int ridePatID, Volunteer user)
    {
        //get ride details and generate msg
        RidePat rp = new RidePat();
        var abc = rp.GetRidePat(ridePatID);
        var msg = "בוטלה נסיעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " בתאריך " + abc.Date.ToShortDateString() + ", בשעה " + abc.Date.ToShortTimeString();

        //insert msg to db
        int msgID = insertMsg(0, "Cancel", "נסיעה בוטלה", msg, ridePatID, DateTime.Now, user.Id, "", true, false, false);

        //create push
        var x = new JObject();
        x.Add("message", msg);
        x.Add("title", "נסיעה בוטלה");
        x.Add("rideID", ridePatID);
        x.Add("status", "Canceled");
        x.Add("msgID", msgID);
        x.Add("content-available", 1);
       
        //send push
        myPushNot push = new myPushNot();
        push.RunPushNotificationOne(user, x);
    }
}