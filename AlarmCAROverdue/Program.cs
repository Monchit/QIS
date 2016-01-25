using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlarmCAROverdue
{
    class Program
    {
        static void Main(string[] args)
        {
            Program pg = new Program();
            try
            {
                QISEntities dbQIS = new QISEntities();
                MailCenterEntities dbMail = new MailCenterEntities();
                DateTime now = DateTime.Now.Date;
                var query = from a in dbQIS.Car
                            where !a.reply_date.HasValue && a.QCReject.status_id == 4 && a.reply_due_date < now
                            select a;
                foreach (var item in query)
                {
                    int? group = 0;
                    using (var db = new QISEntities())
                    {
                        var get_group = (from a in db.Master_Product
                                         where a.product_id == item.QCReject.product_id
                                         select a).FirstOrDefault();
                        if (get_group != null)
                        {
                            group = get_group.group_id;
                        }
                    }
                    string QCMail = pg.GetEmailQC(item.plant_id);
                    string ProdMail = pg.GetEmailProduction(group);
                    string ProdSupMail = pg.GetEmailProdSupDown(group);
                    var receiver = QCMail + ProdMail + ProdSupMail;
                    pg.InsertTemp(receiver.Substring(1), item.qc_reject_id);
                    Console.WriteLine(item.qc_reject_id + " OK");
                }
                //----------------------------------------------------------//
                var getbyemail = from t in dbQIS.TempEmail
                                 where t.type == 2
                                 group t by t.email into g
                                 select new { email = g.Key, id = g };

                foreach (var g in getbyemail)
                {
                    string topemail = g.email.Replace(';', ',');
                    string htmlBody = "<table border=\"1\" style=\"border-collapse:collapse; font-size: 11px;\"><tr><td><b>No.</b></td><td><b>Plant</b></td><td><b>Rejected Date</b></td><td><b>Product</b></td><td><b>Item code</b></td><td><b>Lot no.</b></td><td><b>Problem detail</b></td><td><b>Defective Level</b></td><td><b>Due date</b></td><td><b>Over due (Days)</b></td><td><b>Picture</b></td><td><b>More Detail</b></td></tr>";
                    int count = 0;//Add Count for Check table row //2015-03-17
                    foreach (var n in g.id)
                    {
                        var data = (from q in dbQIS.QCReject
                                    where q.qc_reject_id == n.qc_reject_id
                                    orderby q.product_id
                                    select q).FirstOrDefault();
                        if (data != null)
                        {
                            count++;
                            int over_dt = now.Subtract(data.Car.reply_due_date).Days;
                            if (over_dt > 60)
                            {
                                htmlBody += "<tr bgcolor='#FF0000'>";
                            }
                            else if (over_dt > 30)
                            {
                                htmlBody += "<tr bgcolor='#FFFF00'>";
                            }
                            else
                            {
                                htmlBody += "<tr>";
                            }
                            htmlBody += "<td>" + count + "</td><td>"
                                        + data.Master_Product.Master_Plant.plant_name + "</td><td>"
                                        + data.entry_date.ToString("d MMM yyyy") + "</td><td>"
                                        + data.Master_Product.product_name + "</td><td>"
                                        + data.item_code + "</td><td>"
                                        + data.lot_no + "</td><td>"
                                        + data.problem + "</td><td>"
                                        + data.defective_lv + "</td><td>"
                                        + data.Car.reply_due_date.ToString("d MMM yyyy") + "</td><td>"
                                        + now.Subtract(data.Car.reply_due_date).Days + "</td><td>";
                            htmlBody += data.picture != null ?
                                        "<a href=\"http://webExternal/qis/" + data.picture + "\">Show</a></td><td>" : "No</td><td>";
                            htmlBody += "<a href=\"http://webExternal/qis/qcreject/detail/" + data.qc_reject_id + "\">Click</a></td></tr>";
                        }
                    }
                    htmlBody += "</table>";

                    if (!string.IsNullOrEmpty(topemail) && count != 0)
                    {
                        //string topemail = g.email != "thaworn@nok.co.th" ? g.email : g.email + ",chiba@nok.co.th,yoshioka@nok.co.th,sirichai@nok.co.th";
                        TT_MAIL_WIP ttMail = new TT_MAIL_WIP();
                        ttMail.ProgramID = 3;
                        ttMail.CreateDate = DateTime.Now;
                        ttMail.Sender = "TNCAutoMail-QIS@nok.co.th";
                        //ttMail.Receiver = "monchit@nok.co.th";//For Test
                        ttMail.Receiver = topemail;//Open when Real
                        ttMail.BCC = "wichet@nok.co.th";//Open when Real
                        ttMail.Title = "Alarm CAR Overdue Date.";
                        ttMail.HTMLBody = htmlBody;
                        //ttMail.Flag = 1;//Comment this line when Real.

                        var qinsert = dbMail.TT_MAIL_WIP.Add(ttMail);
                    }
                }
                dbMail.SaveChanges();

                //Delete Temp Email //Open when Real
                //((IObjectContextAdapter)dbQIS).ObjectContext.ExecuteStoreCommand("TRUNCATE TABLE TempEmail");
                ((IObjectContextAdapter)dbQIS).ObjectContext.ExecuteStoreCommand("DELETE FROM TempEmail WHERE type = 2");
                dbQIS.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured: " + ex.GetType().ToString());
                Console.WriteLine("Main Error Message: " + ex.Message);
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("All Completed.");
                Environment.Exit(0);
            }
            Console.ReadLine();
        }

        private string GetEmailQC(int plant)
        {
            string email_list = "";
            using (var db = new QISEntities())
            {
                var query = (from a in db.AuthUser
                             where a.plant_id == plant && a.utype_id == 3
                             select a).FirstOrDefault();
                if (query != null)
                {
                    using (var dbTNC = new TNC_ADMINEntities())
                    {
                        var get_mail = (from a in dbTNC.View_Organization
                                        where a.GroupMgr == query.user_code
                                        select a).FirstOrDefault();
                        if (get_mail != null)
                        {
                            email_list += !string.IsNullOrEmpty(get_mail.GroupMgr_email) ? "," + get_mail.GroupMgr_email : string.Empty;
                            email_list += !string.IsNullOrEmpty(get_mail.DeptMgr_email) ? "," + get_mail.DeptMgr_email : string.Empty;
                            //email_list += !string.IsNullOrEmpty(get_mail.PlantMgr_email) ? "," + get_mail.PlantMgr_email : string.Empty;
                        }
                        else
                        {
                            var get_mail_dept = (from a in dbTNC.View_Organization
                                                 where a.DeptMgr == query.user_code
                                                 select a).FirstOrDefault();
                            if (get_mail_dept != null)
                            {
                                email_list += !string.IsNullOrEmpty(get_mail_dept.DeptMgr_email) ? "," 
                                    + get_mail_dept.DeptMgr_email : string.Empty;
                                //email_list += !string.IsNullOrEmpty(get_mail_dept.PlantMgr_email) ? "," 
                                //    + get_mail_dept.PlantMgr_email : string.Empty;
                            }
                        }
                    }
                }
            }
            return email_list;
        }

        private string GetEmailProduction(int? group_id)
        {
            string email_list = "";
            using (var db = new TNC_ADMINEntities())
            {
                var query = (from a in db.View_Organization
                             where a.group_id == group_id
                             select a).FirstOrDefault();
                
                if (query != null)
                {
                    email_list += !string.IsNullOrEmpty(query.GroupMgr_email) ? "," + query.GroupMgr_email : string.Empty;
                    email_list += !string.IsNullOrEmpty(query.DeptMgr_email) ? "," + query.DeptMgr_email : string.Empty;
                    //email_list += !string.IsNullOrEmpty(query.PlantMgr_email) ? "," + query.PlantMgr_email + ";thaworn@nok.co.th;sirichai@nok.co.th" : string.Empty;
                    if (!string.IsNullOrEmpty(query.PlantMgr_email))
                    {
                        if (query.PlantMgr_email == "manoj@nok.co.th")
                        {
                            email_list += "," + query.PlantMgr_email + ";thaworn@nok.co.th;sirichai@nok.co.th;Saroj@nok.co.th";
                        }
                        else
                        {
                            email_list += "," + query.PlantMgr_email + ";thaworn@nok.co.th;sirichai@nok.co.th";
                        }
                    }else{
                        email_list += string.Empty;
                    }
                }
            }
            return email_list;
        }

        private string GetEmailProdSupDown(int? group_id)
        {
            string email_list = "";
            using (var db = new TNC_ADMINEntities())
            {
                var query = from a in db.tnc_user
                            where a.emp_group == group_id && (a.email != null && a.email != "")
                            && (a.emp_position == 3 || a.emp_position == 5)//Staff or Supervisor
                            select a.email;

                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        email_list += "," + item;
                    }
                    //email_list = email_list.Substring(1);
                }
            }
            return email_list;
        }

        private void InsertTemp(string receiver, int qc_reject_id)
        {
            using (var db = new QISEntities())
            {
                try
                {
                    //char[] delimiters = new char[] { ',', ';' };
                    //var emails = receiver.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    var emails = receiver.Split(',');
                    foreach (string email in emails)
                    {
                        if (email != "")
                        {
                            var demail = "";
                            if (email == "pansak@nok.co.th")//PPP
                            {
                                demail = email + ",metan@nok.co.th";
                            }
                            else if (email == "Poonsab@nok.co.th")//OSP2
                            {
                                demail = email + ",jatuphol@nok.co.th";
                            }
                            else if (email == "wichet@nok.co.th")//OSP1
                            {
                                demail = email + ",sereepap@nok.co.th";
                            }
                            //else if (email == "manoj@nok.co.th")//T&D
                            //{
                            //    demail = email + ",Saroj@nok.co.th";
                            //}
                            else if (email == "terapon@nok.co.th")//PTN
                            {
                                demail = email + ",Saroj@nok.co.th";
                            }
                            else
                            {
                                demail = email;
                            }

                            var tbTemp = new TempEmail();
                            tbTemp.qc_reject_id = qc_reject_id;
                            tbTemp.email = demail;
                            //tbTemp.email = "monchit@nok.co.th";//For Test
                            tbTemp.create_date = DateTime.Now;
                            tbTemp.type = 2;//2 = Alarm Mail
                            db.TempEmail.Add(tbTemp);
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("InsertTemp Error Message: " + ex.Message);
                }
            }
        }
    }
}
