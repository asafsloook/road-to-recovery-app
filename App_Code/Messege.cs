﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

// <summary>
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

        //PUSH ANDROID
        var data = new JObject();
        data.Add("message", message);
        data.Add("title", title);
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushANDROID = new myPushNot();
        pushANDROID.RunPushNotificationAll(volunteersList, data, null);


        //PUSH IOS
        var notification = new JObject();
        notification.Add("title", title);
        notification.Add("body", message);
        data = new JObject();
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushIOS = new myPushNot();
        pushIOS.RunPushNotificationAll(volunteersList, data, notification);
    }

    public void cancelRide(int ridePatID, Volunteer user)
    {
        //get ride details and generate msg
        RidePat rp = new RidePat();
        var abc = rp.GetRidePat(ridePatID);
        var msg = "בוטלה נסיעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " בתאריך " + abc.Date.ToShortDateString() + ", בשעה " + abc.Date.ToShortTimeString();
        if (abc.Date.ToShortTimeString() == "22:14") msg = "בוטלה נסיעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " בתאריך " + abc.Date.ToShortDateString() + "אחה\"צ";
        //insert msg to db
        int msgID = insertMsg(0, "Cancel", "נסיעה בוטלה", msg, ridePatID, DateTime.Now, user.Id, "", true, false, false);


        //PUSH ANDROID
        var data = new JObject();
        data.Add("message", msg);
        data.Add("title", "נסיעה בוטלה");
        data.Add("rideID", ridePatID);
        data.Add("status", "Canceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushANDROID = new myPushNot();
        pushANDROID.RunPushNotificationOne(user, data, null);



        //PUSH IOS
        var notification = new JObject();
        notification.Add("title", "נסיעה בוטלה");
        notification.Add("body", msg);
        data = new JObject();
        data.Add("rideID", ridePatID);
        data.Add("status", "Canceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushIOS = new myPushNot();
        pushIOS.RunPushNotificationOne(user, data, notification);
        
    }

    public void rideIsTomorrow(int ridePatID, Volunteer user)
    {
        //get ride details and generate msg
        RidePat rp = new RidePat();
        var abc = rp.GetRidePat(ridePatID);
        var msg = "מחר מתקיימת הסעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + ", בשעה " + abc.Date.ToShortTimeString();

        if (abc.Date.ToShortTimeString() == "22:14") msg = "מחר מתקיימת הסעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " אחה\"צ";
        //insert msg to db
        int msgID = insertMsg(0, "Reminder", "תזכורת", msg, ridePatID, DateTime.Now, user.Id, "", true, false, false);


        //PUSH ANDROID
        var data = new JObject();
        data.Add("message", msg);
        data.Add("title", "נסיעה קרובה");
        data.Add("rideID", ridePatID);
        data.Add("status", "Reminder");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushANDROID = new myPushNot();
        pushANDROID.RunPushNotificationOne(user, data, null);
        
        //PUSH IOS
        var notification = new JObject();
        notification.Add("title", "נסיעה קרובה");
        notification.Add("body", msg);
        data = new JObject();
        data.Add("rideID", ridePatID);
        data.Add("status", "Canceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushIOS = new myPushNot();
        pushIOS.RunPushNotificationOne(user, data, notification);
    }

    public void driverCanceledRide(int ridePatID, Volunteer user)
    {
        //get ride details and generate message
        RidePat rp = new RidePat();
        var abc = rp.GetRidePat(ridePatID);
        Volunteer coor = new Volunteer();
        coor = abc.Coordinator.getVolunteerByDisplayName(abc.Coordinator.DisplayName);

        var message = "";
        if (user.Gender == "מתנדב")
        {
            message = "הנהג " + user.FirstNameH + " " + user.LastNameH + " ביטל את הנסיעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " שמתקיימת מחר";
            if (abc.Date.ToShortTimeString() == "22:14")
            {
                message += " אחה\"צ";
            }
            else message += " בשעה " + abc.Date.ToShortTimeString();
        }
        else
        {
            message = "הנהגת " + user.FirstNameH + " " + user.LastNameH + " ביטלה את הנסיעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " שמתקיימת מחר";
            if (abc.Date.ToShortTimeString() == "22:14")
            {
                message += " אחה\"צ";
            }
            else message += " בשעה " + abc.Date.ToShortTimeString();
        }
        //insert msg to db
        int msgID = insertMsg(0, "Canceled by driver", "נסיעה בוטלה על ידי נהג\\ת", message, ridePatID, DateTime.Now, user.Id, "", true, false, false);


        //PUSH ANDROID
        var data = new JObject();
        data.Add("message", message);
        data.Add("title", "נסיעה בוטלה על ידי נהג\\ת");
        data.Add("rideID", ridePatID);
        data.Add("status", "Canceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushANDROID = new myPushNot();
        pushANDROID.RunPushNotificationOne(coor, data, null);

        //PUSH IOS
        var notification = new JObject();
        notification.Add("title", "נסיעה בוטלה על ידי נהג\\ת");
        notification.Add("body", message);
        data = new JObject();
        data.Add("rideID", ridePatID);
        data.Add("status", "Canceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushIOS = new myPushNot();
        pushIOS.RunPushNotificationOne(user, data, notification);
    }

    public void changeAnonymousPatient(int ridePatID, Volunteer user)
    {
        //get ride details and generate msg
        RidePat rp = new RidePat();
        var abc = rp.GetRidePat(ridePatID);
        var msg = "עודכן חולה אנונימי ל " + abc.Pat.DisplayName + "בנסיעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " בתאריך " + abc.Date.ToShortDateString() + ", בשעה " + abc.Date.ToShortTimeString();
        if (abc.Date.ToShortTimeString() == "22:14") msg = "עודכן חולה אנונימי ל " + abc.Pat.DisplayName + "הנסיעה מ" + abc.Origin.Name + " ל" + abc.Destination.Name + " בתאריך " + abc.Date.ToShortDateString() + "אחה\"צ";
        //insert msg to db
        int msgID = insertMsg(1, "Anonymous Patient changed", "חולה אנונימי השתנה", msg, ridePatID, DateTime.Now, user.Id, "", true, false, false);
        
        //PUSH ANDROID
        var data = new JObject();
        data.Add("message", msg);
        data.Add("title", "חולה אנונימי השתנה");
        data.Add("rideID", ridePatID);
        data.Add("status", "Anonymous Patient changed");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushANDROID = new myPushNot();
        pushANDROID.RunPushNotificationOne(user, data, null);

        //PUSH IOS
        var notification = new JObject();
        notification.Add("title", "חולה אנונימי השתנה");
        notification.Add("body", msg);
        data = new JObject();
        data.Add("rideID", ridePatID);
        data.Add("status", "Canceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushIOS = new myPushNot();
        pushIOS.RunPushNotificationOne(user, data, notification);
    }

    public int backupToPrimary(int ridePatID)
    {
        string time = "";
        //get ride details and generate msg
        RidePat rp = new RidePat();
        rp = rp.GetRidePat(ridePatID);
        Volunteer v = new Volunteer();
        if (rp.Drivers[0].DriverType == "Secondary") v = rp.Drivers[0];
        else throw new Exception("The assigned driver is the primary driver for this ride");

        if (rp.Date.ToShortTimeString() == "22:14") time = "אחה\"צ"; else time = rp.Date.ToShortTimeString();
        string msg = "האם ברצונך להחליף את הנהג הראשי בנסיעה מ" + rp.Origin.Name + " ל" + rp.Destination.Name + " בתאריך " + rp.Date.ToShortDateString() + ", בשעה " + time + "?";

        //insert msg to db
        int msgID = insertMsg(0, "BackupToPrimary", "החלפת נהג ראשי", msg, ridePatID, DateTime.Now, v.Id, "", true, false, false);
        
        //PUSH ANDROID
        var data = new JObject();
        data.Add("message", msg);
        data.Add("title", "החלפת נהג ראשי");
        data.Add("rideID", ridePatID);
        data.Add("status", "PrimaryCanceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushANDROID = new myPushNot();
        pushANDROID.RunPushNotificationOne(v, data, null);

        //PUSH IOS
        var notification = new JObject();
        notification.Add("title", "החלפת נהג ראשי");
        notification.Add("body", msg);
        data = new JObject();
        data.Add("rideID", ridePatID);
        data.Add("status", "Canceled");
        data.Add("msgID", msgID);
        data.Add("content-available", 1);
        //send push
        myPushNot pushIOS = new myPushNot();
        pushIOS.RunPushNotificationOne(v, data, notification);

        return 1;
    }
}