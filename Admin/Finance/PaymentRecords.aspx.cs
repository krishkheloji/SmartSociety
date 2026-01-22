using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Finance
{
    public partial class PaymentRecords : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadPayments();
            }
        }

        // 🔹 Load all payments with unit and bill info
        private void LoadPayments()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        P.PaymentId,
                        P.BillId,
                        U.UnitNo,
                        P.PaidOn,
                        P.Amount,
                        P.Mode,
                        P.ReferenceNo
                    FROM Payments P
                    INNER JOIN MaintenanceBills MB ON P.BillId = MB.BillId
                    INNER JOIN Units U ON MB.UnitId = U.UnitId
                    ORDER BY P.PaymentId DESC", con);

                con.Open();
                gvPayments.DataSource = cmd.ExecuteReader();
                gvPayments.DataBind();
            }
        }

        // 🔹 Handle grid commands (e.g., verify)
        protected void gvPayments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "VerifyPayment")
            {
                int paymentId = Convert.ToInt32(e.CommandArgument);
                MarkPaymentVerified(paymentId);
            }
        }

        // 🔹 Update payment and bill status (simulation)
        private void MarkPaymentVerified(int paymentId)
        {
            // Since your DB table 'Payments' has no 'Status' column, 
            // we’ll just simulate the verification process.
            try
            {
                lblMessage.CssClass = "text-success";
                lblMessage.Text = "✅ Payment ID " + paymentId + " marked as verified (simulated).";
            }
            catch (Exception ex)
            {
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = "❌ Error: " + ex.Message;
            }
        }
    }
}
