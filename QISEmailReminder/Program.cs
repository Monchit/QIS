using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data.Entity.Infrastructure;

namespace QISEmailReminder
{
    class Program
    {
        static void Main(string[] args)
        {
            Program pgm = new Program();

            try
            {
                QISEntities dbQIS = new QISEntities();
                TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();
                MailCenterEntities dbMail = new MailCenterEntities();
                
                var query = from t in dbQIS.TempEmail
                            where t.type == 1
                            group t by t.email into g
                            select new { email = g.Key, id = g };

                foreach (var g in query)
                {
                    string htmlBody = "<table border=\"1\" style=\"border-collapse:collapse; font-size: 11px;\"><tr><td><b>No.</b></td><td><b>Plant</b></td><td><b>Rejected Date</b></td><td><b>Product</b></td><td><b>Item code</b></td><td><b>Lot no.</b></td><td><b>Problem detail</b></td><td><b>Defective Level</b></td><td><b>Picture</b></td><td><b>Issue CAR</b></td><td><b>More Detail</b></td></tr>";
                    int count = 0;//Add Count for Check table row //2015-02-20
                    foreach (var n in g.id)
                    {
                        var data = (from q in dbQIS.QCReject
                                    where q.qc_reject_id == n.qc_reject_id
                                    orderby q.product_id
                                    select q).FirstOrDefault();
                        if (data != null)
                        {
                            count++;
                            htmlBody += "<tr><td>" + count + "</td><td>"
                                        + data.Master_Product.Master_Plant.plant_name + "</td><td>"
                                        + data.entry_date.ToString("d MMM yyyy") + "</td><td>"
                                        + data.Master_Product.product_name + "</td><td>"
                                        + data.item_code + "</td><td>"
                                        + data.lot_no + "</td><td>"
                                        + data.problem + "</td><td>"
                                        + data.defective_lv + "</td><td>";
                            htmlBody += data.picture != null ?
                                        "<a href=\"http://webExternal/qis/" + data.picture + "\">Show</a></td><td>" : "No</td><td>";
                            htmlBody += data.issue_car != 0 ? (data.Car.attach != null ?
                                        "<a href=\"http://webExternal/qis/" + data.Car.attach + "\">Yes</a></td><td>" : "Yes</td><td>") : "No</td><td>";
                            htmlBody += "<a href=\"http://webExternal/qis/qcreject/detail/" + data.qc_reject_id + "\">Click</a></td></tr>";
                        }
                    }
                    htmlBody += "</table>";

                    if (!string.IsNullOrEmpty(g.email) && count != 0)
                    {
                        string topemail = "";
                        if (g.email == "thaworn@nok.co.th")
                        {
                            topemail = g.email + ",chiba@nok.co.th,yoshioka@nok.co.th,sirichai@nok.co.th";
                        }
                        //Add by Monchit 2015-07-29
                        else if (g.email == "wichet@nok.co.th")//QC OSP1
                        {
                            topemail = g.email + ",sereepap@nok.co.th,suzuki@nok.co.th,teerapoom@nok.co.th";
                        }
                        else if (g.email == "Poonsab@nok.co.th")//QC OSP2
                        {
                            topemail = g.email + ",jatuphol@nok.co.th,suzuki@nok.co.th";
                        }
                        else if (g.email == "widchuda@nok.co.th")//Plant PPP
                        {
                            topemail = g.email + ",metan@nok.co.th";
                        }
                        // Update by Monchit 2016-04-28
                        else if (g.email == "yok@nok.co.th")//BPK3
                        {
                            topemail = g.email + ",Manit@nok.co.th";
                        }
                        else if (g.email == "terapon@nok.co.th")//VCP
                        {
                            topemail = g.email + ",Saroj@nok.co.th,mangkorn@nok.co.th";
                        }
                        else if (g.email == "pansak@nok.co.th")//RSP
                        {
                            topemail = g.email + ",Saroj@nok.co.th,nakamura@nok.co.th";
                        }
                        else if (g.email == "manoj@nok.co.th")//T&D
                        {
                            topemail = g.email + ",Saroj@nok.co.th";
                        }
                        ////Add by Monchit 2015-08-20
                        //else if (g.email == "manoj@nok.co.th" //Plant Tool&Die
                        //    || g.email == "terapon@nok.co.th")//Plant VCP
                        //{
                        //    topemail = g.email + ",Saroj@nok.co.th";
                        //}
                        ////Add by Monchit 2016-02-11
                        //else if (g.email == "Saroj@nok.co.th")//Plant RSP
                        //{
                        //    topemail = g.email + ",nakamura@nok.co.th";
                        //}
                        //else if (g.email == "Manit@nok.co.th")//Plant RSP
                        //{
                        //    topemail = g.email + ",suzuki@nok.co.th";
                        //}
                        //End Add
                        else
                        {
                            topemail = g.email;
                        }
                        TT_MAIL_WIP ttMail = new TT_MAIL_WIP();
                        ttMail.ProgramID = 3;
                        ttMail.CreateDate = DateTime.Now;
                        ttMail.Sender = "TNCAutoMail-QIS@nok.co.th";

                        //ttMail.Receiver = "monchit@nok.co.th";//For Test
                        ttMail.Receiver = topemail;//Open when Real
                        ttMail.BCC = "wichet@nok.co.th";//Open when Real
                        ttMail.Title = "QC Reject Information : Notify new issue.";
                        ttMail.HTMLBody = htmlBody;
                        //ttMail.Flag = 1;//Comment this line when Real.

                        var qinsert = dbMail.TT_MAIL_WIP.Add(ttMail);
                    }
                }
                dbMail.SaveChanges();

                //Delete Temp Email //Open when Real
                //((IObjectContextAdapter)dbQIS).ObjectContext.ExecuteStoreCommand("TRUNCATE TABLE TempEmail");
                ((IObjectContextAdapter)dbQIS).ObjectContext.ExecuteStoreCommand("DELETE FROM TempEmail WHERE type = 1");
                dbQIS.SaveChanges();
            }
            catch (Exception)
            {
                Console.WriteLine("Error");
                Console.ReadLine();
            }
        }
    }
}
