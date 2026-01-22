using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.Maintenance
{
    public partial class PaymentSuccess : System.Web.UI.Page
    {
       


            string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string paymentId = Request.QueryString["payment_id"];
                string billIdStr = Request.QueryString["bill_id"];

                if (!string.IsNullOrEmpty(paymentId) && !string.IsNullOrEmpty(billIdStr))
                {
                    long billId = Convert.ToInt64(billIdStr);
                    SavePayment(paymentId, billId);
                }
                else
                {
                    lblMsg.Text = "Invalid payment confirmation.";
                    lblMsg.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void SavePayment(string paymentId, long billId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    decimal amount = 0;
                    SqlCommand getAmt = new SqlCommand("SELECT TotalAmount FROM MaintenanceBills WHERE BillId=@BillId", con, tran);
                    getAmt.Parameters.AddWithValue("@BillId", billId);
                    object amtObj = getAmt.ExecuteScalar();
                    if (amtObj != null) amount = Convert.ToDecimal(amtObj);

                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Payments (BillId, PaidOn, Amount, Mode, ReferenceNo)
                        VALUES (@BillId, GETDATE(), @Amount, 'Online', @RefNo)", con, tran);
                    cmd.Parameters.AddWithValue("@BillId", billId);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@RefNo", paymentId);
                    cmd.ExecuteNonQuery();

                    SqlCommand update = new SqlCommand("UPDATE MaintenanceBills SET Status='Paid' WHERE BillId=@BillId", con, tran);
                    update.Parameters.AddWithValue("@BillId", billId);
                    update.ExecuteNonQuery();

                    tran.Commit();

                    lblMsg.Text = "Payment Successful! Reference: " + paymentId;
                    lblMsg.ForeColor = System.Drawing.Color.Green;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    lblMsg.Text = "Error saving payment: " + ex.Message;
                    lblMsg.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

    }
}


